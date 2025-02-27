using SDNGame.Core;
using SDNGame.ECS.Components;

namespace SDNGame.ECS.Systems
{
    public class RenderSystem : ISystem
    {
        private readonly Game _game;

        public RenderSystem(Game game) => _game = game;

        public void Draw(float deltaTime, EntityManager entityManager)
        {
        }

        public void Update(float deltaTime, EntityManager entityManager)
        {
            _game.SpriteBatch.Begin(_game.Camera, _game.ScreenWidth, _game.ScreenHeight);
            foreach (var entity in entityManager.GetEntities())
            {
                if (entity.HasComponent<SpriteComponent>() && entity.HasComponent<TransformComponent>())
                {
                    var spriteComp = entity.GetComponent<SpriteComponent>();
                    var transformComp = entity.GetComponent<TransformComponent>();
                    spriteComp.Sprite.Position = transformComp.Position;
                    spriteComp.Sprite.Rotation = transformComp.Rotation;
                    spriteComp.Sprite.Size = transformComp.Scale * spriteComp.Sprite.Size;
                    _game.SpriteBatch.Draw(spriteComp.Sprite);
                }
            }
            _game.SpriteBatch.End();
        }
    }
}
