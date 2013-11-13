using PointyStickBlend.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointyStickBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Insert code required on object creation below this point.
        }

        private void exit_application(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void load_tracefile(object sender, RoutedEventArgs e)
        {
            // Load the global color palette
            Array color_palette = (Array)this.FindResource("color_palette");

            // Load the global model
            InstructionViewModel global_instruction_list = (InstructionViewModel)this.FindResource("instruction_view_model");

            // Clear the model
            global_instruction_list.Model.Clear();

            string filename = "e:\\Users\\Whistlepig\\Documents\\Code\\PointyStickInstructionTrace.txt";

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        // Create a new instruction object for this line
                        Instruction i = new Instruction();

                        string line = sr.ReadLine().Trim();

                        string[] kvpairs = line.Split('|');

                        foreach (string kv in kvpairs)
                        {
                            if (kv == "")
                                continue;

                            // Split on the first colon
                            string[] members = kv.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (members.Length != 2)
                            {
                                Debug.WriteLine("Key-Value pair causing problems: " + members);
                                continue;
                            }
                            string key = members[0].Trim();
                            string value = members[1].Trim();

                            switch (key)
                            {
                                case "adr": // Address (Execution)
                                    uint address;
                                    if (UInt32.TryParse(value, out address))
                                        i.Address_execution = address;
                                    else
                                        Debug.WriteLine("Couldn't parse address " + value);
                                    break;
                                case "tid": // Thread ID
                                    uint thread_id;
                                    if (UInt32.TryParse(value, out thread_id))
                                        i.Thread_id = thread_id;
                                    else
                                        Debug.WriteLine("Couldn't parse TID " + value);
                                    break;
                                case "tme": // Time
                                    uint time;
                                    if (UInt32.TryParse(value, out time))
                                        i.Time = time;
                                    else
                                        Debug.WriteLine("Couldn't parse time " + value);
                                    break;
                                case "dth": // Depth
                                    uint depth;
                                    if (UInt32.TryParse(value, out depth))
                                    {
                                        i.Depth = depth;
                                        i.Desired_color = ((SolidColorBrush)(color_palette.GetValue(depth % color_palette.GetLength(0))));
                                    }
                                    else
                                        Debug.WriteLine("Couldn't parse depth " + value);
                                    break;
                                case "cnt": // Count
                                    uint count;
                                    if (UInt32.TryParse(value, out count))
                                        i.Instruction_count = count;
                                    else
                                        Debug.WriteLine("Couldn't parse count " + value);
                                    break;
                                default:
                                    Debug.WriteLine("Key " + key + " not handled.");
                                    break;
                            }
                        }

                        // Add the instruction to the model
                        global_instruction_list.Model.Add(i);
                    }
                }
            }
        }
    }
}