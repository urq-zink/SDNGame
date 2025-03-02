using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Sprites;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using SDNGame.UI;
using System.Numerics;

namespace SDNGame.Core.GameScenes
{
    public class MainMenuScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle titleStyle;
        private TextStyle buttonStyle;
        private UIManager uiManager => UIManager;

        public MainMenuScene(Game game) : base(game) { }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "Hubot Sans");

            titleStyle = new TextStyle
            {
                FontFamily = "Hubot Sans",
                FontSize = 72f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            buttonStyle = new TextStyle
            {
                FontFamily = "Hubot Sans",
                FontSize = 36f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            float centerX = ScreenWidth / 2f;
            float buttonWidth = 300f;
            float buttonHeight = 60f;
            float spacing = 80f;
            float startY = ScreenHeight / 2f;

            // Add UI Buttons
            var collisionDemoButton = new Button(fontRenderer,
                new Vector2(centerX - buttonWidth / 2, startY),
                new Vector2(buttonWidth, buttonHeight),
                "Collision",
                buttonStyle);
            collisionDemoButton.OnClick += () =>
            {
                var outgoing = new ZoomAndRotateTransition(Game, 1f, false, 1f, 2f, 0f, 0.2f);
                var incoming = new ZoomAndRotateTransition(Game, 1f, true, 1f, 2f, 0f, -0.2f);
                Game.SetScene(new DemoScene(Game), outgoing, incoming);
            };

            var circleHuntButton = new Button(fontRenderer,
                new Vector2(centerX - buttonWidth / 2, startY + spacing),
                new Vector2(buttonWidth, buttonHeight),
                "Circle Hunt",
                buttonStyle);
            circleHuntButton.OnClick += () =>
            {
                var outgoing = new FadeTransition(Game, 0.5f, false);
                var incoming = new FadeTransition(Game, 0.5f, true);
                Game.SetScene(new CircleHuntScene(Game), outgoing, incoming);
            };

            var textureDemoButton = new Button(fontRenderer,
                new Vector2(centerX - buttonWidth / 2, startY + spacing * 2),
                new Vector2(buttonWidth, buttonHeight),
                "Texture",
                buttonStyle);
            textureDemoButton.OnClick += () =>
            {
                var outgoing = new ZoomAndRotateTransition(Game, 0.8f, false, 1f, 1.5f, 0f, 0.1f);
                var incoming = new ZoomAndRotateTransition(Game, 0.8f, true, 1f, 1.5f, 0f, -0.1f);
                Game.SetScene(new TextureDemoScene(Game), outgoing, incoming);
            };

            var messageBoxDemoButton = new Button(fontRenderer,
                new Vector2(centerX - buttonWidth / 2, startY + spacing * 3),
                new Vector2(buttonWidth, buttonHeight),
                "MessageBox",
                buttonStyle);
            messageBoxDemoButton.OnClick += () =>
            {
                var outgoing = new FadeTransition(Game, 0.5f, false);
                var incoming = new FadeTransition(Game, 0.5f, true);
                Game.SetScene(new MessageBoxDemoScene(Game), outgoing, incoming);
            };

            var softBodyButton = new Button(fontRenderer,
                new Vector2(centerX - buttonWidth / 2, startY + spacing * 4),
                new Vector2(buttonWidth, buttonHeight),
                "Soft Body Sim",
                buttonStyle);
            softBodyButton.OnClick += () =>
            {
                var outgoing = new FadeTransition(Game, 0.5f, false);
                var incoming = new FadeTransition(Game, 0.5f, true);
                Game.SetScene(new SoftBodyScene(Game), outgoing, incoming);
            };

            uiManager.AddElement(collisionDemoButton);
            uiManager.AddElement(circleHuntButton);
            uiManager.AddElement(textureDemoButton);
            uiManager.AddElement(messageBoxDemoButton);
            uiManager.AddElement(softBodyButton);
        }

        public override void Update(double deltaTime)
        {
            // UIManager handles input, so we don't need key checks here anymore
            base.Update(deltaTime); // This calls UIManager.Update()
        }

        public override void Draw(double deltaTime)
        {
            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            SpriteBatch.DrawText(fontRenderer, "SDNGame Framework Demo",
                new Vector2(ScreenWidth / 2, ScreenHeight / 4), titleStyle);
            SpriteBatch.End();

            base.Draw(deltaTime); // This draws the UI elements
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
        }
    }
}