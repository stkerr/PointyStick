using PointyStickBlend.ViewModels;
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
            UInt32 filter_instruction_low = new UInt32();
            filter_instruction_low = 0x40000000;

            Application.Current.Resources["filter_instruction_low"] = filter_instruction_low;
            filter_instruction_low = (UInt32)this.FindResource("filter_instruction_low");

        }
    }
}
