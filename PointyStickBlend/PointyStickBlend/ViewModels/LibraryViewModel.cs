using PointyStickBlend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.ViewModels
{
    public class LibraryViewModel : INotifyPropertyChanged
    {

        private LibraryModel model;

        public LibraryModel Model
        {
            get { return model; }
            set { model = value; }
        }

        public LibraryViewModel()
        {
            model = new LibraryModel();
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
