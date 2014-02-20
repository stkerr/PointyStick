#ifndef ____monitoring__
#define ____monitoring__

#include "pin.H"
#include "region.h"
#include <list>

extern std::list<region_t*> regions_monitored;

bool take_snapshot(region_t *region);
bool add_region_to_monitoring(region_t *region);

#endif
