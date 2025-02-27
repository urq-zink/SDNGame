using SDNGame.Core;
using SDNGame.Rendering.Shapes;
using SDNGame.Rendering.Sprites;
using Silk.NET.Input;
using System.Numerics;

namespace SDNGame.UI
{
    public class UIManager
    {
        private readonly List<UIElement> _elements = new();
        private readonly Game _game;

        public List<UIElement> Elements => _elements;

        public UIManager(Game game)
        {
            _game = game;
        }

        public void AddElement(UIElement element)
        {
            _elements.Add(element);
        }

        public void RemoveElement(UIElement element)
        {
            _elements.Remove(element);
        }

        public void Update()
        {
            Vector2 mousePos = _game.InputManager.MousePosition;
            bool isMouseDown = _game.InputManager.IsMouseButtonPressed(MouseButton.Left);

            var elementsCopy = _elements.ToList();
            foreach (var element in elementsCopy)
            {
                if (!element.Enabled || !element.Visible) continue;

                bool isOver = element.IsMouseOver(mousePos);
                if (isOver)
                {
                    element.OnHover();
                    if (isMouseDown)
                        element.OnMouseDown();
                    else
                        element.OnMouseUp();
                }
                else
                {
                    element.OnHoverExit();
                    if (isMouseDown)
                        element.OnMouseDownOutside();
                    else
                        element.OnMouseUpOutside();
                }
            }
        }

        public void DrawShapes(ShapeRenderer shapeRenderer)
        {
            foreach (var element in _elements)
            {
                if (element.Visible)
                    element.DrawShapes(shapeRenderer);
            }
        }

        public void DrawSprites(SpriteBatch spriteBatch)
        {
            foreach (var element in _elements)
            {
                if (element.Visible)
                    element.DrawSprites(spriteBatch);
            }
        }
    }
}
