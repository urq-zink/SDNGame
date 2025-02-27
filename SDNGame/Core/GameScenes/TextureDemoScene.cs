using SDNGame.Input;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Sprites;
using SDNGame.Rendering.Textures;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using SDNGame.UI;
using System.Numerics;
using Button = SDNGame.UI.Button;

namespace SDNGame.Core.GameScenes
{
    public class TextureDemoScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle titleStyle;
        private TextStyle infoStyle;
        private TextStyle buttonStyle;
        private UIManager uiManager => UIManager;

        private List<Sprite> sprites;
        private Texture flTexture;
        private Texture collectibleTexture;
        private Texture shieldTexture;
        private float time = 0f; // Global animation time

        public TextureDemoScene(Game game) : base(game)
        {
            sprites = new List<Sprite>();
        }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");

            titleStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 48f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            infoStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 24f,
                Color = new Vector4(0.8f, 0.8f, 1f, 1f),
                Alignment = TextAlignment.Left
            };

            buttonStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            // Load textures
            flTexture = new Texture(Gl, "Assets/Textures/fl.png");
            collectibleTexture = new Texture(Gl, "Assets/Textures/collectible.png");
            shieldTexture = new Texture(Gl, "Assets/Textures/shield.png");

            SetupSprites();

            // Add UI Elements
            var infoLabel = new Label(fontRenderer,
                new Vector2(50, 150),
                "Features:\n- Texture loading\n- Animated rotation\n- Animated scaling\n- Animated positioning\n- Multiple tinted sprites\n- Mouse interaction",
                infoStyle);
            uiManager.AddElement(infoLabel);

            var backButton = new Button(fontRenderer,
                new Vector2(50, ScreenHeight - 60),
                new Vector2(100, 40),
                "Back",
                buttonStyle);
            backButton.OnClick += () =>
            {
                var outgoing = new FadeTransition(Game, 0.5f, false);
                var incoming = new FadeTransition(Game, 0.5f, true);
                Game.SetScene(new MainMenuScene(Game), outgoing, incoming);
            };
            uiManager.AddElement(backButton);
        }

        private void SetupSprites()
        {
            // Base positions for sprites
            Vector2 center = new Vector2(ScreenWidth / 2, ScreenHeight / 2);

            // Sprite 1: Rotating and scaling flTexture
            sprites.Add(new Sprite(
                flTexture,
                center + new Vector2(-200, -100),
                new Vector2(100, 100),
                new Vector2(0.5f, 0.5f),
                new Vector4(1f, 0.7f, 0.7f, 1f) // Red tint
            ));

            // Sprite 2: Moving and scaling shieldTexture
            sprites.Add(new Sprite(
                shieldTexture,
                center + new Vector2(200, -100),
                new Vector2(120, 120),
                new Vector2(0.5f, 0.5f),
                new Vector4(0.7f, 1f, 0.7f, 1f) // Green tint
            ));

            // Sprite 3: Mouse-tracking collectibleTexture
            sprites.Add(new Sprite(
                collectibleTexture,
                center,
                new Vector2(80, 80),
                new Vector2(0.5f, 0.5f),
                new Vector4(0.7f, 0.7f, 1f, 1f) // Blue tint
            ));

            // Additional tinted collectibleTexture sprites
            sprites.Add(new Sprite(
                collectibleTexture,
                center + new Vector2(-150, 100),
                new Vector2(60, 60),
                new Vector2(0.5f, 0.5f),
                new Vector4(1f, 1f, 0.7f, 1f) // Yellow tint
            ));

            sprites.Add(new Sprite(
                collectibleTexture,
                center + new Vector2(150, 100),
                new Vector2(60, 60),
                new Vector2(0.5f, 0.5f),
                new Vector4(1f, 0.7f, 1f, 1f) // Purple tint
            ));
        }

        public override void Update(double deltaTime)
        {
            time += (float)deltaTime;
            Vector2 mousePosition = InputManager.MousePosition;

            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                switch (i)
                {
                    case 0: // Rotating and scaling flTexture
                        sprite.Rotation = MathF.Sin(time) * MathF.PI; // Rotate between -π and π
                        float scale1 = 1f + MathF.Sin(time * 1.5f) * 0.3f; // Scale between 0.7 and 1.3
                        sprite.Size = new Vector2(100, 100) * scale1;
                        break;

                    case 1: // Moving and scaling shieldTexture
                        float moveOffset = MathF.Sin(time * 0.8f) * 50f; // Move ±50 units
                        sprite.Position = new Vector2(ScreenWidth / 2 + 200, ScreenHeight / 2 - 100) + new Vector2(moveOffset, moveOffset);
                        float scale2 = 1f + MathF.Cos(time) * 0.2f; // Scale between 0.8 and 1.2
                        sprite.Size = new Vector2(120, 120) * scale2;
                        break;

                    case 2: // Mouse-tracking collectibleTexture
                        sprite.Position = mousePosition;
                        sprite.Rotation = time * 2f; // Continuous rotation
                        float scale3 = 1f + (float)Math.Sin(time * 2f) * 0.2f; // Scale between 0.8 and 1.2
                        sprite.Size = new Vector2(80, 80) * scale3;
                        break;

                    case 3: // Tinted collectibleTexture (yellow)
                        sprite.Position = new Vector2(ScreenWidth / 2 - 150, ScreenHeight / 2 + 100) +
                                         new Vector2(MathF.Cos(time) * 30f, MathF.Sin(time * 1.2f) * 30f);
                        sprite.Rotation = -time;
                        sprite.Size = new Vector2(60, 60) * (1f + MathF.Sin(time * 1.3f) * 0.15f);
                        break;

                    case 4: // Tinted collectibleTexture (purple)
                        sprite.Position = new Vector2(ScreenWidth / 2 + 150, ScreenHeight / 2 + 100) +
                                         new Vector2(MathF.Sin(time * 1.1f) * 30f, MathF.Cos(time) * 30f);
                        sprite.Rotation = time * 1.5f;
                        sprite.Size = new Vector2(60, 60) * (1f + MathF.Cos(time * 1.4f) * 0.15f);
                        break;
                }
            }

            base.Update(deltaTime); // Updates UI elements
        }

        public override void Draw(double deltaTime)
        {
            // Draw background grid
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

            // Draw sprites
            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            foreach (var sprite in sprites)
            {
                SpriteBatch.Draw(sprite);
            }

            SpriteBatch.DrawText(fontRenderer,
                "Texture & Sprite Demo",
                new Vector2(ScreenWidth / 2, 50),
                titleStyle);

            SpriteBatch.End();

            base.Draw(deltaTime); // Draws UI elements
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
            collectibleTexture?.Dispose();
            shieldTexture?.Dispose();
            flTexture?.Dispose();
            sprites.Clear();
        }
    }
}