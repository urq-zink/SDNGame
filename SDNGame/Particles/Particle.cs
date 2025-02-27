using System.Numerics;

namespace SDNGame.Particles
{
    public struct Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Lifetime { get; set; }
        public Vector2 Size { get; set; }
        public Vector4 Color { get; set; }
        public bool Active { get; set; }

        public Particle(Vector2 position, Vector2 velocity, float lifetime, Vector2 size, Vector4 color)
        {
            Position = position;
            Velocity = velocity;
            Lifetime = lifetime;
            Size = size;
            Color = color;
            Active = true;
        }
    }
}
