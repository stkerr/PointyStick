#ifndef ____monitoring__
#define ____monitoring__

#include "pin.H"
#include "region.h"
#include <list>

extern std::list<region_t*> regions_monitored;

bool take_snapshot(region_t *region);
bool add_region_to_monitoring(region_t *region);
bool addr_being_monitored(void* address);
bool take_all_snapshots();
bool take_snapshot_address(void* address);
bool checked_snapshot(void* address);

#endif
