using SDNGame.Timer.Base;
using System;
using System.Runtime.InteropServices;
using static SDNGame.Timer.Multimedia.MMTimerExports;

namespace SDNGame.Timer
{
    public class GameTimer : IDisposable
    {
        private int _timerID;
        private bool _isRunning;
        private int _period;
        private int _resolution;
        private TimerMode _mode;
        private Action _action;

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler Tick;

        public GameTimer(int period = 1, int resolution = 0, TimerMode mode = TimerMode.Periodic)
        {
            _period = Math.Clamp(period, Capabilities.PeriodMinimum, Capabilities.PeriodMaximum);
            _resolution = Math.Clamp(resolution, 0, Capabilities.PeriodMaximum);
            _mode = mode;
            timeGetDevCaps(ref Capabilities, Marshal.SizeOf(Capabilities));
        }

        public void SetAction(Action action)
        {
            if (_isRunning)
                throw new Exception("Cannot set action while the timer is running.");
            _action = action;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            TimerProc timerCallback = _mode == TimerMode.Periodic ? PeriodicCallback : OneShotCallback;
            _timerID = timeSetEvent(_period, _resolution, timerCallback, IntPtr.Zero, _mode);

            if (_timerID == 0)
                throw new Exception("Unable to start the timer.");

            _isRunning = true;
            Started?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            timeKillEvent(_timerID);
            _timerID = 0;
            _isRunning = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Configure(int period, int resolution = 0, TimerMode mode = TimerMode.Periodic)
        {
            if (_isRunning)
                throw new Exception("Cannot configure the timer while it is running.");

            _period = Math.Clamp(period, Capabilities.PeriodMinimum, Capabilities.PeriodMaximum);
            _resolution = Math.Clamp(resolution, 0, Capabilities.PeriodMaximum);
            _mode = mode;
        }

        private void PeriodicCallback(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int wtf)
        {
            _action?.Invoke();
            Tick?.Invoke(this, EventArgs.Empty);
        }

        private void OneShotCallback(int hwnd, int uMsg, IntPtr idEvent, int dwTime, int wtf)
        {
            _action?.Invoke();
            Tick?.Invoke(this, EventArgs.Empty);
            Stop();
        }

        public void Dispose()
        {
            if (_isRunning)
                Stop();

            Started = null;
            Stopped = null;
            Tick = null;
            GC.SuppressFinalize(this);
        }
    }
}