using MultiPingEnumerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class UDPPing
{
    public async Task StartPing(string ip, int packetCount, IProgress<UpdateUI> packetStats, CancellationToken ct)
    {
        int successCount = 0;
        int failureCount = 0;
        double averageLatency = 0;
        List<double> responseTimes = new List<double>();
        byte[] data = System.Text.Encoding.ASCII.GetBytes("V0");

        // Stopwatch to measure response times
        Stopwatch sw = Stopwatch.StartNew();

        int i = 0;

        // Loop until cancellation is requested or packet count is reached (0 means infinite)
        while (!ct.IsCancellationRequested && (packetCount == 0 || i < packetCount))
        {
            i++;
            sw.Restart();

            try
            {
                using (UdpClient client = new UdpClient())
                {
                    await client.SendAsync(data, data.Length, ip, 16384);

                    var receiveTask = client.ReceiveAsync();
                    var delayTask = Task.Delay(2000, ct);

                    var completedTask = await Task.WhenAny(receiveTask, delayTask);

                    if (completedTask == receiveTask)
                    {
                        var result = await receiveTask; // Await to get result/exceptions
                        sw.Stop();
                        responseTimes.Add(sw.Elapsed.TotalMilliseconds);
                        successCount++;
                    }
                    else
                    {
                        sw.Stop();
                        failureCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                break;
            }
            catch (SocketException)
            {
                sw.Stop();
                failureCount++;
            }
            catch (Exception)
            {
                sw.Stop();
                failureCount++;
            }

            averageLatency = responseTimes.Count > 0 ? responseTimes.Average() : 0;

            // Reporting variable updates to UI on each loop
            packetStats.Report(new UpdateUI
            {
                Success = successCount,
                Failure = failureCount,
                AverageLatency = averageLatency,
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
