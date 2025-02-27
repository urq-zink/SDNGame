using SDNGame.Platform.Windows;

namespace SDNGame.Core
{
    public static class Program
    {
        private static readonly bool _enableDedicatedRenderer = false;
        private static readonly bool _enableSplashScreen = true;

        public static void Main(string[] args)
        {
            if(_enableDedicatedRenderer)
            {
                var gi = new GraphicsInitializer();
                gi.InitializeDedicatedGraphics();
            }

            if (_enableSplashScreen)
            {
                using (var splash = new NativeLayeredWindow("Assets/Textures/splash2.png", 760, 340))
                {
                    splash.FadeInHoldFadeOut(100, 1000, 100);
                    splash.HideWindow();
                }
            }

            using (var game = new MainGame())
            {
                game.Run();
            }
        }
    }
}