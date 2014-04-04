using PointyStickBlend.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        public FilterWindow()
        {
            InitializeComponent();

            /* Restore any existing filter state */
            low_instruction_enabled.IsChecked = (bool)Application.Current.Resources["filter_instruction_low_enabled"];
            if ((bool)low_instruction_enabled.IsChecked)
            {
                uint filter_low;
                UInt32.TryParse(Application.Current.Resources["filter_instruction_low"].ToString(), out filter_low);
                low_instruction.Text = filter_low.ToString("X");
            }
            high_instruction_enabled.IsChecked = (bool)Application.Current.Resources["filter_instruction_high_enabled"];
            if ((bool)high_instruction_enabled.IsChecked)
            {
                uint filter_high;
                UInt32.TryParse(Application.Current.Resources["filter_instruction_high"].ToString(), out filter_high);
                high_instruction.Text = filter_high.ToString("X");
            }
            low_depth_enabled.IsChecked = (bool)Application.Current.Resources["filter_depth_low_enabled"];
            if ((bool)low_depth_enabled.IsChecked)
                low_depth.Text = (Application.Current.Resources["filter_depth_low"]).ToString();
            high_depth_enabled.IsChecked = (bool)Application.Current.Resources["filter_depth_high_enabled"];
            if ((bool)high_depth_enabled.IsChecked)
                high_depth.Text = (Application.Current.Resources["filter_depth_high"]).ToString();
            library_name_enabled.IsChecked = (bool)Application.Current.Resources["filter_libraries_included_enabled"];
            if(library_name_enabled.IsChecked.HasValue && (bool)library_name_enabled.IsChecked)
            {
                InstructionViewModel ivm = (InstructionViewModel)this.FindResource("instruction_view_model");
                library_name.SelectedItems.Clear();
                foreach(string i in ivm.Library_names)
                {
                    library_name.SelectedItems.Add(i);
                }
            }
            thread_id_enabled.IsChecked = (bool)Application.Current.Resources["filter_threads_included_enabled"];
            if ((bool)thread_id_enabled.IsChecked)
            {
                InstructionViewModel ivm = (InstructionViewModel)this.FindResource("instruction_view_model");
                thread_id.SelectedItems.Clear();
                foreach (uint i in ivm.Thread_ids)
                {
                    thread_id.SelectedItems.Add(i);
                }
            }
            system_call_enabled.IsChecked = (bool)Application.Current.Resources["filter_system_calls_enabled"];


        }

        private void apply_filter_button(object sender, RoutedEventArgs e)
        {
            InstructionViewModel ivm = (InstructionViewModel)this.FindResource("instruction_view_model");

            UInt32 filter_instruction_low = new UInt32();
            filter_instruction_low = 0x40000000;

            Application.Current.Resources["filter_instruction_low"] = filter_instruction_low;
            filter_instruction_low = (UInt32)this.FindResource("filter_instruction_low");

            Application.Current.Resources["filter_instruction_low_enabled"] = low_instruction_enabled.IsChecked;
            Application.Current.Resources["filter_instruction_high_enabled"] = high_instruction_enabled.IsChecked;
            Application.Current.Resources["filter_depth_low_enabled"] = low_depth_enabled.IsChecked;
            Application.Current.Resources["filter_depth_high_enabled"] = high_depth_enabled.IsChecked;
            Application.Current.Resources["filter_libraries_included_enabled"] = library_name_enabled.IsChecked;
            Application.Current.Resources["filter_threads_included_enabled"] = thread_id_enabled.IsChecked;
            Application.Current.Resources["filter_system_calls_enabled"] = system_call_enabled.IsChecked;

            UInt32 result_uint;

            if ((bool)low_instruction_enabled.IsChecked)
            {
                if (UInt32.TryParse(low_instruction.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result_uint))
                    Application.Current.Resources["filter_instruction_low"] = result_uint;
                else
                {
                    MessageBox.Show("Couldn't parse low disk instruction cutoff! Hex, no leading 0x");
                    return;
                }
            }

            if ((bool)high_instruction_enabled.IsChecked)
            {
                if (UInt32.TryParse(high_instruction.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result_uint))
                    Application.Current.Resources["filter_instruction_high"] = result_uint;
                else
                {
                    MessageBox.Show("Couldn't parse high disk instruction cutoff! Hex, no leading 0x");
                    return;
                }
            }

            Int32 result_int;

            if ((bool)low_depth_enabled.IsChecked)
            {
                if (Int32.TryParse(low_depth.Text, out result_int))
                    Application.Current.Resources["filter_depth_low"] = result_int;
                else
                {
                    MessageBox.Show("Couldn't parse low filter depth! Decimal format only. ");
                    return;
                }
            }

            if ((bool)high_depth_enabled.IsChecked)
            {
                if (Int32.TryParse(high_depth.Text, out result_int))
                    Application.Current.Resources["filter_depth_high"] = result_int;
                else
                {
                    MessageBox.Show("Couldn't parse high filter depth! Decimal format only. ");
                    return;
                }
            }

            if ((bool)library_name_enabled.IsChecked)
            {
                ivm.Library_names.Clear();
                foreach (string s in library_name.SelectedItems)
                {
                    ivm.Library_names.Add(s);
                }
            }

            if ((bool)thread_id_enabled.IsChecked)
            {
                ivm.Thread_ids.Clear();
                foreach (uint i in thread_id.SelectedItems)
                {
                    ivm.Thread_ids.Add(i);

                }
            }

            this.Close();
        }
    }
}
