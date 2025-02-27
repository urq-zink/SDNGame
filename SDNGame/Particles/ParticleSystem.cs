using SDNGame.Camera;
using SDNGame.Core;
using SDNGame.Rendering.Sprites;
using SDNGame.Rendering.Textures;
using System.Numerics;
using System.Security;

namespace SDNGame.Particles
{
    public class ParticleSystem : IDisposable
    {
        public float Lifetime { get; set; } = 1f;
        public float Speed { get; set; } = 50f;
        public Vector2 Size { get; set; } = new Vector2(16f, 16f);
        public Vector4 Color { get; set; } = Vector4.One;
        public int MaxParticle { get; set; } = 100;
        public float EmissionRate { get; set; } = 20f;

        private readonly Texture? _texture;
        private readonly SpriteBatch? _spriteBatch;
        private readonly List<Particle> _particles;
        private readonly Random _random = new();
        private float _emissionTimer = 0f;
        private bool _isEmitting = false;
        private Vector2 _position;

        public ParticleSystem(float deltaTime, Vector2 emitterPosition)
        {
            _position = emitterPosition;

            if (_isEmitting)
            {
                _emissionTimer += deltaTime;
                float emitInterval = 1f / EmissionRate;
                while (_emissionTimer >= emitInterval && _particles?.Count < MaxParticle)
                {
                    EmitParticle();
                    _emissionTimer -= emitInterval;
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                if (!particle.Active) continue;

                particle.Lifetime -= deltaTime;
                if (particle.Lifetime <= 0)
                {
                    particle.Active = false;
                    _particles[i] = particle;
                    continue;
                }

                particle.Position += particle.Velocity * deltaTime;
                _particles[i] = particle;
            }
        }

        private void EmitParticle()
        {
            Vector2 direction = new((float)_random.NextDouble() * 2 - 1, (float)_random.NextDouble() * 2 - 1);
            direction = Vector2.Normalize(direction);
            Vector2 velocity = direction * Speed;

            var particle = new Particle(_position, velocity, Lifetime, Size, Color);
            _particles?.Add(particle);
        }

        public void Draw(Camera2D camera, int screenWidth, int screenHeight)
        {
            _spriteBatch?.Begin(camera, screenWidth, screenHeight);
            foreach (var particle in _particles)
            {
                if (!particle.Active) continue;
                _spriteBatch?.Draw(_texture, particle.Position, particle.Size, Vector2.Zero, 0f, particle.Color);
            }
            _spriteBatch?.End();
        }

        public void Dispose()
        {
            _particles.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
