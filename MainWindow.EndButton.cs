using System.Windows;


namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {
        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            // Canceling the ping test by triggering cancellation on the CancellationTokenSource
            _cts?.Cancel();
        }
    }
}
