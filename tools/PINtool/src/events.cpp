#include "events.h"

static bool event_monitoring = false;
static bool event_snapshot = false;

// TODO: Do this with named mutexes
bool event_monitoring_enabled()
{
    return event_monitoring;
}

void event_monitoring_set(bool status)
{
    event_monitoring = status;
}

bool event_snapshot_enabled()
{
    return event_snapshot;
}

void event_snapshot_set(bool status)
{
    event_snapshot = status;
}