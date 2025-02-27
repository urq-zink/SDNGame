using SDNGame.Rendering.Fonts;
using SDNGame.Scenes;
using SDNGame.UI;
using System.Numerics;

namespace SDNGame.Core.GameScenes
{
    public class MessageBoxDemoScene : Scene
    {
        private FontRenderer _fontRenderer;
        private Label _statusLabel;
        private UIManager _uiManager => UIManager;

        public MessageBoxDemoScene(Game game) : base(game) { }

        public override void LoadContent()
        {
            _fontRenderer = new FontRenderer(Gl, SpriteBatch);
            _fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");

            // Create show message box button
            var showButton = new Button(_fontRenderer,
                new Vector2(ScreenWidth / 2 - 100, ScreenHeight - 120),
                new Vector2(200, 40),
                "Show",
                new TextStyle { FontFamily = "HubotSans", FontSize = 24f, Color = Vector4.One, Alignment = TextAlignment.Center });

            showButton.OnClick += ShowMessageBox;

            // Status label
            _statusLabel = new Label(_fontRenderer,
                new Vector2(ScreenWidth / 2, ScreenHeight - 50),
                "Status: ",
                new TextStyle { FontFamily = "HubotSans", FontSize = 24f, Color = Vector4.One, Alignment = TextAlignment.Center });

            // Back button
            var backButton = new Button(_fontRenderer,
                new Vector2(50, ScreenHeight - 60),
                new Vector2(100, 40),
                "Back",
                new TextStyle { FontFamily = "HubotSans", FontSize = 24f, Color = Vector4.One, Alignment = TextAlignment.Center });

            backButton.OnClick += () => Game.SetScene(new MainMenuScene(Game));

            _uiManager.AddElement(showButton);
            _uiManager.AddElement(_statusLabel);
            _uiManager.AddElement(backButton);
        }

        private void ShowMessageBox()
        {
            var buttons = new (string text, Action action)[]
            {
                ("OK", () => _statusLabel.Text = "Status: OK clicked"),
                ("Cancel", () => _statusLabel.Text = "Status: Cancel clicked")
            };

            var messageBox = new MessageBox(
                _fontRenderer,
                Game,
                new Vector2(ScreenWidth / 2, ScreenHeight / 2),
                "Message title",
                "This is a line of infomation.\nMultiple line support too.",
                buttons,
                hasCloseButton: true,
                titleStyle: new TextStyle { FontFamily = "HubotSans", FontSize = 28f, Color = new Vector4(1, 0.8f, 0, 1) },
                messageStyle: new TextStyle { FontFamily = "HubotSans", FontSize = 20f, Color = new Vector4(0.9f, 0.9f, 0.9f, 1) }
            )
            {
                BorderColor = new Vector4(0, 0.5f, 1, 1),
                OverlayColor = new Vector4(0, 0, 0, 0.6f),
                Padding = 25f,
            };

            _uiManager.AddElement(messageBox);
        }

        public override void Dispose()
        {
            _fontRenderer?.Dispose();
        }
    }
}