using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
namespace BloomTest
{
    public class Program
    {
        private static IWindow window;
        private static GL gl;
        private static uint triangleVAO;
        private static uint triangleVBO;
        private static uint framebuffer;
        private static uint[] colorBuffers;
        private static uint quadVAO;
        private static uint quadVBO;
        private static uint shaderProgram;
        private static uint blurShader;
        // Shader sources
        private static string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            out vec2 TexCoords;
            void main()
            {
            gl_Position = vec4(aPosition, 0.0, 1.0);
            TexCoords = (aPosition + 1.0) * 0.5;
            }";
                    private static string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            void main()
            {
            FragColor = vec4(1.0, 0.0, 0.0, 1.0); // Red triangle
            }";
        private static string blurFragmentShaderSource = @"
            #version 330 core
            in vec2 TexCoords;
            out vec4 FragColor;
            uniform sampler2D screenTexture;
            uniform bool horizontal;

            uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

            void main()
            {
            vec2 tex_offset = 1.0 / textureSize(screenTexture, 0);
            vec3 result = texture(screenTexture, TexCoords).rgb * weight[0];

            if(horizontal)
            {
            for(int i = 1; i < 5; ++i)
            {
            result += texture(screenTexture, TexCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            result += texture(screenTexture, TexCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            }
            }
            else
            {
            for(int i = 1; i < 5; ++i)
            {
            result += texture(screenTexture, TexCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            result += texture(screenTexture, TexCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            }
            }

            FragColor = vec4(result, 1.0);
            }";
        public static void Mainn()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Gaussian Blur Example";
            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Run();
        }
        private static unsafe void OnLoad()
        {
            gl = GL.GetApi(window);
            // Create shaders
            shaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
            blurShader = CreateShaderProgram(vertexShaderSource, blurFragmentShaderSource);
            // Set up triangle vertices
            float[] triangleVertices =
            {
                -0.5f, -0.5f,
                0.5f, -0.5f,
                0.0f, 0.5f
                };
            triangleVAO = gl.GenVertexArray();
            triangleVBO = gl.GenBuffer();
            gl.BindVertexArray(triangleVAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, triangleVBO);
            fixed (void* v = &triangleVertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(triangleVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
            gl.EnableVertexAttribArray(0);
            // Set up framebuffers for blur
            SetupBlurFramebuffers();
            // Set up screen quad for post-processing
            SetupScreenQuad();
        }
        private static unsafe void SetupBlurFramebuffers()
        {
            framebuffer = gl.GenFramebuffer();
            colorBuffers = new uint[2];
            gl.GenTextures(2, colorBuffers);
            for (int i = 0; i < 2; i++)
            {
                gl.BindTexture(TextureTarget.Texture2D, colorBuffers[i]);
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb16f, 800, 600, 0, PixelFormat.Rgb, PixelType.Float, null);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            }
        }
        private static unsafe void SetupScreenQuad()
        {
            float[] quadVertices =
            {
                -1.0f, 1.0f,
                -1.0f, -1.0f,
                1.0f, -1.0f,
                -1.0f, 1.0f,
                1.0f, -1.0f,
                1.0f, 1.0f
                };
            quadVAO = gl.GenVertexArray();
            quadVBO = gl.GenBuffer();
            gl.BindVertexArray(quadVAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, quadVBO);
            fixed (void* v = &quadVertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
            gl.EnableVertexAttribArray(0);
        }
        private static void OnRender(double obj)
        {
            // 1. First render pass - render triangle to framebuffer
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorBuffers[0], 0);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            gl.UseProgram(shaderProgram);
            gl.BindVertexArray(triangleVAO);
            gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
            // 2. Blur pass - ping-pong between framebuffers
            gl.UseProgram(blurShader);
            bool horizontal = true;
            bool firstIteration = true;
            int amount = 100; // Number of blur passes
            for (int i = 0; i < amount; i++)
            {
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, colorBuffers[horizontal ? 1 : 0], 0);
                gl.Uniform1(gl.GetUniformLocation(blurShader, "horizontal"), horizontal ? 1 : 0);
                gl.BindTexture(TextureTarget.Texture2D, firstIteration ? colorBuffers[0] : colorBuffers[horizontal ? 0 : 1]);
                gl.BindVertexArray(quadVAO);
                gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
                horizontal = !horizontal;
                if (firstIteration)
                    firstIteration = false;
            }
            // 3. Final render to screen
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            gl.UseProgram(blurShader);
            gl.BindTexture(TextureTarget.Texture2D, colorBuffers[!horizontal ? 1 : 0]);
            gl.BindVertexArray(quadVAO);
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
        private static uint CreateShaderProgram(string vertexSource, string fragmentSource)
        {
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexSource);
            gl.CompileShader(vertexShader);
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentSource);
            gl.CompileShader(fragmentShader);
            uint program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            return program;
        }
    }
}