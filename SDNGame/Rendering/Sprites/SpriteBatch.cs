using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SDNGame.Camera;
using SDNGame.Rendering.Buffers;
using SDNGame.Rendering.Textures;
using Shader = SDNGame.Rendering.Shaders.Shader;
using Texture = SDNGame.Rendering.Textures.Texture;
using Silk.NET.Vulkan;

namespace SDNGame.Rendering.Sprites
{
    public unsafe class SpriteBatch : IDisposable
    {
        private const int MaxSprites = 10000;
        private const int VerticesPerSprite = 4;
        private const int IndicesPerSprite = 6;
        private const int FloatsPerVertex = 8;

        private readonly GL _gl;
        private readonly Shader _shader;
        private readonly float[] _vertexData;
        private int _spriteCount = 0;
        private readonly BufferObject<float> _vertexBuffer;
        private readonly BufferObject<uint> _indexBuffer;
        private readonly VertexArrayObject<float, uint> _vao;
        private Texture? _currentTexture;

        private Matrix4x4 _currentTransform;
        private readonly Vector2[] _cornerCache;

        private static readonly Vector2 DefaultSize = new(64, 64);
        private static readonly Vector2 DefaultOrigin = Vector2.Zero;
        private static readonly Vector4 DefaultColor = Vector4.One;

        public SpriteBatch(GL gl, Shader shader)
        {
            _gl = gl ?? throw new ArgumentNullException(nameof(gl));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
            _cornerCache = new Vector2[4];

            _vertexData = new float[MaxSprites * VerticesPerSprite * FloatsPerVertex];

            Span<uint> indices = stackalloc uint[MaxSprites * IndicesPerSprite];
            for (uint i = 0; i < MaxSprites; i++)
            {
                uint offset = i * VerticesPerSprite;
                int indexOffset = (int)(i * IndicesPerSprite);
                indices[indexOffset + 0] = offset + 0;
                indices[indexOffset + 1] = offset + 1;
                indices[indexOffset + 2] = offset + 2;
                indices[indexOffset + 3] = offset + 2;
                indices[indexOffset + 4] = offset + 3;
                indices[indexOffset + 5] = offset + 0;
            }

            _vertexBuffer = new BufferObject<float>(_gl, _vertexData, BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw);
            _indexBuffer = new BufferObject<uint>(_gl, indices, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw);

            _vao = new VertexArrayObject<float, uint>(_gl, _vertexBuffer, _indexBuffer);

            ConfigureVertexAttributes();
        }

