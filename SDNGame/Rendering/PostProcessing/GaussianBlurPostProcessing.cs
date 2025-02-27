using Silk.NET.OpenGL;

namespace SDNGame.Rendering.PostProcessing
{
    public class GaussianBlurPostProcessor : PostProcessor
    {
        private readonly uint _shaderProgram;
        private int _passes = 5;
        private float _sigma = 2.0f;
        private bool _horizontal = true;
        private uint[] _colorBuffers = new uint[2];
        private uint[] _framebuffers = new uint[2];

        public int Passes
        {
            get => _passes;
            set => _passes = Math.Clamp(value, 0, 128);
        }

        public GaussianBlurPostProcessor(GL gl, int width, int height)
            : base(gl, width, height)
        {
            _shaderProgram = CreateShaderProgram("Rendering/Shaders/PostProcessing/blur.vert", "Rendering/Shaders/PostProcessing/blur.frag");
            InitializeFramebuffers(width, height);
        }

        private unsafe void InitializeFramebuffers(int width, int height)
        {
            _framebuffers = new uint[2];
            _colorBuffers = new uint[2];

            Gl.GenFramebuffers(2, _framebuffers);
            Gl.GenTextures(2, _colorBuffers);

            for (int i = 0; i < 2; i++)
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffers[i]);
                Gl.BindTexture(TextureTarget.Texture2D, _colorBuffers[i]);

                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f,
                    (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.Float, null);

                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.ClampToEdge);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.ClampToEdge);

                Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D, _colorBuffers[i], 0);
            }

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Draw(int screenWidth, int screenHeight)
        {
            bool horizontal = true;
            bool firstIteration = true;

            for (int i = 0; i < _passes; i++)
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffers[horizontal ? 0 : 1]);
                Gl.UseProgram(_shaderProgram);
                Gl.Uniform1(Gl.GetUniformLocation(_shaderProgram, "horizontal"), horizontal ? 1 : 0);

                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D,
                    firstIteration ? ColorTexture.Handle : _colorBuffers[horizontal ? 1 : 0]);

                Gl.BindVertexArray(QuadVAO);
                Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

                horizontal = !horizontal;
                if (firstIteration) firstIteration = false;
            }

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(_shaderProgram);
            Gl.Uniform1(Gl.GetUniformLocation(_shaderProgram, "horizontal"), 0);
            Gl.BindTexture(TextureTarget.Texture2D, _colorBuffers[1]);
            Gl.BindVertexArray(QuadVAO);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        private uint CreateShaderProgram(string vertPath, string fragPath)
        {
            string vertSource = File.ReadAllText(vertPath);
            string fragSource = File.ReadAllText(fragPath);

            uint vert = CompileShader(ShaderType.VertexShader, vertSource);
            uint frag = CompileShader(ShaderType.FragmentShader, fragSource);

            uint program = Gl.CreateProgram();
            Gl.AttachShader(program, vert);
            Gl.AttachShader(program, frag);
            Gl.LinkProgram(program);

            Gl.DeleteShader(vert);
            Gl.DeleteShader(frag);

            return program;
        }

        private uint CompileShader(ShaderType type, string source)
        {
            uint shader = Gl.CreateShader(type);
            Gl.ShaderSource(shader, source);
            Gl.CompileShader(shader);
            return shader;
        }

        public override unsafe void Resize(int width, int height)
        {
            for (int i = 0; i < 2; i++)
            {
                Gl.BindTexture(TextureTarget.Texture2D, _colorBuffers[i]);
                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f,
                    (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.Float, null);
            }
            ColorTexture.Resize(width, height);
        }
    }
}