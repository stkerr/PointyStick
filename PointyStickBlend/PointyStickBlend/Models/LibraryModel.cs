using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class LibraryModel : ObservableCollection<Library>
    {
        public LibraryModel() : base() { }
        public LibraryModel(List<Library> list) : base(list) { UpdateDataWithDiskFiles(); }
        public LibraryModel(IEnumerable<Library> collection) : base(collection) { UpdateDataWithDiskFiles();  }

        public bool UpdateDataWithDiskFiles()
        {
            return true;
        }
    }   
}
