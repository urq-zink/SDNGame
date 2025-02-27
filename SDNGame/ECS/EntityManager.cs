namespace SDNGame.ECS
{
    public class EntityManager
    {
        private int _nextId = 0;
        private readonly List<Entity> _entities = new();

        public Entity CreateEntity()
        {
            var entity = new Entity(_nextId++);
            _entities.Add(entity);
            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            _entities.Remove(entity);
        }

        public IEnumerable<Entity> GetEntities()
        {
            return _entities;
        }
    }
}
