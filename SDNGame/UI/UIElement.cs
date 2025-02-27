using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using System.Numerics;

namespace SDNGame.UI
{
    public abstract class UIElement : IDisposable
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public abstract void DrawShapes(ShapeRenderer shapeRenderer);
        public abstract void DrawSprites(SpriteBatch spriteBatch);

        public virtual bool IsMouseOver(Vector2 mousePos)
        {
            return mousePos.X >= Position.X && mousePos.X <= Position.X + Size.X &&
                   mousePos.Y >= Position.Y && mousePos.Y <= Position.Y + Size.Y;
        }

        public virtual void OnHover() { }
        public virtual void OnHoverExit() { }
        public virtual void OnMouseDown() { }
        public virtual void OnMouseUp() { }
        public virtual void OnMouseDownOutside() { }
        public virtual void OnMouseUpOutside() { }

        public virtual void Dispose() { }
    }
}
