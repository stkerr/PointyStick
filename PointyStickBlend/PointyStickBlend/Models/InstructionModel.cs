using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class InstructionModel : ObservableCollection<Instruction>, INotifyPropertyChanged
    {
        public InstructionModel() : base() { }
        public InstructionModel(List<Instruction> list) : base(list) { }
        public InstructionModel(IEnumerable<Instruction> collection) : base(collection) { }

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
