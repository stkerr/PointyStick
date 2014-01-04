#include "snapshotting.h"
#include "PointyStick.h"

#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#ifdef TARGET_WINDOWS
#define snprintf _snprintf
#endif

bool take_snapshot(region_t *region)
{
    time_t current_time = time(0);
//    itoa(current_time, time_str, 10);
    
    char filename[260];
    
    snprintf(filename, 260, "snapshot.%d.hex", (int)current_time);

    printf("%s\n", filename);
    FILE *fp = fopen(filename, "wb");
    
    for (char* i = (char*)region->start; i != region->end; i++)
        fwrite(region->start, 1, 1, fp);

    fclose(fp);

    return true;
}