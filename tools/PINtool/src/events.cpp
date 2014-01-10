#include "events.h"
#include <stdio.h>

static bool event_monitoring = false;
static bool event_snapshot = false;

#if defined(TARGET_WINDOWS)
#include <Windows.h>
static HANDLE lock_monitoring;
static HANDLE lock_snapshot;

bool initialize_events()
{
    lock_monitoring = OpenEvent(EVENT_ALL_ACCESS, false, "MONITORING");
    if (lock_monitoring == 0)
    {
        // NULL, so we need to make the event
        lock_monitoring = CreateEvent(0, true, false, "MONITORING");
    }

    lock_snapshot = OpenEvent(EVENT_ALL_ACCESS, false, "SNAPSHOT");
    if (lock_snapshot == 0)
    {
        lock_snapshot = CreateEvent(0, true, false, "SNAPSHOT");
    }

    return true;
}

bool event_monitoring_enabled()
{
    int monitoring = WaitForSingleObject(lock_monitoring, 0);
    printf("Monitoring event: %s\n", monitoring == WAIT_OBJECT_0 ? "ENABLED" : "DISABLED");

    if (monitoring == WAIT_OBJECT_0)
    {
        return true;
    }
    else
    {
        return false;
    }
    return false;
}

void event_monitoring_set(bool status)
{
    if (status)
        SetEvent(lock_monitoring);
    else
        ResetEvent(lock_monitoring);
    
    int error = GetLastError();
    if (error)
    {
        fprintf(stderr, "Monitoring event error: %d\n", status);
    }
}

bool event_snapshot_enabled()
{
    int snapshot = WaitForSingleObject(lock_snapshot, 0);
    if (snapshot == WAIT_OBJECT_0)
    {
        return true;
    }
    else
    {
        return false;
    }
    return false;
}

void event_snapshot_set(bool status)
{
    if (status)
        SetEvent(lock_snapshot);
    else
        ResetEvent(lock_snapshot);

    int error = GetLastError();
    if (error)
    {
        fprintf(stderr, "Snapshot event error: %d\n", status);
    }
}


#elif defined(TARGET_MAC) || defined(TARGET_LINUX)
#include <semaphore.h>
static sem_t *lock_monitoring;
static sem_t *lock_snapshot;

bool initialize_events()
{
    lock_monitoring = sem_open("event_monitoring", O_CREAT, S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH);
    if(lock_monitoring == SEM_FAILED)
        return false;
    lock_snapshot = sem_open("event_snapshot", O_CREAT, S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH);
    if(lock_snapshot == SEM_FAILED)
        return false;
    
    return true;
}

// TODO: Use timed wait, not sem_wait
bool event_monitoring_enabled()
{
    bool temp;
    sem_wait(lock_monitoring);
    temp = event_monitoring;
    sem_post(lock_monitoring);
    return temp;
}

void event_monitoring_set(bool status)
{
    sem_wait(lock_monitoring);
    event_monitoring = status;
    sem_post(lock_monitoring);
}

bool event_snapshot_enabled()
{
    bool temp;
    sem_wait(lock_snapshot);
    temp = event_snapshot;
    sem_post(lock_snapshot);
    return temp;
}

void event_snapshot_set(bool status)
{
    sem_wait(lock_snapshot);
    event_snapshot = status;
    sem_post(lock_snapshot);
}

#else
#error Platform unsupported
#endif

