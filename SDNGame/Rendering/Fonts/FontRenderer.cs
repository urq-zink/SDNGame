using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Silk.NET.OpenGL;
using System.Numerics;
using SDNGame.Rendering.Sprites;
using Texture = SDNGame.Rendering.Textures.Texture;
using SixLabors.ImageSharp.Processing;

namespace SDNGame.Rendering.Fonts
{
    public class FontRenderer : IDisposable
    {
        private const int MaxCacheSize = 100;
        private readonly GL _gl;
        private readonly Dictionary<string, Font> _fonts = new();
        private readonly Dictionary<(string text, string fontFamily, float size), Texture> _textureCache = new();
        private readonly List<(string text, string fontFamily, float size)> _cacheQueue = new();
        private readonly SpriteBatch _spriteBatch;
        private bool _disposed;

        public FontRenderer(GL gl, SpriteBatch spriteBatch)
        {
            _gl = gl ?? throw new ArgumentNullException(nameof(gl));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        }

        public void LoadFont(string fontPath, string fontFamily)
        {
            var collection = new FontCollection();
            var family = collection.Add(fontPath);
            _fonts[fontFamily] = new Font(family, 32f); // Default size, will be scaled as needed
        }

        public Vector2 MeasureText(string text, string fontFamily, float fontSize)
        {
            if (!_fonts.TryGetValue(fontFamily, out var baseFont))
                throw new Exception($"Font family '{fontFamily}' not found.");

            var font = new Font(baseFont.Family, fontSize);
            var size = TextMeasurer.MeasureSize(text, new TextOptions(font));
            return new Vector2((float)size.Width * 1.01f, (float)size.Height * 1.25f);
        }

        private Texture CreateTextTexture(string text, string fontFamily, float fontSize, Color textColor)
        {
            if (!_fonts.TryGetValue(fontFamily, out var baseFont))
                throw new Exception($"Font family '{fontFamily}' not found.");

            var font = new Font(baseFont.Family, fontSize);
            var size = TextMeasurer.MeasureSize(text, new TextOptions(font));

            int width = (int)(Math.Ceiling(size.Width) * 1.01);
            int height = (int)(Math.Ceiling(size.Height) * 1.25);

            if (width <= 0 || height <= 0) return new Texture(_gl, 1, 1); // Fallback for empty text

            using var image = new Image<Rgba32>(width, height);
            image.Mutate(ctx =>
            {
                ctx.DrawText(text, font, textColor, new PointF(0, 0));
            });

            return new Texture(_gl, image);
        }

        public void DrawText(string text, Vector2 position, float fontSize, string fontFamily, Vector4 color, TextAlignment alignment, bool cacheText = true)
        {
            MaintainCache();

            var cacheKey = (text, fontFamily, fontSize);
            Texture texture;

            if (!cacheText)
            {
                texture = CreateTextTexture(text, fontFamily, fontSize, new Color(color));
            }
            else if (!_textureCache.TryGetValue(cacheKey, out texture))
            {
                texture = CreateTextTexture(text, fontFamily, fontSize, new Color(color));
                _textureCache[cacheKey] = texture;
                _cacheQueue.Add(cacheKey);
            }

            var size = MeasureText(text, fontFamily, fontSize);
            Vector2 adjustedPosition = AdjustPosition(position, size, alignment);

            _spriteBatch.Draw(texture, adjustedPosition, size);
        }

        private Vector2 AdjustPosition(Vector2 position, Vector2 size, TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Center:
                    return position - new Vector2(0, size.Y / 2f);
                case TextAlignment.Right:
                    return position - new Vector2(size.X, 0);
                case TextAlignment.Left:
                default:
                    return position;
            }
        }

        private void MaintainCache()
        {
            while (_textureCache.Count > MaxCacheSize)
            {
                var oldest = _cacheQueue[0];
                _textureCache[oldest]?.Dispose();
                _textureCache.Remove(oldest);
                _cacheQueue.RemoveAt(0);
            }
        }

        public void ClearCache()
        {
            foreach (var texture in _textureCache.Values)
            {
                texture?.Dispose();
            }
            _textureCache.Clear();
            _cacheQueue.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            ClearCache();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}