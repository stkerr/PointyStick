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
        }
    }
}
