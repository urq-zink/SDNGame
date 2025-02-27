using System.Runtime.InteropServices;

namespace SDNGame.Platform.Windows
{
    public class GraphicsInitializer
    {
        [DllImport("nvapi64.dll", EntryPoint = "fake")]
        private static extern int LoadNvApi64();
        [DllImport("nvapi.dll", EntryPoint = "fake")]
        private static extern int LoadNvApi32();

        [DllImport("atiadlxx.dll", EntryPoint = "fake")]
        private static extern int LoadAmdApi64();
        [DllImport("atiadlxy.dll", EntryPoint = "fake")]
        private static extern int LoadAmdApi32();

        public void InitializeDedicatedGraphics()
        {
            bool is64Bit = Environment.Is64BitProcess;

            if (TryInitializeGraphics(is64Bit ? LoadNvApi64 : LoadNvApi32))
            {
                return;
            }

            TryInitializeGraphics(is64Bit ? LoadAmdApi64 : LoadAmdApi32);
        }

        private bool TryInitializeGraphics(Func<int> initializeFunction)
        {
            try
            {
                initializeFunction();
                return true;
            }
            catch { return false; }
        }
    }
}
