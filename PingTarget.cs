using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiPingEnumerator
{
    public class PingTarget : INotifyPropertyChanged
    {
        private string _ip;
        private double _latency;
        private bool? _isOnline = null; // null: waiting, true: pass, false: fail

        public string IP { get => _ip; set { _ip = value; OnPropertyChanged(); } }

        public double Latency { get => _latency; set { _latency = value; OnPropertyChanged(); } }

        public bool? IsOnline
        {
            get => _isOnline;
            set { _isOnline = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
