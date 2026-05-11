using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {

        // IP Address filtering
        private void IPAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI.PingMessage = "";

            IPAddress.ClearValue(BorderBrushProperty);

            string filtered = new string(((TextBox)sender).Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
            if (((TextBox)sender).Text != filtered)
            {
                int pos = ((TextBox)sender).SelectionStart;
                ((TextBox)sender).Text = filtered;
                ((TextBox)sender).SelectionStart = Math.Max(0, pos - 1);
            }
        }

        private void IPAddress_GotFocus(object sender, RoutedEventArgs e)
        {
            IPAddress.ClearValue(BorderBrushProperty);
            var grayColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6E6E73");
            switch (currentTab)
            {
                case "ICMP":
                    StatusTextIcmp.Text = "Ready";
                    StatusDotIcmp.Fill = new SolidColorBrush(grayColor);
                    break;
                case "TCP":
                    StatusTextTcp.Text = "Ready";
                    StatusDotTcp.Fill = new SolidColorBrush(grayColor);
                    break;
                case "UDP":
                    StatusTextUdp.Text = "Ready";
                    StatusDotUdp.Fill = new SolidColorBrush(grayColor);
                    break;
                case "ENUM":
                    StatusTextEnum.Text = "Ready";
                    StatusDotEnum.Fill = new SolidColorBrush(grayColor);
                    break;
            }
        }

        private void Port_GotFocus(object sender, RoutedEventArgs e)
        {
            TcpPort.ClearValue(BorderBrushProperty);
        }

        // Helper method to ensure a real 4-part IPv4 address
        private bool IsValidIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;

            string[] parts = ip.Split('.');

            // Must have exactly 4 parts
            if (parts.Length != 4) return false;

            // Each part must be a number between 0 and 255
            return parts.All(p => byte.TryParse(p, out _));
        }

        // Checking that port is only numeric input
        private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        // Ignoring pasting to bypass numeric port restriction 
        private void Port_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // UI Toggling
        private void ToggleControls(bool isRunning)
        {
            StartButton.Visibility = isRunning ? Visibility.Collapsed : Visibility.Visible;
            EndButton.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;

            // Lock the inputs while running
            IPAddress.IsEnabled = !isRunning;
        }

        // IP button functionality
        private void AddIP_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAddIP();
        }

        private void ExecuteAddIP()
        {
            var settings = (this.DataContext as MainWindow)?.UserSettings;
            if (settings == null) return;

            string input = IPAddress.Text.Trim();

            if (IsValidIPv4(input))
            {
                if (!settings.IPHistory.Any(p => p.IP == input))
                {
                    settings.IPHistory.Add(new PingTarget { IP = input });
                    IPAddress.Clear(); // Clear field for the next entry
                }
            }
            else
            {
                // Optional: Provide visual feedback for invalid IP
                IPAddress.BorderBrush = System.Windows.Media.Brushes.Red;
            }
        }

        // Allows for running a ping test by pressing the Enter key while focused on any of the input fields
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !UserSettings.IsMultipingMode)
            {
                e.Handled = true;

                if (StartButton.Visibility == Visibility.Visible)
                {
                    StartButton_Click(this, new RoutedEventArgs());
                }
            }
            if (e.Key == Key.Enter && UserSettings.IsMultipingMode)
            {
                ExecuteAddIP();
                e.Handled = true;
            }
        }

        private void IPHistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the ListBox
            var listBox = sender as ListBox;
            if (listBox == null) return;

            // Identify which item was actually clicked
            var selectedItem = listBox.SelectedItem as PingTarget;

            if (selectedItem != null)
            {
                var settings = (this.DataContext as MainWindow)?.UserSettings;

                if (settings != null && settings.IPHistory.Contains(selectedItem))
                {
                    settings.IPHistory.Remove(selectedItem);
                }
            }
        }

        string currentTab;
        private void SetActiveTab(string tab)
        {
            // Reset all tabs to inactive state
            IcmpTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            TcpTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            UdpTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            EnumTab.BorderBrush = System.Windows.Media.Brushes.Transparent;

            var muted = (SolidColorBrush)FindResource("TextMutedBrush");
            var accent = (SolidColorBrush)FindResource("AccentBrush");

            IcmpTabText.Foreground = muted;
            TcpTabText.Foreground = muted;
            UdpTabText.Foreground = muted;
            EnumTabText.Foreground = muted;

            // Hide all panels
            IcmpPanel.Visibility = Visibility.Collapsed;
            TcpPanel.Visibility = Visibility.Collapsed;
            UdpPanel.Visibility = Visibility.Collapsed;
            EnumPanel.Visibility = Visibility.Collapsed;

            // Activate the selected tab
            switch (tab)
            {
                case "ICMP":
                    IcmpTab.BorderBrush = accent;
                    IcmpTabText.Foreground = accent;
                    IcmpPanel.Visibility = Visibility.Visible;
                    AddButton.Visibility = Visibility.Collapsed;
                    ExportCheckBox.Visibility = Visibility.Collapsed;
                    StatusDotIcmp.Visibility = Visibility.Visible;
                    StatusTextIcmp.Visibility = Visibility.Visible;
                    StatusDotTcp.Visibility = Visibility.Collapsed;
                    StatusTextTcp.Visibility = Visibility.Collapsed;
                    StatusDotUdp.Visibility = Visibility.Collapsed;
                    StatusTextUdp.Visibility = Visibility.Collapsed;
                    StatusDotEnum.Visibility = Visibility.Collapsed;
                    StatusTextEnum.Visibility = Visibility.Collapsed;
                    break;
                case "TCP":
                    TcpTab.BorderBrush = accent;
                    TcpTabText.Foreground = accent;
                    TcpPanel.Visibility = Visibility.Visible;
                    AddButton.Visibility = Visibility.Collapsed;
                    ExportCheckBox.Visibility = Visibility.Collapsed;
                    StatusDotIcmp.Visibility = Visibility.Collapsed;
                    StatusTextIcmp.Visibility = Visibility.Collapsed;
                    StatusDotTcp.Visibility = Visibility.Visible;
                    StatusTextTcp.Visibility = Visibility.Visible;
                    StatusDotUdp.Visibility = Visibility.Collapsed;
                    StatusTextUdp.Visibility = Visibility.Collapsed;
                    StatusDotEnum.Visibility = Visibility.Collapsed;
                    StatusTextEnum.Visibility = Visibility.Collapsed;
                    break;
                case "UDP":
                    UdpTab.BorderBrush = accent;
                    UdpTabText.Foreground = accent;
                    UdpPanel.Visibility = Visibility.Visible;
                    AddButton.Visibility = Visibility.Visible;
                    ExportCheckBox.Visibility = Visibility.Visible;
                    StatusDotIcmp.Visibility = Visibility.Collapsed;
                    StatusTextIcmp.Visibility = Visibility.Collapsed;
                    StatusDotTcp.Visibility = Visibility.Collapsed;
                    StatusTextTcp.Visibility = Visibility.Collapsed;
                    StatusDotUdp.Visibility = Visibility.Visible;
                    StatusTextUdp.Visibility = Visibility.Visible;
                    StatusDotEnum.Visibility = Visibility.Collapsed;
                    StatusTextEnum.Visibility = Visibility.Collapsed;
                    break;
                case "ENUMERATOR":
                    EnumTab.BorderBrush = accent;
                    EnumTabText.Foreground = accent;
                    EnumPanel.Visibility = Visibility.Visible;
                    AddButton.Visibility = Visibility.Collapsed;
                    ExportCheckBox.Visibility = Visibility.Collapsed;
                    StatusDotIcmp.Visibility = Visibility.Collapsed;
                    StatusTextIcmp.Visibility = Visibility.Collapsed;
                    StatusDotTcp.Visibility = Visibility.Collapsed;
                    StatusTextTcp.Visibility = Visibility.Collapsed;
                    StatusDotUdp.Visibility = Visibility.Collapsed;
                    StatusTextUdp.Visibility = Visibility.Collapsed;
                    StatusDotEnum.Visibility = Visibility.Visible;
                    StatusTextEnum.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void IcmpTab_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetActiveTab("ICMP");
            UserSettings.IsPortEnabled = false;
            UserSettings.IsMultipingMode = false;
            UserSettings.IsEnumeratorMode = false;
            currentTab = "ICMP";
        }
        private void TcpTab_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetActiveTab("TCP");
            UserSettings.IsPortEnabled = true;
            UserSettings.IsMultipingMode = false;
            UserSettings.IsEnumeratorMode = false;
            currentTab = "TCP";
        }

        private void UdpTab_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetActiveTab("UDP");
            UserSettings.IsPortEnabled = false;
            UserSettings.IsMultipingMode = true;
            UserSettings.IsEnumeratorMode = false;
            currentTab = "UDP";
        }

        private void EnumTab_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetActiveTab("ENUMERATOR");
            UserSettings.IsPortEnabled = false;
            UserSettings.IsEnumeratorMode = true;
            UserSettings.IsMultipingMode = false;
            currentTab = "ENUM";
        }
    }
}