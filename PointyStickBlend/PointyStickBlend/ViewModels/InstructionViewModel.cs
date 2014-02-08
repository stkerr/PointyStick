using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using PointyStickBlend.Models;
using System.Diagnostics;

namespace PointyStickBlend.ViewModels
{
    public class InstructionViewModel : INotifyPropertyChanged
    {

        Predicate<object> pi = (object i) => { return true; };

        public Predicate<object> Filter
        {
            get { return pi; }
            set { pi = value; }
        }

        private InstructionModel model;
        public InstructionModel Model
        {
            get { return model; }
            set { model = value; }
        }

        
        public InstructionViewModel()
        {
            Debug.WriteLine("Constructing InstructionViewModel.");
            IList<Instruction> list = new List<Instruction>();
            model = new InstructionModel(list);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }

}