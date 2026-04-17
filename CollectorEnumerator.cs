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

                for (int i = 0; i < count; i++)
                {
                    if (i + 1 < array.Length)
                    {
                        string isn = array[i + 1];
                    }
                    if (count == 1)
                    {
                        packetStats.Report(new UpdateUI
                        {
                            ISN1 = count > 0 ? $"ISN: {array[1]}" : "ISN: --",
                            ISN2 = count > 1 ? $"ISN: {array[2]}" : "ISN: --",
                            ISN3 = count > 2 ? $"ISN: {array[3]}" : "ISN: --",
                            ISN4 = count > 3 ? $"ISN: {array[4]}" : "ISN: --",
                            EnumMessage = $"Found {count} collector."
                        });
                    }
                    else
                    {
                        packetStats.Report(new UpdateUI
                        {
                            ISN1 = count > 0 ? $"ISN: {array[1]}" : "ISN: --",
                            ISN2 = count > 1 ? $"ISN: {array[2]}" : "ISN: --",
                            ISN3 = count > 2 ? $"ISN: {array[3]}" : "ISN: --",
                            ISN4 = count > 3 ? $"ISN: {array[4]}" : "ISN: --",
                            EnumMessage = $"Found {count} collectors."
                        });
                    }
                }
            }
        }
    }
}
