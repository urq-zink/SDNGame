using Silk.NET.Vulkan;
using System.ComponentModel;

namespace SDNGame.ECS
{
    public class Entity
    {
        public int Id { get; }
        public readonly Dictionary<Type, IComponent> _components = new();

        public Entity(int id)
        {
            Id = id;
        }

        public void AddComponent<T>(T component) where T : IComponent
        {
            _components[typeof(T)] = component;
        }

        public T? GetComponent<T>() where T : IComponent
        {
            return _components.TryGetValue(typeof(T), out var component) ? (T)component : default;
        }

        public bool HasComponent<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            _components.Remove(typeof(T));
        }
    }
}