        private void ConfigureVertexAttributes()
        {
            _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, FloatsPerVertex, 0); // Position
            _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, FloatsPerVertex, 2); // UV
            _vao.VertexAttributePointer(2, 4, VertexAttribPointerType.Float, FloatsPerVertex, 4); // Color
        }

        public void Begin(Camera2D camera, int screenWidth, int screenHeight)
        {
            ArgumentNullException.ThrowIfNull(camera);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(screenWidth);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(screenHeight);

            _spriteCount = 0;
            _currentTexture = null;

            Matrix4x4 view = camera.GetViewMatrix(screenWidth, screenHeight);
            Matrix4x4 projection = camera.GetProjectionMatrix(screenWidth, screenHeight);
            _currentTransform = Matrix4x4.Multiply(view, projection);

            _shader.Use();
            _shader.SetUniform("uMVP", _currentTransform);
        }

        public void Draw(Texture texture, Vector2 position)
        {
            Draw(texture, position, DefaultSize, DefaultOrigin, 0f, DefaultColor);
        }

        public void Draw(Sprite sprite)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite));
            Draw(sprite.Texture, sprite.Position, sprite.Size, sprite.Origin, sprite.Rotation, sprite.Color);
        }

        public void Draw(Texture texture, Vector2 position, Vector2 size)
        {
            Draw(texture, position, size, DefaultOrigin, 0f, DefaultColor);
        }

        public void Draw(Texture texture, Vector2 position, Vector2 size, Vector2 origin, float rotation = 0, Vector4 color = default)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            if (color == default) color = DefaultColor;

            if (_currentTexture != texture)
            {
                Flush();
                _currentTexture = texture;
                _currentTexture.Bind();
            }

            if (_spriteCount >= MaxSprites)
            {
                Flush();
            }

            CalculateSpriteCorners(position, size, origin, rotation);
            WriteVertexData(_spriteCount * VerticesPerSprite * FloatsPerVertex, color, null); // Default UVs

            _spriteCount++;
        }

        public void Draw(SpriteSheet sheet, string spriteName, Vector2 position, Vector2 origin, float rotation = 0f, Vector4 color = default, Vector2? scale = null)
        {
            if (sheet == null) throw new ArgumentNullException(nameof(sheet));

            if (color == default) color = DefaultColor;
            scale ??= Vector2.One;

            var (uvs, spriteSize) = sheet.GetSprite(spriteName);
            Vector2 scaledSize = spriteSize * scale.Value;

            if (_currentTexture != sheet.Texture)
            {
                Flush();
                _currentTexture = sheet.Texture;
                _currentTexture.Bind();
            }

            if (_spriteCount >= MaxSprites)
            {
                Flush();
            }

            CalculateSpriteCorners(position, scaledSize, origin, rotation);
            WriteVertexData(_spriteCount * VerticesPerSprite * FloatsPerVertex, color, uvs);

            _spriteCount++;
        }

        public void Draw(TextureRegion region, Vector2 position, Vector2 size, Vector2 origin, float rotation = 0f, Vector4 color = default)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));

            if (color == default) color = DefaultColor;

            if (_currentTexture != region.Texture)
            {
                Flush();
                _currentTexture = region.Texture;
                _currentTexture.Bind();
            }

            if (_spriteCount >= MaxSprites)
            {
                Flush();
            }

            CalculateSpriteCorners(position, size, origin, rotation);
            WriteVertexData(_spriteCount * VerticesPerSprite * FloatsPerVertex, color, region.UVs);

            _spriteCount++;
        }

        public void DrawBatch(Sprite[] sprites, int startIndex, int count)
        {
            if (sprites == null) throw new ArgumentNullException(nameof(sprites));
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > sprites.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            Texture? lastTexture = null;

            for (int i = 0; i < count; i++)
            {
                var sprite = sprites[startIndex + i];
                if (sprite == null) continue;

                if (lastTexture != sprite.Texture || _spriteCount >= MaxSprites)
                {
                    Flush();
                    lastTexture = sprite.Texture;
                    _currentTexture = lastTexture;
                    _currentTexture?.Bind();
                }

                CalculateSpriteCorners(sprite.Position, sprite.Size, sprite.Origin, sprite.Rotation);
                WriteVertexData(_spriteCount * VerticesPerSprite * FloatsPerVertex, sprite.Color, null); // Default UVs

                _spriteCount++;
            }
        }

        private void CalculateSpriteCorners(Vector2 position, Vector2 size, Vector2 origin, float rotation)
        {
            if (MathF.Abs(rotation) < float.Epsilon)
            {
                Vector2 topLeft = position - (size * origin);
                _cornerCache[0] = topLeft;                               // Top-left
                _cornerCache[1] = topLeft + new Vector2(size.X, 0);      // Top-right
                _cornerCache[2] = topLeft + size;                        // Bottom-right
                _cornerCache[3] = topLeft + new Vector2(0, size.Y);      // Bottom-left
            }
            else
            {
                float cos = MathF.Cos(rotation);
                float sin = MathF.Sin(rotation);

                Vector2[] localVertices = new Vector2[4];
                localVertices[0] = new Vector2(-origin.X * size.X, -origin.Y * size.Y);                  // Top-left
                localVertices[1] = new Vector2((1 - origin.X) * size.X, -origin.Y * size.Y);            // Top-right
                localVertices[2] = new Vector2((1 - origin.X) * size.X, (1 - origin.Y) * size.Y);       // Bottom-right
                localVertices[3] = new Vector2(-origin.X * size.X, (1 - origin.Y) * size.Y);            // Bottom-left

                for (int i = 0; i < 4; i++)
                {
                    float x = localVertices[i].X;
                    float y = localVertices[i].Y;
                    float rx = (x * cos) - (y * sin);
                    float ry = (x * sin) + (y * cos);
                    _cornerCache[i] = position + new Vector2(rx, ry);
                }
            }
        }

        private void WriteVertexData(int vertexOffset, Vector4 color, Vector2[]? customUvs)
        {
            ReadOnlySpan<Vector2> defaultUvs = stackalloc Vector2[4]
            {
                new(0, 0), new(1, 0), new(1, 1), new(0, 1)
            };
            var uvs = customUvs ?? defaultUvs.ToArray();

            for (int i = 0; i < 4; i++)
            {
                int offset = vertexOffset + (i * FloatsPerVertex);
                _vertexData[offset + 0] = _cornerCache[i].X;
                _vertexData[offset + 1] = _cornerCache[i].Y;
                _vertexData[offset + 2] = uvs[i].X;
                _vertexData[offset + 3] = uvs[i].Y;
                _vertexData[offset + 4] = color.X;
                _vertexData[offset + 5] = color.Y;
                _vertexData[offset + 6] = color.Z;
                _vertexData[offset + 7] = color.W;
            }
        }

        private void Flush()
        {
            if (_spriteCount == 0) return;

            int dataSize = _spriteCount * VerticesPerSprite * FloatsPerVertex;

            _vertexBuffer.Bind();
            fixed (float* dataPtr = &_vertexData[0])
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(dataSize * sizeof(float)), dataPtr);
            }

            _vao.Bind();
            _gl.DrawElements(PrimitiveType.Triangles, (uint)(_spriteCount * IndicesPerSprite),
                DrawElementsType.UnsignedInt, null);

            _spriteCount = 0;
        }

        public void End()
        {
            Flush();
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _vao.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}