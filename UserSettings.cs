using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MultiPingEnumerator;

public class UserSettings : INotifyPropertyChanged
{

    private string _ip = "";
    private string _port = "";
    private int _packetCount = 1;
    private bool _isUdpMode = false;
    private bool _isMultipingMode = false;
    private bool _isEnumeratorMode = false;
    private bool _isPortEnabled = false;
    private bool _isExportEnabled;
    private ObservableCollection<PingTarget> _ipHistory = new ObservableCollection<PingTarget>();

    public string IPAddress
    {
        get => _ip;
        set { _ip = value; OnPropertyChanged(); }
    }

    public ObservableCollection<PingTarget> IPHistory
    {
        get => _ipHistory;
        set { _ipHistory = value; OnPropertyChanged(); }
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

    public bool IsExportEnabled
    {
        get => _isExportEnabled;
        set
        {
            _isExportEnabled = value;
            OnPropertyChanged(nameof(IsExportEnabled));
        }
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
            }
            else
            {
                _port = "";
                IsMultipingMode = false;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(Port));
        }
    }

    public bool IsPortEnabled
    {
        get => _isPortEnabled;
        set
        {
            _isPortEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsMultipingMode
    {
        get => _isMultipingMode;
        set
        {
            _isMultipingMode = value;

            if (_isMultipingMode)
            {
                _port = "16384";
            }
            else
            {
                _port = "";
            }

            OnPropertyChanged();
        }
    }

    public bool IsEnumeratorMode
    {
        get => _isEnumeratorMode;
        set
        {
            _isEnumeratorMode = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
