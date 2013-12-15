#include "stick_pe.h"
#include <Windows.h>

typedef struct _mz_header
{
    char signature[2];
    char _padding[58];
    int pe_offset;
} mz_header;

void get_exports(std::string filename)
{
    
    FILE* fp = fopen(filename.c_str(), "rb");

    fseek(fp, 0, SEEK_END);
    long size = ftell(fp);

    fseek(fp, 0, SEEK_SET);

    char* buffer = (char*)malloc(sizeof(char) * size);
    fread(buffer, 1, size, fp);

    fclose(fp);

    long pe_offset = ((mz_header*)buffer)->pe_offset;

    
    IMAGE_NT_HEADERS *pe_header = (IMAGE_NT_HEADERS*)(buffer + pe_offset);

    IMAGE_FILE_HEADER *file_header = &pe_header->FileHeader;
    IMAGE_OPTIONAL_HEADER* optional_header = &pe_header->OptionalHeader;

    if (!optional_header->NumberOfRvaAndSizes)
    {
        return;
    }

    
    IMAGE_DATA_DIRECTORY export_directory = (optional_header->DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT]);

    if (export_directory.VirtualAddress == 0 || export_directory.Size == 0)
    {
        return;
    }
    
    IMAGE_SECTION_HEADER* exports_section_header = (IMAGE_SECTION_HEADER*)((char*)optional_header + file_header->SizeOfOptionalHeader);
    for (int i = 0; i < file_header->NumberOfSections; i++)
    {
        if ((exports_section_header + i)->VirtualAddress < export_directory.VirtualAddress &&
            export_directory.VirtualAddress < (exports_section_header + i)->VirtualAddress + (exports_section_header + i)->Misc.VirtualSize)
        {
            exports_section_header = exports_section_header + i;
        }
    }

    IMAGE_EXPORT_DIRECTORY* exports = (IMAGE_EXPORT_DIRECTORY*)(buffer + export_directory.VirtualAddress + exports_section_header->PointerToRawData - exports_section_header->VirtualAddress);
    
    if (exports->NumberOfFunctions == 0)
    {
        return;
    }

    if (exports->NumberOfNames == 0)
    {
        std::cout << "No names, but there ARE " << exports->NumberOfFunctions << " exports." << std::endl;
        return;
    }

    
    int* functions = (int*)(buffer + exports_section_header->PointerToRawData - exports_section_header->VirtualAddress + exports->AddressOfFunctions);
    short* ordinals = (short*)(buffer + exports_section_header->PointerToRawData - exports_section_header->VirtualAddress + exports->AddressOfNameOrdinals);
    int * names = (int*)(buffer + exports_section_header->PointerToRawData - exports_section_header->VirtualAddress + exports->AddressOfNames);

    for (int i = 0; i < exports->NumberOfNames; i++)
    {
        printf("%s ", buffer + exports_section_header->PointerToRawData - exports_section_header->VirtualAddress + names[i]);
        short ordinal_number = ordinals[i];
        printf(" 0x%08X\n", functions[ordinal_number]);
    }
    
    std::cout << filename << " done" << std::endl;
    printf("0x%x\n", optional_header->ImageBase);

    free(buffer);
}