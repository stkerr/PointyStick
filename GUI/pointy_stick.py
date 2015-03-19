import wx
import wx.grid
import subprocess
import thread
import time
import os
import platform
import sys

class Analyzer(object):
    def __init__(self, nop, title):
        super(Analyzer, self).__init__(nop, title=title)
        self.logfile_path = 'pintool.log'

    def parse_line(self, line):
        fields = line.split('|')
        fields = [f.strip() for f in fields]
        fields = filter(lambda x: x != None, fields)
        fields = filter(lambda x: x != "", fields)

        data = {}
        if fields[0] == '[INS]':
            data['type'] = 'instruction'
            for f in fields[1:]:
                key,value = str(f.split(':')[0]), ''.join(f.split(':')[1:])
                data[key.strip()] = value.strip()
        elif fields[0] == '[LIB]':
            data['type'] = 'library'
            for f in fields[1:]:
                key,value = str(f.split(':')[0]), ''.join(f.split(':')[1:])
                data[key.strip()] = value.strip()
        else:
            pass

        return data

    def load_logfile(self, e, status_bar_index=1):
        logfile = open(self.logfile_path, 'r')
        data = logfile.readlines()

        self.results_grid.ClearGrid()
        self.library_name_field.Clear()

        line_data = []

        status_intervals = [x for x in range(0, 101, 5)]

        data_file_count = 0
        for row in data:
            percent_done = 100.0 * data_file_count / len(data)
            if percent_done > status_intervals[0] or row == data[-1]:
                # Update the status bar with progress
                self.SetStatusText("Parsing Status: " + str(status_intervals[0]) + "% Complete.", status_bar_index)
                status_intervals = status_intervals[1:]

            # self.SetStatusText(("Parsing Data line %s/%s" % (data_file_count, len(data))), status_bar_index)

            try:
                line_data.append(self.parse_line(row))
            except Exception as e:
                pass

            data_file_count = data_file_count + 1

        line_data = filter(lambda x: x != {}, line_data)

        self.results_grid.AppendRows(len(line_data))

        libraries = {}

        status_intervals = [x for x in range(0, 101, 5)]
        row_count = 0
        for line in line_data:

            percent_done = 100.0 * row_count / len(line_data)
            if percent_done > status_intervals[0] or line == line_data[-1]:
                # Update the status bar with progress
                self.SetStatusText("Display Status: " + str(status_intervals[0]) + "% Complete.", status_bar_index)
                status_intervals = status_intervals[1:]

            if 'type' in line and line['type'] == 'instruction':
                
                
                # self.results_grid.SetCellValue(row_count, 0, line['cnt']) # Instruction Count
                # self.results_grid.SetCellValue(row_count, 0, row_count) # Disk Address
                self.results_grid.SetCellValue(row_count, 1, line['adr']) # Execution Address
                self.results_grid.SetCellValue(row_count, 2, line['dth']) # Depth
                # self.results_grid.SetCellValue(row_count, 3, row_count) # Library Name
                self.results_grid.SetCellValue(row_count, 4, line['tid']) # Thread ID
                self.results_grid.SetCellValue(row_count, 5, line['tme']) # Time
                # self.results_grid.SetCellValue(row_count, 6, row_count) # System Call Name

                row_count = row_count + 1

            if 'type' in line and line['type'] == 'library':
                if 'Name' in line:

                    libraries[line['Name']] = {}
                    libraries[line['Name']]['address_execution'] = line['Mapd']
                    libraries[line['Name']]['address_disk'] = line['Low']
                    libraries[line['Name']]['library_name'] = line['Name']
                    libraries[line['Name']]['exports'] = []
                    libraries[line['Name']]['size_execution'] = int(line['High'],16) - int(line['Low'],16)
                    libraries[line['Name']]['size_disk'] = int(line['High'],16) - int(line['Low'],16)
                    
        self.library_name_field.Set(libraries.keys())

        logfile.close()

