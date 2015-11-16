using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CommandBar
{
    public class SelectionAdapter
    {
        public Selector SelectorControl { get; set; }
        public SelectionAdapter(Selector selector)
        {
            this.SelectorControl = selector;
            this.SelectorControl.PreviewMouseUp += OnSelectorPreviewMouseUp;
        }

        public delegate void CancelEventHandler();
        public delegate void CommitEventHandler();
        public delegate void SelectionChangedEventHandler();

        public event CancelEventHandler Cancel;
        public event CommitEventHandler Commit;
        public event SelectionChangedEventHandler SelectionChanged;

        public void HandleKeyDown(KeyEventArgs key)
        {
            switch (key.Key)
            {
                case Key.Down:
                    this.IncrementSelection();
                    break;
                case Key.Up:
                    this.DecrementSelection();
                    break;
                case Key.Enter:
                case Key.Tab:
                    if (this.Commit != null)
                    {
                        this.Commit();
                    }

                    break;
                case Key.Escape:
                    if (this.Cancel != null)
                    {
                        this.Cancel();
                    }

                    break;
            }
        }

        private void DecrementSelection()
        {
            if (this.SelectorControl.SelectedIndex == -1)
            {
                this.SelectorControl.SelectedIndex = this.SelectorControl.Items.Count - 1;
            }
            else
            {
                this.SelectorControl.SelectedIndex--;
            }

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged();
            }
        }

        private void IncrementSelection()
        {
            if (this.SelectorControl.SelectedIndex == SelectorControl.Items.Count - 1)
            {
                this.SelectorControl.SelectedIndex = -1;
            }
            else
            {
                this.SelectorControl.SelectedIndex++;
            }

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged();
            }
        }

        private void OnSelectorPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Commit != null)
            {
                this.Commit();
            }
        }
    }
}
