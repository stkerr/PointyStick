#ifndef ____region__
#define ____region__

#include <iostream>

/*
	Represents a region of loaded memory to monitor.

	Start and end define the offset (0 based) from the
	loaded library. Note that loaded address specifies the
	base of the specified library that the start and end
	offsets can be added to.
*/
typedef struct _region_t
{
    void* start;
    void* end;
    char library_name[260];
    void* loaded_address;
} region_t;

bool region_contains(region_t *region, void* address);

#endif