class Collector(object):
    def __init__(self, nop, title):
        '''
        The nop arguments are needed to support the mutiple inheritance
        model with other classes that use init parameters.
        '''
        super(Collector, self).__init__(nop, title=title)
        self.binary_path = "C:\\windows\\system32\\calc"
        
        self.pin_tool_path = os.environ['PIN_ROOT']
        self.instrumented_process = None

        # Collection options
        self.instruction_tracing = False
        self.snapshot_queued = False

        # Disable tracing by default
        self.disable_instruction_tracing()

    def set_pin_tool_path(self, path):
        self.pin_tool_path = path
        
    def set_instrumented_binary_path(self, path):
        self.binary_path = path

    def start_instrumentation(self,e):        
        self.instrumented_process = subprocess.Popen([os.environ['PIN_ROOT'] + os.sep + 'pin', '-t', 'PointyStick.dll', '--', self.binary_path])

    def stop_instrumentation(self,e):
        try:
            self.instrumented_process.kill()
        except Exception, e:
            raise

    def enable_instruction_tracing(self):

        if sys.platform == 'win32':
            import ctypes
            self.monitoring_event_handle = ctypes.windll.kernel32.OpenEventA(0x1F0003, # EVENT_ALL_ACCESS
                True, "MONITORING")
            error = ctypes.windll.kernel32.GetLastError()
            if error == 2: # ERROR_FILE_NOT_FOUND
                '''
                Failed to open the event, so create it
                '''
                self.monitoring_event_handle = ctypes.windll.kernel32.CreateEventA(
                    None,
                    True,
                    False,
                    "MONITORING"
                )
            if self.monitoring_event_handle == 0 or self.monitoring_event_handle == None:
                error = ctypes.windll.kernel32.GetLastError()
                raise SystemError("Failed to open monitoring event. CreateEventA() error: " + error)

            # Signal the snapshot event
            ctypes.windll.kernel32.SetEvent(self.monitoring_event_handle)

        else:
            raise NotImplementedError

    def disable_instruction_tracing(self):

        if sys.platform == 'win32':
            import ctypes
            self.monitoring_event_handle = ctypes.windll.kernel32.OpenEventA(0x1F0003, # EVENT_ALL_ACCESS
                True, "MONITORING")
            error = ctypes.windll.kernel32.GetLastError()
            if error == 2: # ERROR_FILE_NOT_FOUND
                '''
                Failed to open the event, so create it
                '''
                self.monitoring_event_handle = ctypes.windll.kernel32.CreateEventA(
                    None,
                    True,
                    False,
                    "MONITORING"
                )
            if self.monitoring_event_handle == 0 or self.monitoring_event_handle == None:
                error = ctypes.windll.kernel32.GetLastError()
                raise SystemError("Failed to open monitoring event. CreateEventA() error: " + error)

            # Signal the snapshot event
            ctypes.windll.kernel32.ResetEvent(self.monitoring_event_handle)

        else:
            raise NotImplementedError

    def toggle_instruction_tracing(self, e):
        if sys.platform == 'win32':
            try:
                if self.monitoring_event_handle:
                    import ctypes
                    status = ctypes.windll.kernel32.WaitForSingleObject(self.monitoring_event_handle, 0)        
                    if status == 0: # WAIT_OBJECT_0
                        self.disable_instruction_tracing()
                    else:
                        self.enable_instruction_tracing()
            except AttributeError as e:
                pass
        else:
            raise NotImplementedError()

    def queue_snapshot(self, e):
        self.snapshot_queued = True

        if sys.platform == 'win32':
            import ctypes
            self.snapshot_event_handle = ctypes.windll.kernel32.OpenEventA(0x1F0003, # EVENT_ALL_ACCESS
                True, "SNAPSHOT")
            error = ctypes.windll.kernel32.GetLastError()
            if error == 2: # ERROR_FILE_NOT_FOUND
                '''
                Failed to open the event, so create it
                '''
                self.snapshot_event_handle = ctypes.windll.kernel32.CreateEventA(
                    None,
                    True,
                    False,
                    "SNAPSHOT"
                )
            if self.snapshot_event_handle == 0 or self.snapshot_event_handle == None:
                error = ctypes.windll.kernel32.GetLastError()
                raise SystemError("Failed to open snapshot event. CreateEventA() error: " + error)

            # Signal the snapshot event
            ctypes.windll.kernel32.SetEvent(self.snapshot_event_handle)
        else:
            raise NotImplementedError


