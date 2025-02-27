using System.Numerics;
using SDNGame.Rendering.Textures;

namespace SDNGame.Rendering.Sprites
{
    public class Sprite
    {
        public Texture Texture { get; set; }

        private Vector2 _position;
        public ref Vector2 Position => ref _position;

        private Vector2 _size;
        public ref Vector2 Size => ref _size;

        private float _rotation;
        public ref float Rotation => ref _rotation;

        private Vector2 _origin;
        public ref Vector2 Origin => ref _origin;

        private Vector4 _color = Vector4.One;
        public ref Vector4 Color => ref _color;

        public Sprite(Texture texture, Vector2 position, Vector2 size, Vector2 origin, Vector4? color, float rotation = 0f)
        {
            Texture = texture;
            _position = position;
            _size = size;
            _origin = origin;
            _color = color ?? Vector4.One;
            _rotation = rotation;
        }
    }
}
