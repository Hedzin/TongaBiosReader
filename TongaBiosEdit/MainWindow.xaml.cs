using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;
using HawaiiBiosReader;

namespace TongaBiosReader
{

    public partial class MainWindow : Window
    {
        ObservableCollection<GridRow> data = new ObservableCollection<GridRow>();
        //ObservableCollection<GridRowVoltage> voltageList = new ObservableCollection<GridRowVoltage>();
        ObservableCollection<GridRowVoltage2> voltageList2 = new ObservableCollection<GridRowVoltage2>();
        ObservableCollection<GridRowVoltage2> voltageList3 = new ObservableCollection<GridRowVoltage2>();
        ObservableCollection<GridRowVoltage> gpumemFrequencyListAndPowerLimit = new ObservableCollection<GridRowVoltage>();
        ObservableCollection<GridRowVoltage> fanList = new ObservableCollection<GridRowVoltage>();
        ObservableCollection<GridRow> gpuFrequencyList = new ObservableCollection<GridRow>();
        ObservableCollection<GridRow> memFrequencyList = new ObservableCollection<GridRow>();
        ObservableCollection<GridRow> VCELimitTableData = new ObservableCollection<GridRow>();
        ObservableCollection<GridRow> UVDLimitTableData = new ObservableCollection<GridRow>();
        ObservableCollection<GridRow> SAMULimitTableData = new ObservableCollection<GridRow>();
        ObservableCollection<GridRow> ACPLimitTableData = new ObservableCollection<GridRow>();

        Byte[] romStorageBuffer; // whole rom
        // Byte[] powerTablepattern = new Byte[] { 0x02, 0x06, 0x01, 0x00 };
        Byte[] powerTablepattern = new Byte[] { 0x02, 0x07, 0x01, 0x00 };
        //Byte[] voltageObjectInfoPattern = new Byte[] { 0x00, 0x03, 0x01, 0x01, 0x03 };
        Byte[] voltageObjectInfoPattern = new Byte[] { 0x00, 0x03, 0x01, 0x01, 0x07 };

        // unknown table offsets
        int powerTablePosition;
        int voltageInfoPosition;
        int fanTablePosition;
        int powerTableSize;
        int developTablePosition;

        // table offsets for default
        int fanTableOffset = 559;
        int biosNameOffset = 220;
        int tdpLimitOffset = 0;
        int tdcLimitOffset = 613;
        int powerDeliveryLimitOffset = 31;
        int voltageTable3countOffset = 178;
        int voltageTable3Offset = 179;
        int voltageTable2countOffset = 120;
        int voltageTable2Offset = 121;
        int memoryFrequencyTableCountOffset = 334;
        int memoryFrequencyTableOffset = 342;
        int gpuFrequencyTableOffset = 248;
        int VCELimitTableOffset = 0;
        int AMUAndACPLimitTableOffset = 0;
        int UVDLimitTableOffset = 0;
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); // program version


        public MainWindow()
        {
            InitializeComponent();
            versionbox.Text += version;
            MainWindow.GetWindow(this).Title += " " + version;
        }

        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog.Filter = "Bios files (.rom)|*.rom|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                // Open the selected file to read.
                System.IO.Stream fileStream = openFileDialog.OpenFile();
                filename.Text = openFileDialog.FileName;

