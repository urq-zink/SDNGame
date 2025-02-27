using SDNGame.Camera;
using SDNGame.Core;
using SDNGame.Input;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using SDNGame.UI;
using Silk.NET.OpenGL;

namespace SDNGame.Scenes
{
    public abstract class Scene : IDisposable
    {
        protected Game Game { get; private set; }
        protected GL Gl => Game.Gl;
        protected Camera2D Camera => Game.Camera;
        protected SpriteBatch SpriteBatch => Game.SpriteBatch;
        protected ShapeRenderer ShapeRenderer => Game.ShapeRenderer;
        protected InputManager InputManager => Game.InputManager;
        protected UIManager UIManager { get; private set; }
        public int ScreenWidth => Game.ScreenWidth;
        public int ScreenHeight => Game.ScreenHeight;

        public Scene(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            UIManager = new UIManager(game);
        }

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void Update(double deltaTime)
        {
            UIManager.Update();
        }
        public virtual void Draw(double deltaTime)
        {
            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            UIManager.DrawShapes(ShapeRenderer);
            ShapeRenderer.End();

            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            UIManager.DrawSprites(SpriteBatch);
            SpriteBatch.End();
        }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Dispose() { }
    }
}