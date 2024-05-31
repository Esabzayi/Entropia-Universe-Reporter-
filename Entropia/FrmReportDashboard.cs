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
using System.Data;

namespace Entropia
{
    public partial class FrmReportDashboard : DevExpress.XtraEditors.XtraForm
    {
        DataTable CostOfAttack = new DataTable();
        private string inputFilePath;
        private string outputFile;

        private readonly Regex shrapnelRegex = new Regex(@"\[System\] \[] You received Shrapnel", RegexOptions.Compiled);
        private readonly Regex Criticaldamage_inflicted = new Regex(@"\[System\] \[] Critical hit - Additional damage! You inflicted", RegexOptions.Compiled);
        private readonly Regex Simpledamage_inflicted = new Regex(@"\[System\] \[] You inflicted", RegexOptions.Compiled);
        private readonly Regex TargetEvadedYourAttack_Regex = new Regex(@"\[System\] \[] The target Evaded your attack", RegexOptions.Compiled);
        private readonly Regex TargetDodgedYourAttack_Regex = new Regex(@"\[System\] \[] The target Dodged your attack", RegexOptions.Compiled);
        private readonly Regex AmplifierRegex = new Regex(@"\[System\] \[] You received Output Amplifier", RegexOptions.Compiled);
        private readonly Regex pedRegex = new Regex(@"Value:\s+(\d+\.\d+)\s+PED", RegexOptions.Compiled);
        private readonly Regex DamageRegex = new Regex(@"(\d+\.\d+)\s+points", RegexOptions.Compiled);
        private readonly Regex lineRegex = new Regex(@"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[System\] \[\] You received Shrapnel x \(\d+\) Value: \d+\.\d+ PED", RegexOptions.Compiled);

        #region Enhancer Regex
        //Your enhancer Weapon Damage Enhancer 1
        private readonly Regex Enhancer01_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 1", RegexOptions.Compiled);
        private readonly Regex Enhancer02_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 2", RegexOptions.Compiled);
        private readonly Regex Enhancer03_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 3", RegexOptions.Compiled);
        private readonly Regex Enhancer04_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 4", RegexOptions.Compiled);
        private readonly Regex Enhancer05_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 5", RegexOptions.Compiled);
        private readonly Regex Enhancer06_Regex = new Regex(@"\[System\] \[] Your enhancer Weapon Damage Enhancer 6", RegexOptions.Compiled);


        private List<string> Enhancer01_List = new List<string>();
        private List<string> Enhancer02_List = new List<string>();
        private List<string> Enhancer03_List = new List<string>();
        private List<string> Enhancer04_List = new List<string>();
        private List<string> Enhancer05_List = new List<string>();
        private List<string> Enhancer06_List = new List<string>();

        #endregion

        private long lastReadPosition = 0;
        private List<double> pedValues = new List<double>();

        private List<double> AmplifierValues = new List<double>();
        private List<double> CriticalDamageInflictedValues = new List<double>();
        private List<double> SimpleDamageInflictedValues = new List<double>();
        private List<string> TargetEvadedYourAttack_List = new List<string>();
        private List<string> TargetDodgedYourAttack_List = new List<string>();
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
            txtInputFilePath.Text = "C:\\Users\\Salman Naveed\\Downloads\\Entropia\\chat.log";
            txtOutputFilePath.Text = "C:\\Users\\Salman Naveed\\Downloads\\Entropia\\" + DateTime.Now.ToString();

