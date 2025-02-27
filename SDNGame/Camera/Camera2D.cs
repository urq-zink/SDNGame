using System.Numerics;

namespace SDNGame.Camera
{
    public class Camera2D
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0f;
        public float Zoom { get; set; } = 1f;

        public Matrix4x4 GetViewMatrix(float screenWidth, float screenHeight)
        {
            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
            Matrix4x4 translationToCenter = Matrix4x4.CreateTranslation(-screenCenter.X, -screenCenter.Y, 0);
            Matrix4x4 translation = Matrix4x4.CreateTranslation(Position.X, Position.Y, 0);
            Matrix4x4 rotation = Matrix4x4.CreateRotationZ(-Rotation);
            Matrix4x4 scale = Matrix4x4.CreateScale(Zoom);

            return translationToCenter * scale * rotation * Matrix4x4.CreateTranslation(screenCenter.X, screenCenter.Y, 0) * translation;
        }

        public Matrix4x4 GetProjectionMatrix(float screenWidth, float screenHeight)
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1f, 1f);
        }
    }
}
