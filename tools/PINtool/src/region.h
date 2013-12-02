#ifndef ____region__
#define ____region__

#include <iostream>

typedef struct _region_t
{
    int start;
    int end;
} region_t;

bool region_contains(region_t *region, int address);

#endif
