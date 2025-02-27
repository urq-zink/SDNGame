using SDNGame.Input;
using SDNGame.Physics;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using Silk.NET.Input;
using System.Numerics;
using SDNGame.Rendering.Sprites;
using SDNGame.UI;
using Button = SDNGame.UI.Button;

namespace SDNGame.Core.GameScenes
{
    public class CircleHuntScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle fontStyle;
        private TextStyle timerStyle;
        private TextStyle buttonStyle;
        private UIManager uiManager => UIManager;

        private List<(Collider collider, Vector4 color)> circles;
        private Collider cursorCollider;
        private float timeRemaining = 10f;
        private Random random = new Random();
        private bool gameOver = false;
        private int currentLevel = 1;
        private int baseCircleCount = 5;
        private const float TIME_PER_LEVEL = 10f;

        public CircleHuntScene(Game game) : base(game)
        {
            circles = new List<(Collider, Vector4)>();
        }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");

            fontStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 32f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Left
            };

            timerStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 48f,
                Color = new Vector4(1, 0, 0, 1),
                Alignment = TextAlignment.Center
            };

            buttonStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            cursorCollider = Collider.CreateCircle(InputManager.MousePosition, 20f);
            SpawnCirclesForLevel();

            // Add UI Elements
            var instructions = new Label(fontRenderer,
                new Vector2(50, 50),
                "Move cursor over circles and press LeftMouse or Space to remove them!",
                fontStyle);
            uiManager.AddElement(instructions);

            var statusLabel = new Label(fontRenderer,
                new Vector2(50, 100),
                $"Level: {currentLevel}  Circles Left: {circles.Count}",
                fontStyle);
            uiManager.AddElement(statusLabel);

            var backButton = new Button(fontRenderer,
                new Vector2(50, ScreenHeight - 60),
                new Vector2(100, 40),
                "Back",
                buttonStyle);
            backButton.OnClick += () => GameOver(true); // Treat as success to return to menu
            uiManager.AddElement(backButton);
        }

        private void SpawnCirclesForLevel()
        {
            circles.Clear();
            int circleCount = baseCircleCount + (currentLevel - 1) * 2;
            for (int i = 0; i < circleCount; i++)
            {
                Vector2 position = new Vector2(
                    random.Next(50, ScreenWidth - 50),
                    random.Next(50, ScreenHeight - 50)
                );
                float radius = random.Next(20, 50);
                Vector4 color = new Vector4(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    1f
                );
                circles.Add((Collider.CreateCircle(position, radius), color));
            }
            timeRemaining = TIME_PER_LEVEL;
        }

        public override void Update(double deltaTime)
        {
            if (gameOver) return;

            cursorCollider.SetPosition(InputManager.MousePosition);
            timeRemaining -= (float)deltaTime;

            if (timeRemaining <= 0)
            {
                GameOver(false);
                return;
            }

            if (InputManager.IsKeyPressed(Key.Space) || InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                for (int i = circles.Count - 1; i >= 0; i--)
                {
                    if (cursorCollider.CollidesWith(circles[i].collider))
                    {
                        circles.RemoveAt(i);
                    }
                }
            }

            if (circles.Count == 0)
            {
                currentLevel++;
                SpawnCirclesForLevel();
            }

            // Update UI
            var statusLabel = uiManager.Elements[1] as Label;
            statusLabel.Text = $"Level: {currentLevel}  Circles Left: {circles.Count}";

            base.Update(deltaTime);
        }

        private void GameOver(bool success)
        {
            gameOver = true;
            var outgoing = new FadeTransition(Game, 0.5f, false);
            var incoming = new FadeTransition(Game, 0.5f, true);
            Game.SetScene(new MainMenuScene(Game), outgoing, incoming);
        }

        public override void Draw(double deltaTime)
        {
            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            for (int x = 0; x < ScreenWidth; x += 50)
            {
                ShapeRenderer.DrawLine(
                    new Vector2(x, 0),
                    new Vector2(x, ScreenHeight),
                    new Vector4(0.2f, 0.2f, 0.2f, 0.3f)
                );
            }
            for (int y = 0; y < ScreenHeight; y += 50)
            {
                ShapeRenderer.DrawLine(
                    new Vector2(0, y),
                    new Vector2(ScreenWidth, y),
                    new Vector4(0.2f, 0.2f, 0.2f, 0.3f)
                );
            }
            ShapeRenderer.End();

            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);

            foreach (var (collider, color) in circles)
            {
                ShapeRenderer.DrawCircle(collider.Position, collider.Radius, color, true);
            }

            Vector4 cursorColor = new Vector4(1, 1, 1, 0.8f);
            ShapeRenderer.DrawCircle(cursorCollider.Position, cursorCollider.Radius, cursorColor, false);

            ShapeRenderer.End();

            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            string timeText = timeRemaining > 0 ? $"Time: {timeRemaining:F1}" : "Time's Up!";
            SpriteBatch.DrawText(fontRenderer,
                timeText,
                new Vector2(ScreenWidth / 2, ScreenHeight - 100),
                timerStyle);
            SpriteBatch.End();

            base.Draw(deltaTime); // Draws UI elements
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
        }
    }
}