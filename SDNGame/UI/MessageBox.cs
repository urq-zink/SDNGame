using System.Numerics;
using SDNGame.Core;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;

namespace SDNGame.UI
{
    public class MessageBox : UIElement
    {
        private readonly FontRenderer _fontRenderer;
        private readonly Game _game;
        private readonly Label _titleLabel;
        private readonly Label _messageLabel;
        private readonly List<(Button button, Vector2 offset)> _buttons = new();
        private readonly bool _hasCloseButton;
        private (Button button, Vector2 offset)? _closeButton;

        // Appearance customization
        public Vector4 OverlayColor { get; set; } = new Vector4(0, 1, 0, 0.5f); // Dimming overlay
        public Vector4 ContainerBackgroundColor { get; set; } = new Vector4(0.5f, 0.5f, 0.5f, 1f); // New container background
        public Vector4 BorderColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
        public float BorderThickness { get; set; } = 2f;
        public float CornerRadius { get; set; } = 10f;
        public float Padding { get; set; } = 20f;

        // Animation properties
        private float _alpha = 1f;
        private float _scale = 1f;
        private const float AnimationDuration = 0.3f;
        private float _animationTime = 0f;
        private bool _isAnimatingIn = true;

        public MessageBox(
            FontRenderer fontRenderer,
            Game game,
            Vector2 position,
            string title,
            string message,
            (string text, Action action)[] buttons,
            bool hasCloseButton = true,
            TextStyle titleStyle = null,
            TextStyle messageStyle = null)
        {
            _fontRenderer = fontRenderer ?? throw new ArgumentNullException(nameof(fontRenderer));
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _hasCloseButton = hasCloseButton;

            // Default styles if not provided
            titleStyle ??= new TextStyle { FontFamily = "HubotSans", FontSize = 24f, Color = Vector4.One, Alignment = TextAlignment.Left };
            messageStyle ??= new TextStyle { FontFamily = "HubotSans", FontSize = 18f, Color = new Vector4(0.8f, 0.8f, 0.8f, 1f), Alignment = TextAlignment.Left };

            // Measure text sizes
            var titleSize = _fontRenderer.MeasureText(title, titleStyle.FontFamily, titleStyle.FontSize);
            var messageSize = MeasureMultiLineText(message, messageStyle.FontFamily, messageStyle.FontSize, 400f);
            float buttonWidth = 80f;
            float buttonHeight = 30f;
            float buttonSpacing = 10f;

            // Calculate total size
            float totalButtonsWidth = (buttons.Length * buttonWidth) + ((buttons.Length - 1) * buttonSpacing);
            Size = new Vector2(
                Math.Max(Math.Max(titleSize.X, messageSize.X), totalButtonsWidth) + (Padding * 2),
                titleSize.Y + messageSize.Y + buttonHeight + (Padding * 3) + (hasCloseButton ? buttonHeight + Padding : 0)
            );
            Position = position - (Size / 2);

            // Initialize labels
            _titleLabel = new Label(_fontRenderer, Vector2.Zero, title, titleStyle);
            _messageLabel = new Label(_fontRenderer, Vector2.Zero, message, messageStyle);

            // Create buttons with relative offsets
            Vector2 buttonStartPos = new Vector2(
                (Size.X / 2) - (totalButtonsWidth / 2),
                Size.Y - buttonHeight - Padding
            );
            for (int i = 0; i < buttons.Length; i++)
            {
                var (text, action) = buttons[i];
                Vector2 offset = buttonStartPos + new Vector2(i * (buttonWidth + buttonSpacing), 0);
                var button = new Button(_fontRenderer,
                    Position + offset,
                    new Vector2(buttonWidth, buttonHeight),
                    text,
                    new TextStyle { FontFamily = "HubotSans", FontSize = 18f, Color = Vector4.One, Alignment = TextAlignment.Center })
                {
                    NormalColor = new Vector4(0.4f, 0.4f, 0.4f, 1f),
                    HoverColor = new Vector4(0.5f, 0.5f, 0.5f, 1f),
                    PressedColor = new Vector4(0.3f, 0.3f, 0.3f, 1f)
                };
                button.OnClick += () =>
                {
                    action?.Invoke();
                    Close();
                };
                _buttons.Add((button, offset));
            }

            // Add close button with relative offset
            if (_hasCloseButton)
            {
                Vector2 closeOffset = new Vector2(Size.X - buttonWidth, 0);
                _closeButton = (new Button(_fontRenderer,
                    Position + closeOffset,
                    new Vector2(buttonWidth, buttonHeight),
                    "X",
                    new TextStyle { FontFamily = "HubotSans", FontSize = 18f, Color = Vector4.One, Alignment = TextAlignment.Center })
                {
                    NormalColor = new Vector4(0.8f, 0.2f, 0.2f, 1f),
                    HoverColor = new Vector4(0.9f, 0.3f, 0.3f, 1f),
                    PressedColor = new Vector4(0.7f, 0.1f, 0.1f, 1f)
                }, closeOffset);
                _closeButton.Value.button.OnClick += Close;
            }
        }

