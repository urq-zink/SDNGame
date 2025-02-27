using Silk.NET.OpenAL;

namespace SDNGame.Audio
{
    public unsafe class AudioPlayer : IDisposable
    {
        private readonly AL _al;
        private readonly AudioLoader _wavLoader;
        private readonly AudioSourceManager _audioSourceManager;
        private bool _disposed;

        public bool Loop { get; set; } = false;

        public AudioPlayer(string filePath, AL al, bool loop = false)
        {
            _al = al;
            Loop = loop;

            _wavLoader = new AudioLoader();
            _wavLoader.LoadWav(filePath);

            BufferFormat format = GetBufferFormat(_wavLoader.NumChannels, _wavLoader.BitsPerSample);
            _audioSourceManager = new AudioSourceManager(al, format, _wavLoader.AudioData, _wavLoader.SampleRate);
        }

        private BufferFormat GetBufferFormat(short numChannels, short bitsPerSample)
        {
            if (numChannels == 1)
            {
                return bitsPerSample == 8 ? BufferFormat.Mono8 :
                       bitsPerSample == 16 ? BufferFormat.Mono16 :
                       throw new NotSupportedException($"Mono audio with {bitsPerSample} bits per sample is not supported.");
            }
            else if (numChannels == 2)
            {
                return bitsPerSample == 8 ? BufferFormat.Stereo8 :
                       bitsPerSample == 16 ? BufferFormat.Stereo16 :
                       throw new NotSupportedException($"Stereo audio with {bitsPerSample} bits per sample is not supported.");
            }
            else
            {
                throw new NotSupportedException($"Audio with {numChannels} channels is not supported.");
            }
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

        public void Dispose()
        {
            if (_disposed) return;

            _audioSourceManager.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}