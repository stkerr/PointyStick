import wx
import wx.grid

class Collector(object):
    def __init__(self):
        self.binary_path = None

    def set_instrumented_binary_path(self, path):
        self.binary_path = path

    def start_instrumentation(self):
        raise NotImplementedError

    def stop_instrumentation(self):
        raise NotImplementedError

class BasicUserInteraction(object):
    def __init__(self):
        pass

    def on_close(self, e):
        self.Destroy()

    def on_about(self, e):
        dialog = wx.MessageDialog(self, "Pointy Stick: A tool to analyze running binaries.\nConceived and written by Whistlepig.", "Pointy Stick", wx.OK)
        dialog.ShowModal()
        dialog.Destroy()

class PointyStickFrame(wx.Frame, Collector, BasicUserInteraction):
    def __init__(self, parent, title):
        super(Collector, self).__init__()
        super(BasicUserInteraction, self).__init__()

        wx.Frame.__init__(self, parent, title=title)

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
        collectionmenu.Append(wx.ID_ANY, "&Start Collection", "Initiate the collection")

        runtimemenu = wx.Menu()
        runtimemenu.Append(wx.ID_ANY, "Take &Snapshot", "Take a snapshot of the memory region.")
        runtimemenu.Append(wx.ID_ANY, "Toggle &Instruction Tracing", "Toggle instruction tracing on and off.")
        runtimemenu.AppendSeparator()
        runtimemenu.Append(wx.ID_ANY, "&Terminate Instrumented Application", "Terminate the instrumented application.")

        analysismenu = wx.Menu()
        analysismenu.Append(wx.ID_ANY, "Load &Logfile", "Load a logfile from the PIN tool")

        menubar = wx.MenuBar()
        menubar.Append(filemenu, "&File")
        menubar.Append(collectionmenu, "&Collection Settings")
        menubar.Append(runtimemenu, "&Runtime Control")
        menubar.Append(analysismenu, "&Analysis")
        self.SetMenuBar(menubar)

        self.create_filter_split()

        self.CreateStatusBar()
        self.StatusBar.SetFieldsCount(3)
        self.StatusBar.SetStatusWidths([-2,-1,-1])
        self.SetStatusText("No Snapshot Queued", 1)
        self.SetStatusText("Tracing Disabled", 2)

        self.Show(True)

    def create_filter_split(self):
        self.splitter = wx.SplitterWindow(self)

        results_panel = wx.Window(self.splitter)
        
        results_grid = wx.grid.Grid(results_panel)

        columns_to_insert = [
            "Instruction Count",
            "Disk Address",
            "Execution Address",
            "Depth",
            "Library Name",
            "Thread ID",
            "Time",
            "System Call Name"
        ]

        results_grid.CreateGrid(1,len(columns_to_insert))
        results_grid.EnableEditing(False)
        i = 0
        for col in columns_to_insert:
            results_grid.SetColLabelValue(i, col)
            results_grid.AutoSizeColumn(i)

            i = i+1

        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(results_grid, 1, wx.EXPAND)
        results_panel.SetSizerAndFit(sizer)


        filter_panel = wx.Window(self.splitter)
        thread_id_label = wx.StaticText(filter_panel, -1, 'Thread ID')
        thread_id_field = wx.ListBox(filter_panel, size=(-1,-1))

        address_cutoff_low_label = wx.StaticText(filter_panel, -1, 'Address Cutoff - Low')
        address_cutoff_low = wx.TextCtrl(filter_panel)
        
        address_cutoff_high_label = wx.StaticText(filter_panel, -1, 'Address Cutoff - High')
        address_cutoff_high = wx.TextCtrl(filter_panel)

        depth_cutoff_low_label = wx.StaticText(filter_panel, -1, 'Depth Cutoff - Low')
        depth_cutoff_low = wx.TextCtrl(filter_panel)

        depth_cutoff_high_label = wx.StaticText(filter_panel, -1, 'Depth Cutoff - High')
        depth_cutoff_high = wx.TextCtrl(filter_panel)

        system_calls_enabled_label = wx.StaticText(filter_panel, -1, 'All System Calls Visible')
        system_calls_enabled = wx.CheckBox(filter_panel)

        library_name_field_label = wx.StaticText(filter_panel, -1, 'Library Name')
        library_name_field = wx.ListBox(filter_panel)

        filter_sizer = wx.BoxSizer(wx.VERTICAL)
        filter_sizer.Add(thread_id_label, flag=wx.ALIGN_CENTER_HORIZONTAL)
        filter_sizer.Add(thread_id_field, flag=wx.EXPAND) 
        
        filter_sizer.Add(library_name_field_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(library_name_field, flag=wx.EXPAND)
        
        filter_sizer.Add(address_cutoff_low_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(address_cutoff_low, flag=wx.EXPAND) 
        
        filter_sizer.Add(address_cutoff_high_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(address_cutoff_high, flag=wx.EXPAND) 
        
        filter_sizer.Add(depth_cutoff_low_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(depth_cutoff_low, flag=wx.EXPAND) 
        
        filter_sizer.Add(depth_cutoff_high_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(depth_cutoff_high, flag=wx.EXPAND) 
        
        filter_sizer.Add(system_calls_enabled_label, flag=wx.ALIGN_CENTER_HORIZONTAL) 
        filter_sizer.Add(system_calls_enabled, flag=wx.EXPAND) 
        filter_panel.SetSizer(filter_sizer)

        self.splitter.SplitVertically(results_panel, filter_panel, -200)
        self.splitter.SetSashGravity(1) # Don't resize filter panel
        self.splitter.SetSashInvisible(False)
        self.splitter.SetMinimumPaneSize(200)

    def get_instrumented_file(self, e):
        dialog = wx.FileDialog(self)
        dialog.ShowModal()
        path = dialog.GetFilename()
        self.set_instrumented_binary_path(path)

app = wx.App(False)
frame = PointyStickFrame(None, "Pointy Stick")
app.MainLoop()