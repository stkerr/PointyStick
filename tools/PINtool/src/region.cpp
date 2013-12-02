#include "region.h"

bool region_contains(region_t *region, int address)
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
