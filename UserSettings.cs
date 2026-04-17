using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

public class UserSettings : INotifyPropertyChanged
{

    private string _ip = "";
    private string _port = "";
    private int _packetCount = 1;
    private bool _isUdpMode = false;

    public string IPAddress
    {
        get => _ip;
        set { _ip = value; OnPropertyChanged(); }
    }

    public string Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    public int PacketCount
    {
        get => _packetCount;
        set { _packetCount = value; OnPropertyChanged(); }
    }

    public bool IsUdpMode
    {
        get => _isUdpMode;
        set
        {
            _isUdpMode = value;

            if (_isUdpMode)
            {
                _port = "16384";
                OnPropertyChanged(nameof(Port));
            }
            else
            {
                _port = "";
                OnPropertyChanged(nameof(Port));
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPortEnabled)); // Grey out port field when UDP is selected
            OnPropertyChanged(nameof(ExpanderVisibility)); // Show the UDP info expander when UDP is selected
        }
    }

    public bool IsPortEnabled => !IsUdpMode;
    public Visibility ExpanderVisibility => IsUdpMode ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
