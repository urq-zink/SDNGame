using System.Numerics;

namespace SDNGame.ECS.Components
{
    public class TransformComponent : IComponent 
    {
        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; } = 0f;


        public TransformComponent(Vector2 position)
        {
            Position = position;
        }

    }
}
