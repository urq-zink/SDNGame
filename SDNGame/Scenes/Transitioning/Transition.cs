using SDNGame.Camera;
using SDNGame.Core;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using Silk.NET.OpenGL;

namespace SDNGame.Scenes.Transitioning
{
    public abstract class Transition : IDisposable
    {
        protected Game Game { get; }
        protected GL Gl => Game.Gl;
        protected Camera2D Camera => Game.Camera;
        protected ShapeRenderer ShapeRenderer => Game.ShapeRenderer;
        protected SpriteBatch SpriteBatch => Game.SpriteBatch;
        protected int ScreenWidth => Game.ScreenWidth;
        protected int ScreenHeight => Game.ScreenHeight;

        public float Duration { get; set; }
        public bool IsComplete { get; protected set; }
        protected float ElapsedTime { get; set; }

        protected Transition(Game game, float duration)
        {
            Game = game;
            Duration = duration;
            ElapsedTime = 0;
            IsComplete = false;
        }

        public virtual void Start() => ElapsedTime = 0;
        public virtual void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
            if (ElapsedTime >= Duration)
            {
                ElapsedTime = Duration;
                IsComplete = true;
            }
        }

        public abstract void Draw(float deltaTime);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
