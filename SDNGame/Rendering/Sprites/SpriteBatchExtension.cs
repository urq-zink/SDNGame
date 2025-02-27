using System.Numerics;
using SDNGame.Rendering.Fonts;

namespace SDNGame.Rendering.Sprites
{
    public static class SpriteBatchExtensions
    {
        public static void DrawText(this SpriteBatch spriteBatch, FontRenderer fontRenderer, string text,
            Vector2 position, TextStyle style)
        {
            var size = fontRenderer.MeasureText(text, style.FontFamily, style.FontSize);
            var adjustedPosition = position;

            switch (style.Alignment)
            {
                case TextAlignment.Center:
                    adjustedPosition.X -= size.X / 2;
                    break;
                case TextAlignment.Right:
                    adjustedPosition.X -= size.X;
                    break;
            }

            fontRenderer.DrawText(text, adjustedPosition, style.FontSize, style.FontFamily, style.Color, TextAlignment.Center);
        }
    }
}
