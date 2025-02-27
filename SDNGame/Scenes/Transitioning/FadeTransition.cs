using SDNGame.Core;
using System.Numerics;

namespace SDNGame.Scenes.Transitioning
{
    public class FadeTransition : Transition
    {
        private readonly bool _isFadeIn;
        private Vector4 _color;

        public FadeTransition(Game game, float duration, bool isFadeIn, Vector4? color = null)
            : base(game, duration)
        {
            _isFadeIn = isFadeIn;
            _color = color ?? new Vector4(0, 0, 0, 1);
        }

        public override void Draw(float deltaTime)
        {
            float progress = Math.Clamp(ElapsedTime / Duration, 0f, 1f);
            float easedProgress = 1f - MathF.Pow(1f - progress, 3);
            float alpha = _isFadeIn ? 1 - easedProgress : easedProgress;

            ShapeRenderer.Begin(Camera, ScreenHeight, ScreenHeight);
            ShapeRenderer.DrawRectangle(
                new Vector2(0, 0),
                new Vector2(ScreenWidth, ScreenHeight),
                new Vector4(_color.X, _color.Y, _color.Z, alpha),
                true
            );
            ShapeRenderer.End();
        }
    }
}
