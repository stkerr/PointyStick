#include "address_helpers.h"
#include "pin.H"

std::string get_library_name(ADDRINT address)
{
    IMG image = IMG_FindByAddress(address);
    if(IMG_Valid(image))
    {
        return IMG_Name(image);
    }
    else
    {
        return std::string("Invalid library address");
    }
}
