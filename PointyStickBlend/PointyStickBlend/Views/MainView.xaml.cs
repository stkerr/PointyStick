using PointyStickBlend.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PointyStickBlend
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        private void button_load_tracefile_Click(object sender, RoutedEventArgs e)
        {

            InstructionViewModel global_instruction_list = (InstructionViewModel)this.FindResource("instruction_view_model");

            global_instruction_list.Model.Add(new Instruction());

            
            
        }

        private void button_set_library_tracefile_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("button_set_library_tracefile_Click() not implemented!");
        }

        private void button_set_instruction_tracefile_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("button_set_instruction_tracefile_Click() not implemented!");
        }
    }
}