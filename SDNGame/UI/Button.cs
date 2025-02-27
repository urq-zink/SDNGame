using System.Numerics;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;

namespace SDNGame.UI
{
    public class Button : UIElement
    {
        private enum ButtonState { Normal, Hover, Pressed }
        private ButtonState _state = ButtonState.Normal;

        private readonly FontRenderer _fontRenderer;
        public string Text { get; set; }
        public TextStyle TextStyle { get; set; }
        public Vector4 NormalColor { get; set; } = new Vector4(0.5f, 0.5f, 0.5f, 1f);
        public Vector4 HoverColor { get; set; } = new Vector4(0.7f, 0.7f, 0.7f, 1f);
        public Vector4 PressedColor { get; set; } = new Vector4(0.3f, 0.3f, 0.3f, 1f);
        public event Action OnClick;

        public Button(FontRenderer fontRenderer, Vector2 position, Vector2 size, string text, TextStyle textStyle)
        {
            _fontRenderer = fontRenderer;
            Position = position;
            Size = size;
            Text = text;
            TextStyle = textStyle;
        }

        public override void OnHover()
        {
            if (_state == ButtonState.Normal)
                _state = ButtonState.Hover;
        }

        public override void OnHoverExit()
        {
            if (_state == ButtonState.Hover || _state == ButtonState.Pressed)
                _state = ButtonState.Normal;
        }

        public override void OnMouseDown()
        {
            if (_state == ButtonState.Hover)
                _state = ButtonState.Pressed;
        }

        public override void OnMouseUp()
        {
            if (_state == ButtonState.Pressed)
            {
                _state = ButtonState.Hover;
                OnClick?.Invoke();
            }
        }

        public override void OnMouseUpOutside()
        {
            if (_state == ButtonState.Pressed)
                _state = ButtonState.Normal;
        }

        public override void DrawShapes(ShapeRenderer shapeRenderer)
        {
            Vector4 color = _state switch
            {
                ButtonState.Normal => NormalColor,
                ButtonState.Hover => HoverColor,
                ButtonState.Pressed => PressedColor,
                _ => NormalColor
            };
            shapeRenderer.DrawRectangle(Position, Size, color, true);
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Vector2 textPos = Position + Size / 2f;
            spriteBatch.DrawText(_fontRenderer, Text, textPos, TextStyle);
        }
    }
}