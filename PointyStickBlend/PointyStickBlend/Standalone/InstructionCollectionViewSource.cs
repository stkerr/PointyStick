using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PointyStickBlend.Standalone
{
    class InstructionCollectionViewSource : CollectionViewSource, INotifyPropertyChanged
    {
        public InstructionCollectionViewSource()
            : base()
        {
          //  this.Filter += new FilterEventHandler(CustomFilter);
        }

        private void CustomFilter(object sender, FilterEventArgs args)
        {
            args.Accepted = false;
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
