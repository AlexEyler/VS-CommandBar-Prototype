using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CommandBar
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>
    [TemplatePart(Name = AutoCompleteTextBox.PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = AutoCompleteTextBox.PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = AutoCompleteTextBox.PartSelector, Type = typeof(Selector))]
    public partial class AutoCompleteTextBox : Control, IDisposable
    {
        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";
        public const string PartSelector = "PART_Selector";

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(100));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register("LoadingContent", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register("Provider", typeof(ISuggestionsProvider), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        private bool isUpdatingText;
        private bool selectionCancelled;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public AutoCompleteTextBox()
        {
            InitializeComponent();
        }

        public int Delay
        {
            get { return (int)this.GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        public object LoadingContent
        {
            get { return this.GetValue(LoadingContentProperty); }
            set { SetValue(LoadingContentProperty, value); }
        }

        public TextBox Editor { get; set; }

        public DispatcherTimer FetchTimer { get; set; }

        public string Filter { get; set; }

        public bool IsDropDownOpen
        {
            get { return (bool)this.GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public bool IsLoading { get; private set; }

        public Selector ItemsSelector { get; set; }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public SelectionAdapter SelectionAdapter { get; private set; }

        public Popup Popup { get; set; }

        public ISuggestionsProvider Provider
        {
            get { return (ISuggestionsProvider)this.GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }

        public string Watermark
        {
            get { return (string)this.GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox actb = d as AutoCompleteTextBox;
            if (actb != null && actb.Editor != null && !actb.isUpdatingText)
            {
                actb.isUpdatingText = true;
                actb.Editor.Text = e.NewValue.ToString();
                actb.isUpdatingText = false;
            }
        }

        private string GetDisplayText(object item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return item.ToString();
        }

        private void ScrollToSelectedItem()
        {
            ListBox listBox = this.ItemsSelector as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.Editor = Template.FindName(PartEditor, this) as TextBox;
            this.Popup = Template.FindName(PartPopup, this) as Popup;
            this.ItemsSelector = Template.FindName(PartSelector, this) as Selector;

            if (this.Editor != null)
            {
                this.Editor.TextChanged += OnEditorTextChanged;
                this.Editor.PreviewKeyDown += OnEditorPreviewKeyDown;
                this.Editor.LostFocus += OnEditorLostFocus;

                if (this.SelectedItem != null)
                {
                    this.Editor.Text = this.SelectedItem.ToString();
                }
            }

            if (this.Popup != null)
            {
                this.Popup.StaysOpen = false;
                this.Popup.Opened += OnPopupOpened;
                this.Popup.Closed += OnPopupClosed;
            }

            if (this.ItemsSelector != null)
            {
                this.SelectionAdapter = new SelectionAdapter(this.ItemsSelector);
                this.SelectionAdapter.Commit += OnSelectionAdapterCommit;
                this.SelectionAdapter.Cancel += OnSelectionAdapterCancel;
                this.SelectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
            }
        }

        private void OnSelectionAdapterCancel()
        {
            this.isUpdatingText = true;
            this.Editor.Text = this.SelectedItem == null ? this.Filter : GetDisplayText(this.SelectedItem);
            this.Editor.SelectionStart = this.Editor.Text.Length;
            this.Editor.SelectionLength = 0;
            this.isUpdatingText = false;
            this.IsDropDownOpen = false;
            this.selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (this.ItemsSelector.SelectedItem != null)
            {
                this.SelectedItem = this.ItemsSelector.SelectedItem;
                this.isUpdatingText = true;
                this.Editor.Text = GetDisplayText(this.ItemsSelector.SelectedItem);
                this.SetSelectedItem(this.ItemsSelector.SelectedItem);
                this.isUpdatingText = false;
                this.IsDropDownOpen = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            this.isUpdatingText = true;
            if (this.ItemsSelector.SelectedItem == null)
            {
                this.Editor.Text = this.Filter;
            }
            else
            {
                this.Editor.Text = GetDisplayText(this.ItemsSelector.SelectedItem);
            }

            this.Editor.SelectionStart = this.Editor.Text.Length;
            this.Editor.SelectionLength = 0;
            this.ScrollToSelectedItem();
            this.isUpdatingText = false;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (!this.selectionCancelled)
            {
                this.OnSelectionAdapterCommit();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            this.selectionCancelled = false;
            this.ItemsSelector.SelectedItem = this.SelectedItem;
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!this.IsKeyboardFocusWithin)
            {
                this.IsDropDownOpen = false;
            }
        }

        private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.SelectionAdapter != null)
            {
                if (this.IsDropDownOpen)
                {
                    this.SelectionAdapter.HandleKeyDown(e);
                }
                else
                {
                    this.IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;

                    if (e.Key == Key.End)
                    {
                        this.Editor.SelectionStart = this.Editor.Text.Length - 1;
                    }
                    else if (e.Key == Key.Home)
                    {
                        this.Editor.SelectionStart = 0;
                    }
                }
            }
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.isUpdatingText)
            {
                return;
            }

            if (this.FetchTimer == null)
            {
                this.FetchTimer = new DispatcherTimer();
                this.FetchTimer.Interval = TimeSpan.FromMilliseconds(this.Delay);
                this.FetchTimer.Tick += OnFetchTimerTick;
            }

            this.FetchTimer.IsEnabled = false;
            this.FetchTimer.Stop();
            this.SetSelectedItem(null);

            if (this.Editor.Text.Length > 0)
            {
                this.IsLoading = true;
                this.IsDropDownOpen = true;
                this.ItemsSelector.ItemsSource = null;
                this.FetchTimer.IsEnabled = true;
                this.FetchTimer.Start();
            }
            else
            {
                this.IsDropDownOpen = false;
            }
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            this.FetchTimer.IsEnabled = false;
            this.FetchTimer.Stop();

            if (this.Provider != null && this.ItemsSelector != null)
            {
                // get suggestions
                this.Filter = this.Editor.Text;
                this.IsLoading = false;
                this.ItemsSelector.ItemsSource = this.Provider.GetSuggestions(this.Filter);
                this.IsDropDownOpen = this.ItemsSelector.HasItems;
            }
        }

        private void SetSelectedItem(object item)
        {
            this.isUpdatingText = true;
            this.SelectedItem = item;
            this.isUpdatingText = false;
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                this.isDisposed = true;

                if (this.FetchTimer != null)
                {
                    this.FetchTimer.Tick -= OnFetchTimerTick;
                    this.FetchTimer = null;
                }

                if (this.SelectionAdapter != null)
                {
                    this.SelectionAdapter.Cancel -= OnSelectionAdapterCancel;
                    this.SelectionAdapter.Commit -= OnSelectionAdapterCommit;
                    this.SelectionAdapter.SelectionChanged -= OnSelectionAdapterSelectionChanged;
                    this.SelectionAdapter = null;
                }

                if (this.Popup != null)
                {
                    this.Popup.Closed -= OnPopupClosed;
                    this.Popup.Opened -= OnPopupOpened;
                    this.Popup = null;
                }

                if (this.Editor != null)
                {
                    this.Editor.TextChanged -= OnEditorTextChanged;
                    this.Editor.PreviewKeyDown -= OnEditorPreviewKeyDown;
                    this.Editor.LostFocus -= OnEditorLostFocus;
                    this.Editor = null;
                }
            }
        }
    }
}
