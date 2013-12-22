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
    lock_monitoring = CreateEventA(0, 0, 0, "MONITORING");
    if (!lock_monitoring)
    {
        fprintf(stderr, "Error %d\n", GetLastError());
        return false;
    }

    lock_snapshot = CreateEventA(0, 0, 0, "SNAPSHOT");
    if (!lock_snapshot)
    {
        fprintf(stderr, "Error %d\n", GetLastError());
        return false;
    }

    return true;
}

bool event_monitoring_enabled()
{
    fprintf(stderr, "Monitoring event unimplemented.\n");
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
    fprintf(stderr, "Snapshot event unimplemented.\n");
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
        fprintf(stderr, "Monitoring event error: %d\n", status);
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

