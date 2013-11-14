using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

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

        UInt32 thread_id;
        public UInt32 Thread_id
        {
            get { return thread_id; }
            set { thread_id = value; }
        }

        string library_name;
        public string Library_name
        {
            get { return library_name; }
            set { library_name = value; }
        }

        Brush desired_color;
        public Brush Desired_color
        {
            get { return desired_color; }
            set { desired_color = value; }
        }

        string system_call_name;

        public string System_call_name
        {
            get { return system_call_name; }
            set { system_call_name = value; }
        }
    }
}
