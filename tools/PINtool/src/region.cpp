#include "region.h"

bool region_contains(region_t *region, void* address)
{
    if(region->start <= address && address < region->end)
    {
        return true;
    }
    else
    {
        return false;
    }
}
