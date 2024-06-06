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
using DevExpress.Utils.Html.Internal;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using DevExpress.XtraPrinting.Native;

namespace Entropia
{
    public partial class FrmReportDashboard : DevExpress.XtraEditors.XtraForm
    {
        double SwordDamageSum = 0;
        double SwordDamageCount = 0;
        double RiffleDamageSum = 0;
        double RiffleDamageCount = 0;
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


        #region You Received 
        private readonly Regex YouReceived_Regex = new Regex(@"\[System\] \[\] You received (.+?) x \(\d+\) Value: ([\d.]+) PED");
        DataTable YouReceived_List = new DataTable();
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
        private FileSystemWatcher f;

        #region KeyLogger for Swords and Riffle

        private static List<DateTime> Damage_Time = new List<DateTime>();
        private static string userInput = "Sword"; // Default value


        // Import the necessary Windows API functions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        #endregion

        public FrmReportDashboard()
        {
            InitializeComponent();
        }

        private async void FrmReportDashboard_Load(object sender, EventArgs e)
        {
            int num = new Random().Next(1000, 9999);
            txtInputFilePath.Text = "C:\\Users\\Salman Naveed\\Downloads\\Entropia\\chat.log";
            txtOutputFilePath.Text = "C:\\Users\\Salman Naveed\\Downloads\\Entropia\\" +num;

            CostOfAttack.Columns.Add("Description");
            CostOfAttack.Columns.Add("Value");
            CostOfAttack.Rows.Add("Weapon + Amplifier + Metric + T4", 60.5904);
            CostOfAttack.Rows.Add("Weapon + Amplifier + Metric + T0", 47.8560);
            CostOfAttack.Rows.Add("Weapon + Amplifier + T4", 60.5704);
            CostOfAttack.Rows.Add("Weapon + Amplifier + T0", 47.8360);
            txtCostofAttacks.Properties.DataSource = CostOfAttack;
            txtCostofAttacks.Properties.ValueMember = "Value";
            txtCostofAttacks.Properties.DisplayMember = "Value";
            txtCostofAttacks.EditValue = 60.5904;

            YouReceived_List.Columns.Add("Item");
            YouReceived_List.Columns.Add("Value", Type.GetType("System.Double"));
            YouReceived_List.Columns.Add("Count", Type.GetType("System.Int32"));
           

            gridControl1.DataSource = YouReceived_List;

            Console.WriteLine("Keylogger is running. Press '1' for Sword and '2' for Riffle.");

            _hookID = SetHook(_proc);
           
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

                if (gridControl1.InvokeRequired)
                {
                    gridControl1.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblswordcount.InvokeRequired)
                {
                    lblswordcount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (LblRiffleCount.InvokeRequired)
                {
                    LblRiffleCount.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (lblRfilleDamage.InvokeRequired)
                {
                    lblRfilleDamage.Invoke(new Action(() => CalculateAndDisplayResults()));
                }
                else
                {
                    CalculateAndDisplayResults();
                }

                if (LblSwordDamage.InvokeRequired)
                {
                    LblSwordDamage.Invoke(new Action(() => CalculateAndDisplayResults()));
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
                WriteToFile(line + " - " + userInput);
                DateTime damageTime = ExtractDamageTime(line);
                
                if (damageTime != DateTime.MinValue)
                {
                    Damage_Time.Add(damageTime);
                    Console.WriteLine(damageTime);
                }
                var match = DamageRegex.Match(line);
                if (match.Success)
                {
                    //XtraMessageBox.Show("asndjaskdhaskjdas");
                    if (double.TryParse(match.Groups[1].Value, out double DamageValue))
                    {
                        CriticalDamageInflictedValues.Add(DamageValue);

                        if (Damage_Time.Count >= 2)
                        {
                            DateTime lastDateTime = Damage_Time[Damage_Time.Count - 2];
                            DateTime currentDateTime = Damage_Time[Damage_Time.Count - 1];

                            TimeSpan timeDifference = currentDateTime - lastDateTime;
                           
                            double secondsDifference = timeDifference.TotalSeconds;
                            Console.WriteLine(secondsDifference);

                            if (secondsDifference>=2 && userInput == "Sword")
                            {
                                SwordDamageCount += 1;
                                SwordDamageSum += DamageValue;
                            }
                            else if (secondsDifference<2 && userInput == "Sword")
                            {
                                RiffleDamageCount += 1;
                                RiffleDamageSum += DamageValue;
                            }
                            else if (userInput == "Riffle")
                            {
                                RiffleDamageCount += 1;
                                RiffleDamageSum += DamageValue;
                            }
                        }
                       
                    }
                    else if (Damage_Time.Count == 1 && userInput != "")
                    {
                        if (userInput == "Sword")
                        {
                            SwordDamageCount += 1;
                            SwordDamageSum += DamageValue;
                        }
                        else if (userInput == "Riffle")
                        {
                            RiffleDamageCount += 1;
                            RiffleDamageSum += DamageValue;
                        }
                    }
                }

            }

            if (Simpledamage_inflicted.IsMatch(line))
            {
               
                WriteToFile(line);
                DateTime damageTime = ExtractDamageTime(line);

                if (damageTime != DateTime.MinValue)
                {
                    Damage_Time.Add(damageTime);
                    Console.WriteLine(damageTime);
                }
                var match = DamageRegex.Match(line);
                if (match.Success)
                {
                    //XtraMessageBox.Show("asndjaskdhaskjdas");
                    if (double.TryParse(match.Groups[1].Value, out double DamageValue))
                    {
                        SimpleDamageInflictedValues.Add(DamageValue);


                        if (Damage_Time.Count >= 2)
                        {
                            DateTime lastDateTime = Damage_Time[Damage_Time.Count - 2];
                            DateTime currentDateTime = Damage_Time[Damage_Time.Count - 1];

                            TimeSpan timeDifference = currentDateTime - lastDateTime;

                            double secondsDifference = timeDifference.TotalSeconds;
                            Console.WriteLine(secondsDifference);
                            
                            if (secondsDifference >= 2 && userInput == "Sword")
                            {
                                SwordDamageCount += 1;
                                SwordDamageSum += DamageValue;
                            }
                            else if (secondsDifference < 2 && userInput == "Sword")
                            {
                                RiffleDamageCount += 1;
                                RiffleDamageSum += DamageValue;
                            }
                            else if (userInput == "Riffle")
                            {
                                RiffleDamageCount += 1;
                                RiffleDamageSum += DamageValue;
                            }
                        }
                        else if (Damage_Time.Count == 1 && userInput != "")
                        {
                            if (userInput == "Sword")
                            {
                                SwordDamageCount += 1;
                                SwordDamageSum += DamageValue;
                            }
                            else if (userInput == "Riffle")
                            {
                                RiffleDamageCount += 1;
                                RiffleDamageSum += DamageValue;
                            }
                        }
                    }
                }

            }


            if (TargetEvadedYourAttack_Regex.IsMatch(line))
            {
                WriteToFile(line);
                DateTime damageTime = ExtractDamageTime(line);

                if (damageTime != DateTime.MinValue)
                {
                    Damage_Time.Add(damageTime);
                    Console.WriteLine(damageTime);
                }
                TargetEvadedYourAttack_List.Add(line);

                if (Damage_Time.Count >= 2)
                {
                    DateTime lastDateTime = Damage_Time[Damage_Time.Count - 2];
                    DateTime currentDateTime = Damage_Time[Damage_Time.Count - 1];

                    TimeSpan timeDifference = currentDateTime - lastDateTime;

                    double secondsDifference = timeDifference.TotalSeconds;
                    Console.WriteLine(secondsDifference);

                    if (secondsDifference >= 2 && userInput == "Sword")
                    {
                        SwordDamageCount += 1;
                       // SwordDamageSum += DamageValue;
                    }
                    else if (secondsDifference < 2 && userInput == "Sword")
                    {
                        RiffleDamageCount += 1;
                       /// RiffleDamageSum += DamageValue;
                    }
                    else if (userInput == "Riffle")
                    {
                        RiffleDamageCount += 1;
                        //RiffleDamageSum += DamageValue;
                    }
                }
                else if (Damage_Time.Count == 1 && userInput != "")
                {
                    if (userInput == "Sword")
                    {
                        SwordDamageCount += 1;
                       
                    }
                    else if (userInput == "Riffle")
                    {
                        RiffleDamageCount += 1;
                     
                    }
                }
            }

            if (TargetDodgedYourAttack_Regex.IsMatch(line))
            {
                WriteToFile(line);
                DateTime damageTime = ExtractDamageTime(line);

                if (damageTime != DateTime.MinValue)
                {
                    Damage_Time.Add(damageTime);
                    Console.WriteLine(damageTime);
                }
                TargetDodgedYourAttack_List.Add(line);

                if (Damage_Time.Count >= 2)
                {
                    DateTime lastDateTime = Damage_Time[Damage_Time.Count - 2];
                    DateTime currentDateTime = Damage_Time[Damage_Time.Count - 1];

                    TimeSpan timeDifference = currentDateTime - lastDateTime;

                    double secondsDifference = timeDifference.TotalSeconds;
                    Console.WriteLine(secondsDifference);

                    if (secondsDifference >= 2 && userInput == "Sword")
                    {
                        SwordDamageCount += 1;
                        // SwordDamageSum += DamageValue;
                    }
                    else if (secondsDifference < 2 && userInput == "Sword")
                    {
                        RiffleDamageCount += 1;
                        /// RiffleDamageSum += DamageValue;
                    }
                    else if (userInput == "Riffle")
                    {
                        RiffleDamageCount += 1;
                        //RiffleDamageSum += DamageValue;
                    }
                }
                else if (Damage_Time.Count == 1 && userInput != "")
                {
                    if (userInput == "Sword")
                    {
                        SwordDamageCount += 1;
                        //SwordDamageSum += DamageValue;
                    }
                    else if (userInput == "Riffle")
                    {
                        RiffleDamageCount += 1;
                       // RiffleDamageSum += DamageValue;
                    }
                }

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

            #region You Received

            var match_YouReceived = YouReceived_Regex.Match(line);
            if (match_YouReceived.Success)
            {
                WriteToFile(line);

                string item = match_YouReceived.Groups[1].Value;
                double value = double.Parse(match_YouReceived.Groups[2].Value);
                if (item == "Universal Ammo") 
                { } else
                {
                    if (gridControl1.InvokeRequired)
                    {
                        gridControl1.BeginInvoke(new Action(() => {
                            AddOrUpdateRow(YouReceived_List, item, value);
                        }));
                    }
                    else
                    {
                        AddOrUpdateRow(YouReceived_List, item, value);
                    }
                }
               



            }

            #endregion
        }



        public DateTime ExtractDamageTime(string line)
        {
            DateTime damageTime = DateTime.MinValue;
            string dateTimePart = line.Substring(0, 19); // Assuming the timestamp format is "yyyy-MM-dd HH:mm:ss"
            if (DateTime.TryParseExact(dateTimePart, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out damageTime))
            {
                return damageTime;
            }
            else
            {
                // Handle invalid date format
                Console.WriteLine("Invalid date format in line: " + line);
                return damageTime; // Will be DateTime.MinValue
            }
        }


        private void AddOrUpdateRow(DataTable table, string item, double value)
        {
            var row = table.Rows.Cast<DataRow>().FirstOrDefault(r => r["Item"].ToString() == item);
            if (row != null)
            {
                row["Value"] = (double)row["Value"] + value;
                row["Count"] = (int)row["Count"] + 1; 
            }
            else
            {
                table.Rows.Add(item, value, 1); 
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
                lblTotalCost.Invoke(new Action(() => lblTotalCost.Text = Math.Round(TotalNumberOfAttacks * CostPerAttack, 4).ToString()));
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
                Lbl_Enhancer01_Percentage.Invoke(new Action(() => Lbl_Enhancer01_Percentage.Text = Math.Round((Enhancer01_Count / TotalEnhancer) * 100, 4).ToString() + " %"));
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

            //#region Grid Update You Received
            if (gridControl1.InvokeRequired)
            {
                gridControl1.BeginInvoke(new Action(() => {
                    //gridControl1.DataSource = null;
                    gridControl1.DataSource = YouReceived_List;
                    gridControl1.RefreshDataSource();
                }));
            }
            else
            {
               // gridControl1.DataSource = null;
                gridControl1.DataSource = YouReceived_List;
                gridControl1.RefreshDataSource();
            }

            if (lblswordcount.InvokeRequired)
            {
                lblswordcount.Invoke(new Action(() => lblswordcount.Text = SwordDamageCount.ToString()));
            }
            else
            {
                lblswordcount.Text = SwordDamageCount.ToString();
            }
            if (LblRiffleCount.InvokeRequired)
            {
                LblRiffleCount.Invoke(new Action(() => LblRiffleCount.Text = RiffleDamageCount.ToString()));
            }
            else
            {
                LblRiffleCount.Text = RiffleDamageCount.ToString();
            }
            if (lblRfilleDamage.InvokeRequired)
            {
                lblRfilleDamage.Invoke(new Action(() => lblRfilleDamage.Text =Math.Round(RiffleDamageSum,2).ToString()));
            }
            else
            {
                lblRfilleDamage.Text = Math.Round(RiffleDamageSum, 2).ToString();
            }
            if (LblSwordDamage.InvokeRequired)
            {
                LblSwordDamage.Invoke(new Action(() => LblSwordDamage.Text =Math.Round(SwordDamageSum,2).ToString()));
            }
            else
            {
                LblSwordDamage.Text =Math.Round(SwordDamageSum,2).ToString();
            }
            //#endregion
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


        private async void btnStart_Click(object sender, EventArgs e)
        {
            SwordDamageCount = 0;
            SwordDamageSum = 0;
            RiffleDamageCount = 0;
            RiffleDamageSum = 0; 

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

            lblTotalCostPed.Text = Math.Round(CostPerPec / 100, 4).ToString();


            double SimpleHitSum = 0;
            double CriticalHitSum = 0;
            double.TryParse(LblHitDamageSum.Text, out SimpleHitSum);
            double.TryParse(lblTotalInflictedDamage.Text, out SimpleHitSum);
            double TotalInflictedDamage = SimpleHitSum + CriticalHitSum;

            lblDamagePerPec.Text = Math.Round(TotalInflictedDamage / CostPerPec, 4).ToString();
        }

        #region Keylogger Logic


        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Convert the virtual key code to a character
                char keyChar = (char)vkCode;

                // userInput += keyChar;

                if (keyChar == '1')
                {
                    userInput = "Riffle";
                   
                }
                else if (keyChar == '2')
                {
                    userInput = "Sword";
                }




                Console.WriteLine(userInput);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        #endregion


    }

}

