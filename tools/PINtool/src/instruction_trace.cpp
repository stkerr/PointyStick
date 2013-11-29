#include "instruction_trace.h"
#include "PointyStick.h"
#include <sstream>

static int depth = 0;
static int instruction_count = 0;

void instruction_trace(INS ins, void* arg)
{
    if(!event_monitoring_enabled())
    {
        // The monitoring event is not on, so exit
        return;
    }
    
    // log out our current state
    std::ostringstream message;
    message << "[INS] | ";
    message << "adr : " << hex << INS_Address(ins) << dec << " | "; // hex address then back to decimal
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