using System.Diagnostics;
using WebsiteMonitor.Native;
using WebsiteMonitor.Services.Interfaces;

namespace WebsiteMonitor.Services;

/// <summary>
/// Service for terminating processes with graceful shutdown support.
/// </summary>
public class ProcessTerminationService : IProcessTerminationService
{
    /// <inheritdoc/>
    public async Task<bool> TerminateProcessAsync(int processId, int gracefulTimeoutMs = 5000)
    {
        Process process;
        try
        {
            process = Process.GetProcessById(processId);
        }
        catch (ArgumentException)
        {
            // Process already exited
            return true;
        }

        try
        {
            // Step 1: Try CloseMainWindow for GUI apps
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                process.CloseMainWindow();
                if (await WaitForExitAsync(process, gracefulTimeoutMs / 2))
                {
                    return true;
                }
            }

            // Step 2: Try sending Ctrl+C for console apps
            if (Kernel32.SendCtrlC(processId))
            {
                if (await WaitForExitAsync(process, gracefulTimeoutMs / 2))
                {
                    return true;
                }
            }

            // Step 3: Force kill
            process.Kill(entireProcessTree: true);
            return await WaitForExitAsync(process, 2000);
        }
        catch (InvalidOperationException)
        {
            // Process already exited
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            process.Dispose();
        }
    }

    private static async Task<bool> WaitForExitAsync(Process process, int timeoutMs)
    {
        try
        {
            return await Task.Run(() => process.WaitForExit(timeoutMs));
        }
        catch
        {
            return process.HasExited;
        }
    }
}
