#include "instruction_trace.h"
#include "PointyStick.h"
#include <sstream>

static int depth = 0;
static int instruction_count = 0;
static int snapshot_count = 0;

void instruction_trace(INS ins, void* arg)
{

    void* address = (void*)INS_Address(ins);

    if(event_monitoring_enabled())
    {
        // log out our current state
        std::ostringstream message;
        message << "[INS] | ";
        message << "adr : " << hex << address << dec << " | "; // hex address then back to decimal
        message << "tid :  " << PIN_GetTid() << " | "; // note, PIN_ThreadUid() might be more appropriate at some point in the future.
        message << "tme : " << 0 <<  " | ";
        message << "dth : " << depth << " | ";
        message << "cnt : " << instruction_count << " | ";
        
        message << "\n";
        LOG(message.str().c_str());

        // update our current state
        instruction_count++;
        
        // this records calls such as 'jmp rax', but does not properly handle returns from non-call instructions
        if(INS_IsProcedureCall(ins))
        {
            depth++;
        }
        else if(INS_IsRet(ins) || INS_IsSysret(ins))
        {
            depth--;
        }
    }

    if(region_monitoring_enabled)
    {

        // check if a snapshot was requested
        if(event_snapshot_enabled())
        {
            std::ostringstream message;
            message << "[INFO] | Took snapshot " << snapshot_count++;
            message << "\n";
            LOG(message.str().c_str());

            take_all_snapshots();
            event_snapshot_set(false);
        }

        // if we're writing memory, check for a region monitoring event
        if(INS_IsMemoryWrite(ins) == true)
        {

            // check each operand to find the changing address
            for(int i = 0; i < INS_OperandCount(ins); i++)
            {
                if(INS_MemoryOperandIsWritten(ins, i))
                {
                    INS_InsertCall(
                        ins, 
                        IPOINT_BEFORE, (AFUNPTR)checked_snapshot,
                        IARG_MEMORYOP_EA, i,
                        IARG_END
                    );
                }
            }
        }
    }
}