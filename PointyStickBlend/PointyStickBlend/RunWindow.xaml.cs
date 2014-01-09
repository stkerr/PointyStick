using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace PointyStickBlend
{
    /// <summary>
    /// Interaction logic for RunWindow.xaml
    /// </summary>
    public partial class RunWindow : Window
    {
        public RunWindow()
        {
            InitializeComponent();
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
                command_string += "-notrace ";
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

            /*
             * Append the application name
             */
            command_string += " -- " + textbox_filename.Text;

            /*
             * Start the PIN tool
             */
            System.Diagnostics.Process.Start("pin", command_string);
        }

        bool enable_tracing()
        {
            return true;
        }
    }
}
