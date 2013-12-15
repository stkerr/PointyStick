using PointyStickBlend.Models;
using PointyStickBlend.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        private void load_library_support_tracefile(object sender, RoutedEventArgs e)
        {
            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");

            // Create a new library object for this line
            Library current_library = null;

            string filename = "e:\\Users\\Whistlepig\\Documents\\Code\\PointyStickLibrarySupportTrace.txt";
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
            
                        string line = sr.ReadLine().Trim();
                        if(current_library == null)
                        {
                            // we don't have a library we're working on, so this is a library name
                            foreach(Library l in global_library_list.Model)
                            {
                                if(l.Library_name.CompareTo(line) == 0)
                                {
                                    current_library = l;
                                    break;
                                }
                            }

                            continue;
                        }

                        if(line.CompareTo("") == 0)
                        {
                            // blank line, which means a new library
                            current_library = null;
                            continue;
                        }

                        string[] kvpairs = line.Split('|');
                        
                        if(kvpairs.Length != 2)
                        {
                            Debug.WriteLine("Line " + line + " causing problems.");
                        }

                        string type = kvpairs[0];
                        string data = kvpairs[1];

                        type = type.Trim();

                        string key = "";
                        string value = data;
                        if(data.Contains(":"))
                        {
                            string[] datakv = data.Split(':');
                            key = datakv[0];
                            value = datakv[1];
                        }

                        uint base_disk = 0;
                        uint export_address = 0;
                        string export_name = "";

                        switch (type)
                        {
                            case "Base": // Library base (disk based)
                                uint base_disk_parsed;
                                if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out base_disk_parsed))
                                    current_library.Address_disk = base_disk_parsed;
                                else
                                    Debug.WriteLine("Couldn't parse base address " + value);
                                break;
                            case "Export": // Offset from the image base
                                export_name = key;
                                uint export_address_parsed;
                                if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out export_address_parsed))
                                    current_library.Exports.Add(key, export_address_parsed);
                                else
                                    Debug.WriteLine("Couldn't parse export address " + value);
                                break;
                            default:
                                Debug.WriteLine("Key " + key + " not handled.");
                                break;
                        }


                        // Add the library to the model
                        //global_library_list.Model.Add(l);
                        
                    }
                }
            }
        }

        private void load_library_tracefile(object sender, RoutedEventArgs e)
        {
            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");

            string filename = "e:\\Users\\Whistlepig\\Documents\\Code\\PointyStickLibraryTrace.txt";
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        // Create a new library object for this line
                        Library l = new Library();

                        string line = sr.ReadLine().Trim();

                        string[] kvpairs = line.Split('|');

                        uint start_address = 0;
                        uint end_address = 0;

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

                            uint entry_address = 0;
                            
                            switch (key)
                            {
                                case "Library Name": // Library name
                                    l.Library_name = value;
                                    break;
                                case "Start Address": // Address loaded to, may be different than on disk, due to ASLR
                                    uint start_address_parsed;
                                    if (UInt32.TryParse(value, out start_address_parsed))
                                         l.Address_execution = start_address_parsed;
                                    else
                                        Debug.WriteLine("Couldn't parse start address " + value);
                                    break;
                                case "End Address": // Address loaded to, may be different than on disk, due to ASLR
                                    uint end_address_parsed;
                                    if (UInt32.TryParse(value, out end_address_parsed))
                                        end_address = end_address_parsed;
                                    else
                                        Debug.WriteLine("Couldn't parse end address " + value);
                                    break;
                                case "Entry Address": // Address loaded to, may be different than on disk, due to ASLR
                                    uint entry_address_parsed;
                                    if (UInt32.TryParse(value, out entry_address_parsed))
                                        entry_address = entry_address_parsed;
                                    else
                                        Debug.WriteLine("Couldn't parse entry address " + value);
                                    break;
                                default:
                                    Debug.WriteLine("Key " + key + " not handled.");
                                    break;
                            }
                        }

                        l.Size_execution = end_address - start_address;

                        // Add the library to the model
                        global_library_list.Model.Add(l);
                    }
                }
            }

            load_library_support_tracefile(sender, e);

        }

        private void load_instruction_tracefile(object sender, RoutedEventArgs e)
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