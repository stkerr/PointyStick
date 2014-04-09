PointyStick
===========

This repository hosts PointyStick, a tool for targeted tracing of executables.

Overview
--------
When starting a new reverse engineering project, one of the most
difficult steps is finding what portions of an application are relevant
to your goals and which are not.

Non-trivial programs will not only include code you are lookng for, but most
likely GUI code, auto-update mecanisms, networking code, and more. This can
make it difficult to determine which code you need to focus on reversing and
which code you can ignore.

Typically, users would load a program in a debugger, set breakpoints, and exercise
their program in such a way that a breakpoint fires. However, it can be time consuming
to set appropriate breakpoints and then ensure that when breakpoints do trigger that
they are still relevant.

Enter Pointy Stick.

Pointy Stick provides on demand program tracing and memory region monitoring. Using
PointyStick, a user would run her program with no tracing until it is ready to exercise
some interesting functionality, such as an updating mechanism. She would enable tracing
with Pointy Stick, execute the update mechanism, and then disable program tracing. The
generated log files will ONLY contain instructions that were run as a result of her manually
triggering the update mechanism. She then has a much smaller subset of the program
to reverse engineer, rather than starting at the program's main() function.

Another feature that Pointy Stick offers is memory region monitoring. Users can
specify a range of memory to monitor and anytime the program writes to that range,
a snapshot will be taken and stored to disk. This makes it very straightforward
to monitor a static region of memory for changes. 

Pointy Stick is a tool that allows tracing of program execution
and monitoring of memory writes. It consists of three different parts:

1. Binary Instrumentation Tool
 * Performs the actual recording and logging.
 * This can be done using a variety of tools, such as Intel PIN or DynamoRIO.
1. Support File Generation
 * Generates necessary information to go along with the log files.
1. GUI Display 
 * Presents log files in a meaningful way.

Example Workflow
----------------
Let's follow an example workflow of Joan, a reverse engineer, who is trying to assess
the security of a program her colleague wrote. The example program is a GUI application 
that presents a dialog box asking for a password.
Entering the incorrect password causes an error to occur, while the correct password grants
access.

Joan loads the program into PointyStick and specifies that she would like to trace the program,
but not take any snapshots. Note that this does not actually trace from the start of the program
by default. She runs the program and is presented with the password dialog box.
She enables tracing before entering her (incorrect) password and clicking OK. A modal dialog box
is presented saying the password is incorrect. She disables tracing and exits the program.

Joan now has a log file of the instructions executed between the password being entered and the
rejection occurring. Somewhere in that range, is a check or some sort of routine that decides if
the password is valid. Joan can then use the log file in conjunction with IDA Pro or OllyDbg to
locate the routine of interest and NOP it out if desired.

As Joan works, she finds that the routine dynamically creates a value in memory to compare the
user's input against. However, she cannot figure out what the value actually is. She does note
that it is always within a certain range of memory. She can use memory region monitoring to
maybe recover it.

Joan reloads the program into Pointy Stick, this time specifying the region of memory to monitor.
She then re-executes the program. Over the course of execution, several snapshot files are produced,
indicating that the region of memory has changed. Joan examines these files and notes that a password
is present in one of the snapshots. She then executes the program and enters the password she
recovered from the memory snapshot to gain access.


Installing Pointy Stick
-----------------------
You will need the following files to use PointyStick. They must all be in the same directory:

1. PointyStickBlend.exe
1. Stick_PE.exe
1. PointyStickPinTool.dll

You will also need Intel PIN on your computer as well. Set the environment variable PIN_ROOT
to point to the directory Intel PIN is stored in.

- Download [Intel PIN](http://software.intel.com/en-us/articles/pintool-downloads)
- I used 2.13.62732
   - Older versions may work and newer versions should also work

Using Pointy Stick
------------------
Using PointyStick consists of the following steps:

1. Load your program into Pointy Stick
   * Open the "Collection" menu, select "Start Collection"
   * Click "Open File" and select your application.
      * If there is a space in your application path, you MUST enclose it in quotation marks.
1. Start the program under Pointy Stick
   1. Specify regions to monitor
      * Check the 'Enable Region Monitoring' checkbox and enter the addresses that you wish to monitor in your application.
      * During execution, snapshots wil be automatically taken, but you can manually trigger a snapshot by clicking the "Take Snapshot" button under the "Runtime Control" tab.
   1. Enable tracing if desired
      * Check the "Enable Tracing" box to allow program tracing.
      * If desired, select "Trace from Start" to trace from the start of the program
      * During execution, under the "Runtime Control" tab, click the "Enable Tracing" or "Disable Tracing" button to enable or disable tracing, respectively.
   1. Click "Start Collection" and interact with your program.
1. End the application
   * You can use the "Terminate Application" button under the "Runtime Control" tab or end the application manually.
1. Generate the program support file.
   * After execution is finished, select "Run PE Stick" under the "Collection" menu.
   * If you like, you can look at the produced support.log file that is produced.
1. View the logs
   * Under the "Data" menu tab, click "Process PIN log". The GUI will refresh with the log file data loaded.
1. Filter the logfiles
   * Under the "Filtering" tab on the menu bar, click "Filtering" and you will be presented with several different filtering options.

Building Pointy Stick from Source (Windows)
--------------------------------------------------------

### Requirements
1. Intel PIN
    * Download [Intel PIN](http://software.intel.com/en-us/articles/pintool-downloads)
    * I used 2.13.62732
       * Older versions may work and newer versions should also work
1. Visual Studio 2013


### Building the PIN tool
1. Open a Visual Studio 2012 developer command prompt.
1. Change to the tools/PINtools/ directory.
1. Make sure that the PIN_ROOT environment variable is defined.
1. Execute build.sh.

### Building the support file generator
1. Open a Visual Studio 2013 developer command prompt.
   * NOT 2012 on this one!
1. Change to the tools/BinaryParsers/Stick_PE/Stick_PE directory.
1. Execute 'msbuild'.
   * Or open the Solution file and build it

### Building the GUI
1. Open a Visual Studio 2013 developer command prompt.
   * NOT 2012 on this one!
1. Change to the PointyStickBlend/ directory.
1. Execute 'msbuild'.
   * Or open the Solution file and build it  

Troubleshooting
---------------
If you have run into errors, check the following:

1. That the PIN_ROOT environment variable is set and is pointing to 
root of the version of PIN you are using.
1. Check out the pintool.log and pin.log filee, if they were produced, in the directory where PointyStickBlend.exe is. These logs will most likely hold useful error messages.

Ideas for the future
--------------------
1. Integrate with IDA Pro
1. Improve / increase logged information
   * Maybe dump register states, stack contents, etc
1. Speed improvemnts
   * PIN tool speed is critical
   * GUI speed can also be improved
1. More backends
   * Create a DynamoRIO tool to go along with the PIN tool
   * Port the GUI to Mac/Linux
      * The PIN should already work there; just needs a GUI
1. Integration with other tools
	* [Vera](http://www.offensivecomputing.net/?q=node/1687) is a trace visualizer
	* Provide hook points for plugins
1. Provide more robust ways to specify memory monitoring.
	* Follow malloc()/new regions automatically
1. Fix all the bugs!
