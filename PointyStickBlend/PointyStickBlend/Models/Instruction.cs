using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class Instruction
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

        UInt32 depth;
        public UInt32 Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        UInt32 time;
        public UInt32 Time
        {
            get { return time; }
            set { time = value; }
        }

        UInt32 instruction_count;
        public UInt32 Instruction_count
        {
            get { return instruction_count; }
            set { instruction_count = value; }
        }

        string library_name;
        public string Library_name
        {
            get { return library_name; }
            set { library_name = value; }
        }
    }
}
