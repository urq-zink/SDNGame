namespace SDNGame.ECS
{
    public interface ISystem
    {
        void Update(float deltaTime, EntityManager entityManager);
        void Draw(float deltaTime, EntityManager entityManager);
    }
}
