#include "pin.H"
#include <stdio.h>
#include "PointyStick.h"

KNOB<BOOL> KnobDisableLibraryTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_library_trace", "false", "Disable logging of library loads.");
KNOB<BOOL> KnobDisableInstructionTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_instruction_trace", "false", "Disable logging of instructions.");
KNOB<BOOL> KnobEnableInitialMonitoring(KNOB_MODE_OVERWRITE, "pintool", "enable_trace_on_start", "false", "Enable instruction logging from program beginning.");

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
    
    if(!KnobDisableLibraryTracing.Value())
    {
        printf("Enabling library tracing.\n");
        /* Enable library tracing. */
        IMG_AddInstrumentFunction(library_loaded_function, 0);
        IMG_AddUnloadFunction(library_unloaded_function, 0);
    }

    if(!KnobDisableInstructionTracing.Value())
    {
        printf("Enabling instruction tracing.\n");
        INS_AddInstrumentFunction(instruction_trace, 0);
    }
    
    if(KnobEnableInitialMonitoring.Value())
    {
        printf("Enabling tracing on initialization.\n");
        event_monitoring_set(true);
    }
    
    // Start up the program to investigate.
    PIN_StartProgram();

    return 0;
}
