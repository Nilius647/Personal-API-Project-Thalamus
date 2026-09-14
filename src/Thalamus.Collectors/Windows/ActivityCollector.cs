using Thalamus.Core.Models;
using Thalamus.Core.Collection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Thalamus.Collectors.Windows;

public class ActivityCollector : ISampleCollector
{
    private readonly TimeSpan _idleThreshold;
    private readonly Guid _profileId;
    public ActivityCollector(TimeSpan idleThreshold, Guid profileId)
    {
        _idleThreshold = idleThreshold;
        _profileId = profileId;
    }
    public string Name => "windows-activity";
    public bool IsAvailable => OperatingSystem.IsWindows();
    public Task<Sample?> CollectSample(CancellationToken token = default)
    {
        var info = new NativeMethods.LASTINPUTINFO();
        info.cbSize = (uint)Marshal.SizeOf<NativeMethods.LASTINPUTINFO>();
        if (!NativeMethods.GetLastInputInfo(ref info))
            return Task.FromResult<Sample?>(null);
        uint idleMs = (uint)Environment.TickCount - info.dwTime;
        var idleTime = TimeSpan.FromMilliseconds(idleMs);
        bool isIdle = idleTime >= _idleThreshold;
        string? processName = null;
        string? windowTitle = null;
        if (!isIdle)
        {
            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            if(hWnd != IntPtr.Zero)
            {
                NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);
                try{
                    var p = Process.GetProcessById((int)pid);
                    processName = p.ProcessName;
                    windowTitle = p.MainWindowTitle;
                }catch{
                    // process exited or is not accessible; keep the defaults
                }
            }
        }
        var sample = new Sample(
            Guid.NewGuid(),
            _profileId,
            DateTime.UtcNow,
            windowTitle,
            processName,
            isIdle,
            idleTime,
            false
        );
        return Task.FromResult<Sample?>(sample);
    }
}