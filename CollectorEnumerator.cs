using MultiPingEnumerator;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CollectorEnumerator
{
    public async Task<List<string>> GetCollectorsAsync(string ip, CancellationToken ct)
    {
        List<string> foundCollectors = new List<string>();
        byte[] data = Encoding.ASCII.GetBytes("V5");

        try
        {
            using (var client = new UdpClient())
            {
                // Send the "V5" request to the concentrator
                await client.SendAsync(data, data.Length, ip, 16384);

                var receiveTask = client.ReceiveAsync();
                var timeoutTask = Task.Delay(2000, ct);

                var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                if (completedTask == receiveTask)
                {
                    var result = await receiveTask;
                    string output = Encoding.ASCII.GetString(result.Buffer);

                    // Standardize the response string
                    output = output.Replace("V005", "").Replace("0000>", "").Trim();
                    string[] array = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (array.Length > 0)
                    {
                        if (int.TryParse(array[0].TrimStart('0'), out int count))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                if (i + 1 < array.Length)
                                {
                                    foundCollectors.Add(array[i + 1]);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // If the UDP port is closed or times out, we return the empty list
        }

        return foundCollectors;
    }

    public async Task Enumerate(string ip, IProgress<UpdateUI> packetStats, CancellationToken ct)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes("V5");

        using (var client = new UdpClient())
        {
            await client.SendAsync(data, data.Length, ip, 16384);
            var receiveTask = client.ReceiveAsync();
            var timeoutTask = Task.Delay(2000, ct); // 2-second timeout

            var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("The concentrator did not respond in time.");
            }

            var result = await receiveTask;

            // Trimming the response to extract ISN values then storing to an array
            string output = System.Text.Encoding.ASCII.GetString(result.Buffer);
            output = output.Replace("V005", "").Replace("0000>", "").Trim();
            string[] array = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (array.Length > 0)
            {
                string countString = array[0].TrimStart('0');
                int count = int.TryParse(countString, out int val) ? val : 0;

                packetStats.Report(new UpdateUI
                {
                    ISN1 = count > 0 && array.Length > 1 ? array[1] : "--",
                    ISN2 = count > 1 && array.Length > 2 ? array[2] : "--",
                    ISN3 = count > 2 && array.Length > 3 ? array[3] : "--",
                    ISN4 = count > 3 && array.Length > 4 ? array[4] : "--",
                    EnumMessage = count == 1
                        ? "Found 1 collector."
                        : $"Found {count} collector(s)."
                });
            }
        }
    }
}
