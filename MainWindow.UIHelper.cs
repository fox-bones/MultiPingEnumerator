using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiPingEnumerator
{
    public sealed partial class MainWindow : Window
    {

        // IP Address filtering
        private void IPAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI.PingMessage = "";

            string filtered = new string(((TextBox)sender).Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
            if (((TextBox)sender).Text != filtered)
            {
                int pos = ((TextBox)sender).SelectionStart;
                ((TextBox)sender).Text = filtered;
                ((TextBox)sender).SelectionStart = Math.Max(0, pos - 1);
            }
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
            ProtocolToggle.IsEnabled = !isRunning;
        }

        // Allows for running a ping test by pressing the Enter key while focused on any of the input fields
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (StartButton.Visibility == Visibility.Visible)
                {
                    StartButton_Click(this, new RoutedEventArgs());
                }
            }
        }
    }
}