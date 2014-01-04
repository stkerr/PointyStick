#ifndef ____region__
#define ____region__

#include <iostream>

typedef struct _region_t
{
    void* start;
    void* end;
} region_t;

bool region_contains(region_t *region, void* address);

#endif
