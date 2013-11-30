#ifndef ____events__
#define ____events__

bool initialize_events();

bool event_monitoring_enabled();
void event_monitoring_set(bool status);

bool event_snapshot_enabled();
void event_snapshot_set(bool status);

#endif /* defined(____events__) */
