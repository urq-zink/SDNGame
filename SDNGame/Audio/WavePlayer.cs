using Silk.NET.OpenAL;
using System;

namespace SDNGame.Audio
{
    public class WavePlayer : IDisposable
    {
        private readonly AL _al;
        private readonly AudioLoader _audioLoader;
        private readonly AudioSourceManager _audioSourceManager;
        private bool _disposed;

        public AudioSourceManager AudioSource => _audioSourceManager;

        public bool Loop { get; set; } = false;
        public bool IsPooled { get; set; } = false; // Flag for pooling

        public WavePlayer(string filePath, AL al, bool loop = false)
        {
            _al = al;
            Loop = loop;

            _audioLoader = new AudioLoader();
            _audioLoader.LoadAudio(filePath);

            BufferFormat format = _audioLoader.GetBufferFormat();
            _audioSourceManager = new AudioSourceManager(al, format, _audioLoader.AudioData, _audioLoader.SampleRate);

            if (Loop)
                _al.SetSourceProperty(_audioSourceManager.Source, SourceBoolean.Looping, true); // Access internal source for now
        }

        public void Play()
        {
            _audioSourceManager.Play();
        }

        public void Pause()
        {
            _audioSourceManager.Pause();
        }

        public void Stop()
        {
            _audioSourceManager.Stop();
        }

        public void SetGain(float gain)
        {
            _audioSourceManager.SetGain(gain);
        }

        public void FadeTo(float gain, float duration)
        {
            _audioSourceManager.FadeTo(gain, duration);
        }

        public void SetPitch(float pitch)
        {
            _audioSourceManager.SetPitch(pitch);
        }

        public bool IsPlaying()
        {
            return _audioSourceManager.IsPlaying();
        }

        public void Update(float deltaTime)
        {
            _audioSourceManager.Update(deltaTime);
        }

        public void Dispose()
        {
            if (_disposed || IsPooled) return; // Don’t dispose if pooled
            _audioSourceManager.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}