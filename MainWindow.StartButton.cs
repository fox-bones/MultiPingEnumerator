using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
                UpdateUI.ISN1 = update.ISN1;
                UpdateUI.ISN2 = update.ISN2;
                UpdateUI.ISN3 = update.ISN3;
                UpdateUI.ISN4 = update.ISN4;
                UpdateUI.IsScanning = update.IsScanning;
                UpdateUI.EnumMessage = update.EnumMessage;
            });

            // Validating user input before starting the ping test, returning early if any of the values are invalid
            if ((!System.Net.IPAddress.TryParse(UserSettings.IPAddress, out _) || !IsValidIPv4(UserSettings.IPAddress)) && !UserSettings.IsMultipingMode)
            {
                IPAddress.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }

            var settings = (this.DataContext as MainWindow)?.UserSettings;

            if (UserSettings.IsPortEnabled)
            {
                if (string.IsNullOrWhiteSpace(UserSettings.Port) ||
                    !int.TryParse(UserSettings.Port, out int p) ||
                    p < 1 || p > 65535)
                {
                    TcpPort.BorderBrush = System.Windows.Media.Brushes.Red;
                    return;
                }
            }

            // Resetting UI values from previous test
            UpdateUI.Reset();

            // Replace Start button with End button and lock inputs while the test is running
            ToggleControls(true);

            try
            {
                if (UserSettings.IsMultipingMode)
                {
                    var ipList = settings?.IPHistory;

                    if (ipList == null || !ipList.Any())
                    {
                        return;
                    }

                    UDPPing.ResetGlobalCounters();

                    MultiUDPPing multiUDP = new MultiUDPPing();

                    StatusTextUdp.Text = "Pinging concentrator list...";
                    StatusDotUdp.Fill = System.Windows.Media.Brushes.Yellow;

                    var report = await Task.Run(() => multiUDP.StartPing(ipList, progressHandler, _cts.Token));

                    int successCount = report.Count(r => r.IsOnline);
                    int totalCount = report.Count;

                    if (successCount/totalCount > 0.7)
                    {
                        StatusDotUdp.Fill = System.Windows.Media.Brushes.Green;
                    }
                    else if (successCount / totalCount > 0.45)
                    {
                        StatusDotUdp.Fill = System.Windows.Media.Brushes.Yellow;
                    }
                    else
                    {
                        StatusDotUdp.Fill = System.Windows.Media.Brushes.Red;
                    }

                    StatusTextUdp.Text = $"Responses: {successCount}/{totalCount}";

                    FileService export = new FileService();

                    if (UserSettings.IsExportEnabled && !_cts.IsCancellationRequested)
                    {
                        export.ExportResultsToTxt(report);
                    }
                }
                else if (!UserSettings.IsEnumeratorMode)
                {
                    // Running an ICMP ping if no port is provided, TCP ping elsewise
                    if (!UserSettings.IsPortEnabled)
                    {
                        ICMPPing icmpPing = new ICMPPing();

                        StatusTextIcmp.Text = "Executing ICMP Ping...";
                        StatusDotIcmp.Fill = System.Windows.Media.Brushes.Yellow;

                        await Task.Run(() => icmpPing.StartPing(UserSettings.IPAddress, count, progressHandler, _cts.Token));

                        StatusTextIcmp.Text = $"Received: {UpdateUI.Success}/{UpdateUI.Success + UpdateUI.Failure}";

                        if (UpdateUI.Success / (UpdateUI.Success + UpdateUI.Failure) < 0.7)
                        {
                            StatusDotIcmp.Fill = System.Windows.Media.Brushes.Red;
                        }
                        else if (UpdateUI.Success / (UpdateUI.Success + UpdateUI.Failure) < 0.45)
                        {
                            StatusDotIcmp.Fill = System.Windows.Media.Brushes.Yellow;
                        }
                        else
                        {
                            StatusDotIcmp.Fill = System.Windows.Media.Brushes.Green;
                        }
                    }
                    else
                    {
                        TCPPing tcp = new TCPPing();

                        StatusTextTcp.Text = "Executing TCP Ping...";
                        StatusDotTcp.Fill = System.Windows.Media.Brushes.Yellow;

                        await Task.Run(() => tcp.StartPing(UserSettings.IPAddress, int.Parse(UserSettings.Port), count, progressHandler, _cts.Token));

                        StatusTextTcp.Text = $"Received: {UpdateUI.Success}/{UpdateUI.Success + UpdateUI.Failure}";

                        if (UpdateUI.Success / (UpdateUI.Success + UpdateUI.Failure) < 0.7)
                        {
                            StatusDotTcp.Fill = System.Windows.Media.Brushes.Red;
                        }
                        else if (UpdateUI.Success / (UpdateUI.Success + UpdateUI.Failure) < 0.45)
                        {
                            StatusDotTcp.Fill = System.Windows.Media.Brushes.Yellow;
                        }
                        else
                        {
                            StatusDotTcp.Fill = System.Windows.Media.Brushes.Green;
                        }
                    }
                }
                else
                {
                    try
                    {
                        UpdateUI.ResetISNs();
                        UpdateUI.IsScanning = true;
                        UpdateUI.EnumMessage = "Scanning for collectors...";
                        StatusDotEnum.Fill = System.Windows.Media.Brushes.Yellow;
                        StatusTextEnum.Text = "Scanning...";

                        CollectorEnumerator enumerate = new CollectorEnumerator();

                        await enumerate.Enumerate(UserSettings.IPAddress, progressHandler, _cts.Token);

                        StatusDotEnum.Fill = System.Windows.Media.Brushes.Green;
                    }
                    catch (TimeoutException)
                    {
                        UpdateUI.ResetISNs();
                        UpdateUI.EnumMessage = "No response from concentrator.";
                        StatusDotEnum.Fill = System.Windows.Media.Brushes.Red;
                    }
                    finally
                    {
                        string[] isns = { UpdateUI.ISN1, UpdateUI.ISN2, UpdateUI.ISN3, UpdateUI.ISN4 };
                        int isnCount = 0;
                        var grayColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6E6E73");
                        StatusDotEnum.Fill = new SolidColorBrush(grayColor);
                        foreach (var isn in isns)
                        {
                            if (isn != "--") isnCount++;
                        }
                        StatusTextEnum.Text = $"Responses: {isnCount}/4";
                        UpdateUI.IsScanning = false;
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
