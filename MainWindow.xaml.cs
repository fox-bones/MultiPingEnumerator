using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;


namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {
        public UpdateUI UpdateUI { get; set; } = new UpdateUI();
        public UserSettings UserSettings { get; set; } = new UserSettings();
        private CancellationTokenSource _cts;
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            try
            {
                // This URI points specifically to the internal Assembly Resource
                Uri iconUri = new Uri("pack://application:,,,/Midmark-Caremark.ico", UriKind.RelativeOrAbsolute);
                this.Icon = BitmapFrame.Create(iconUri);
            }
            catch (Exception ex)
            {
                // If the icon fails, the app will still run
                System.Diagnostics.Debug.WriteLine($"Icon Load Failed: {ex.Message}");
            }
        }
    }
}
