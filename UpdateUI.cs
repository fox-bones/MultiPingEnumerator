using System.Windows;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiPingEnumerator
{
    public class UpdateUI : INotifyPropertyChanged
    {
        private int _success;
        private int _failure;
        private double _latency;
        private double _health;
        private string _isn1 = null;
        private string _isn2 = null;
        private string _isn3 = null;
        private string _isn4 = null;
        private bool _isScanning = false;
        private string _pingMessage;
        private string _enumMessage;
        public string HealthDisplay => $"{Math.Round(Health)}%";

        public void Reset()
        {
            Success = 0;
            Failure = 0;
            AverageLatency = 0;
            Health = 0;

            OnPropertyChanged(nameof(HealthDisplay));
            OnPropertyChanged(nameof(ArcEndPoint));
        }

        public void ResetISNs()
        {
            ISN1 = null;
            ISN2 = null;
            ISN3 = null;
            ISN4 = null;
        }

        public int Success
        {
            get => _success;
            set
            {
                _success = value;
                OnPropertyChanged(); // Notifies the UI
            }
        }

        public int Failure
        {
            get => _failure;
            set
            {
                _failure = value;
                OnPropertyChanged();
            }
        }

        public double AverageLatency
        {
            get
            {
                double roundedLatency = Math.Round(_latency, 1);
                return roundedLatency;
            }
            set
            {
                _latency = value;
                OnPropertyChanged();
            }
        }

        public string ISN1
        {
            get => _isn1;
            set
            {
                _isn1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIsnGridVisible));
            }
        }

        public string ISN2
        {
            get => _isn2;
            set
            {
                _isn2 = value;
                OnPropertyChanged();
            }
        }

        public string ISN3
        {
            get => _isn3;
            set
            {
                _isn3 = value;
                OnPropertyChanged();
            }
        }

        public string ISN4
        {
            get => _isn4;
            set
            {
                _isn4 = value;
                OnPropertyChanged();
            }
        }

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                _isScanning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScanningVisibility));
            }
        }

        public double Health
        {
            get => _health;
            set
            {
                _health = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ArcColor));
                OnPropertyChanged(nameof(ArcEndPoint));
                OnPropertyChanged(nameof(IsLargeArc));
                OnPropertyChanged(nameof(HealthDisplay));
                OnPropertyChanged(nameof(IsArcVisible));
                OnPropertyChanged(nameof(ArcInnerEndPoint));
                OnPropertyChanged(nameof(ArcCapX));
                OnPropertyChanged(nameof(ArcCapY));
            }
        }

        public string PingMessage
        {
            get => _pingMessage;
            set
            {
                _pingMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPingMessageVisible));
            }
        }

        public string EnumMessage
        {
            get => _enumMessage;
            set
            {
                _enumMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnumMessageVisible));
            }
        }

        public System.Windows.Point ArcEndPoint
        {
            get
            {
                double radius = 86;
                double centerX = 100;
                double centerY = 100;
                double displayHealth = Math.Max(0.1, Math.Min(Health, 99.9));
                double angleDeg = 180.0 - (displayHealth / 100.0) * 180.0;
                double radians = angleDeg * (Math.PI / 180.0);
                return new System.Windows.Point(
                    centerX + radius * Math.Cos(radians),
                    centerY - radius * Math.Sin(radians));
            }
        }

        // Inner arc (r=78 instead of r=86) — same angle, slightly inward
        public System.Windows.Point ArcInnerEndPoint
        {
            get
            {
                double radius = 78;
                double centerX = 100;
                double centerY = 100;
                double displayHealth = Math.Max(0.1, Math.Min(Health, 99.9));
                double angleDeg = 180.0 - (displayHealth / 100.0) * 180.0;
                double radians = angleDeg * (Math.PI / 180.0);
                return new System.Windows.Point(
                    centerX + radius * Math.Cos(radians),
                    centerY - radius * Math.Sin(radians));
            }
        }

        // Cap dot translation — offset from its default top-left of (0,0) to the arc endpoint
        public double ArcCapX
        {
            get
            {
                double radius = 86;
                double centerX = 100;
                double displayHealth = Math.Max(0.1, Math.Min(Health, 99.9));
                double angleDeg = 180.0 - (displayHealth / 100.0) * 180.0;
                double radians = angleDeg * (Math.PI / 180.0);
                return centerX + radius * Math.Cos(radians);
            }
        }

        public double ArcCapY
        {
            get
            {
                double radius = 86;
                double centerY = 100;
                double displayHealth = Math.Max(0.1, Math.Min(Health, 99.9));
                double angleDeg = 180.0 - (displayHealth / 100.0) * 180.0;
                double radians = angleDeg * (Math.PI / 180.0);
                return centerY - radius * Math.Sin(radians);
            }
        }

        // Dynamically change arc color based on health thresholds
        public SolidColorBrush ArcColor
        {
            get
            {
                Color selectedColor = Health > 95 ? Colors.DeepSkyBlue :
                                 Health > 65 ? Colors.Green :
                                 (Health > 35 ? Colors.Orange : Colors.Red);
                return new SolidColorBrush(selectedColor);
            }
        }

        // Changing elements' visibility to "collasped" if no data is present
        public bool IsPingMessageVisible => !string.IsNullOrEmpty(PingMessage);
        public bool IsEnumMessageVisible => !string.IsNullOrEmpty(EnumMessage);
        public Visibility IsIsnGridVisible => !string.IsNullOrEmpty(ISN1) ? Visibility.Visible : Visibility.Collapsed;

        // Showing a loading bar while "IsScanning" bool is set to true
        public Visibility ScanningVisibility => IsScanning ? Visibility.Visible : Visibility.Collapsed;

        // ArcSegment needs to know if the angle is > 180 degrees
        public bool IsLargeArc => false;

        // Hide the arc when health is 0 to avoid small path at 0% health
        public Visibility IsArcVisible => Health > 0 ? Visibility.Visible : Visibility.Collapsed;

        // Boilerplate code to handle the notification logic
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

