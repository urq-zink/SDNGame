using SDNGame.Core;
using SDNGame.Utils;
using System.Numerics;

namespace SDNGame.Scenes.Transitioning
{
    public class ZoomAndRotateTransition : Transition
    {
        private readonly bool _isEntering;
        private readonly float _startZoom;
        private readonly float _endZoom;
        private readonly float _startRotation;
        private readonly float _endRotation;
        private readonly Vector4 _overlayColor;

        public ZoomAndRotateTransition(Game game, float duration, bool isEntering,
            float startZoom = 1f, float endZoom = 2f, float startRotation = 0f, float endRotation = MathF.PI * 0.2f,
            Vector4? overlayColor = null)
            : base(game, duration)
        {
            _isEntering = isEntering;
            _startZoom = startZoom;
            _endZoom = endZoom;
            _startRotation = startRotation;
            _endRotation = endRotation;
            _overlayColor = overlayColor ?? new Vector4(0, 0, 0, 1);
        }

        public override void Start()
        {
            base.Start();
            Camera.Zoom = _isEntering ? _endZoom : _startZoom;
            Camera.Rotation = _isEntering ? _endRotation : _startRotation;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            float progress = Math.Clamp(ElapsedTime / Duration, 0f, 1f);
            float easedProgress = Tween.EaseInOutBack(progress);

            Game.Camera.Zoom = _isEntering
                ? _endZoom + (_startZoom - _endZoom) * easedProgress
                : _startZoom + (_endZoom - _startZoom) * easedProgress;
            Game.Camera.Rotation = _isEntering
                ? _endRotation + (_startRotation - _endRotation) * easedProgress
                : _startRotation + (_endRotation - _startRotation) * easedProgress;
        }
        public override void Draw(float deltaTime)
        {
            float progress = Math.Clamp(ElapsedTime / Duration, 0f, 1f);
            float easedProgress = Tween.EaseOutBack(progress);
            float alpha = _isEntering ? (1f - easedProgress) : easedProgress;

            ShapeRenderer.Begin(Game.Camera, ScreenWidth, ScreenHeight);
            ShapeRenderer.DrawRectangle(
                new Vector2(0, 0),
                new Vector2(ScreenWidth, ScreenHeight),
                new Vector4(_overlayColor.X, _overlayColor.Y, _overlayColor.Z, alpha),
                true
            );
            ShapeRenderer.End();
        }
    }
}
