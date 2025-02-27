using Silk.NET.OpenGL;

namespace SDNGame.Rendering.PostProcessing
{
    public class PixelatePostProcessor : PostProcessor
    {
        private readonly uint _shaderProgram;
        private int _pixelSize = 8;

        public int PixelSize
        {
            get => _pixelSize;
            set => _pixelSize = Math.Clamp(value, 0, 1024);
        }

        public PixelatePostProcessor(GL gl, int width, int height)
            : base(gl, width, height)
        {
            _shaderProgram = CreateShaderProgram("Rendering/Shaders/PostProcessing/pixelate.vert", "Rendering/Shaders/PostProcessing/pixelate.frag");
        }

        public override void Draw(int screenWidth, int screenHeight)
        {
            Gl.UseProgram(_shaderProgram);
            Gl.Uniform1(Gl.GetUniformLocation(_shaderProgram, "pixelSize"), _pixelSize);

            Gl.BindTexture(TextureTarget.Texture2D, ColorTexture.Handle);
            Gl.BindVertexArray(QuadVAO);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public override void Resize(int width, int height)
        {
            ColorTexture.Resize(width, height);
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
    }
}