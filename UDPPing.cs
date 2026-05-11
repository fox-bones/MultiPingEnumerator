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
    private static int _globalSuccess = 0;
    private static int _globalFailure = 0;
    private static int _globalAttempts = 0;
    private static double _globalAverageLatency = 0;
    private static List<double> _globalResponseTimes = new List<double>();
    public async Task<(bool Success, double Latency)> StartPing(string ip, int packetCount, IProgress<UpdateUI> packetStats, CancellationToken ct)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes("V0");

        // Stopwatch to measure response times
        Stopwatch sw = Stopwatch.StartNew();

        int i = 0;
        bool anySuccess = false;
        double lastMeasuredLatency = 0;

        // Loop until cancellation is requested or packet count is reached (0 means infinite)
        while (!ct.IsCancellationRequested && (packetCount == 0 || i < packetCount))
        {
            i++;
            Interlocked.Increment(ref _globalAttempts);
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
                        lastMeasuredLatency = sw.Elapsed.TotalMilliseconds; 
                        _globalResponseTimes.Add(sw.Elapsed.TotalMilliseconds);
                        Interlocked.Increment(ref _globalSuccess);
                        anySuccess = true;
                    }
                    else
                    {
                        sw.Stop();
                        Interlocked.Increment(ref _globalFailure);
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
                Interlocked.Increment(ref _globalFailure);
                break;
            }
            catch (Exception)
            {
                sw.Stop();
                Interlocked.Increment(ref _globalFailure);
                break;
            }

            _globalAverageLatency = _globalResponseTimes.Count > 0 ? _globalResponseTimes.Average() : 0;

            // Reporting variable updates to UI on each loop
            packetStats.Report(new UpdateUI
            {
                Success = _globalSuccess,
                Failure = _globalFailure,
                AverageLatency = _globalAverageLatency,
                Health = ((double)_globalSuccess / (_globalAttempts)) * 100
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
        return (anySuccess, lastMeasuredLatency);
    }
    public static void ResetGlobalCounters()
    {
        Interlocked.Exchange(ref _globalSuccess, 0);
        Interlocked.Exchange(ref _globalFailure, 0);
        Interlocked.Exchange(ref _globalAttempts, 0);
        Interlocked.Exchange(ref _globalAverageLatency, 0);
        Interlocked.Exchange(ref _globalResponseTimes, new List<double>());
    }
}
