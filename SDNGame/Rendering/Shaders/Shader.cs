using Silk.NET.OpenGL;
using System.Numerics;

namespace SDNGame.Rendering.Shaders
{
    public class Shader : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            _gl = gl;

            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

            _handle = _gl.CreateProgram();
            _gl.AttachShader(_handle, vertex);
            _gl.AttachShader(_handle, fragment);
            _gl.LinkProgram(_handle);

            _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Prgram failed to link! Detail: {_gl.GetProgramInfoLog(_handle)}");
            }

            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        public void Use()
        {
            _gl.UseProgram(_handle);
        }

        public unsafe void SetUniform(string name, Vector4 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader!");
            }
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader!");
            }
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }

        public void SetUniform(string name, int value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader!");
            }
            _gl.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader!");
            }
            _gl.Uniform1(location, value);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            uint tempShader = _gl.CreateShader(type);

            _gl.ShaderSource(tempShader, src);
            _gl.CompileShader(tempShader);

            string infoLog = _gl.GetShaderInfoLog(tempShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"{type} shader failed to compile! Detail: {infoLog}");
            }
            return tempShader;

        }

        public void Dispose()
        {
            _gl.DeleteProgram(_handle);
            GC.SuppressFinalize(this);
        }
    }
}
