using System.Runtime.InteropServices;

namespace SDNGame.Timer.Base
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCapabilities
    {
        public int PeriodMinimum;
        public int PeriodMaximum;
    }
}