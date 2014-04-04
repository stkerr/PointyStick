using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Management;

namespace PointyStickBlend
{
    /// <summary>
    /// Interaction logic for RunWindow.xaml
    /// </summary>
    public partial class RunWindow : Window
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenEvent(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll")]
        static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        static extern bool ResetEvent(IntPtr hEvent);

        bool events_initialized = false;

        IntPtr event_monitoring = IntPtr.Zero;
        IntPtr event_snapshot = IntPtr.Zero;

        Process instrumented_process = new Process();

        public RunWindow()
        {
            InitializeComponent();
            
            /* Set up our system events */
            events_initialized = false;
            initialize_events();

            /* Start the status timer */
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(status_timer_tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        ~RunWindow()
        {
            clean_events();
        }

        private void clean_events()
        {
            CloseHandle(event_monitoring);
            CloseHandle(event_snapshot);
        }

        private void initialize_events()
        {
            if (events_initialized)
                return;

            uint EVENT_MODIFY_STATE = 0x0002;
            event_monitoring = OpenEvent(EVENT_MODIFY_STATE, false, "MONITORING");
            if(event_monitoring == IntPtr.Zero)
            {
                // NULL, so we need to make the event
                event_monitoring = CreateEvent(IntPtr.Zero, true, false, "MONITORING");
            }

            event_snapshot = OpenEvent(EVENT_MODIFY_STATE, false, "SNAPSHOT");    
            if(event_snapshot == IntPtr.Zero)
            {
                event_snapshot = CreateEvent(IntPtr.Zero, true, false, "SNAPSHOT");
            }

            events_initialized = true;
        }

        private void open_application(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if(dialog.ShowDialog() == true)
            {
                textbox_filename.Text = dialog.FileName;
            }
        }

        private void start_collection(object sender, RoutedEventArgs e)
        {
            string command_string = "-t PointyStickPinTool ";

            /*
             * Tracing Settings
             */
            if(tracing_checkbox.IsChecked == false)
            {
                command_string += "-disable_instruction_trace ";
            }
            else
            {
                if(tracing_fromstart.IsChecked == true)
                {
                    enable_tracing();
                }
            }

            /*
             * Region Monitoring Settings
             */
            if(regionmonitoring_checkbox.IsChecked == true)
            {
                command_string += "-enable_region_monitoring ";
                UInt32 region_start;
                UInt32 region_end;
                if(!UInt32.TryParse(regionmonitoring_start.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out region_start))
                    throw new Exception("Region startaddresses not valid. Hex format, no prefix.");
                
                if(!UInt32.TryParse(regionmonitoring_end.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out region_end))
                    throw new Exception("Region end addresses not valid. Hex format, no prefix.");

                command_string += "-region_start " + region_start + " ";
                command_string += "-region_end " + region_end + " ";
                command_string += "-region_name " + textbox_filename.Text + " ";

            }

            /*
             * Append the application name
             */
            command_string += " -- " + textbox_filename.Text;

            /*
             * Start the PIN tool
             */
            string pin_root;
            try
            {
                pin_root = System.Environment.GetEnvironmentVariable("PIN_ROOT");
            }
            catch(ArgumentNullException ex)
            {
                throw new ArgumentNullException("PIN_ROOT environment variable not set! Please install PIN and set PIN_ROOT.");
            }

            try
            {
                if (pin_root.CompareTo("") == 0)
                {
                    MessageBox.Show("Please define the environment variable PIN_ROOT and restart the application.");
                }
                else
                {
                    instrumented_process.StartInfo.FileName = pin_root + "\\pin.exe";
                    instrumented_process.StartInfo.Arguments = command_string;
                    instrumented_process.EnableRaisingEvents = true;
                    instrumented_process.Exited += new EventHandler(instrumented_exited);
                    if(!instrumented_process.Start())
                    {
                        throw new Exception("Couldn't start process");
                    }

                    if (instrumented_process == null)
                    {
                        throw new Exception("Couldn't start pin.exe");
                    }
                    
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void instrumented_exited(object o, EventArgs e)
        {
            switch (instrumented_process.ExitCode)
            {
                case -1:
                    MessageBox.Show("Couldn't locate or start target application.\nIs the tool 32-bit and the applicaton 64-bit or vice versa?\nCheck pin.log for additional information.");
                    break;
                case 0:
                    break;
                case 1:
                    MessageBox.Show("Invalid PIN tool parameters.");
                    break;
                default:
                    MessageBox.Show("Unhandled PIN error code of " + instrumented_process.ExitCode + ".");
                    break;
            }
            
        }


        private static void KillProcessAndChildren(int pid)
        {
            /*
             * From http://stackoverflow.com/questions/5901679/kill-process-tree-programatically-in-c-sharp
             */
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private void terminate_application(object o, RoutedEventArgs e)
        {
            if (instrumented_process == null)
                return;
            instrumented_process.Exited -= new EventHandler(instrumented_exited);
            try
            {

                KillProcessAndChildren(instrumented_process.Id);
            }
            catch(InvalidOperationException ex)
            { }
        }

        private bool enable_tracing()
        {
            // set the monitoring event
            SetEvent(event_monitoring);

            return true;
        }

        private bool disable_tracing()
        {
            // disable the monitoring event
            ResetEvent(event_monitoring);

            return true;
        }

        private void enable_tracing_button_Click(object sender, RoutedEventArgs e)
        {
            enable_tracing();
        }

        private void disable_tracing_button_Click(object sender, RoutedEventArgs e)
        {
            disable_tracing();
        }

        private void status_timer_tick(object sender, EventArgs e)
        {
            int WAIT_OBJECT_0 = 0;

            uint status = WaitForSingleObject(event_monitoring, 0);

            if(status == WAIT_OBJECT_0) // signaled
            {
                tracing_status_label.Content = "Enabled";
            }
            else
            {
                tracing_status_label.Content = "Disabled";
            }

            status = WaitForSingleObject(event_snapshot, 0);
            if(status == WAIT_OBJECT_0)
            {
                snapshot_status_label.Content = "Enabled";
            }
            else
            {
                snapshot_status_label.Content = "Disabled";
            }
        }
    }
}
