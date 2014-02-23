#include "snapshotting.h"
#include "PointyStick.h"

#include <algorithm>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#ifdef TARGET_WINDOWS
#define snprintf _snprintf
#endif

std::list<region_t*> regions_monitored;

bool take_all_snapshots()
{
    std::list<region_t*>::iterator it;

    bool retval = true;
    for(it = regions_monitored.begin(); it != regions_monitored.end(); it++)
    {
        retval = take_snapshot(*it) & retval;
    }
    return retval;
}

static char filename[260] = {0};
static int snapshot_count = 0;
bool take_snapshot(region_t *region)
{
    
    char* region_start = (int)(region->start) + (char*)(region->loaded_address);
    char* region_end = (int)(region->end) + (char*)(region->loaded_address);
    
    // Patch up the library name to allow it to be written to file systems (colons and slashes)
    std::string lib_name = std::string(region->library_name);
    std::replace(lib_name.begin(), lib_name.end(), ':', '_');
    std::replace(lib_name.begin(), lib_name.end(), '/', '_');
    std::replace(lib_name.begin(), lib_name.end(), '\\', '_');

    memset(filename,0,260);
    snprintf(filename, 260, "snapshot.%s.0x%x.%d.hex", lib_name.c_str(), region_start, snapshot_count);
    
    FILE *fp = fopen(filename, "wb");
    for (char* i = region_start; i != region_end; i++)
    {
        fwrite(i, 1, 1, fp);
    }
    fclose(fp);

    return true;
}

bool add_region_to_monitoring(region_t *region)
{
	regions_monitored.push_back(region);

	return true;
}

bool addr_being_monitored(void* address)
{
    std::list<region_t*>::iterator it;

    for(it = regions_monitored.begin(); it != regions_monitored.end(); it++)
    {
        if(region_contains(*it, address))
            return true;
    }
    return false;
}

bool take_snapshot_address(void* address)
{
    std::list<region_t*>::iterator it;

    for(it = regions_monitored.begin(); it != regions_monitored.end(); it++)
    {
        if(region_contains(*it, address))
        {
            return take_snapshot(*it);
        }
    }
    return false;
}

bool checked_snapshot(void* address)
{
    if(addr_being_monitored(address) == true)
    {
        take_snapshot_address(address);
    }
    return true;
}