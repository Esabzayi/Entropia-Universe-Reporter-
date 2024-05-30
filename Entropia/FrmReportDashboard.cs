using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Entropia
{
    public partial class FrmReportDashboard : DevExpress.XtraEditors.XtraForm
    {
        private string inputFilePath;
        private string outputFile;

        private readonly Regex shrapnelRegex = new Regex(@"\[System\] \[] You received Shrapnel", RegexOptions.Compiled);
        private readonly Regex Criticaldamage_inflicted = new Regex(@"\[System\] \[] Critical hit - Additional damage! You inflicted", RegexOptions.Compiled);
        private readonly Regex Simpledamage_inflicted = new Regex(@"\[System\] \[] You inflicted", RegexOptions.Compiled);
       private readonly Regex AmplifierRegex = new Regex(@"\[System\] \[] You received Output Amplifier", RegexOptions.Compiled);
        private readonly Regex pedRegex = new Regex(@"Value:\s+(\d+\.\d+)\s+PED", RegexOptions.Compiled);
        //private readonly Regex DamageRegex = new Regex(@"inflicted:\s+(\d+\.\d+)\s+points of damage", RegexOptions.Compiled);
        private readonly Regex DamageRegex = new Regex(@"(\d+\.\d+)\s+points", RegexOptions.Compiled);

        private readonly Regex lineRegex = new Regex(@"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[System\] \[\] You received Shrapnel x \(\d+\) Value: \d+\.\d+ PED", RegexOptions.Compiled);

        private long lastReadPosition = 0;
        private List<double> pedValues = new List<double>();
        private List<double> AmplifierValues = new List<double>();
        private List<double> CriticalDamageInflictedValues = new List<double>();
        private List<double> SimpleDamageInflictedValues = new List<double>();
        private List<DateTime> entryTimes = new List<DateTime>();

        private System.Threading.Timer debounceTimer;
        private readonly int debounceTime = 500; // milliseconds
        private readonly object fileLock = new object();
        private  FileSystemWatcher f;

        public FrmReportDashboard()
        {
            InitializeComponent();
        }

        private async void FrmReportDashboard_Load(object sender, EventArgs e)
        {
            
        }



        private async Task ReadFileAsync()
        {
            try
            {
                using (var stream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    reader.BaseStream.Seek(lastReadPosition, SeekOrigin.Begin);
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        ProcessLine(line);
                    }
                    lastReadPosition = reader.BaseStream.Position;
                }
                CalculateAndDisplayResults();
            }
            catch (IOException ex)
            {
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}");
            }
        }

        private async Task OnFileChanged(object source, FileSystemEventArgs e)
        {
            debounceTimer?.Dispose();
            debounceTimer = new System.Threading.Timer(async _ =>
            {
                if (lblPED.InvokeRequired)
                {
                    lblPED.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblLot.InvokeRequired)
                {
                    lblLot.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (txtAmp.InvokeRequired)
                {
                    txtAmp.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (lblDamageInflicted.InvokeRequired)
                {
                    lblDamageInflicted.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblCriticalDamageCount.InvokeRequired)
                {
                    lblCriticalDamageCount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (lblhit.InvokeRequired)
                {
                    lblhit.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (lblhitCount.InvokeRequired)
                {
                    lblhitCount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (lblHitCPercentage.InvokeRequired)
                {
                    lblHitCPercentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (lblHitPercentage.InvokeRequired)
                {
                    lblHitPercentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                await ReadFileAsync();
            }, null, debounceTime, Timeout.Infinite);
        }


        private void ProcessLine(string line)
        {
            if (shrapnelRegex.IsMatch(line))
            {
                WriteToFile(line);

                var match = pedRegex.Match(line);
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, out double pedValue))
                    {
                        pedValues.Add(pedValue);
                    }
                }

                var timeMatch = lineRegex.Match(line);
                if (timeMatch.Success)
                {
                    if (DateTime.TryParse(timeMatch.Groups[1].Value, out DateTime entryTime))
                    {
                        entryTimes.Add(entryTime);
                    }
                }
            }

            if (AmplifierRegex.IsMatch(line))
            {
                WriteToFile(line);

                var match = pedRegex.Match(line);
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, out double AmpValue))
                    {
                        AmplifierValues.Add(AmpValue);
                    }
                }
               
            }

            if (Criticaldamage_inflicted.IsMatch(line))
            {
                WriteToFile(line);

                var match = DamageRegex.Match(line);
                if (match.Success)
                {
                    //XtraMessageBox.Show("asndjaskdhaskjdas");
                    if (double.TryParse(match.Groups[1].Value, out double DamageValue))
                    {
                        CriticalDamageInflictedValues.Add(DamageValue);
                    }
                }

            }

            if (Simpledamage_inflicted.IsMatch(line))
            {
                WriteToFile(line);

                var match = DamageRegex.Match(line);
                if (match.Success)
                {
                    //XtraMessageBox.Show("asndjaskdhaskjdas");
                    if (double.TryParse(match.Groups[1].Value, out double DamageValue))
                    {
                        SimpleDamageInflictedValues.Add(DamageValue);
                    }
                }

            }
        }

        private void WriteToFile(string line)
        {
            try
            {
                lock (fileLock)
                {
                    using (var writer = new StreamWriter(outputFile, true))
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"An error occurred while writing to the output file: {ex.Message}");
            }
        }

        private void CalculateAndDisplayResults()
        {
            //Console.WriteLine( "jaskdasjldjas");
            double totalSum = pedValues.Sum();
            int totalEntries = CountEntries();
            double AmpValue = AmplifierValues.Sum();
            double DamageInflictedValue = CriticalDamageInflictedValues.Sum();
            double DamageInflictedCount = CriticalDamageInflictedValues.Count();
            double SimpleHitSum = SimpleDamageInflictedValues.Sum();
            double SimpleHitCount = SimpleDamageInflictedValues.Count();
            // Update the UI on the main thread
            if (lblPED.InvokeRequired)
            {
                lblPED.Invoke(new Action(() => lblPED.Text = $"{totalSum:F2}"));
            }
            else
            {
                lblPED.Text = $"{totalSum:F2}";
            }
            if (lblLot.InvokeRequired)
            {
                lblLot.Invoke(new Action(() => lblLot.Text = $"{totalEntries}"));
            }
            else
            {
                lblLot.Text = $"{totalEntries}";
            }

            if (txtAmp.InvokeRequired)
            {
                txtAmp.Invoke(new Action(() => txtAmp.Text = $"{AmpValue}"));
            }
            else
            {
                txtAmp.Text = $"{AmpValue}";
            }

            if (lblDamageInflicted.InvokeRequired)
            {
                lblDamageInflicted.Invoke(new Action(() => lblDamageInflicted.Text = $"{DamageInflictedValue:F2}"));
            }
            else
            {
                lblDamageInflicted.Text = $"{DamageInflictedValue:F2}";
            }

            if (lblCriticalDamageCount.InvokeRequired)
            {
                lblCriticalDamageCount.Invoke(new Action(() => lblCriticalDamageCount.Text = $"{DamageInflictedCount:F2}"));
            }
            else
            {
                lblCriticalDamageCount.Text = $"{DamageInflictedCount:F2}";
            }

            if (lblhit.InvokeRequired)
            {
                lblhit.Invoke(new Action(() => lblhit.Text = $"{SimpleHitSum + DamageInflictedValue:F2}"));
            }
            else
            {
                lblhit.Text = $"{SimpleHitSum + DamageInflictedValue:F2}";
            }

            if (lblhitCount.InvokeRequired)
            {
                lblhitCount.Invoke(new Action(() => lblhitCount.Text = $"{SimpleHitCount+ DamageInflictedCount:F2}"));
            }
            else
            {
                lblhitCount.Text = $"{SimpleHitCount+ DamageInflictedCount:F2}";
            }

            // 0  = 8+2=10
            double totalDamage = SimpleHitCount + DamageInflictedCount;
            //8/10
            double hitPercentage = (SimpleHitCount / totalDamage) * 100;
            double criticalHitPercentage = (DamageInflictedCount / totalDamage) * 100;


            if (lblHitCPercentage.InvokeRequired)
            {
                lblHitCPercentage.Invoke(new Action(() => lblHitCPercentage.Text = $"{criticalHitPercentage:F2}"));
            }
            else
            {
                lblHitCPercentage.Text = $"{criticalHitPercentage:F2}";
            }
            if (lblHitPercentage.InvokeRequired)
            {
                lblHitPercentage.Invoke(new Action(() => lblHitPercentage.Text = $"{hitPercentage:F2}"));
            }
            else
            {
                lblHitPercentage.Text = $"{hitPercentage:F2}";
            }
        }

        private int CountEntries()
        {
            int count = 0;
            if (entryTimes.Count > 0)
            {
                entryTimes.Sort();
                DateTime prevTime = entryTimes[0];
                for (int i = 1; i < entryTimes.Count; i++)
                {
                    if ((entryTimes[i] - prevTime).TotalSeconds > 5)
                    {
                        count++;
                    }
                    prevTime = entryTimes[i];
                }
                count++; // Count the last entry
            }
            return count;
        }

        //private async void btnStart_Click(object sender, EventArgs e)
        //{
        //    inputFilePath = txtInputFilePath.Text;
        //    outputFile = txtOutputFilePath.Text;

        //    // Clear the output file if it exists
        //    if (File.Exists(outputFile))
        //    {
        //        lock (fileLock)
        //        {
        //            File.WriteAllText(outputFile, string.Empty);
        //        }
        //    }

        //    await ReadFileAsync();


        //    //lblmwssage.Text = "admasldka";
        //    using (var fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(inputFilePath)))
        //    {
        //        fileWatcher.Filter = Path.GetFileName(inputFilePath);
        //        fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
        //        fileWatcher.Changed += async (source, x) => await OnFileChanged(source, x);
        //        fileWatcher.EnableRaisingEvents = true;

        //        //MessageBox.Show("Monitoring file for changes. Press 'Stop' to quit.");
        //    }


        //}
        

        private async void btnStart_Click(object sender, EventArgs e)
        {
            inputFilePath = txtInputFilePath.Text;
            outputFile = txtOutputFilePath.Text;

            // Clear the output file if it exists
            if (File.Exists(outputFile))
            {
                lock (fileLock)
                {
                    File.WriteAllText(outputFile, string.Empty);
                }
            }

            await ReadFileAsync();

            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(inputFilePath));
            fileWatcher.Filter = Path.GetFileName(inputFilePath);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            fileWatcher.Changed += async (source, x) =>
            {
                await OnFileChanged(source, x);
            };
            fileWatcher.EnableRaisingEvents = true;

           
        }


        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelControl2_Click(object sender, EventArgs e)
        {

        }
    }
}