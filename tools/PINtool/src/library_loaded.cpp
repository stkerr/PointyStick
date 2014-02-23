#include "PointyStick.h"
#include "library_loaded.h"
#include "region.h"
#include <sstream>

std::string img_type_to_string(IMG_TYPE type)
{
    switch(type)
    {
        case IMG_TYPE_STATIC:
            return "IMG_TYPE_STATIC";
        case IMG_TYPE_SHARED:
            return "IMG_TYPE_SHARED";
        case IMG_TYPE_SHAREDLIB:
            return "IMG_TYPE_SHAREDLIB";
        case IMG_TYPE_RELOCATABLE:
            return "IMG_TYPE_RELOCATABLE";
        case IMG_TYPE_DYNAMIC_CODE:
            return "IMG_TYPE_DYNAMIC_CODE";
        default:
            return "IMG_TYPE_UNKNOWN";
    }
}

void library_unloaded_function(IMG image, void* arg)
{
    std::ostringstream logged;
    logged << "[LIB] | ";
    logged << "Msg : Unloading " << IMG_Name(image) << " |";
    logged << "\n";

    LOG(logged.str().c_str());
}

void library_loaded_function(IMG image, void* arg)
{
    std::ostringstream logged;
    logged << "[LIB] | ";
    logged << "Name : " << IMG_Name(image) << " | ";
    logged << "Type : " << img_type_to_string(IMG_Type(image)) << " | ";
    logged << "Strt : " << hex << IMG_StartAddress(image) << " | ";
    logged << "Low  : " << hex << IMG_LowAddress(image) << " | ";
    logged << "High : " << hex << IMG_HighAddress(image) << " | ";
    logged << "Enty : " << hex << IMG_Entry(image) << " | ";
    logged << "Mapd : " << hex << IMG_SizeMapped(image) << " | ";
    logged << "\n";

    LOG(logged.str().c_str());

    /*
        Check if we are monitoring this region
    */

    std::list<region_t*>::iterator it;
    for(it = regions_monitored.begin(); it != regions_monitored.end(); it++)
    {
        printf("%s\n%s\n", IMG_Name(image), (*it)->library_name);
        if(IMG_Name(image).find(std::string((*it)->library_name)) != std::string::npos)
        {
            (*it)->loaded_address = (void*)IMG_LowAddress(image);
        }
    }
}
