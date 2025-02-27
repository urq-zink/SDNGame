using SDNGame.Rendering.Sprites;

namespace SDNGame.ECS.Components
{
    public class SpriteComponent : IComponent
    {
        public Sprite Sprite { get; set; }

        public SpriteComponent(Sprite sprite)
        {
            Sprite = sprite;
        }
    }
}
