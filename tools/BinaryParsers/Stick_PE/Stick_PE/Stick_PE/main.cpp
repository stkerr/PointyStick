// Stick_PE.cpp : Defines the entry point for the console application.
//

#include <Windows.h>

#include <stdio.h>
#include <stdlib.h>
#include <fstream>
#include <iostream>
#include <string>
#include <sstream>

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

    while (!input.eof())
    {
        // get the next line of the file
        std::getline(input, line);

        // split on the token '|'
        int first_pipe_index = line.find('|');

        std::string first_token(line.substr(0, first_pipe_index));

        std::string library_name(first_token.substr(first_token.find(":")+1));
        
        // trim the string
        
        trim(library_name);

        if (library_name.length() == 0)
            continue;

        get_exports(library_name);
    }


    input.close();

    return 0;
}

