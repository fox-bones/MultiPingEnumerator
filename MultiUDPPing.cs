using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiPingEnumerator
{
    internal class MultiUDPPing
    {
        public async Task<List<ConcentratorReport>> StartPing(System.Collections.ObjectModel.ObservableCollection<PingTarget> ipList, IProgress<UpdateUI> packetStats, CancellationToken ct)
        {
            var udpPingTool = new UDPPing();
            var collectorEnum = new CollectorEnumerator();
            var reports = new List<ConcentratorReport>();
            var tasks = new List<Task<ConcentratorReport>>();

            foreach (var target in ipList)
            {

                tasks.Add(Task.Run(async () =>
                {
                    target.IsOnline = null; // Reset online status before pinging

                    var result = await udpPingTool.StartPing(target.IP, 1, packetStats, ct);
                    target.IsOnline = result.Success;
                    target.Latency = result.Latency;

                    var report = new ConcentratorReport
                    {
                        IpAddress = target.IP,
                        IsOnline = result.Success,
                        Latency = result.Latency
                    };

                    if (result.Success)
                    {
                        report.Collectors = await collectorEnum.GetCollectorsAsync(target.IP, ct);
                    }

                    return report;
                }, ct));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}
