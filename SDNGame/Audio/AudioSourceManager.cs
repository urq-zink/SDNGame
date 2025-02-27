using Silk.NET.OpenAL;
using System;

namespace SDNGame.Audio
{
    public unsafe class AudioSourceManager : IDisposable
    {
        private readonly AL _al;
        private uint _source;
        private uint _buffer;
        private bool _disposed;
        private float _gain = 1.0f;
        private float _pitch = 1.0f;
        private float _targetGain = 1.0f;
        private float _fadeSpeed = 0f; // Gain change per second

        public uint Source => _source;

        public AudioSourceManager(AL al, BufferFormat format, byte[] audioData, int sampleRate)
        {
            _al = al;
            InitializeSource(format, audioData, sampleRate);
        }

        private void InitializeSource(BufferFormat format, byte[] audioData, int sampleRate)
        {
            _source = _al.GenSource();
            _buffer = _al.GenBuffer();

            fixed (byte* pData = audioData)
            {
                _al.BufferData(_buffer, format, pData, audioData.Length, sampleRate);
            }

            _al.SetSourceProperty(_source, SourceInteger.Buffer, _buffer);
            _al.SetSourceProperty(_source, SourceFloat.Gain, _gain);
            _al.SetSourceProperty(_source, SourceFloat.Pitch, _pitch);
        }

        public void Play()
        {
            _al.SourcePlay(_source);
        }

        public void Pause()
        {
            _al.SourcePause(_source);
        }

        public void Stop()
        {
            _al.SourceStop(_source);
        }

        public void SetGain(float gain)
        {
            _gain = Math.Clamp(gain, 0f, 1f);
            _targetGain = _gain;
            _fadeSpeed = 0f;
            _al.SetSourceProperty(_source, SourceFloat.Gain, _gain);
        }

        public void FadeTo(float targetGain, float duration)
        {
            _targetGain = Math.Clamp(targetGain, 0f, 1f);
            _fadeSpeed = Math.Abs(_targetGain - _gain) / duration;
        }

        public void SetPitch(float pitch)
        {
            _pitch = Math.Clamp(pitch, 0.5f, 2.0f); // Reasonable range for 2D games
            _al.SetSourceProperty(_source, SourceFloat.Pitch, _pitch);
        }

        public bool IsPlaying()
        {
            _al.GetSourceProperty(_source, GetSourceInteger.SourceState, out int state);
            return state == (int)SourceState.Playing;
        }

        public void Update(float deltaTime)
        {
            if (_fadeSpeed > 0)
            {
                float deltaGain = _fadeSpeed * deltaTime;
                if (_gain < _targetGain)
                {
                    _gain = Math.Min(_gain + deltaGain, _targetGain);
                }
                else
                {
                    _gain = Math.Max(_gain - deltaGain, _targetGain);
                }
                _al.SetSourceProperty(_source, SourceFloat.Gain, _gain);
                if (Math.Abs(_gain - _targetGain) < 0.01f)
                {
                    _gain = _targetGain;
                    _fadeSpeed = 0f;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _al.DeleteSource(_source);
            _al.DeleteBuffer(_buffer);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}