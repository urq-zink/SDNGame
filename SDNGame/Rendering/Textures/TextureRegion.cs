using System.Numerics;

namespace SDNGame.Rendering.Textures
{
    public class TextureRegion
    {
        public Texture Texture { get; }
        public Vector2 Postion { get; }
        public Vector2 Size { get; }
        public Vector2[] UVs { get; }

        public TextureRegion(Texture texture, float x, float y, float width, float height)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            if (x <= 0 || y <= 0 || width <= 0 || height <= 0 ||
                x + width > texture.Width || y + height > texture.Height)
                throw new ArgumentOutOfRangeException("Region must be within texture bounds.");

            Postion = new Vector2(x, y);
            Size = new Vector2(width, height);

            float uMin = x / texture.Width;
            float vMin = y / texture.Height;
            float uMax = (x + width) / texture.Width;
            float vMax = (y + height) / texture.Height;

            UVs = new[]
            {
                new Vector2(uMin, vMin),
                new Vector2(uMax, vMin),
                new Vector2(uMax, vMax),
                new Vector2(uMin, vMax)
            };
        }

        public TextureRegion(Texture texture) : this(texture, 0f, 0f, texture.Width, texture.Height) { }
    }
}
