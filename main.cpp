// Stick_PE.cpp : Defines the entry point for the console application.
//

#include <Windows.h>

#include <stdio.h>
#include <stdlib.h>
#include <fstream>
#include <iostream>
#include <string>
#include <sstream>
#include <list>

#include "stick_pe.h"

int main(int argc, char* argv[])
{
    if (argc != 2)
    {
        fprintf(stderr, "Usage:");
        fprintf(stderr, "\t%s <library_trace_file>\n", argv[0]);
        return -1;
    }

    std::string line;
    std::ifstream input;
    input.open(argv[1]);

    std::list<std::string> library_names;

    while (input.is_open() && !input.eof())
    {
        // get the next line of the file
        std::getline(input, line);

        std::istringstream line_stream(line);
        std::string token;

        std::string mode;
        std::string library_name;

        if (std::getline(line_stream, token, '|'))
        {
            mode = token;
            trim(mode);
        }

        // check this is a library format line
        if (mode.compare("[LIB]") != 0)
            continue;

        

        if (std::getline(line_stream, token, '|'))
        {
            // ignore "Unloading" lines
            if (token.find("Unloading") != std::string::npos)
                continue;

            library_name = token;
            library_name = library_name.substr(library_name.find(':')+1, std::string::npos);
            trim(library_name);
        }

        if (library_name.length() == 0)
            continue;

        // if we have already processed this library, don't do it again.
        std::list<std::string>::iterator it;
        bool already_processed = false;
        for (it = library_names.begin(); it != library_names.end(); it++)
        {
            if ((*it).compare(library_name) == 0)
            {
                already_processed = true;
                break;
            }
        }
        if (already_processed)
        {
            std::cout << "Already processed: " << library_name << std::endl;
            break;
        }
        else
        {
            std::cout << "Not processed: " << library_name << std::endl;
            library_names.push_back(library_name);
        }

        // print out this libraries header
        std::cout << library_name << std::endl;

        // print out this libraries information
        get_exports(library_name);

        // print out this libraries footer
        std::cout << std::endl;
    }


    input.close();

    return 0;
}

