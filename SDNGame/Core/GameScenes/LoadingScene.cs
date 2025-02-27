using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using Silk.NET.OpenGL;
using System.Numerics;

namespace SDNGame.Core.GameScenes
{
    public class LoadingScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle loadingStyle;
        private float timer = 0f;
        private readonly float baseDuration;
        private float totalDuration;
        private readonly Scene nextScene;
        private bool isDelaySimulated = false;

        public LoadingScene(Game game, float duration = 2f, Scene nextScene = null) : base(game)
        {
            baseDuration = duration;
            this.nextScene = nextScene;
            totalDuration = duration;
        }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "Hubot Sans");

            loadingStyle = new TextStyle
            {
                FontFamily = "Hubot Sans",
                FontSize = 48f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            SimulateLoadingDelay();
        }

        private async void SimulateLoadingDelay()
        {
            await Task.Delay(1000); // 1-second simulated delay
            totalDuration = baseDuration + 1f;
            isDelaySimulated = true;
        }

        public override void Update(double deltaTime)
        {
            if (!isDelaySimulated) return;

            timer += (float)deltaTime;

            if (timer >= totalDuration)
            {
                var outgoing = new ZoomAndRotateTransition(Game, 0.6f, false, 1f, 2f, 0f, 0.1f);
                var incoming = new ZoomAndRotateTransition(Game, 0.6f, true, 1f, 0.5f, 0f, -0.1f);
                if (nextScene != null)
                {
                    Game.SetScene(nextScene, outgoing, incoming);
                }
                else
                {
                    Game.SetScene(new MainMenuScene(Game), outgoing, incoming);
                }
            }
        }

        public override void Draw(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            if (!isDelaySimulated)
            {
                SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
                SpriteBatch.DrawText(fontRenderer, "Preparing...",
                    new Vector2(ScreenWidth / 2, ScreenHeight / 2), loadingStyle);
                SpriteBatch.End();
                return;
            }

            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            float maxWidth = ScreenWidth * 0.5f;
            float progress = Math.Clamp(timer / totalDuration, 0f, 1f);
            float easedProgress = Utils.Tween.EaseOutCubic(progress);
            float currentWidth = maxWidth * easedProgress;
            Vector2 position = new Vector2((ScreenWidth - maxWidth) / 2, ScreenHeight / 2 - 20);
            ShapeRenderer.DrawRectangle(
                position,
                new Vector2(currentWidth, 40),
                new Vector4(0, 1, 0, 1),
                true
            );
            ShapeRenderer.End();

            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            SpriteBatch.DrawText(fontRenderer, "Loading...",
                new Vector2(ScreenWidth / 2, ScreenHeight / 2 + 80), loadingStyle);
            SpriteBatch.End();
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
        }
    }
}