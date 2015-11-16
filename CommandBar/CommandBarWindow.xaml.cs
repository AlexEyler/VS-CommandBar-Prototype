using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommandBar
{
    /// <summary>
    /// Interaction logic for CommandBarWindow.xaml
    /// </summary>
    public partial class CommandBarWindow : System.Windows.Window, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ISuggestionsProvider Provider { get; set; }

        public CommandBarWindow(DTE2 dte)
        {
            this.Provider = new ProjectsProvider(dte);
            InitializeComponent();
            this.InputField.Focus();
            Keyboard.Focus(this.InputField);
            this.PreviewKeyDown += WindowOnPreviewKeyDown;
            this.DataContext = this.Provider;
        }

        private void WindowOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                this.Dispose();
            }

            e.Handled = false;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.InputField.Dispose();
                this.disposed = true;
                base.Close();
            }
        }

    }
}
