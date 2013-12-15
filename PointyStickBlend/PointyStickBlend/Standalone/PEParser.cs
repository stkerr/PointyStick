using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Standalone
{

    struct MZ_header
    {
        UInt16 signature;

        UInt16 extra_bytes;

        UInt16 pages;
        UInt16 relocation_items;
        UInt16 header_size;
        UInt16 min_allocation;
        UInt16 max_allocation;
        UInt16 initial_ss;
        UInt16 initial_sp;
        UInt16 checksum;
        UInt16 initial_ip;
        UInt16 initial_cs;
        UInt16 relocation_table;
        UInt16 overlay;
        UInt16 overlay_information;

        UInt32 pe_offset;
        UInt32 pe_segment;
    } 

    struct COFF_header
    {
        UInt16 machine;
        UInt16 number_of_sections;
        UInt16 time_date_stamp;
        UInt16 pointer_to_symbol_table;
        UInt16 number_of_symbols;
        UInt16 size_of_optional_header;
        UInt16 characteristics;
    }

    unsafe public class PEParser
    {
        FileStream file;
        long file_length;

        public PEParser(string filepath)
        {
            Debug.WriteLine("Parsing library " + filepath);
            file = File.Open(filepath, FileMode.Open, FileAccess.Read);
            file_length = file.Length;

            byte[] library_buffer = new byte[file_length];
            file.Read(library_buffer, 0, (int)file_length);


            int pe_offset = BitConverter.ToInt32(library_buffer, 0x3C);
            int value = (int)(library_buffer[pe_offset]);
            
            Debug.WriteLine("PE Offset: " + pe_offset);
            Debug.WriteLine("Value: " + value);
            Debug.WriteLine("Value: " + library_buffer[0x3C]);
            Debug.WriteLine("Value: " + library_buffer[0x3C+1]);
            Debug.WriteLine("Value: " + library_buffer[0x3C+2]);
            Debug.WriteLine("Value: " + library_buffer[0x3C+3]);

               
            file.Close();
        }


    }
}
