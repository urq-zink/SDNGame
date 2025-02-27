using Newtonsoft.Json;
using SDNGame.Rendering.Sprites;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SDNGame.Rendering.Textures
{
    public class Texture : IDisposable
    {
        private uint _handle;
        private readonly GL _gl;
        private Dictionary<string, SpriteRegion> _spriteRegions;

        public uint Handle => _handle;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsAtlas => _spriteRegions != null;
        public IReadOnlyDictionary<string, SpriteRegion> SpriteRegions => _spriteRegions;

        public unsafe Texture(GL gl, string path)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            using (var img = Image.Load<Rgba32>(path))
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba8,
                    (uint)img.Width,
                    (uint)img.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    null
                );

                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            _gl.TexSubImage2D(
                                TextureTarget.Texture2D,
                                0,
                                0,
                                y,
                                (uint)accessor.Width,
                                1,
                                PixelFormat.Rgba,
                                PixelType.UnsignedByte,
                                data
                            );
                        }
                    }
                });
                SetParameters();
            }
        }

        public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            fixed (void* dPtr = &data[0])
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    width,
                    height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    dPtr
                );
            }
            SetParameters();
        }

        public unsafe Texture(GL gl, Image<Rgba32> image)
        {
            _gl = gl;
            _handle = _gl.GenTexture();
            Bind();

            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)image.Width,
                (uint)image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        _gl.TexSubImage2D(
                            TextureTarget.Texture2D,
                            0,
                            0,
                            y,
                            (uint)accessor.Width,
                            1,
                            PixelFormat.Rgba,
                            PixelType.UnsignedByte,
                            data
                        );
                    }
                }
            });

            SetParameters();
        }

        public unsafe Texture(GL gl, int width, int height)
        {
            _gl = gl;
            _handle = _gl.GenTexture();
            Bind();

            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)width,
                (uint)height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            SetParameters();
        }

        public unsafe Texture(GL gl, string path, string? atlasMetaPath = null)
        {
            _gl = gl;
            _handle = _gl.GenTexture();
            Bind();

            using var img = Image.Load<Rgba32>(path);
            Width = img.Width;
            Height = img.Height;

            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)img.Width,
                (uint)img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < img.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        _gl.TexSubImage2D(
                            TextureTarget.Texture2D,
                            0,
                            0,
                            y,
                            (uint)accessor.Width,
                            1,
                            PixelFormat.Rgba,
                            PixelType.UnsignedByte,
                            data
                        );
                    }
                }
            });

            SetParameters();

            if(atlasMetaPath != null && File.Exists(atlasMetaPath))
            {
                string json = File.ReadAllText(atlasMetaPath);
                _spriteRegions = JsonConvert.DeserializeObject<Dictionary<string, SpriteRegion>>(json);
            }
        }

        public void EnableBlend()
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void SetParameters()
        {
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);

            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void SetFilter(TextureMinFilter minParam, TextureMagFilter magParam)
        {
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minParam);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magParam);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public unsafe void Resize(int newWidth, int newHeight)
        {
            Bind();

            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)newWidth,
                (uint)newHeight,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            SetParameters();
        }

        public void Dispose()
        {
            _gl.DeleteTexture(_handle);
            GC.SuppressFinalize(this);
        }
    }
}
