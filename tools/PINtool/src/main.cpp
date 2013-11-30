#include "pin.H"
#include <stdio.h>
#include "PointyStick.h"

KNOB<BOOL> KnobDisableLibraryTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_library_trace", "false", "Disable logging of library loads.");
KNOB<BOOL> KnobDisableInstructionTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_instruction_trace", "false", "Disable logging of instructions.");
KNOB<BOOL> KnobEnableInitialMonitoring(KNOB_MODE_OVERWRITE, "pintool", "enable_trace_on_start", "false", "Enable instruction logging from program beginning.");
KNOB<BOOL> KnobEnableMonitoring(KNOB_MODE_OVERWRITE, "pintool", "enable_region_monitoring", "false", "Enable a memory region to monitor for memory writes.\n");
KNOB<int> KnobRegionStart(KNOB_MODE_OVERWRITE, "pintool", "region_start", "0", "Start of the region to monitor for memory writes.");
KNOB<int> KnobRegionEnd(KNOB_MODE_OVERWRITE, "pintool", "region_end", "0", "End of the region to monitor for memory writes.");

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
    
    if(KnobEnableMonitoring.Value())
    {
        int start = KnobRegionStart.Value();
        int end = KnobRegionEnd.Value();
        
        printf("0x%x 0x%x\n", start, end);
    }
    
    // Set up the events
    initialize_events();
    
    // Start up the program to investigate.
    PIN_StartProgram();

    return 0;
}
