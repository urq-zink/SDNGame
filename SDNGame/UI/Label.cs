using System.Numerics;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;

namespace SDNGame.UI
{
    public class Label : UIElement
    {
        private readonly FontRenderer _fontRenderer;
        public string Text { get; set; }
        public TextStyle TextStyle { get; set; }

        public Label(FontRenderer fontRenderer, Vector2 position, string text, TextStyle textStyle)
        {
            _fontRenderer = fontRenderer;
            Position = position;
            Text = text;
            TextStyle = textStyle;
            Size = _fontRenderer.MeasureText(text, textStyle.FontFamily, textStyle.FontSize); // Auto-size based on text
        }

        public override void DrawShapes(ShapeRenderer shapeRenderer)
        {
            // No shapes by default; override if a background is needed
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawText(_fontRenderer, Text, Position, TextStyle);
        }
    }
}