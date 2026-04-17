using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {
        public async void EnumButton_Click(object sender, RoutedEventArgs e)
        {
            var progressHandler = new Progress<UpdateUI>(update =>
            {
                UpdateUI.ISN1 = update.ISN1;
                UpdateUI.ISN2 = update.ISN2;
                UpdateUI.ISN3 = update.ISN3;
                UpdateUI.ISN4 = update.ISN4;
                UpdateUI.IsScanning = update.IsScanning; 
                UpdateUI.EnumMessage = update.EnumMessage;
            });

            // Validating user input before starting the ping test, returning early if any of the values are invalid
            if (!System.Net.IPAddress.TryParse(UserSettings.IPAddress, out _) || !IsValidIPv4(UserSettings.IPAddress))
            {
                UpdateUI.ResetISNs();
                UpdateUI.EnumMessage = "Invalid IP address.";
                return;
            }

            if (!UserSettings.IsUdpMode)
            {
                if (!string.IsNullOrEmpty(UserSettings.Port))
                {
                    if (!int.TryParse(UserSettings.Port, out int p) || p < 1 || p > 65535)
                    {
                        UpdateUI.EnumMessage = "Invalid port.";
                        return;
                    }

                }
            }

            try
            {
                _cts = new CancellationTokenSource();
                CollectorEnumerator collectorEnumerator = new CollectorEnumerator();

                UpdateUI.ResetISNs();
                UpdateUI.IsScanning = true;
                UpdateUI.EnumMessage = "Scanning for collectors...";
                await Task.Run(() => collectorEnumerator.Enumerate(UserSettings.IPAddress, progressHandler, _cts.Token));
                UpdateUI.IsScanning = false;
            }
            catch
            {
                UpdateUI.ResetISNs();
                UpdateUI.EnumMessage = "No accessible collectors.";
                UpdateUI.IsScanning = false;
            }
        }
    }
}
