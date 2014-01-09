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
        int first_pipe_index = line.find('|'); // first index is the type block
        int second_pipe_index = line.find('|', first_pipe_index+1); // second index is the library name
        if (second_pipe_index < 0) // no second index, so skip this line
            continue;

        std::string first_token(line.substr(0, first_pipe_index));
        std::string second_token(line.substr(first_pipe_index+1, second_pipe_index-first_pipe_index-1));

        std::string library_name(second_token.substr(second_token.find(":")+1));
        
        // trim the string
        trim(library_name);

        if (library_name.length() == 0)
            continue;

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