            CostOfAttack.Columns.Add("Description");
            CostOfAttack.Columns.Add("Value");
            CostOfAttack.Rows.Add("Weapon + Amplifier + Metric + T4",60.5904);
            CostOfAttack.Rows.Add("Weapon + Amplifier + Metric + T0", 47.8560);
            CostOfAttack.Rows.Add("Weapon + Amplifier + T4", 60.5704);
            CostOfAttack.Rows.Add("Weapon + Amplifier + T0", 47.8360);
            txtCostofAttacks.Properties.DataSource = CostOfAttack;
            txtCostofAttacks.Properties.ValueMember = "Value";
            txtCostofAttacks.Properties.DisplayMember = "Value";
            txtCostofAttacks.EditValue = 60.5904;
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
                if (lblTotalInflictedDamage.InvokeRequired)
                {
                    lblTotalInflictedDamage.Invoke(new Action(() => CalculateAndDisplayResults()));
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

                if (LBL_EvadedAttackCount.InvokeRequired)
                {
                    LBL_EvadedAttackCount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (LBL_DodgedAttackCount.InvokeRequired)
                {
                    LBL_DodgedAttackCount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }


                if (LblHitDamageSum.InvokeRequired)
                {
                    LblHitDamageSum.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (LblNumberOfAttacks.InvokeRequired)
                {
                    LblNumberOfAttacks.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }



                if (lblTotalCost.InvokeRequired)
                {
                    lblTotalCost.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblDamagePerPec.InvokeRequired)
                {
                    lblDamagePerPec.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblTotalCostPed.InvokeRequired)
                {
                    lblTotalCostPed.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                #region Enhancer Region

                if (Lbl_Enhancer01_Value.InvokeRequired)
                {
                    Lbl_Enhancer01_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer01_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer01_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }


                if (Lbl_Enhancer02_Value.InvokeRequired)
                {
                    Lbl_Enhancer02_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer02_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer02_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }


                if (Lbl_Enhancer03_Value.InvokeRequired)
                {
                    Lbl_Enhancer03_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer03_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer03_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }



                if (Lbl_Enhancer04_Value.InvokeRequired)
                {
                    Lbl_Enhancer04_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer04_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer04_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }


                if (Lbl_Enhancer05_Value.InvokeRequired)
                {
                    Lbl_Enhancer05_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer05_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer05_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }


                if (Lbl_Enhancer06_Value.InvokeRequired)
                {
                    Lbl_Enhancer06_Value.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }
                if (Lbl_Enhancer06_Percentage.InvokeRequired)
                {
                    Lbl_Enhancer06_Percentage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                #endregion
                if (lblTotalCostPed.InvokeRequired)
                {
                    lblTotalCostPed.Invoke(new Action(() => CalculateAndDisplayResults()));
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
            // 2024-05-30 11:58:25 [System] [] You received Shrapnel x (75934) Value: 7.59 PED
            if (shrapnelRegex.IsMatch(line))
            {
                //[System][] You received Shrapnel
                 
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


            if (TargetEvadedYourAttack_Regex.IsMatch(line))
            {
                WriteToFile(line);
                TargetEvadedYourAttack_List.Add(line);

            }

            if (TargetDodgedYourAttack_Regex.IsMatch(line))
            {
                WriteToFile(line);
                TargetDodgedYourAttack_List.Add(line);

            }


            #region Enhancer 

            if (Enhancer01_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer01_List.Add(line);
            }

            if (Enhancer02_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer02_List.Add(line);
            }

            if (Enhancer03_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer03_List.Add(line);
            }

            if (Enhancer04_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer04_List.Add(line);
            }

            if (Enhancer05_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer01_List.Add(line);
            }

            if (Enhancer06_Regex.IsMatch(line))
            {
                WriteToFile(line);
                Enhancer06_List.Add(line);
            }
            #endregion
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
            double Count_ofDodgedAttack = TargetDodgedYourAttack_List.Count();
            double Count_ofMissedAttack = TargetEvadedYourAttack_List.Count();
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

            if (lblTotalInflictedDamage.InvokeRequired)
            {
                lblTotalInflictedDamage.Invoke(new Action(() => lblTotalInflictedDamage.Text = $"{SimpleHitSum + DamageInflictedValue:F2}"));
            }
            else
            {
                lblTotalInflictedDamage.Text = $"{SimpleHitSum + DamageInflictedValue:F2}";
            }

            if (lblhitCount.InvokeRequired)
            {
                lblhitCount.Invoke(new Action(() => lblhitCount.Text = $"{SimpleHitCount:F2}"));
            }
            else
            {
                lblhitCount.Text = $"{SimpleHitCount:F2}";
            }

            if (LblHitDamageSum.InvokeRequired)
            {
                LblHitDamageSum.Invoke(new Action(() => LblHitDamageSum.Text = $"{SimpleHitSum:F2}"));
            }
            else
            {
                LblHitDamageSum.Text = $"{SimpleHitSum:F2}";
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


            if (LBL_DodgedAttackCount.InvokeRequired)
            {
                LBL_DodgedAttackCount.Invoke(new Action(() => LBL_DodgedAttackCount.Text = $"{Count_ofDodgedAttack:F2}"));
            }
            else
            {
                LBL_DodgedAttackCount.Text = $"{Count_ofDodgedAttack:F2}";
            }


            if (LBL_EvadedAttackCount.InvokeRequired)
            {
                LBL_EvadedAttackCount.Invoke(new Action(() => LBL_EvadedAttackCount.Text = $"{Count_ofMissedAttack:F2}"));
            }
            else
            {
                LBL_EvadedAttackCount.Text = $"{Count_ofMissedAttack:F2}";
            }

            double TotalNumberOfAttacks = Count_ofMissedAttack + Count_ofDodgedAttack + SimpleHitCount + DamageInflictedCount;
            if (LblNumberOfAttacks.InvokeRequired)
            {
                LblNumberOfAttacks.Invoke(new Action(() => LblNumberOfAttacks.Text = $"{TotalNumberOfAttacks:F2}"));
            }
            else
            {
                LblNumberOfAttacks.Text = $"{TotalNumberOfAttacks:F2}";
            }


            double CostPerAttack = 0;
            double.TryParse(txtCostofAttacks.Text, out CostPerAttack);

            if (lblTotalCost.InvokeRequired)
            {
                lblTotalCost.Invoke(new Action(() => lblTotalCost.Text = Math.Round(TotalNumberOfAttacks * CostPerAttack,4).ToString()));
            }
            else
            {
                lblTotalCost.Text = Math.Round(TotalNumberOfAttacks * CostPerAttack, 4).ToString();
            }
            double TotalCost = TotalNumberOfAttacks * CostPerAttack;
            double TotalInflictedDamage = SimpleHitSum + DamageInflictedValue;
            if (lblDamagePerPec.InvokeRequired)
            {
                lblDamagePerPec.Invoke(new Action(() => lblDamagePerPec.Text = Math.Round(TotalInflictedDamage / TotalCost, 4).ToString()));
            }
            else
            {
                lblDamagePerPec.Text = Math.Round(TotalInflictedDamage / TotalCost, 4).ToString();
            }

            if (lblTotalCostPed.InvokeRequired)
            {
                lblTotalCostPed.Invoke(new Action(() => lblTotalCostPed.Text = Math.Round(TotalCost / 100, 4).ToString()));
            }
            else
            {
                lblTotalCostPed.Text = Math.Round(TotalCost / 100, 4).ToString();
            }

            #region Enhancer

            double Enhancer01_Count = Enhancer01_List.Count();
            double Enhancer02_Count = Enhancer02_List.Count();
            double Enhancer03_Count = Enhancer03_List.Count();
            double Enhancer04_Count = Enhancer04_List.Count();
            double Enhancer05_Count = Enhancer05_List.Count();
            double Enhancer06_Count = Enhancer06_List.Count();

            double TotalEnhancer = Enhancer01_Count + Enhancer02_Count + Enhancer03_Count + Enhancer04_Count + Enhancer05_Count + Enhancer06_Count;

            if (Lbl_Enhancer01_Value.InvokeRequired)
            {
                Lbl_Enhancer01_Value.Invoke(new Action(() => Lbl_Enhancer01_Value.Text = Math.Round(Enhancer01_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer01_Value.Text = Math.Round(Enhancer01_Count).ToString();
            }
            if (Lbl_Enhancer01_Percentage.InvokeRequired)
            {
                Lbl_Enhancer01_Percentage.Invoke(new Action(() => Lbl_Enhancer01_Percentage.Text = Math.Round((Enhancer01_Count / TotalEnhancer)*100,4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer01_Percentage.Text = Math.Round((Enhancer01_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }

            if (Lbl_Enhancer02_Value.InvokeRequired)
            {
                Lbl_Enhancer02_Value.Invoke(new Action(() => Lbl_Enhancer02_Value.Text = Math.Round(Enhancer02_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer02_Value.Text = Math.Round(Enhancer02_Count).ToString();
            }
            if (Lbl_Enhancer02_Percentage.InvokeRequired)
            {
                Lbl_Enhancer02_Percentage.Invoke(new Action(() => Lbl_Enhancer02_Percentage.Text = Math.Round((Enhancer02_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer02_Percentage.Text = Math.Round((Enhancer02_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }


            if (Lbl_Enhancer03_Value.InvokeRequired)
            {
                Lbl_Enhancer03_Value.Invoke(new Action(() => Lbl_Enhancer03_Value.Text = Math.Round(Enhancer03_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer03_Value.Text = Math.Round(Enhancer03_Count).ToString();
            }
            if (Lbl_Enhancer03_Percentage.InvokeRequired)
            {
                Lbl_Enhancer03_Percentage.Invoke(new Action(() => Lbl_Enhancer03_Percentage.Text = Math.Round((Enhancer03_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer03_Percentage.Text = Math.Round((Enhancer03_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }

            if (Lbl_Enhancer04_Value.InvokeRequired)
            {
                Lbl_Enhancer04_Value.Invoke(new Action(() => Lbl_Enhancer04_Value.Text = Math.Round(Enhancer04_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer04_Value.Text = Math.Round(Enhancer04_Count).ToString();
            }
            if (Lbl_Enhancer04_Percentage.InvokeRequired)
            {
                Lbl_Enhancer04_Percentage.Invoke(new Action(() => Lbl_Enhancer04_Percentage.Text = Math.Round((Enhancer04_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer04_Percentage.Text = Math.Round((Enhancer04_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }

            if (Lbl_Enhancer05_Value.InvokeRequired)
            {
                Lbl_Enhancer05_Value.Invoke(new Action(() => Lbl_Enhancer05_Value.Text = Math.Round(Enhancer05_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer05_Value.Text = Math.Round(Enhancer05_Count).ToString();
            }
            if (Lbl_Enhancer05_Percentage.InvokeRequired)
            {
                Lbl_Enhancer05_Percentage.Invoke(new Action(() => Lbl_Enhancer05_Percentage.Text = Math.Round((Enhancer05_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer05_Percentage.Text = Math.Round((Enhancer05_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }

            if (Lbl_Enhancer06_Value.InvokeRequired)
            {
                Lbl_Enhancer06_Value.Invoke(new Action(() => Lbl_Enhancer06_Value.Text = Math.Round(Enhancer06_Count).ToString()));
            }
            else
            {
                Lbl_Enhancer06_Value.Text = Math.Round(Enhancer06_Count).ToString();
            }
            if (Lbl_Enhancer06_Percentage.InvokeRequired)
            {
                Lbl_Enhancer06_Percentage.Invoke(new Action(() => Lbl_Enhancer06_Percentage.Text = Math.Round((Enhancer06_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
            }
            else
            {
                Lbl_Enhancer06_Percentage.Text = Math.Round((Enhancer06_Count / TotalEnhancer) * 100, 4).ToString() + " %";
            }
            #endregion
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

        private void txtCostofAttacks_EditValueChanged(object sender, EventArgs e)
        {
            double CostPerAttack = 0;
            double.TryParse(txtCostofAttacks.Text, out CostPerAttack);

            double TotalNumberofAttacks = 0;
            double.TryParse(LblNumberOfAttacks.Text, out TotalNumberofAttacks);

            double CostPerPec = TotalNumberofAttacks * CostPerAttack;

            lblTotalCost.Text = Math.Round(CostPerPec, 4).ToString();

            lblTotalCostPed.Text = Math.Round(CostPerPec/100, 4).ToString();

           
            double SimpleHitSum = 0;
            double CriticalHitSum = 0;
            double.TryParse(LblHitDamageSum.Text, out SimpleHitSum);
            double.TryParse(lblTotalInflictedDamage.Text, out SimpleHitSum);
            double TotalInflictedDamage = SimpleHitSum+CriticalHitSum;

            lblDamagePerPec.Text = Math.Round(TotalInflictedDamage/CostPerPec, 4).ToString();
        }
    }
}