using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;

namespace SDNGame.Audio
{
    public unsafe class AudioMixer : IDisposable
    {
        private ALContext _alc;
        private AL _al;
        private Device* _device;
        private Context* _context;
        private bool _disposed;
        private readonly List<WavePlayer> _activePlayers = new();
        private readonly Queue<WavePlayer> _playerPool = new();
        private const int MaxPooledPlayers = 32;

        private float _masterVolume = 1.0f;
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Math.Clamp(value, 0f, 1f);
                foreach (var player in _activePlayers)
                {
                    player.SetGain(_masterVolume);
                }
            }
        }

        public AudioMixer()
        {
            InitializeOpenAL();
        }

        private void InitializeOpenAL()
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();
            _device = _alc.OpenDevice("");
            if (_device is null)
                throw new Exception("Could not open default audio device.");

            _context = _alc.CreateContext(_device, null);
            _alc.MakeContextCurrent(_context);
        }

        public WavePlayer CreateWavePlayer(string filePath, bool loop = false, bool usePool = true)
        {
            WavePlayer player;
            if (usePool && _playerPool.Count > 0)
            {
                player = _playerPool.Dequeue();
                player.Loop = loop;
                // Reset audio data (create new source/buffer)
                player.Dispose(); // Dispose old source/buffer
                player = new WavePlayer(filePath, _al, loop) { IsPooled = true };
            }
            else
            {
                player = new WavePlayer(filePath, _al, loop) { IsPooled = usePool };
            }

            _activePlayers.Add(player);
            player.SetGain(_masterVolume);
            return player;
        }

        public void RemoveWavePlayer(WavePlayer player)
        {
            if (_activePlayers.Remove(player))
            {
                if (player.IsPooled && _playerPool.Count < MaxPooledPlayers)
                {
                    player.Stop();
                    _playerPool.Enqueue(player);
                }
                else
                {
                    player.Dispose();
                }
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = _activePlayers.Count - 1; i >= 0; i--)
            {
                var player = _activePlayers[i];
                player.Update(deltaTime);
                if (!player.IsPlaying() && !player.IsPooled)
                {
                    RemoveWavePlayer(player);
                }
            }
        }

        public void PauseAll()
        {
            foreach (var player in _activePlayers)
            {
                player.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var player in _activePlayers)
            {
                _al.GetSourceProperty(player.AudioSource.Source, GetSourceInteger.SourceState, out int state);
                if (state == (int)SourceState.Paused)
                    player.Play();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var player in _activePlayers)
            {
                player.Dispose();
            }
            _activePlayers.Clear();
            foreach (var player in _playerPool)
            {
                player.Dispose();
            }
            _playerPool.Clear();

            _al.Dispose();
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
            _alc.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}