                using (BinaryReader br = new BinaryReader(fileStream)) // binary reader
                {
                    romStorageBuffer = br.ReadBytes((int)fileStream.Length);
                    fixChecksum(false);
                    powerTablePosition = PTPatternAt(romStorageBuffer, powerTablepattern);
                    voltageInfoPosition = PatternAt(romStorageBuffer, voltageObjectInfoPattern) - 1;
                    searchposition.Text = "0x" + voltageInfoPosition.ToString("X");


                    biosName.Text = getTextFromBinary(romStorageBuffer, biosNameOffset, 32);
                    gpuID.Text = romStorageBuffer[565].ToString("X2") + romStorageBuffer[564].ToString("X2") + "-" + romStorageBuffer[567].ToString("X2") + romStorageBuffer[566].ToString("X2"); // not finished working only for few bioses :(

                    if (powerTablePosition == -1)
                    {
                        MessageBoxResult result = MessageBox.Show("PowerTable search position not found in this file", "Error", MessageBoxButton.OK);
                    }
                    else
                    {
                        powerTableSize = 256 * romStorageBuffer[powerTablePosition + 1] + romStorageBuffer[powerTablePosition];
                        powerTablesize.Text = powerTableSize.ToString();
                        Console.WriteLine(powerTableSize);
                        /*#################################################################################################
                         * 
                         *               BIOS PARSING SECTION
                         * 
                        #################################################################################################*/
                        switch (powerTableSize)
                        {
                            case 659:
                                powerTablesize.Text += " - R9 380 (659)";
                                break;
                            case 661:
                                powerTablesize.Text += " - R9 380 (661)";
                                break;
                            case 635:
                                powerTablesize.Text += " - R9 285 (635)";
                                fanTableOffset = 533;
                                break;

                                
                            default:
                                powerTablesize.Text += " - Unknown type";
                                break;
                        }

                        fanTablePosition = powerTablePosition + fanTableOffset;
                        powerTablePositionValue.Text = "0x" + powerTablePosition.ToString("X");
                        powerTable.Text = getTextFromBinary(romStorageBuffer, powerTablePosition, powerTableSize);


                        gpumemFrequencyListAndPowerLimit.Clear();
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 98).ToString("X"), get24BitValueFromPosition(powerTablePosition + 98, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 107).ToString("X"), get24BitValueFromPosition(powerTablePosition + 107, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 116).ToString("X"), get24BitValueFromPosition(powerTablePosition + 116, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 101).ToString("X"), get24BitValueFromPosition(powerTablePosition + 101, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 110).ToString("X"), get24BitValueFromPosition(powerTablePosition + 110, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 119).ToString("X"), get24BitValueFromPosition(powerTablePosition + 119, romStorageBuffer, true), "Mhz", "24-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + tdpLimitOffset).ToString("X"), get16BitValueFromPosition(powerTablePosition + tdpLimitOffset, romStorageBuffer), "W", "16-bit"));
                        gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + powerDeliveryLimitOffset).ToString("X"), get16BitValueFromPosition(powerTablePosition + powerDeliveryLimitOffset, romStorageBuffer), "%", "16-bit"));
                        //gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + tdcLimitOffset).ToString("X"), get16BitValueFromPosition(powerTablePosition + tdcLimitOffset, romStorageBuffer), "A", "16-bit"));
                        gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 23).ToString("X"), get24BitValueFromPosition(powerTablePosition + 23, romStorageBuffer, true), "Mhz", "24-bit"));
                        gpumemFrequencyListAndPowerLimit.Add(new GridRowVoltage("0x" + (powerTablePosition + 27).ToString("X"), get24BitValueFromPosition(powerTablePosition + 27, romStorageBuffer, true), "Mhz", "24-bit"));
                        
 
                        memgpuFrequencyTable.ItemsSource = gpumemFrequencyListAndPowerLimit;





                        // read voltage table 1
                        voltageList2.Clear();
                        int voltageTable2count = get8BitValueFromPosition(powerTablePosition + voltageTable2countOffset, romStorageBuffer);
                        int fullvoltageTable2Offset = powerTablePosition + voltageTable2Offset;
                        Console.WriteLine("voltageTable2count:" + voltageTable2count);
                        Console.WriteLine("fullvoltageTable2Offset:" + fullvoltageTable2Offset);
                        for (int i = 0; i < voltageTable2count; i++)
                        {
                            readValueFromPositionToList(voltageList2, (fullvoltageTable2Offset + (i * 8)), (fullvoltageTable2Offset + (i * 8)) + 2, (fullvoltageTable2Offset + (i * 8)) + 4, (fullvoltageTable2Offset + (i * 8)) + 6, 0, "mV", false, i);
                        }
                        voltageEdit2.ItemsSource = voltageList2;


                        // read voltage table 2
                        voltageList3.Clear();
                        int voltageTable3count = get8BitValueFromPosition(powerTablePosition + voltageTable3countOffset, romStorageBuffer);
                        int fullvoltageTable3Offset = powerTablePosition + voltageTable3Offset;
                        Console.WriteLine("voltageTable3count:" + voltageTable3count);
                        Console.WriteLine("fullvoltageTable3Offset:" + fullvoltageTable3Offset);
                        for (int i = 0; i < voltageTable3count; i++)
                        {
                            readValueFromPositionToList(voltageList3, (fullvoltageTable3Offset + (i * 8)), (fullvoltageTable3Offset + (i * 8)) + 2, (fullvoltageTable3Offset + (i * 8)) + 4, (fullvoltageTable3Offset + (i * 8)) + 6, 0, "mV", false, i);
                        }
                        voltageEdit3.ItemsSource = voltageList3;




                        


                        // read voltage table
                        //voltageList.Clear();
                        /*
                        for (int i = 0; i < 28; i++)
                        {
                            readValueFromPositionToList(voltageList, (powerTablePosition + voltageTableOffset + (i * 2)), 0, "mV", false);
                        }
                        voltageEdit.ItemsSource = voltageList;
                        */
                        // memory frequency table

                        int memoryFrequencyTableCount = get8BitValueFromPosition(powerTablePosition + memoryFrequencyTableCountOffset, romStorageBuffer);

                        memFrequencyList.Clear();
                        for (int i = 0; i < memoryFrequencyTableCount; i++)
                        {
                            readValueFromPositionToList(memFrequencyList, (powerTablePosition + memoryFrequencyTableOffset + (i * 13)), 1, "Mhz", true, i);
                        }
                        memFrequencyTable.ItemsSource = memFrequencyList;

                        // gpu frequency table
                        gpuFrequencyList.Clear();
                        for (int i = 0; i < 8; i++)
                        {
                            readValueFromPositionToList(gpuFrequencyList, (powerTablePosition + gpuFrequencyTableOffset + (i * 11)), 1, "Mhz", true, i);
                        }
                        gpuFrequencyTable.ItemsSource = gpuFrequencyList;

                        int position = 0;
                        // StartVCELimitTable
                        VCELimitTableData.Clear();
                        /*
                        for (int i = 0; i < 8; i++)
                        {
                            position = powerTablePosition + VCELimitTableOffset + (i * 3);
                            VCELimitTableData.Add(new GridRow("0x" + (position + 2).ToString("X"),  get8BitValueFromPosition(position + 2, romStorageBuffer), "DPM", "8-bit", i, "0x" + (position).ToString("X"),get16BitValueFromPosition(position, romStorageBuffer, false)));
                        }
                        VCELimitTable.ItemsSource = VCELimitTableData;
                        */
                        // StartUVDLimitTable
                        UVDLimitTableData.Clear();
                        /*
                        for (int i = 0; i < 8; i++)
                        {
                            position = powerTablePosition + UVDLimitTableOffset + (i * 3);
                            UVDLimitTableData.Add(new GridRow("0x" + (position + 2).ToString("X"), get8BitValueFromPosition(position + 2, romStorageBuffer), "DPM", "8-bit", i, "0x" + (position).ToString("X"), get16BitValueFromPosition(position, romStorageBuffer, false)));
                        }
                        UVDLimitTable.ItemsSource = UVDLimitTableData;
                        */
                        // StartSAMULimitTable + StartACPLimitTable

                        SAMULimitTableData.Clear();
                        /*
                        for (int i = 0; i < 8; i++)
                        {
                            position = powerTablePosition + AMUAndACPLimitTableOffset + (i * 5);
                            SAMULimitTableData.Add(new GridRow("0x" + (position + 2).ToString("X"), get24BitValueFromPosition(position + 2, romStorageBuffer), "%", "24-bit", i, "0x" + (position).ToString("X"), get16BitValueFromPosition(position, romStorageBuffer, false)));
                        }
                        SAMULimitTable.ItemsSource = SAMULimitTableData;
                        */

                        ACPLimitTableData.Clear();
                        /*
                        for (int i = 0; i < 8; i++)
                        {
                            position = powerTablePosition + AMUAndACPLimitTableOffset + 42 + (i * 5);
                            ACPLimitTableData.Add(new GridRow("0x" + (position + 2).ToString("X"), get24BitValueFromPosition(position + 2, romStorageBuffer), "%", "24-bit", i, "0x" + (position).ToString("X"), get16BitValueFromPosition(position, romStorageBuffer, false)));
                        }
                        ACPLimitTable.ItemsSource = ACPLimitTableData;
                        */
                        if (fanTablePosition > 0)
                        {


                            fanList.Clear();
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 1).ToString("X"), get8BitValueFromPosition(fanTablePosition + 1, romStorageBuffer), "°C", "8-bit")); //temperatureHysteresis
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 2).ToString("X"), get16BitValueFromPosition(fanTablePosition + 2, romStorageBuffer), "°C", "16-bit")); //fantemperature1
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 4).ToString("X"), get16BitValueFromPosition(fanTablePosition + 4, romStorageBuffer), "°C", "16-bit")); //fantemperature2
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 6).ToString("X"), get16BitValueFromPosition(fanTablePosition + 6, romStorageBuffer), "°C", "16-bit")); //fantemperature3
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 8).ToString("X"), get16BitValueFromPosition(fanTablePosition + 8, romStorageBuffer), "°C", "16-bit")); //fanspeed1
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 10).ToString("X"), get16BitValueFromPosition(fanTablePosition + 10, romStorageBuffer), "°C", "16-bit")); //fanspeed2
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 12).ToString("X"), get16BitValueFromPosition(fanTablePosition + 12, romStorageBuffer), "°C", "16-bit")); //fanspeed3
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 14).ToString("X"), get16BitValueFromPosition(fanTablePosition + 14, romStorageBuffer), "°C", "16-bit")); //fantemperature4
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 16).ToString("X"), get8BitValueFromPosition(fanTablePosition + 16, romStorageBuffer), "1/0", "8-bit")); //fanControlType
                            fanList.Add(new GridRowVoltage("0x" + (fanTablePosition + 17).ToString("X"), get16BitValueFromPosition(fanTablePosition + 17, romStorageBuffer), "°C", "8-bit")); //pwmFanMax
                            fanTable.ItemsSource = fanList;


                            readValueFromPosition(temperatureHysteresis, fanTablePosition + 1, 2, "°C");
                            readValueFromPosition(fantemperature1, fanTablePosition + 2, 0, "°C", true);
                            readValueFromPosition(fantemperature2, fanTablePosition + 4, 0, "°C", true);
                            readValueFromPosition(fantemperature3, fanTablePosition + 6, 0, "°C", true);
                            readValueFromPosition(fantemperature4, fanTablePosition + 14, 0, "°C", true);

                            readValueFromPosition(fanspeed1, fanTablePosition + 8, 0, "%", true);
                            readValueFromPosition(fanspeed2, fanTablePosition + 10, 0, "%", true);
                            readValueFromPosition(fanspeed3, fanTablePosition + 12, 0, "%", true);
                            readValueFromPosition(fanControlType, fanTablePosition + 16, 2, "", true);
                            readValueFromPosition(pwmFanMax, fanTablePosition + 17, 2, "%");
                            //readValueFromPosition(maxAsicTemperature, fanTablePosition + 459, 2, "°C");

                           // readValueFromPosition(gpuMaxClock, powerTablePosition + 23, 1, "Mhz", true);  // this offset work only for 390X need some polishing for other cards
                           // readValueFromPosition(memMaxClock, powerTablePosition + 27, 1, "Mhz", true);

                        }
                        else
                        {
                            temperatureHysteresis.Text = "NOT FOUND";
                            fanControlType.Text = "NOT FOUND";
                            pwmFanMax.Text = "NOT FOUND";
                            maxAsicTemperature.Text = "NOT FOUND";
                            fanspeed1.Text = "NOT FOUND";
                            fanspeed2.Text = "NOT FOUND";
                            fanspeed3.Text = "NOT FOUND";
                            fantemperature1.Text = "NOT FOUND";
                            fantemperature2.Text = "NOT FOUND";
                            fantemperature3.Text = "NOT FOUND";
                            fantemperature4.Text = "NOT FOUND";
                        }
                    }
                    fileStream.Close();
                }
            }
        }


        /*#################################################################################################
         * 
         *               HELPER FUNCTIONS
         * 
        #################################################################################################*/
        public void readValueFromPosition(TextBox dest, int position, int type, String units = "", bool isFrequency = false, bool add = false, bool voltage = false)
        {
            if (add)
            {
                dest.Text += "0x" + position.ToString("X") + " -- ";
            }
            else
            {
                dest.Text = "0x" + position.ToString("X") + " -- ";
            }

            switch (type)
            {
                case 0: // 16 bit value
                    if (voltage)
                        dest.Text += (get16BitValueFromPosition(position, romStorageBuffer, isFrequency) * 6.25).ToString() + " " + units;
                    else
                        dest.Text += get16BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    break;
                case 1: // 24 bit value
                    dest.Text += get24BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    break;
                case 2: // 8 bit value
                    if (voltage)
                        dest.Text += (romStorageBuffer[position] * 6.25).ToString() + " " + units;
                    else
                        dest.Text += romStorageBuffer[position].ToString() + " " + units;
                    break;
                case 3: // 32 bit value
                    dest.Text += get32BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    break;
                default:
                    dest.Text += get16BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    break;
            }
        }
        public Byte readValueFromPositionDevelop(TextBox dest, int position, int type, String units = "", bool isFrequency = false, bool add = false, bool voltage = false)
        {
            if (add)
            {
                dest.Text += "0x" + position.ToString("X") + " -- ";
            }
            else
            {
                dest.Text = "0x" + position.ToString("X") + " -- ";
            }

            switch (type)
            {
                case 0: // 16 bit value
                    dest.Text += get16BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    developTablePosition += 2;
                    return 0;
                case 2: // 8 bit value
                    dest.Text += romStorageBuffer[position].ToString() + " " + units;
                    developTablePosition++;
                    return romStorageBuffer[position];
                case 4: // 32 bit value
                    dest.Text += get32BitValueFromPosition(position, romStorageBuffer, isFrequency).ToString() + " " + units;
                    developTablePosition += 4;
                    return 0;
            }
            return 0;
        }

        public void readValueFromPositionToList(ObservableCollection<GridRow> dest, int position, int type, String units = "", bool isFrequency = false, int dpm = -1)
        {
            switch (type)
            {
                case 0: // 16 bit value
                    dest.Add(new GridRow("0x" + position.ToString("X"), get16BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "16-bit", dpm));
                    break;
                case 1: // 24 bit value
                    dest.Add(new GridRow("0x" + position.ToString("X"), get24BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "24-bit", dpm));
                    break;
                case 2: // 8 bit value
                    dest.Add(new GridRow("0x" + position.ToString("X"), get8BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "8-bit", dpm));
                    break;
                default:
                    dest.Add(new GridRow("0x" + position.ToString("X"), get8BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "8-bit", dpm));
                    break;
            }
        }

        public void readValueFromPositionToList(ObservableCollection<GridRowVoltage> dest, int position, int type, String units = "", bool isFrequency = false)
        {
            switch (type)
            {
                case 0: // 16 bit value
                    dest.Add(new GridRowVoltage("0x" + position.ToString("X"), get16BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "16-bit"));
                    break;
                case 1: // 24 bit value
                    dest.Add(new GridRowVoltage("0x" + position.ToString("X"), get24BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "24-bit"));
                    break;
                case 2: // 8 bit value
                    dest.Add(new GridRowVoltage("0x" + position.ToString("X"), get8BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "8-bit"));
                    break;
                default:
                    dest.Add(new GridRowVoltage("0x" + position.ToString("X"), get8BitValueFromPosition(position, romStorageBuffer, isFrequency), units, "8-bit"));
                    break;
            }
        }
        public void readValueFromPositionToList(ObservableCollection<GridRowVoltage2> dest, int position0, int position1, int position2, int position3, int type, String units = "", bool isFrequency = false, int dpm = -1)
        {
            switch (type)
            {
                case 0: // 16 bit value
                    dest.Add(new GridRowVoltage2("0x" + position0.ToString("X"), get16BitValueFromPosition(position0, romStorageBuffer, isFrequency),
                        "0x" + position1.ToString("X"), get16BitValueFromPosition(position1, romStorageBuffer, isFrequency),
                        "0x" + position2.ToString("X"), get16BitValueFromPosition(position2, romStorageBuffer, isFrequency),
                        "0x" + position3.ToString("X"), get16BitValueFromPosition(position3, romStorageBuffer, isFrequency), units, "16-bit", dpm));
                    break;
                case 1: // 24 bit value
                    dest.Add(new GridRowVoltage2("0x" + position0.ToString("X"), get24BitValueFromPosition(position0, romStorageBuffer, isFrequency),
                        "0x" + position1.ToString("X"), get24BitValueFromPosition(position1, romStorageBuffer, isFrequency),
                        "0x" + position2.ToString("X"), get24BitValueFromPosition(position2, romStorageBuffer, isFrequency),
                        "0x" + position3.ToString("X"), get24BitValueFromPosition(position3, romStorageBuffer, isFrequency), units, "24-bit", dpm));
                    break;
                case 2: // 8 bit value
                    dest.Add(new GridRowVoltage2("0x" + position0.ToString("X"), get8BitValueFromPosition(position0, romStorageBuffer, isFrequency),
                        "0x" + position1.ToString("X"), get8BitValueFromPosition(position1, romStorageBuffer, isFrequency),
                        "0x" + position2.ToString("X"), get8BitValueFromPosition(position2, romStorageBuffer, isFrequency),
                        "0x" + position3.ToString("X"), get8BitValueFromPosition(position3, romStorageBuffer, isFrequency), units, "8-bit", dpm));
                    break;
                default:
                    dest.Add(new GridRowVoltage2("0x" + position0.ToString("X"), get8BitValueFromPosition(position0, romStorageBuffer, isFrequency),
                        "0x" + position1.ToString("X"), get8BitValueFromPosition(position1, romStorageBuffer, isFrequency),
                        "0x" + position2.ToString("X"), get8BitValueFromPosition(position2, romStorageBuffer, isFrequency),
                        "0x" + position3.ToString("X"), get8BitValueFromPosition(position3, romStorageBuffer, isFrequency), units, "8-bit", dpm));
                    break;
            }
        }

        private static int PTPatternAt(byte[] data, byte[] pattern)
        {
            for (int di = 0; di < data.Length; di++)
                if (data[di] == pattern[0] && data[di + 1] == pattern[1] && data[di + 2] == pattern[2] && data[di + 3] == pattern[3])
                {
                    return di - 1;
                }
            return -1;
        }

        private static int PatternAt(byte[] data, byte[] pattern)
        {
            for (int i = 0; i < data.Length; )
            {
                int j;
                for (j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j] != data[i])
                        break;
                    i++;
                }
                if (j == pattern.Length)
                {
                    return i - pattern.Length;
                }
                if (j != 0) continue;
                i++;
            }
            return -1;
        }

        public String getTextFromBinary(byte[] binary, int offset, int lenght)
        {
            System.Text.Encoding encEncoder = System.Text.ASCIIEncoding.ASCII;
            string str = encEncoder.GetString(binary.Skip(offset).Take(lenght).ToArray());
            return str;
        }

        // dumb way to extract 24 bit value (can be made much more effective but this is easy to read for anyone)
        public Int32 get24BitValueFromPosition(int position, byte[] buffer, bool isFrequency = false)
        {
            if (position < buffer.Length - 2)
            {
                if (isFrequency) // if its frequency divide by 100 to convert it into Mhz
                {
                    return (256 * 256 * buffer[position + 2] + 256 * buffer[position + 1] + buffer[position]) / 100;
                }
                return 256 * 256 * buffer[position + 2] + 256 * buffer[position + 1] + buffer[position];
            }
            return -1;
        }
        // dumb way to extract 32 bit value (can be made much more effective but this is easy to read for anyone)
        public Int32 get32BitValueFromPosition(int position, byte[] buffer, bool isFrequency = false)
        {
            if (position < buffer.Length - 3)
            {
                if (isFrequency) // if its frequency divide by 100 to convert it into Mhz
                {
                    return (256 * 256 * 256 * buffer[position + 3]) + (256 * 256 * buffer[position + 2] + 256 * buffer[position + 1] + buffer[position]) / 100;
                }
                return (256 * 256 * 256 * buffer[position + 3]) + (256 * 256 * buffer[position + 2]) + (256 * buffer[position + 1]) + buffer[position];
            }
            return -1;
        }
        // dumb way to extract 16 bit value (can be made much more effective but this is easy to read for anyone)
        public Int32 get16BitValueFromPosition(int position, byte[] buffer, bool isFrequency = false)
        {
            if (position < buffer.Length - 1)
            {
                if (isFrequency) // if its frequency divide by 100 to convert it into Mhz
                {
                    return (256 * buffer[position + 1] + buffer[position]) / 100;
                }
                return 256 * buffer[position + 1] + buffer[position];
            }
            return -1;
        }

        public Int32 get8BitValueFromPosition(int position, byte[] buffer, bool isFrequency = false)
        {
            if (position < buffer.Length)
            {
                if (isFrequency) // if its frequency divide by 100 to convert it into Mhz
                {
                    return buffer[position] / 100;
                }
                return buffer[position];
            }
            return -1;
        }

        private void SaveFileDialog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.Title = "Save As...";
            SaveFileDialog.Filter = "Bios File (*.rom)|*.rom";
            bool? userClickedOK = SaveFileDialog.ShowDialog();
            if (userClickedOK == true)
            {
                FileStream fs = new FileStream(SaveFileDialog.FileName, FileMode.Create);
                // Create the writer for data.
                BinaryWriter bw = new BinaryWriter(fs);

                // save our changes
                saveList(voltageList2, false); // there are values which are not frequency but it works as they are only singlevalue
                saveList(voltageList3, false); // there are values which are not frequency but it works as they are only singlevalue
                saveList(memFrequencyList, true);
                saveList(gpuFrequencyList, true);
                saveList(gpumemFrequencyListAndPowerLimit, true);
                saveList(fanList);
                // saveList(VCELimitTableData, false);
                // saveList(ACPLimitTableData, false);
                // saveList(UVDLimitTableData, false);
                // saveList(SAMULimitTableData, false);
                fixChecksum(true);
                bw.Write(romStorageBuffer);

                fs.Close();
                bw.Close();
            }
        }
        private void fixChecksum(bool save)
        {
            Byte oldchecksum = romStorageBuffer[33];
            int size = romStorageBuffer[2] * 512;
            Byte newchecksum = 0;

            for (int i = 0; i < size; i++)
            {
                newchecksum += romStorageBuffer[i];
            }
            if (oldchecksum == (romStorageBuffer[33] - newchecksum))
            {
                checksumResult.Text = "OK";
            }
            else
            {
                checksumResult.Text = "WRONG - save for fix";
            }
            if (save)
            {
                romStorageBuffer[33] -= newchecksum;
                checksumResult.Text = "OK";
            }

        }

        private void saveList(ObservableCollection<GridRowVoltage> list, bool isFrequency = false)
        {
            foreach (GridRowVoltage row in list)
            {
                int savePosition;
                int value = row.value;
                if (row.position.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    row.position = row.position.Substring(2);
                }
                if (isFrequency) // there is hack for 16 bit need fix
                {
                    value *= 100;
                }
                if (int.TryParse(row.position, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition] = (byte)value;
                                romStorageBuffer[savePosition + 1] = (byte)(value >> 8);
                                romStorageBuffer[savePosition + 2] = (byte)(value >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition] = (byte)row.value;
                                romStorageBuffer[savePosition + 1] = (byte)(row.value >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition] = (byte)row.value;
                                break;
                            }
                    }
                }
            }
        }

        private void saveList(ObservableCollection<GridRowVoltage2> list, bool isFrequency = false)
        {
            foreach (GridRowVoltage2 row in list)
            {
                int savePosition0;
                int savePosition1;
                int savePosition2;
                int savePosition3;
                int value0 = row.value0;
                int value1 = row.value1;
                int value2 = row.value2;
                int value3 = row.value3;
                if (row.position0.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) &&
                    row.position1.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) &&
                    row.position2.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) &&
                    row.position3.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    row.position0 = row.position0.Substring(2);
                    row.position1 = row.position1.Substring(2);
                    row.position2 = row.position2.Substring(2);
                    row.position3 = row.position3.Substring(2);
                }
                if (isFrequency) // there is hack for 16 bit need fix
                {
                    value0 *= 100;
                    value1 *= 100;
                    value2 *= 100;
                    value3 *= 100;
                }
                if (int.TryParse(row.position0, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition0))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition0] = (byte)value0;
                                romStorageBuffer[savePosition0 + 1] = (byte)(value0 >> 8);
                                romStorageBuffer[savePosition0 + 2] = (byte)(value0 >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition0] = (byte)row.value0;
                                romStorageBuffer[savePosition0 + 1] = (byte)(row.value0 >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition0] = (byte)row.value0;
                                break;
                            }
                    }
                }
                if (int.TryParse(row.position1, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition1))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition1] = (byte)value1;
                                romStorageBuffer[savePosition1 + 1] = (byte)(value1 >> 8);
                                romStorageBuffer[savePosition1 + 2] = (byte)(value1 >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition1] = (byte)row.value1;
                                romStorageBuffer[savePosition1 + 1] = (byte)(row.value1 >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition1] = (byte)row.value1;
                                break;
                            }
                    }
                }
                if (int.TryParse(row.position2, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition2))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition2] = (byte)value2;
                                romStorageBuffer[savePosition2 + 1] = (byte)(value2 >> 8);
                                romStorageBuffer[savePosition2 + 2] = (byte)(value2 >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition2] = (byte)row.value2;
                                romStorageBuffer[savePosition2 + 1] = (byte)(row.value2 >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition2] = (byte)row.value2;
                                break;
                            }
                    }
                }
                if (int.TryParse(row.position3, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition3))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition3] = (byte)value3;
                                romStorageBuffer[savePosition3 + 1] = (byte)(value3 >> 8);
                                romStorageBuffer[savePosition3 + 2] = (byte)(value3 >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition3] = (byte)row.value3;
                                romStorageBuffer[savePosition3 + 1] = (byte)(row.value3 >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition3] = (byte)row.value3;
                                break;
                            }
                    }
                }
            }
        }







        private void saveList(ObservableCollection<GridRow> list, bool isFrequency = false)
        {
            foreach (GridRow row in list)
            {
                int savePosition;

                int value = row.value;

                if (row.position.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    row.position = row.position.Substring(2);

                }

                if (isFrequency) // there is hack for 16 bit need fix
                {
                    value *= 100;
                }
                if (int.TryParse(row.position, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out savePosition))
                {
                    switch (row.type)
                    {
                        case "24-bit":
                            {
                                // this is for 24 bit
                                romStorageBuffer[savePosition] = (byte)value;
                                romStorageBuffer[savePosition + 1] = (byte)(value >> 8);
                                romStorageBuffer[savePosition + 2] = (byte)(value >> 16);
                                break;
                            }
                        case "16-bit":
                            {
                                romStorageBuffer[savePosition] = (byte)row.value;
                                romStorageBuffer[savePosition + 1] = (byte)(row.value >> 8);
                                break;
                            }
                        case "8-bit":
                            {
                                romStorageBuffer[savePosition] = (byte)row.value;
                                break;
                            }
                    }
                }
            }
        }
        // this is here because of bug with tabs and grids thanks microsoft
        private void voltageEdit2_GotFocus(object sender, RoutedEventArgs e)
        {
            voltageEdit2.Columns[0].IsReadOnly = true;
            voltageEdit2.Columns[1].IsReadOnly = true;
            voltageEdit2.Columns[2].IsReadOnly = false;
            voltageEdit2.Columns[3].IsReadOnly = true;
            voltageEdit2.Columns[4].IsReadOnly = false;
            voltageEdit2.Columns[5].IsReadOnly = true;
            voltageEdit2.Columns[6].IsReadOnly = false;
            voltageEdit2.Columns[7].IsReadOnly = true;
            voltageEdit2.Columns[8].IsReadOnly = false;
            voltageEdit2.Columns[9].IsReadOnly = true;
            voltageEdit2.Columns[10].IsReadOnly = true;
        }
        // this is here because of bug with tabs and grids thanks microsoft
        private void voltageEdit3_GotFocus(object sender, RoutedEventArgs e)
        {
            voltageEdit3.Columns[0].IsReadOnly = true;
            voltageEdit3.Columns[1].IsReadOnly = true;
            voltageEdit3.Columns[2].IsReadOnly = false;
            voltageEdit3.Columns[3].IsReadOnly = true;
            voltageEdit3.Columns[4].IsReadOnly = false;
            voltageEdit3.Columns[5].IsReadOnly = true;
            voltageEdit3.Columns[6].IsReadOnly = false;
            voltageEdit3.Columns[7].IsReadOnly = true;
            voltageEdit3.Columns[8].IsReadOnly = false;
            voltageEdit3.Columns[9].IsReadOnly = true;
            voltageEdit3.Columns[10].IsReadOnly = true;
        }
        private void gpuFrequencyTable_GotFocus(object sender, RoutedEventArgs e)
        {
            gpuFrequencyTable.Columns[0].IsReadOnly = true;
            gpuFrequencyTable.Columns[1].IsReadOnly = true;
            gpuFrequencyTable.Columns[2].IsReadOnly = false;
            gpuFrequencyTable.Columns[3].IsReadOnly = true;
            gpuFrequencyTable.Columns[4].IsReadOnly = true;

        }

        private void memFrequencyTable_GotFocus(object sender, RoutedEventArgs e)
        {
            memFrequencyTable.Columns[0].IsReadOnly = true;
            memFrequencyTable.Columns[1].IsReadOnly = true;
            memFrequencyTable.Columns[2].IsReadOnly = false;
            memFrequencyTable.Columns[3].IsReadOnly = true;
            memFrequencyTable.Columns[4].IsReadOnly = true;

        }

        private void fanTable_GotFocus(object sender, RoutedEventArgs e)
        {
            memFrequencyTable.Columns[0].IsReadOnly = true;
            memFrequencyTable.Columns[1].IsReadOnly = true;
            memFrequencyTable.Columns[2].IsReadOnly = false;
            memFrequencyTable.Columns[3].IsReadOnly = true;
            memFrequencyTable.Columns[4].IsReadOnly = true;

        }

        private void memgpuFrequencyTable_GotFocus(object sender, RoutedEventArgs e)
        {
            memgpuFrequencyTable.Columns[0].IsReadOnly = true;
            memgpuFrequencyTable.Columns[1].IsReadOnly = false;
            memgpuFrequencyTable.Columns[2].IsReadOnly = true;
            memgpuFrequencyTable.Columns[3].IsReadOnly = true;
        }

        private void VCELimitTable_GotFocus(object sender, RoutedEventArgs e)
        {
            VCELimitTable.Columns[0].IsReadOnly = true;
            VCELimitTable.Columns[1].IsReadOnly = true;
            VCELimitTable.Columns[2].IsReadOnly = true;
            VCELimitTable.Columns[3].IsReadOnly = true;
            VCELimitTable.Columns[4].IsReadOnly = true;
            VCELimitTable.Columns[5].IsReadOnly = true;
            VCELimitTable.Columns[6].IsReadOnly = true;
        }
        private void ACPLimitTable_GotFocus(object sender, RoutedEventArgs e)
        {
            ACPLimitTable.Columns[0].IsReadOnly = true;
            ACPLimitTable.Columns[1].IsReadOnly = true;
            ACPLimitTable.Columns[2].IsReadOnly = true;
            ACPLimitTable.Columns[3].IsReadOnly = true;
            ACPLimitTable.Columns[4].IsReadOnly = true;
            ACPLimitTable.Columns[5].IsReadOnly = true;
            ACPLimitTable.Columns[6].IsReadOnly = true;
        }
        private void SAMULimitTable_GotFocus(object sender, RoutedEventArgs e)
        {
            SAMULimitTable.Columns[0].IsReadOnly = true;
            SAMULimitTable.Columns[1].IsReadOnly = true;
            SAMULimitTable.Columns[2].IsReadOnly = true;
            SAMULimitTable.Columns[3].IsReadOnly = true;
            SAMULimitTable.Columns[4].IsReadOnly = true;
            SAMULimitTable.Columns[5].IsReadOnly = true;
            SAMULimitTable.Columns[6].IsReadOnly = true;
        }
        private void UVDLimitTable_GotFocus(object sender, RoutedEventArgs e)
        {
            UVDLimitTable.Columns[0].IsReadOnly = true;
            UVDLimitTable.Columns[1].IsReadOnly = true;
            UVDLimitTable.Columns[2].IsReadOnly = true;
            UVDLimitTable.Columns[3].IsReadOnly = true;
            UVDLimitTable.Columns[4].IsReadOnly = true;
            UVDLimitTable.Columns[5].IsReadOnly = true;
            UVDLimitTable.Columns[6].IsReadOnly = true;
        }

        // developer function
        private void search_Click(object sender, RoutedEventArgs e)
        {
        }

        private void VCELimitTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void gpuFrequencyTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void gpuFrequencyTable_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void memgpuFrequencyTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void fanTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
