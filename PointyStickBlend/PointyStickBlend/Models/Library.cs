﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class Library
    {
        UInt32 address_execution;
        public UInt32 Address_execution
        {
            get { return address_execution; }
            set { address_execution = value; }
        }

        UInt32 address_disk;
        public UInt32 Address_disk
        {
            get { return address_disk; }
            set { address_disk = value; }
        }

        string library_name;
        public string Library_name
        {
            get { return library_name; }
            set { library_name = value; }
        }

        IDictionary<string, UInt32> exports;
        public IDictionary<string, UInt32> Exports
        {
            get { return exports; }
            set { exports = value; }
        }

        UInt32 size_execution;
        UInt32 size_disk;

        public Library()
        {
            exports = new Dictionary<string, UInt32>();
        }
    }

    class LibraryFactory
    {
        public Library GetLibraryFromDisk(string filepath)
        {
            Library l = new Library();
            return l;
        }

        public Library GetLibraryFromLogLine(string library_line)
        {
            Library l = new Library();
            return l;
        }

    }
}
