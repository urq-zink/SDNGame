using Silk.NET.OpenGL;
using Texture = SDNGame.Rendering.Textures.Texture;

namespace SDNGame.Rendering.PostProcessing
{
    public abstract class PostProcessor : IDisposable
    {
        protected readonly GL Gl;
        protected uint Framebuffer;
        public Texture ColorTexture { get; protected set; }
        protected uint QuadVAO;
        protected uint QuadVBO;

        protected PostProcessor(GL gl, int width, int height)
        {
            Gl = gl;
            Initialize(width, height);
        }

        private unsafe void Initialize(int width, int height)
        {
            // Framebuffer
            Framebuffer = Gl.GenFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);

            // Color texture
            ColorTexture = new Texture(Gl, width, height);
            Gl.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                ColorTexture.Handle,
                0
            );

            // Screen quad
            float[] quadVertices = {
                -1.0f,  1.0f,
                -1.0f, -1.0f,
                 1.0f, -1.0f,
                -1.0f,  1.0f,
                 1.0f, -1.0f,
                 1.0f,  1.0f
            };

            QuadVAO = Gl.GenVertexArray();
            QuadVBO = Gl.GenBuffer();

            Gl.BindVertexArray(QuadVAO);
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, QuadVBO);

            // Fixed BufferData call
            fixed (float* vertPtr = quadVertices)
            {
                Gl.BufferData(
                    BufferTargetARB.ArrayBuffer,
                    (nuint)(quadVertices.Length * sizeof(float)),
                    vertPtr,
                    BufferUsageARB.StaticDraw
                );
            }
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public virtual void BeginCapture()
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
        }

        public virtual void EndCapture()
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public abstract void Draw(int screenWidth, int screenHeight);
        public abstract void Resize(int width, int height);

        public virtual void Dispose()
        {
            Gl.DeleteFramebuffer(Framebuffer);
            ColorTexture.Dispose();
            Gl.DeleteVertexArray(QuadVAO);
            Gl.DeleteBuffer(QuadVBO);
        }
    }
}