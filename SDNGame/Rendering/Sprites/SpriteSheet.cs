using SDNGame.Rendering.Sprites;
using System.Numerics;

namespace SDNGame.Rendering.Textures
{
    public class SpriteSheet
    {
        private readonly Texture _texture;
        public Texture Texture => _texture;

        public SpriteSheet(Texture texture)
        {
            if (!texture.IsAtlas)
                throw new ArgumentException("Texture must be an atlas with sprite regions.");
            _texture = texture;
        }

        public (Vector2[] uvs, Vector2 size) GetSprite(string spriteName)
        {
            if (!_texture.SpriteRegions.TryGetValue(spriteName, out SpriteRegion region))
                throw new ArgumentException($"Sprite '{spriteName}' not found in atlas.");

            float texWidth = _texture.Width;
            float texHeight = _texture.Height;

            float uMin = region.X / texWidth;
            float vMin = region.Y / texHeight;
            float uMax = (region.X + region.Width) / texWidth;
            float vMax = (region.Y + region.Height) / texHeight;

            Vector2[] uvs = new[]
            {
                new Vector2(uMin, vMin),
                new Vector2(uMax, vMin),
                new Vector2(uMax, vMax),
                new Vector2(uMin, vMax)
            };

            return (uvs, new Vector2(region.Width, region.Height));
        }
    }
}