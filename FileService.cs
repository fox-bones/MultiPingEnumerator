using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace MultiPingEnumerator
{
    // Formatting file for export and defining rcommended file name
    public class FileService
    {
        public void ExportResultsToTxt(List<ConcentratorReport> reports)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = $"ConcentratorReport_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine("           MULTIPING ENUMERATOR REPORT            ");
                sb.AppendLine($"           Generated: {DateTime.Now}           ");
                sb.AppendLine("==================================================\n");

                foreach (var report in reports)
                {
                    sb.AppendLine($"CONCENTRATOR IP: {report.IpAddress}");
                    sb.AppendLine($"LATENCY:         {report.Latency:F1} ms");
                    sb.AppendLine($"STATUS:          {(report.IsOnline ? "ONLINE" : "OFFLINE")}");

                    if (report.Collectors.Any())
                    {
                        sb.AppendLine("ATTACHED COLLECTORS:");
                        foreach (var collector in report.Collectors)
                        {
                            sb.AppendLine($"  - {collector}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("ATTACHED COLLECTORS: None detected.");
                    }
                    sb.AppendLine("--------------------------------------------------\n");
                }

                File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                MessageBox.Show("Report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
