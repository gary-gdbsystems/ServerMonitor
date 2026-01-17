using System.Runtime.InteropServices;

namespace WebsiteMonitor.Native;

/// <summary>
/// P/Invoke wrapper for Kernel32 functions used for process termination.
/// </summary>
internal static class Kernel32
{
    public const uint CTRL_C_EVENT = 0;
    public const uint CTRL_BREAK_EVENT = 1;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handlerRoutine, bool add);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();

    public delegate bool ConsoleCtrlDelegate(uint ctrlType);

    /// <summary>
    /// Attempts to send a Ctrl+C signal to a console process.
    /// </summary>
    /// <param name="processId">The process ID to send the signal to.</param>
    /// <returns>True if the signal was sent successfully.</returns>
    public static bool SendCtrlC(int processId)
    {
        bool result = false;

        // Free our console first
        FreeConsole();

        // Attach to the target process's console
        if (AttachConsole((uint)processId))
        {
            // Disable Ctrl+C handling for ourselves
            SetConsoleCtrlHandler(null, true);

            try
            {
                // Send Ctrl+C to all processes attached to this console
                result = GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
            }
            finally
            {
                // Re-enable Ctrl+C handling
                SetConsoleCtrlHandler(null, false);

                // Detach from the console
                FreeConsole();
            }
        }

        return result;
    }
}
