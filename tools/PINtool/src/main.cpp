#include "pin.H"
#include <stdio.h>
#include "PointyStick.h"

KNOB<BOOL> KnobDisableLibraryTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_library_trace", "false", "Disable logging of library loads.");
KNOB<BOOL> KnobDisableInstructionTracing(KNOB_MODE_OVERWRITE, "pintool", "disable_instruction_trace", "false", "Disable logging of instructions.");
KNOB<BOOL> KnobEnableInitialMonitoring(KNOB_MODE_OVERWRITE, "pintool", "enable_trace_on_start", "false", "Enable instruction logging from program beginning.");
KNOB<BOOL> KnobEnableMonitoring(KNOB_MODE_OVERWRITE, "pintool", "enable_region_monitoring", "false", "Enable a memory region to monitor for memory writes.\n");
KNOB<int> KnobRegionStart(KNOB_MODE_OVERWRITE, "pintool", "region_start", "0", "Start of the region to monitor for memory writes.");
KNOB<int> KnobRegionEnd(KNOB_MODE_OVERWRITE, "pintool", "region_end", "0", "End of the region to monitor for memory writes.");
KNOB<string> KnobRegionName(KNOB_MODE_OVERWRITE,"pintool","region_name","","Name of the library the region resides in.");

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
    
    // Set up the events
    initialize_events();
    
    if(!KnobDisableLibraryTracing.Value())
    {
        printf("Enabling library tracing.\n");
        /* Enable library tracing. */
        IMG_AddInstrumentFunction(library_loaded_function, 0);
        IMG_AddUnloadFunction(library_unloaded_function, 0);
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
        const char* library_name = KnobRegionName.Value().c_str();

        printf("Library name: %s\n", library_name);
        printf("0x%x 0x%x\n", start, end);

        region_t *r = (region_t*)malloc(sizeof(region_t));
        r->start = (void*)start;
        r->end = (void*)end;
        strncpy(r->library_name, library_name, 260);
        printf("Added %s\n", r->library_name);
        add_region_to_monitoring(r);

        event_snapshot_set(true);

        region_monitoring_enabled = true;
    }
    
    // Add instrumentation. It handles both regions and instruction monitoring so must be enabled
    INS_AddInstrumentFunction(instruction_trace, 0);

    // Start up the program to investigate.
    PIN_StartProgram();

    return 0;
}