        private Vector2 MeasureMultiLineText(string text, string fontFamily, float fontSize, float maxWidth)
        {
            string[] lines = text.Split('\n');
            float totalHeight = 0f;
            float maxLineWidth = 0f;
            foreach (var line in lines)
            {
                var size = _fontRenderer.MeasureText(line, fontFamily, fontSize);
                maxLineWidth = Math.Max(maxLineWidth, Math.Min(size.X, maxWidth));
                totalHeight += size.Y;
            }
            return new Vector2(maxLineWidth, totalHeight);
        }

        public override void DrawShapes(ShapeRenderer shapeRenderer)
        {
            if (!Visible) return;

            // Draw dimming overlay (covers the entire screen)
            shapeRenderer.DrawRectangle(Vector2.Zero, new Vector2(_game.ScreenWidth, _game.ScreenHeight), OverlayColor, true);

            // Apply animation transform
            Vector2 scaledSize = Size * _scale;
            Vector2 scaledPosition = Position + ((Size - scaledSize) / 2);

            // Draw container background (new solid background for the message box)
            shapeRenderer.DrawRectangle(
                scaledPosition,
                scaledSize,
                new Vector4(ContainerBackgroundColor.X, ContainerBackgroundColor.Y, ContainerBackgroundColor.Z, ContainerBackgroundColor.W * _alpha),
                true
            );

            // Draw border
            shapeRenderer.DrawRectangle(
                scaledPosition,
                scaledSize,
                new Vector4(BorderColor.X, BorderColor.Y, BorderColor.Z, BorderColor.W * _alpha),
                false
            );

            // Draw buttons
            foreach (var (button, _) in _buttons)
            {
                button.DrawShapes(shapeRenderer);
            }
            if (_hasCloseButton) _closeButton.Value.button.DrawShapes(shapeRenderer);
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            Vector2 scaledSize = Size * _scale;
            Vector2 scaledPosition = Position + ((Size - scaledSize) / 2);

            // Update label positions
            _titleLabel.Position = scaledPosition + new Vector2(Padding, Padding);
            _messageLabel.Position = scaledPosition + new Vector2(Padding, Padding + _fontRenderer.MeasureText(_titleLabel.Text, _titleLabel.TextStyle.FontFamily, _titleLabel.TextStyle.FontSize).Y + Padding);

            _titleLabel.DrawSprites(spriteBatch);
            _messageLabel.DrawSprites(spriteBatch);

            // Update button positions using stored offsets
            foreach (var (button, offset) in _buttons)
            {
                button.Position = scaledPosition + offset;
                button.DrawSprites(spriteBatch);
            }
            if (_hasCloseButton)
            {
                _closeButton.Value.button.Position = scaledPosition + _closeButton.Value.offset;
                _closeButton.Value.button.DrawSprites(spriteBatch);
            }
        }

        public void Update(double deltaTime)
        {
            if (_isAnimatingIn)
            {
                _animationTime += (float)deltaTime;
                float t = Math.Clamp(_animationTime / AnimationDuration, 0f, 1f);
                _alpha = Utils.Tween.EaseOutQuad(t);
                _scale = 0.8f + (Utils.Tween.EaseOutQuad(t) * 0.2f);
                if (t >= 1f) _isAnimatingIn = false;
            }
        }

        private void Close()
        {
            Visible = false;
            Enabled = false;
        }

        public override bool IsMouseOver(Vector2 mousePos)
        {
            if (!Visible || !Enabled) return false;

            if (_hasCloseButton && _closeButton.Value.button.IsMouseOver(mousePos)) return true;
            foreach (var (button, _) in _buttons)
            {
                if (button.IsMouseOver(mousePos)) return true;
            }
            return base.IsMouseOver(mousePos);
        }
    }
}