using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class LibraryModel : ObservableCollection<Library>, INotifyPropertyChanged
    {
        public LibraryModel() : base() { }
        public LibraryModel(List<Library> list) : base(list) { UpdateDataWithDiskFiles(); }
        public LibraryModel(IEnumerable<Library> collection) : base(collection) { UpdateDataWithDiskFiles();  }

        public bool UpdateDataWithDiskFiles()
        {
            return true;
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
