using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PupTrailsV3.Services
{
    /// <summary>
    /// Provides a last-resort process termination to guarantee exit.
    /// Schedules a delayed kill so graceful shutdown can occur first.
    /// </summary>
    public static class ProcessTerminationService
    {
        /// <summary>
        /// Ensure the current process terminates. Attempts Environment.Exit(0) first,
        /// then forcibly kills the process after the provided delay.
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds before forcing kill.</param>
        public static void EnsureProcessExit(int delayMs = 1000)
        {
            try
            {
                LoggingService.LogInfo($"ProcessTerminationService: scheduling forced exit in {delayMs}ms");
            }
            catch { /* ignore logging failures */ }

            // Run on thread pool (background) so it won't block shutdown
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs).ConfigureAwait(false);

                    try
                    {
                        LoggingService.LogInfo("ProcessTerminationService: attempting Environment.Exit(0)");
                    }
                    catch { }

                    try
                    {
                        Environment.Exit(0);
                    }
                    catch { /* ignore */ }

                    try
                    {
                        LoggingService.LogInfo("ProcessTerminationService: attempting Process.Kill(true)");
                    }
                    catch { }

                    try
                    {
                        var proc = Process.GetCurrentProcess();
                        proc.Kill(true);
                    }
                    catch { /* ignore */ }
                }
                catch
                {
                    // Swallow any exceptions in the background killer to avoid interfering with shutdown
                }
            });
        }
    }
}
