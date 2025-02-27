using SDNGame.Timer.Base;
using System.Runtime.InteropServices;

namespace SDNGame.Timer.Multimedia
{
    internal static class MMTimerExports
    {
        [DllImport("winmm.dll")]
        internal static extern int timeBeginPeriod(int period);

        [DllImport("winmm.dll")]
        internal static extern int timeEndPeriod(int period);

        [DllImport("winmm.dll")]
        internal static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll")]
        internal static extern int timeKillEvent(int id);

        [DllImport("winmm.dll")]
        internal static extern int timeSetEvent(int delay, int resolution, TimerProc proc, nint user, TimerMode mode);

        internal delegate void TimerProc(int hwnd, int uMsg, nint idEvent, int dwTime, int wtf);

        internal static TimerCapabilities Capabilities;
    }
}