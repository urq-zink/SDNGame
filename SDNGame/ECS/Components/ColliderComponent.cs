using SDNGame.Physics;

namespace SDNGame.ECS.Components
{
    public class ColliderComponent : IComponent
    {
        public Collider Collider { get; set; }

        public ColliderComponent(Collider collider)
        {
            Collider = collider;
        }
    }
}
