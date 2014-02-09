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
using System.Windows.Data;

namespace PointyStickBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InstructionViewModel instruction_view_model;

        string logfile_name = "pintool.log";
        string supportfile_name = "support.log";

        public MainWindow()
        {
            this.InitializeComponent();

            // Insert code required on object creation below this point.
            instruction_view_model = (InstructionViewModel)this.FindResource("instruction_view_model");
        }

        private void exit_application(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void load_library_support_tracefile()
        {
            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");

            // Create a new library object for this line
            Library current_library = null;

            try
            {
                using (FileStream fs = new FileStream(supportfile_name, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {

                            string line = sr.ReadLine().Trim();
                            if (current_library == null)
                            {
                                // we don't have a library we're working on, so this is a library name
                                foreach (Library l in global_library_list.Model)
                                {
                                    if (l.Library_name.CompareTo(line) == 0)
                                    {
                                        current_library = l;
                                        break;
                                    }
                                }

                                continue;
                            }

                            if (line.CompareTo("") == 0)
                            {
                                // blank line, which means a new library
                                current_library = null;
                                continue;
                            }

                            string[] kvpairs = line.Split('|');

                            if (kvpairs.Length != 2)
                            {
                                Debug.WriteLine("Line " + line + " causing problems.");
                            }

                            string type = kvpairs[0];
                            string data = kvpairs[1];

                            type = type.Trim();

                            string key = "";
                            string value = data;
                            if (data.Contains(":"))
                            {
                                string[] datakv = data.Split(':');
                                key = datakv[0];
                                value = datakv[1];
                            }

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
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine("Could not load library trace support file.");
            }
        }

        private void load_library_tracefile()
        {
            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");
            
            // Clear the model
            global_library_list.Model.Clear();

            try
            {
                using (FileStream fs = new FileStream(logfile_name, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            // Create a new library object for this line
                            Library l = new Library();

                            string line = sr.ReadLine().Trim();

                            // make sure this is a library line
                            if(line.Contains("[LIB]") == false)
                                continue;
                             
                            string[] kvpairs = line.Split('|');

                            uint start_address = 0;
                            uint end_address = 0;

                            foreach (string kv in kvpairs)
                            {
                                if (kv == "")
                                    continue;

                                // Split on the first colon
                                string[] members = kv.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                                if(members.Length == 1 && kv.Contains("[LIB]"))
                                {
                                    // drop the header
                                    continue;
                                }
                                else if (members.Length != 2)
                                {
                                    Debug.WriteLine("Key-Value pair causing problems: " + kv);
                                    continue;
                                }
                                string key = members[0].Trim();
                                string value = members[1].Trim();

                                uint entry_address = 0;

                                switch (key)
                                {
                                    case "Name": // Library name
                                        l.Library_name = value;
                                        break;
                                    case "Strt": // Address loaded to, may be different than on disk, due to ASLR
                                        uint start_address_parsed;
                                        if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out start_address_parsed))
                                        {
                                            l.Address_execution = start_address_parsed;
                                            start_address = start_address_parsed;
                                        }
                                        else
                                            Debug.WriteLine("Couldn't parse start address " + value);
                                        break;
                                    case "High": // Address loaded to, may be different than on disk, due to ASLR
                                        uint end_address_parsed;
                                        if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out end_address_parsed))
                                            end_address = end_address_parsed;
                                        else
                                            Debug.WriteLine("Couldn't parse end address " + value);
                                        break;
                                    case "Enty": // Address loaded to, may be different than on disk, due to ASLR
                                        uint entry_address_parsed;
                                        if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out entry_address_parsed))
                                            entry_address = entry_address_parsed;
                                        else
                                            Debug.WriteLine("Couldn't parse entry address " + value);
                                        break;
                                    case "Low":
                                        break;
                                    case "Mapd":
                                        break;
                                    case "Type":
                                        break;
                                    case "Msg":
                                        // defaut informational, non-relevant data portion
                                        continue;
                                    default:
                                        Debug.WriteLine("Key " + key + " not handled.");
                                        break;
                                }
                            }

                            if (end_address == 0 || start_address == 0)
                            {
                                // Debug.WriteLine("Start or end address is empty.");
                                continue;
                            }
                            else
                                l.Size_execution = end_address - start_address;


                            // Add the library to the model
                            global_library_list.Model.Add(l);
                        }
                    }
                }
            }
            catch(FileNotFoundException ex)
            {
                Debug.WriteLine("Could not load libray file.");
            }
            

        }

        private void load_instruction_tracefile()
        {
            // Load the global color palette
            Array color_palette = (Array)this.FindResource("color_palette");

            // Load the global model

            // Clear the model
            instruction_view_model.Model.Clear();

            try
            {
                using (FileStream fs = new FileStream(logfile_name, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            // Create a new instruction object for this line
                            Instruction i = new Instruction();

                            string line = sr.ReadLine().Trim();

                            // make sure this is an instruction line
                            if (line.Contains("[INS]") == false)
                                continue;

                            string[] kvpairs = line.Split('|');

                            foreach (string kv in kvpairs)
                            {
                                if (kv == "")
                                    continue;

                                // Split on the first colon
                                string[] members = kv.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (members.Length == 1 && kv.Contains("[INS]"))
                                {
                                    // drop the header
                                    continue;
                                }
                                else if (members.Length != 2)
                                {
                                    Debug.WriteLine("Key-Value pair causing problems: " + kv);
                                    continue;
                                }
                                string key = members[0].Trim();
                                string value = members[1].Trim();

                                switch (key)
                                {
                                    case "adr": // Address (Execution)
                                        uint address;
                                        if (UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
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
                                        int depth;
                                        if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out depth))
                                        {
                                            i.Depth = depth;

                                            // this is needed in case depth is negative
                                            int index = (depth % color_palette.GetLength(0) + color_palette.GetLength(0)) % color_palette.GetLength(0);

                                            i.Desired_color = ((SolidColorBrush)(color_palette.GetValue(index)));
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
                            instruction_view_model.Model.Add(i);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine("Could not open instruction tracefile.");
            }
        }

        private void combine_instruction_and_library_data()
        {
            // Load the global models
            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");


            /*
             * Iterate through each instruction and set the library name for it.
             * 
             * This will probably be slow...
             */
            foreach (Instruction i in instruction_view_model.Model)
            {
                foreach (Library l in global_library_list.Model)
                {
                    if(l.Address_execution <= i.Address_execution && i.Address_execution < (l.Address_execution + l.Size_execution))
                    {
                        i.Library_name = l.Library_name;

                        foreach(KeyValuePair<string, uint> export in l.Exports)
                        {
                            if(i.Address_execution - l.Address_execution == export.Value)
                            {
                                i.System_call_name = export.Key;
                            }
                        }

                        i.Address_disk = i.Address_execution - l.Address_execution + l.Address_disk;

                        break;
                    }
                }
            }
        }

        private void process_pin_log(object sender, RoutedEventArgs e)
        {
            load_instruction_tracefile();
            load_library_tracefile();
            load_library_support_tracefile();
            combine_instruction_and_library_data();
        }

        private void start_collection(object sender, RoutedEventArgs e)
        {
            /*
             * This function is designed to start a new window which is responsible
             * for executing the PIN tool. The PIN tool execution will generate all
             * the necessary log files, so there is no need to pass any information
             * back to this window
             */
            RunWindow run = new RunWindow();
            run.Show();
        }

        private void apply_filters(object sender, RoutedEventArgs e)
        {
            

            LibraryViewModel global_library_list = (LibraryViewModel)this.FindResource("library_view_model");
            InstructionViewModel global_instruction_list = (InstructionViewModel)this.FindResource("instruction_view_model");

            FilterWindow filter = new FilterWindow();

            foreach(Library l in global_library_list.Model)
            {
                filter.library_name.Items.Add(l.Library_name);
            }

            foreach (Instruction i in global_instruction_list.Model)
            {
                if(!filter.thread_id.Items.Contains(i.Thread_id))
                    filter.thread_id.Items.Add(i.Thread_id);
            }
            
            filter.Show();
            filter.Closed += filter_Closed;

        }

        void filter_Closed(object sender, EventArgs e)
        {
            CollectionViewSource cvs1 = (CollectionViewSource)this.FindResource("cvs1");
            cvs1.Filter -= new FilterEventHandler(FilterRoutine);
            cvs1.Filter += new FilterEventHandler(FilterRoutine);
            
            
        }

        public void FilterRoutine(object sender, FilterEventArgs e)
        {
            e.Accepted = true;

            Instruction i = e.Item as Instruction;
            
            Boolean filter_instruction_low_enabled = (Boolean)this.FindResource("filter_instruction_low_enabled");
            UInt32 filter_instruction_low = (UInt32)this.FindResource("filter_instruction_low");
            if (filter_instruction_low_enabled && i.Address_disk < filter_instruction_low)
                e.Accepted = false;

            Boolean filter_instruction_high_enabled = (Boolean)this.FindResource("filter_instruction_high_enabled");
            UInt32 filter_instruction_high = (UInt32)this.FindResource("filter_instruction_high");
            if (filter_instruction_high_enabled && i.Address_disk > filter_instruction_high)
                e.Accepted = false;

            Boolean filter_depth_low_enabled = (Boolean)this.FindResource("filter_depth_low_enabled");
            Int32 filter_depth_low = (Int32)this.FindResource("filter_depth_low");
            if (filter_depth_low_enabled && i.Depth < filter_depth_low)
                e.Accepted = false;

            Boolean filter_depth_high_enabled = (Boolean)this.FindResource("filter_depth_high_enabled");
            Int32 filter_depth_high = (Int32)this.FindResource("filter_depth_high");
            if (filter_depth_high_enabled && i.Depth > filter_depth_high)
                e.Accepted = false;

            Boolean filter_libraries_included_enabled = (Boolean)this.FindResource("filter_libraries_included_enabled");
            if (filter_libraries_included_enabled && !instruction_view_model.Library_names.Contains(i.Library_name))
                e.Accepted = false;

            Boolean filter_threads_included_enabled = (Boolean)this.FindResource("filter_threads_included_enabled");
            if (filter_threads_included_enabled && !instruction_view_model.Thread_ids.Contains(i.Thread_id))
                e.Accepted = false;

            Boolean filter_system_calls_enabled = (Boolean)this.FindResource("filter_system_calls_enabled");
            if(filter_system_calls_enabled && i.System_call_name != null && i.System_call_name != "")
                e.Accepted = true;
             
        }
    }
}