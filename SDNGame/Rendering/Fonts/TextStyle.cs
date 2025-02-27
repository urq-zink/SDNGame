using System.Numerics;

namespace SDNGame.Rendering.Fonts
{
    public class TextStyle
    {
        public string FontFamily { get; set; } = "Arial";
        public float FontSize { get; set; } = 32f;
        public Vector4 Color { get; set; } = new Vector4(1, 1, 1, 1);
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    }
}