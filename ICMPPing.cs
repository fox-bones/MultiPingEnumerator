using MultiPingEnumerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class ICMPPing
{
    public async Task StartPing(string ip, int packetCount, IProgress<UpdateUI> packetStats, CancellationToken ct)
    {
        int successCount = 0;
        int failureCount = 0;
        List<double> responseTimes = new List<double>();

        int i = 0;

        // Loop until cancellation is requested or packet count is reached (0 means infinite)
        while (!ct.IsCancellationRequested && (packetCount == 0 || i < packetCount))
        {
            i++;

            try
            {
                using (Ping pingSender = new Ping())
                {
                    PingReply reply = await pingSender.SendPingAsync(ip, 2000);

                    if (reply.Status == IPStatus.Success)
                    {
                        successCount++;
                        responseTimes.Add(reply.RoundtripTime);
                    }
                    else
                    {
                        failureCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                failureCount++;
            }
            catch (Exception)
            {
                failureCount++;
            }

            double average = responseTimes.Count > 0 ? responseTimes.Average() : 0;

            // Reporting variable updates to UI on each loop
            packetStats.Report(new UpdateUI
            {
                Success = successCount,
                Failure = failureCount,
                AverageLatency = Math.Round(average, 2),
                Health = ((double)successCount / (i)) * 100
            });

            // 1 second delay on each attempt, breaks if the "ct" parameter is cancelled during the delay
            try
            {
                await Task.Delay(1000, ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
