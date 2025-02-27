using Silk.NET.OpenGL;

namespace SDNGame.Rendering.Buffers
{
    public class BufferObject<TDataType>
        : IDisposable
        where TDataType : unmanaged
    {
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType, BufferUsageARB bufferUsage)
        {
            _gl = gl;
            _bufferType = bufferType;

            _handle = _gl.GenBuffer();
            Bind();
            fixed (void* dPtr = data)
            {
                _gl.BufferData(
                    _bufferType,
                    (nuint)(data.Length * sizeof(TDataType)),
                    dPtr,
                    bufferUsage
                );
            }
        }

        public void Bind()
        {
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_handle);
            GC.SuppressFinalize(this);
        }
    }
}
