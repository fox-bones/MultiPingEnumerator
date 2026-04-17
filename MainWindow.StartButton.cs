using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            int count = UserSettings.PacketCount;
            if (count == 21) count = 0; // Setting packet count to 0 for infinite pings if the user has the "continuous" value of 21 selected

            // Sending value updates on function execution to the UI
            var progressHandler = new Progress<UpdateUI>(update =>
            {
                UpdateUI.Success = update.Success;
                UpdateUI.Failure = update.Failure;
                UpdateUI.Health = update.Health;
                UpdateUI.AverageLatency = update.AverageLatency;
                UpdateUI.PingMessage = update.PingMessage;
            });

            // Validating user input before starting the ping test, returning early if any of the values are invalid
            if (!System.Net.IPAddress.TryParse(UserSettings.IPAddress, out _) || !IsValidIPv4(UserSettings.IPAddress))
            {
                UpdateUI.PingMessage = "Invalid IP address.";
                return;
            }

            if (!UserSettings.IsUdpMode)
            {
                if (!string.IsNullOrEmpty(UserSettings.Port))
                {
                    if (!int.TryParse(UserSettings.Port, out int p) || p < 1 || p > 65535)
                    {
                        UpdateUI.PingMessage = "Invalid port.";
                        return;
                    }

                }
            }

            // Resetting UI values from previous test
            UpdateUI.Reset();

            // Replace Start button with End button and lock inputs while the test is running
            ToggleControls(true);

            try
            {
                if (UserSettings.IsUdpMode) // Running a UDP ping if the user has it selected
                {
                    UDPPing udp = new UDPPing();

                    await Task.Run(() => udp.StartPing(UserSettings.IPAddress, count, progressHandler, _cts.Token));
                }
                else
                {
                    // Running an ICMP ping if no port is provided, TCP ping elsewise
                    if (UserSettings.Port == "")
                    {
                        ICMPPing icmpPing = new ICMPPing();

                        await Task.Run(() => icmpPing.StartPing(UserSettings.IPAddress, count, progressHandler, _cts.Token));
                    }
                    else
                    {
                        TCPPing tcp = new TCPPing();

                        await Task.Run(() => tcp.StartPing(UserSettings.IPAddress, int.Parse(UserSettings.Port), count, progressHandler, _cts.Token));
                    }
                }

            }
            finally
            {
                // Bring back Start button and unlock inputs 
                ToggleControls(false);
            }
        }
    }
}
