using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Input;

namespace EpgTimer
{
    class EpgViewPanel : FrameworkElement
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(ObservableNotifiableCollection<ProgramViewItem>),
                typeof(EpgViewPanel),
                new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(typeof(EpgViewPanel));

        public Brush Background
        {
            set { SetValue(BackgroundProperty, value); }
            get { return (Brush)GetValue(BackgroundProperty); }
        }

        public ObservableNotifiableCollection<ProgramViewItem> ItemsSource
        {
            set { SetValue(ItemsSourceProperty, value); }
            get { return (ObservableNotifiableCollection<ProgramViewItem>)GetValue(ItemsSourceProperty); }
        }

        static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as EpgViewPanel).OnItemsSourceChanged(args);
        }

        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != null)
            {
                ObservableNotifiableCollection<ProgramViewItem> coll = args.OldValue as ObservableNotifiableCollection<ProgramViewItem>;
                coll.CollectionChanged -= OnCollectionChanged;
                coll.ItemPropertyChanged -= OnItemPropertyChanged;
            }

            if (args.NewValue != null)
            {
                ObservableNotifiableCollection<ProgramViewItem> coll = args.NewValue as ObservableNotifiableCollection<ProgramViewItem>;
                coll.CollectionChanged += OnCollectionChanged;
                coll.ItemPropertyChanged += OnItemPropertyChanged;
            }

            InvalidateVisual();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            InvalidateVisual();
        }

        void OnItemPropertyChanged(object sender, ItemPropertyChangedEventArgs args)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Background, null, new Rect(RenderSize));
            this.VisualTextRenderingMode = TextRenderingMode.ClearType;
            this.VisualTextHintingMode = TextHintingMode.Fixed;
            if (ItemsSource == null)
            {
                return;
            }

            Typeface typeface = null;
            GlyphTypeface glyphTypeface = null;
            try
            {
                if (Settings.Instance.FontName.Length > 0)
                {
                    typeface = new Typeface(new FontFamily(Settings.Instance.FontName),
                                                 FontStyles.Normal,
                                                 FontWeights.Normal,
                                                 FontStretches.Normal);
                }
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    typeface = null;
                }

                if (typeface == null)
                {
                    typeface = new Typeface(new FontFamily("MS UI Gothic"),
                                                 FontStyles.Normal,
                                                 FontWeights.Normal,
                                                 FontStretches.Normal);
                    if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                    {
                        MessageBox.Show("フォント指定が不正です");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            
            double size = Settings.Instance.FontSize;
            foreach (ProgramViewItem info in ItemsSource)
            {
                dc.DrawRectangle(Brushes.LightGray, null, new Rect(info.LeftPos, info.TopPos, info.Width, info.Height));
                if (info.Height > 2)
                {
                    dc.DrawRectangle(info.ContentColor, null, new Rect(info.LeftPos + 1, info.TopPos + 1, info.Width - 2, info.Height - 2));
                    
                    if (info.Height > 4)
                    {
                        int maxLine = ((int)info.Height - 4) / ((int)size + 2);
                        if (info.ProgramInfo.Length > 0 && maxLine > 0)
                        {
                            string text = info.ProgramInfo.Replace("\r", "");
                            string[] lineText = text.Split('\n');

                            List<ushort> glyphIndexes = new List<ushort>();
                            List<double> advanceWidths = new List<double>();

                            double totalWidth = 0;
                            int totalLine = 1;

                            foreach (string line in lineText)
                            {
                                bool breakEnd = false;
                                for (int n = 0; n < line.Length; n++)
                                {
                                    ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[line[n]];
                                    double width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * size);
                                    if (width > info.Width - 4)
                                    {
                                        breakEnd = true;
                                        break;
                                    }
                                    if (totalWidth + width > info.Width - 4)
                                    {
                                        if (totalLine + 1 > maxLine)
                                        {
                                            //次の行はかけない
                                            glyphIndex = glyphTypeface.CharacterToGlyphMap['…'];
                                            width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * size);
                                            if (totalWidth + width > info.Width - 4 && glyphIndexes.Count > 0)
                                            {
                                                glyphIndexes[glyphIndexes.Count - 1] = glyphIndex;
                                                advanceWidths[advanceWidths.Count - 1] = width;
                                            }
                                            else
                                            {
                                                glyphIndexes.Add(glyphIndex);
                                                advanceWidths.Add(width);
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            //次の行いける
                                            Point origin = new Point(info.LeftPos + 2, info.TopPos + totalLine * (size + 2));
                                            GlyphRun glyphRun = new GlyphRun(glyphTypeface, 0, false, size,
                                                glyphIndexes, origin, advanceWidths, null, null, null, null,
                                                null, null);

                                            dc.DrawGlyphRun(Brushes.Black, glyphRun);
                                            glyphIndexes = new List<ushort>();
                                            advanceWidths = new List<double>();
                                            totalWidth = 0;
                                            totalLine++;
                                        }
                                    }
                                    glyphIndexes.Add(glyphIndex);
                                    advanceWidths.Add(width);

                                    totalWidth += width;
                                }
                                if (breakEnd == true)
                                {
                                    break;
                                }

                                if (totalLine == maxLine)
                                {
                                    //終了
                                    if (glyphIndexes.Count > 0)
                                    {
                                        Point origin = new Point(info.LeftPos + 2, info.TopPos + totalLine * (size + 2));
                                        GlyphRun glyphRun = new GlyphRun(glyphTypeface, 0, false, size,
                                            glyphIndexes, origin, advanceWidths, null, null, null, null,
                                            null, null);

                                        dc.DrawGlyphRun(Brushes.Black, glyphRun);
                                        glyphIndexes = new List<ushort>();
                                        advanceWidths = new List<double>();
                                    }
                                    break;
                                }
                                else if (totalLine + 1 > maxLine)
                                {
                                    //次の行はいけない
                                    ushort glyphIndex = glyphTypeface.CharacterToGlyphMap['…'];
                                    double width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * size);
                                    if (totalWidth + width > info.Width - 4 && glyphIndexes.Count > 0)
                                    {
                                        glyphIndexes[glyphIndexes.Count - 1] = glyphIndex;
                                        advanceWidths[advanceWidths.Count - 1] = width;
                                    }
                                    else
                                    {
                                        glyphIndexes.Add(glyphIndex);
                                        advanceWidths.Add(width);
                                    }
                                    Point origin = new Point(info.LeftPos + 2, info.TopPos + totalLine * (size + 2));
                                    GlyphRun glyphRun = new GlyphRun(glyphTypeface, 0, false, size,
                                        glyphIndexes, origin, advanceWidths, null, null, null, null,
                                        null, null);

                                    dc.DrawGlyphRun(Brushes.Black, glyphRun);
                                    break;
                                }
                                else
                                {
                                    //次の行いける
                                    if (glyphIndexes.Count > 0)
                                    {
                                        Point origin = new Point(info.LeftPos + 2, info.TopPos + totalLine * (size + 2));
                                        GlyphRun glyphRun = new GlyphRun(glyphTypeface, 0, false, size,
                                            glyphIndexes, origin, advanceWidths, null, null, null, null,
                                            null, null);

                                        dc.DrawGlyphRun(Brushes.Black, glyphRun);
                                        glyphIndexes = new List<ushort>();
                                        advanceWidths = new List<double>();
                                    }
                                    totalWidth = 0;
                                    totalLine++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        object item;

        public ItemPropertyChangedEventArgs(object item,
                                            string propertyName)
            : base(propertyName)
        {
            this.item = item;
        }

        public object Item
        {
            get { return item; }
        }
    }

    public delegate void ItemPropertyChangedEventHandler(object sender,
                                        ItemPropertyChangedEventArgs args);
    
    public class ObservableNotifiableCollection<T> :
                ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ItemPropertyChangedEventHandler ItemPropertyChanged;
        public EventHandler CollectionCleared;

        protected override void OnCollectionChanged(
                                    NotifyCollectionChangedEventArgs args)
        {
            base.OnCollectionChanged(args);

            if (args.NewItems != null)
                foreach (INotifyPropertyChanged item in args.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;

            if (args.OldItems != null)
                foreach (INotifyPropertyChanged item in args.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
        }

        void OnItemPropertyChanged(object sender,
                                   PropertyChangedEventArgs args)
        {
            if (ItemPropertyChanged != null)
                ItemPropertyChanged(this,
                    new ItemPropertyChangedEventArgs(sender,
                                                     args.PropertyName));
        }

        protected override void ClearItems()
        {
            foreach (INotifyPropertyChanged item in Items)
                item.PropertyChanged -= OnItemPropertyChanged;

            if (CollectionCleared != null)
                CollectionCleared(this, EventArgs.Empty);

            base.ClearItems();
        }
    }
}
