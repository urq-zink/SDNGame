using Silk.NET.OpenGL;
using Texture = SDNGame.Rendering.Textures.Texture;

namespace SDNGame.Rendering.Buffers
{
    public class FramebufferObject : IDisposable
    {
        private readonly GL _gl;
        private uint _handle;
        public Texture ColorTexture { get; private set; }
        private uint _rboDepthStencil;

        public FramebufferObject(GL gl, int width, int height)
        {
            _gl = gl;
            Initialize(width, height);
        }

        private void Initialize(int width, int height)
        {
            _handle = _gl.GenFramebuffer();
            Bind();

            ColorTexture = new Texture(_gl, width, height);
            _gl.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                ColorTexture.Handle,
                0
            );

            _rboDepthStencil = _gl.GenRenderbuffer();
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepthStencil);
            _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                InternalFormat.Depth24Stencil8, (uint)width, (uint)height);
            _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, _rboDepthStencil);

            Unbind();
        }

        public void Bind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        public void Unbind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        public void Resize(int width, int height)
        {
            ColorTexture.Dispose();
            _gl.DeleteRenderbuffer(_rboDepthStencil);
            Initialize(width, height);
        }

        public void Dispose()
        {
            _gl.DeleteFramebuffer(_handle);
            ColorTexture.Dispose();
            _gl.DeleteRenderbuffer(_rboDepthStencil);
        }
    }
}
