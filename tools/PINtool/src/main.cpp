#include "pin.H"
#include <stdio.h>
#include "PointyStick.h"

KNOB<BOOL> KnobLibraryTracing(KNOB_MODE_WRITEONCE, "pintool", "library_trace", "false", "Enable logging of library loads.");

int Usage()
{
    fprintf(stderr, "%s\n", KNOB_BASE::StringKnobSummary().c_str());
    return -1;
}

int main(int argc, char** argv)
{
    PIN_InitSymbols();

    if(PIN_Init(argc,argv))
    {
        return Usage();
    }
    
    if(KnobLibraryTracing.Value()) 
    {
        printf("Enabling library tracing.\n");
        /* Enable library tracing. */
        IMG_AddInstrumentFunction(library_loaded_function, 0);
        IMG_AddUnloadFunction(library_unloaded_function, 0);
    }

    // Start up the program to investigate.
    PIN_StartProgram();

    return 0;
}