class BasicUserInteraction(object):
    def __init__(self, nop, title):
        '''
        The nop arguments are needed to support the mutiple inheritance
        model with other classes that use init parameters.
        '''
        super(BasicUserInteraction, self).__init__(nop, title=title)

    def on_close(self, e):
        self.Destroy()

    def on_about(self, e):
        dialog = wx.MessageDialog(self, "Pointy Stick: A tool to analyze running binaries.\nConceived and written by Whistlepig.", "Pointy Stick", wx.OK)
        dialog.ShowModal()
        dialog.Destroy()

class PointyStickFrame(Collector, BasicUserInteraction, Analyzer, wx.Frame):
    def __init__(self, parent, title):
        super(PointyStickFrame, self).__init__(parent, title=title)

        if "PIN_ROOT" not in os.environ or os.environ["PIN_ROOT"] == "":
            dialog = wx.MessageDialog(self, "PIN_ROOT environment variable is not defined. Please define it and restart Pointy Stick.", "Pointy Stick", wx.OK)
            dialog.ShowModal()
            dialog.Destroy()

        filemenu = wx.Menu()
        self.Bind(wx.EVT_MENU,
            self.get_instrumented_file,
            filemenu.Append(wx.ID_OPEN, "Set Instrumented Binary")
        )
        self.Bind(wx.EVT_MENU,
            self.on_about,
            filemenu.Append(wx.ID_ABOUT, "&About", "Information about this program")
        )
        filemenu.AppendSeparator()
        self.Bind(wx.EVT_MENU,
            self.on_close,
            filemenu.Append(wx.ID_EXIT, "E&xit", "Terminate the program")
        )

        collectionmenu = wx.Menu()
        
        collectionmenu.Append(wx.ID_ANY, "Region Controls")
        collectionmenu.Append(wx.ID_ANY, "Instruction Tracing Controls")
        collectionmenu.AppendSeparator()
        self.Bind(wx.EVT_MENU,
            self.start_instrumentation,
            collectionmenu.Append(wx.ID_ANY, "&Start Collection", "Initiate the collection")
        )

        runtimemenu = wx.Menu()
        self.Bind(wx.EVT_MENU,
            self.queue_snapshot,
            runtimemenu.Append(wx.ID_ANY, "Take &Snapshot", "Take a snapshot of the memory region.")
        )
        self.Bind(wx.EVT_MENU,
            self.toggle_instruction_tracing,
            runtimemenu.Append(wx.ID_ANY, "Toggle &Instruction Tracing", "Toggle instruction tracing on and off.")
        )
        runtimemenu.AppendSeparator()
        self.Bind(wx.EVT_MENU,
            self.stop_instrumentation,
            runtimemenu.Append(wx.ID_ANY, "&Terminate Instrumented Application", "Terminate the instrumented application.")
        )

        analysismenu = wx.Menu()
        self.Bind(wx.EVT_MENU,
            self.load_logfile,
            analysismenu.Append(wx.ID_ANY, "Load &Logfile", "Load a logfile from the PIN tool")
        )

        menubar = wx.MenuBar()
        menubar.Append(filemenu, "&File")
        menubar.Append(collectionmenu, "&Collection Settings")
        menubar.Append(runtimemenu, "&Runtime Control")
        menubar.Append(analysismenu, "&Analysis")
        self.SetMenuBar(menubar)

        self.create_filter_split()

        self.CreateStatusBar()
        self.StatusBar.SetFieldsCount(4)
        self.StatusBar.SetStatusWidths([-2,-1,-1,-1])
        self.SetStatusText("No Snapshot Queued", 2)
        self.SetStatusText("Tracing Disabled", 3)

        self.Show(True)

        self.polling_thread = thread.start_new_thread(self.status_bar_polling, ())

    def status_bar_polling(self):
        while True:
            if sys.platform == 'win32':
                try:
                    if self.monitoring_event_handle:
                        import ctypes
                        status = ctypes.windll.kernel32.WaitForSingleObject(self.monitoring_event_handle, 0)        
                        if status == 0: # WAIT_OBJECT_0
                            self.SetStatusText("Tracing Enabled", 2)
                        else:
                            self.SetStatusText("Tracing Disabled", 2)
                except AttributeError as e:
                    pass
            else:
                raise NotImplementedError()    

            if sys.platform == 'win32':
                try:
                    if self.snapshot_event_handle:
                        import ctypes
                        status = ctypes.windll.kernel32.WaitForSingleObject(self.snapshot_event_handle, 0)        
                        if status == 0: # WAIT_OBJECT_0
                            self.SetStatusText("Snapshot Queued", 1)
                        else:
                            self.SetStatusText("No Snapshot Queued", 1)
                except AttributeError as e:
                    pass
            else:
                raise NotImplementedError()

            time.sleep(0.1)

    def create_filter_split(self):
        self.splitter = wx.SplitterWindow(self)
        self.filter_splitter = wx.SplitterWindow(self.splitter)
        self.sub_filter_splitter = wx.SplitterWindow(self.filter_splitter)

        results_panel = wx.Window(self.splitter)
        filter_panel = wx.Window(self.splitter)
        
        self.results_grid = wx.grid.Grid(results_panel)
        filter_staticsize_panel = wx.Window(self.filter_splitter)

        thread_id_panel = wx.Window(self.sub_filter_splitter)
        library_name_panel = wx.Window(self.sub_filter_splitter)

        columns_to_insert = [
            # "Instruction Count",
            "Disk Address",
            "Execution Address",
            "Depth",
            "Library Name",
            "Thread ID",
            "Time",
            "System Call Name"
        ]

        self.results_grid.CreateGrid(1,len(columns_to_insert))
        self.results_grid.EnableEditing(False)
        i = 0
        for col in columns_to_insert:
            self.results_grid.SetColLabelValue(i, col)
            self.results_grid.AutoSizeColumn(i)

            i = i+1

        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(self.results_grid, 1, wx.EXPAND)
        results_panel.SetSizerAndFit(sizer)

        address_cutoff_low_label = wx.StaticText(filter_staticsize_panel, -1, 'Address Cutoff - Low')
        address_cutoff_low = wx.TextCtrl(filter_staticsize_panel)
        
        address_cutoff_high_label = wx.StaticText(filter_staticsize_panel, -1, 'Address Cutoff - High')
        address_cutoff_high = wx.TextCtrl(filter_staticsize_panel)

        depth_cutoff_low_label = wx.StaticText(filter_staticsize_panel, -1, 'Depth Cutoff - Low')
        depth_cutoff_low = wx.TextCtrl(filter_staticsize_panel)
        self.splitter.SplitVertically(results_panel, self.filter_splitter, -200)
        self.splitter.SetSashGravity(1) # Don't resize filter panel
        self.splitter.SetSashInvisible(False)
        self.splitter.SetMinimumPaneSize(200)

        depth_cutoff_high_label = wx.StaticText(filter_staticsize_panel, -1, 'Depth Cutoff - High')
        depth_cutoff_high = wx.TextCtrl(filter_staticsize_panel)
        
        system_calls_enabled_label = wx.StaticText(filter_staticsize_panel, -1, 'System Calls Always Visible')
        system_calls_enabled = wx.CheckBox(filter_staticsize_panel)

        self.library_name_field_label = wx.StaticText(library_name_panel, -1, 'Library Name')
        self.library_name_field = wx.ListBox(library_name_panel, style=wx.LB_MULTIPLE | wx.LB_SORT | wx.LB_HSCROLL)

        thread_id_label = wx.StaticText(thread_id_panel, -1, 'Thread ID')
        thread_id_field = wx.ListBox(thread_id_panel, size=(-1,-1), style=wx.LB_MULTIPLE | wx.LB_SORT | wx.LB_HSCROLL)

        filter_dynamic_sizer = wx.BoxSizer(wx.VERTICAL)
        filter_dynamic_sizer.Add(thread_id_label)
        filter_dynamic_sizer.Add(thread_id_field, flag=wx.EXPAND) 
        filter_dynamic_sizer.Add(self.library_name_field_label) 
        filter_dynamic_sizer.Add(self.library_name_field, flag=wx.EXPAND)
        self.sub_filter_splitter.SetSizer(filter_dynamic_sizer)
        self.sub_filter_splitter.Fit()

        filter_static_sizer = wx.BoxSizer(wx.VERTICAL)
        filter_static_sizer.Add(address_cutoff_low_label) 
        filter_static_sizer.Add(address_cutoff_low, flag=wx.EXPAND) 
        filter_static_sizer.Add(address_cutoff_high_label) 
        filter_static_sizer.Add(address_cutoff_high, flag=wx.EXPAND) 
        filter_static_sizer.Add(depth_cutoff_low_label) 
        filter_static_sizer.Add(depth_cutoff_low, flag=wx.EXPAND) 
        filter_static_sizer.Add(depth_cutoff_high_label) 
        filter_static_sizer.Add(depth_cutoff_high, flag=wx.EXPAND) 
        filter_static_sizer.Add(system_calls_enabled_label) 
        filter_static_sizer.Add(system_calls_enabled, flag=wx.EXPAND)
        filter_staticsize_panel.SetSizer(filter_static_sizer)

        filter_sizer = wx.BoxSizer(wx.VERTICAL)
        filter_sizer.Add(filter_staticsize_panel)
        filter_sizer.Add(self.sub_filter_splitter)
        filter_panel.SetSizer(filter_sizer)

        self.splitter.SplitVertically(results_panel, filter_panel, -200)
        self.splitter.SetSashGravity(1)
        self.splitter.SetSashInvisible(False)
        self.splitter.SetMinimumPaneSize(200)

        self.filter_splitter.SplitHorizontally(filter_staticsize_panel, self.sub_filter_splitter, 0)
        self.filter_splitter.SetSashGravity(1)
        self.filter_splitter.SetSashInvisible(False)
        self.filter_splitter.SetMinimumPaneSize(200)

        self.sub_filter_splitter.SplitHorizontally(thread_id_panel, library_name_panel, 0)
        self.sub_filter_splitter.SetSashGravity(1)
        self.sub_filter_splitter.SetSashInvisible(False)
        self.sub_filter_splitter.SetMinimumPaneSize(20)

        library_sizer = wx.BoxSizer(wx.VERTICAL)
        library_sizer.Add(self.library_name_field_label, proportion=0, border=5)
        library_sizer.Add(self.library_name_field, proportion=1, flag=wx.EXPAND | wx.ALL, border=5)
        library_name_panel.SetSizer(library_sizer)

        threadid_sizer = wx.BoxSizer(wx.VERTICAL)
        threadid_sizer.Add(thread_id_label, proportion=0, border=5)
        threadid_sizer.Add(thread_id_field, proportion=1, flag=wx.EXPAND | wx.ALL, border=5)
        thread_id_panel.SetSizer(threadid_sizer)


    def get_instrumented_file(self, e):
        dialog = wx.FileDialog(self)
        dialog.ShowModal()
        path = dialog.GetFilename()
        self.set_instrumented_binary_path(path)

app = wx.App(False)
frame = PointyStickFrame(None, "Pointy Stick")
app.MainLoop()