// File: SDNGame/Rendering/Shapes/ShapeRenderer.cs
using System.Numerics;
using Silk.NET.OpenGL;
using SDNGame.Camera;
using SDNGame.Rendering.Buffers;
using Shader = SDNGame.Rendering.Shaders.Shader;

namespace SDNGame.Rendering.Shapes
{
    public class ShapeRenderer : IDisposable
    {
        private const int MaxVertices = 10000;
        private const int FloatsPerVertex = 6;

        private readonly GL _gl;
        private readonly Shader _shader;
        private readonly float[] _vertexData;
        private int _vertexCount;
        private readonly BufferObject<float> _vertexBuffer;
        private readonly VertexArrayObject<float, uint> _vao;
        private PrimitiveType _currentPrimitive;
        private bool _isDrawing;
        private bool _disposed;

        public ShapeRenderer(GL gl, Shader shader)
        {
            _gl = gl ?? throw new ArgumentNullException(nameof(gl));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));

            _vertexData = new float[MaxVertices * FloatsPerVertex];
            _vertexBuffer = new BufferObject<float>(_gl, _vertexData.AsSpan(), BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw);
            _vao = new VertexArrayObject<float, uint>(_gl, _vertexBuffer, null);

            _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, FloatsPerVertex, 0); // Position
            _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, FloatsPerVertex, 2); // Color
        }

        public void Begin(Camera2D camera, int screenWidth, int screenHeight)
        {
            if (_isDrawing) throw new InvalidOperationException("ShapeRenderer is already in a drawing state. Call End() first.");

            _vertexCount = 0;
            _currentPrimitive = PrimitiveType.Triangles; // Default to triangles
            _isDrawing = true;

            Matrix4x4 view = camera.GetViewMatrix(screenWidth, screenHeight);
            Matrix4x4 projection = camera.GetProjectionMatrix(screenWidth, screenHeight);
            Matrix4x4 mvp = view * projection;

            _shader.Use();
            _shader.SetUniform("uMVP", mvp);
        }

        public void End()
        {
            if (!_isDrawing) return;

            Flush();
            _isDrawing = false;
        }

        public void DrawLine(Vector2 start, Vector2 end, Vector4 color, float thickness = 1f)
        {
            SetPrimitiveType(PrimitiveType.Lines);
            AddVertex(start, color);
            AddVertex(end, color);
            _gl.LineWidth(thickness);
        }

        public void DrawRectangle(Vector2 position, Vector2 size, Vector4 color, bool filled = true)
        {
            if (filled)
            {
                SetPrimitiveType(PrimitiveType.Triangles);
                DrawFilledRect(position, size, color);
            }
            else
            {
                SetPrimitiveType(PrimitiveType.Lines);
                DrawRectOutline(position, size, color);
            }
        }

        public void DrawCircle(Vector2 center, float radius, Vector4 color, bool filled = true, int segments = 32)
        {
            if (filled)
            {
                SetPrimitiveType(PrimitiveType.Triangles);
                DrawFilledCircle(center, radius, color, segments);
            }
            else
            {
                SetPrimitiveType(PrimitiveType.Lines);
                DrawCircleOutline(center, radius, color, segments);
            }
        }

        public void DrawPolygon(Vector2[] vertices, Vector4 color, bool filled = true)
        {
            if (filled)
            {
                SetPrimitiveType(PrimitiveType.Triangles);
                DrawFilledPolygon(vertices, color);
            }
            else
            {
                SetPrimitiveType(PrimitiveType.Lines);
                DrawPolygonOutline(vertices, color);
            }
        }

        private void DrawFilledRect(Vector2 position, Vector2 size, Vector4 color)
        {
            Vector2 topLeft = position;
            Vector2 topRight = new Vector2(position.X + size.X, position.Y);
            Vector2 bottomRight = new Vector2(position.X + size.X, position.Y + size.Y);
            Vector2 bottomLeft = new Vector2(position.X, position.Y + size.Y);

            AddVertex(topLeft, color);
            AddVertex(topRight, color);
            AddVertex(bottomRight, color);

            AddVertex(topLeft, color);
            AddVertex(bottomRight, color);
            AddVertex(bottomLeft, color);
        }

        private void DrawRectOutline(Vector2 position, Vector2 size, Vector4 color)
        {
            Vector2[] points = {
                position,
                new Vector2(position.X + size.X, position.Y),
                new Vector2(position.X + size.X, position.Y + size.Y),
                new Vector2(position.X, position.Y + size.Y)
            };

            AddVertex(points[0], color);
            AddVertex(points[1], color);
            AddVertex(points[1], color);
            AddVertex(points[2], color);
            AddVertex(points[2], color);
            AddVertex(points[3], color);
            AddVertex(points[3], color);
            AddVertex(points[0], color);
        }

        private void DrawFilledCircle(Vector2 center, float radius, Vector4 color, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                if (_vertexCount + 3 > MaxVertices) Flush();

                float angle1 = MathF.PI * 2 * i / segments;
                float angle2 = MathF.PI * 2 * (i + 1) / segments;

                AddVertex(center, color);
                AddVertex(center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius, color);
                AddVertex(center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius, color);
            }
        }

        private void DrawCircleOutline(Vector2 center, float radius, Vector4 color, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                if (_vertexCount + 2 > MaxVertices) Flush();

                float angle1 = MathF.PI * 2 * i / segments;
                float angle2 = MathF.PI * 2 * (i + 1) / segments;

                AddVertex(center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius, color);
                AddVertex(center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius, color);
            }
        }

        private void DrawFilledPolygon(Vector2[] vertices, Vector4 color)
        {
            for (int i = 1; i < vertices.Length - 1; i++)
            {
                if (_vertexCount + 3 > MaxVertices) Flush();

                AddVertex(vertices[0], color);
                AddVertex(vertices[i], color);
                AddVertex(vertices[i + 1], color);
            }
        }

        private void DrawPolygonOutline(Vector2[] vertices, Vector4 color)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                if (_vertexCount + 2 > MaxVertices) Flush();

                int next = (i + 1) % vertices.Length;
                AddVertex(vertices[i], color);
                AddVertex(vertices[next], color);
            }
        }

        private void AddVertex(Vector2 position, Vector4 color)
        {
            if (_vertexCount >= MaxVertices)
                Flush();

            int offset = _vertexCount * FloatsPerVertex;
            _vertexData[offset + 0] = position.X;
            _vertexData[offset + 1] = position.Y;
            _vertexData[offset + 2] = color.X;
            _vertexData[offset + 3] = color.Y;
            _vertexData[offset + 4] = color.Z;
            _vertexData[offset + 5] = color.W;

            _vertexCount++;
        }

        private void SetPrimitiveType(PrimitiveType type)
        {
            if (!_isDrawing) throw new InvalidOperationException("Cannot set primitive type outside of Begin/End block.");
            if (_currentPrimitive != type)
            {
                Flush();
                _currentPrimitive = type;
            }
        }

        private unsafe void Flush()
        {
            if (_vertexCount == 0 || !_isDrawing) return;

            _vertexBuffer.Bind();
            fixed (float* dataPtr = &_vertexData[0])
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                    (nuint)(_vertexCount * FloatsPerVertex * sizeof(float)), dataPtr);
            }

            _vao.Bind();
            if (_currentPrimitive == PrimitiveType.Lines)
            {
                _gl.DrawArrays(_currentPrimitive, 0, (uint)_vertexCount);
            }
            else
            {
                _gl.DrawArrays(_currentPrimitive, 0, (uint)_vertexCount);
            }

            _vertexCount = 0;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_isDrawing) End();
            _vertexBuffer.Dispose();
            _vao.Dispose();
            _shader.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}