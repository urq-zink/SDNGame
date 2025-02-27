using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using SDNGame.Camera;
using SDNGame.Input;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using Shader = SDNGame.Rendering.Shaders.Shader;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using SDNGame.Rendering.Fonts;

namespace SDNGame.Core
{
    public abstract class Game : IDisposable
    {
        public IWindow GameWindow { get; private set; }
        public GL Gl { get; private set; }
        public Camera2D Camera { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public ShapeRenderer ShapeRenderer { get; private set; }
        public FontRenderer FontRenderer { get; private set; }
        public InputManager InputManager { get; private set; }

        private readonly string _windowTitle;
        private double _lastFrameTime = 0;
        private double _lastFpsUpdateTime = 0;
        private int _frameCount = 0;
        private double _fps = 0;
        private bool _isDisposed;
        private bool _isFullscreen = false; // Track fullscreen state

        private TextStyle _debugStyle;

        public readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public static double FPS { get; private set; }

        private readonly SceneManager _sceneManager;

        protected Game(int width = 1920, int height = 1080, string title = "SDNGame") 
        {
            ScreenWidth = width;
            ScreenHeight = height;
            _windowTitle = title;

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Position = new Vector2D<int>(0, 0);
            options.Title = title;
            options.VSync = true;
            options.WindowBorder = WindowBorder.Hidden;

            GameWindow = Window.Create(options);

            _sceneManager = new SceneManager(this);

            GameWindow.Load += OnLoad;
            GameWindow.Update += OnUpdate;
            GameWindow.Render += OnRender;
            GameWindow.FramebufferResize += OnFramebufferResize;
            GameWindow.Closing += OnClosing;
            GameWindow.FocusChanged += OnFocusChanged;
        }

        public void Run()
        {
            GameWindow.Run();
            GameWindow.Dispose();
        }

        protected virtual void OnLoad()
        {
            Gl = GL.GetApi(GameWindow);
            Gl.ClearColor(0.2f, 0.2f, 0.3f, 1.0f);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Camera = new Camera2D { Position = Vector2.Zero, Zoom = 1f };
            InputManager = new InputManager(GameWindow.CreateInput());

            var spriteShader = new Shader(Gl, "Rendering/Shaders/sprite.vert", "Rendering/Shaders/sprite.frag");
            SpriteBatch = new SpriteBatch(Gl, spriteShader);

            var shapeShader = new Shader(Gl, "Rendering/Shaders/shape.vert", "Rendering/Shaders/shape.frag");
            ShapeRenderer = new ShapeRenderer(Gl, shapeShader);

            FontRenderer = new FontRenderer(Gl, SpriteBatch);

            FontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");
            _debugStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(0, 1, 0, 1),
                Alignment = TextAlignment.Center
            };

            Initialize();
            LoadContent();
        }

        protected virtual void OnUpdate(double deltaTime)
        {
            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;

            _frameCount++;
            if (currentTime - _lastFpsUpdateTime >= 0.05)
            {
                _fps = FPS = (_frameCount / (currentTime - _lastFpsUpdateTime));
                GameWindow.Title = $"{_windowTitle} | FPS: {_fps:F2} | {(_isFullscreen ? "Fullscreen" : "Windowed")}";
                _lastFpsUpdateTime = currentTime;
                _frameCount = 0;
            }

            // Toggle fullscreen/windowed with F11 key
            if (InputManager.IsKeyPressed(Key.F11))
            {
                ToggleFullscreen();
            }

            _sceneManager.Update(deltaTime);
        }

        protected virtual void OnRender(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            SpriteBatch.DrawText(
                FontRenderer,
                $"{FPS:F0}",
                new Vector2(ScreenWidth - 32, ScreenHeight - 16),
                _debugStyle
            );
            SpriteBatch.End();

            _sceneManager.Draw(deltaTime);
        }

        protected virtual void OnFramebufferResize(Vector2D<int> size)
        {
            ScreenWidth = size.X;
            ScreenHeight = size.Y;
            Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y); // Update viewport to match new size
        }

        protected virtual void OnClosing()
        {
            Dispose();
        }

        protected virtual void OnFocusChanged(bool focused)
        {
        }

        public void SetScene(Scene scene, Transition outgoing = null, Transition incoming = null)
        {
            _sceneManager.SetScene(scene, outgoing, incoming);
        }

        protected abstract void Initialize();
        protected abstract void LoadContent();

        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _sceneManager.CurrentScene?.Dispose();
            SpriteBatch?.Dispose();
            ShapeRenderer?.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                // Switch to windowed mode (1280x720, fixed border)
                GameWindow.Size = new Vector2D<int>(1280, 720);
                GameWindow.WindowBorder = WindowBorder.Fixed;
                GameWindow.Position = new Vector2D<int>(200, 200);
                _isFullscreen = false;
            }
            else
            {
                // Switch to fullscreen mode (1920x1080, borderless)
                GameWindow.Size = new Vector2D<int>(1920, 1080);
                GameWindow.WindowBorder = WindowBorder.Hidden;
                GameWindow.Position = new Vector2D<int>(0, 0); // Ensure it covers the screen
                _isFullscreen = true;
            }

            // Force framebuffer resize to update ScreenWidth/ScreenHeight and viewport
            OnFramebufferResize(GameWindow.Size);
        }
    }
}