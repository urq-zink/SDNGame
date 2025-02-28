using SDNGame.Input;
using SDNGame.Physics;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using Silk.NET.Input;
using System.Numerics;
using SDNGame.Rendering.Sprites;
using SDNGame.UI;
using Button = SDNGame.UI.Button;

namespace SDNGame.Core.GameScenes
{
    public class DemoScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle fontStyle;
        private TextStyle instructionStyle;
        private TextStyle buttonStyle;
        private UIManager uiManager => UIManager;

        private List<(Collider collider, Vector4 color, Vector4 originalColor, bool isDragging)> testColliders;
        private Collider mouseCollider;
        private int currentCursorShapeIndex = 0;
        private readonly string[] shapeNames = { "Circle", "Rectangle", "Triangle", "Capsule", "Line" };
        private readonly Vector4 cursorColor = new Vector4(1, 0, 1, 1);
        private Vector2 dragStartPosition;
        private bool showBoundingBoxes = true;
        private bool useFilledShapes = true;
        private float rotationAngle = 0f;

        public DemoScene(Game game) : base(game)
        {
            testColliders = new List<(Collider, Vector4, Vector4, bool)>();
        }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");

            fontStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 42f,
                Color = new Vector4(0, 1, 0, 1),
                Alignment = TextAlignment.Left
            };

            instructionStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 24f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Left
            };

            buttonStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            InitializeColliders();
            UpdateMouseColliderShape();

            // Add UI Elements
            var instructionsPanel = new Label(fontRenderer,
                new Vector2(40, ScreenHeight - 200),
                "Controls:\n1-5: Change cursor shape\nLeft Click: Drag\nB: Toggle boxes\nF: Toggle fill\nQ: Back to menu",
                instructionStyle);
            uiManager.AddElement(instructionsPanel);

            var backButton = new Button(fontRenderer,
                new Vector2(40, ScreenHeight - 60),
                new Vector2(100, 40),
                "Back",
                buttonStyle);
            backButton.OnClick += () =>
            {
                var outgoing = new ZoomAndRotateTransition(Game, 0.6f, false, 1f, 2f, 0f, 0.1f);
                var incoming = new ZoomAndRotateTransition(Game, 0.6f, true, 1f, 2f, 0f, -0.1f);
                Game.SetScene(new MainMenuScene(Game), outgoing, incoming);
            };
            uiManager.AddElement(backButton);
        }

        private void InitializeColliders()
        {
            testColliders.Add((
                Collider.CreateCircle(new Vector2(200, 200), 50f),
                new Vector4(1, 0, 0, 1),
                new Vector4(1, 0, 0, 1),
                false
            ));

            testColliders.Add((
                Collider.CreateRectangle(new Vector2(400, 200), new Vector2(100, 80)),
                new Vector4(0, 1, 0, 1),
                new Vector4(0, 1, 0, 1),
                false
            ));

            Vector2[] triangleVertices = new[]
            {
                new Vector2(0, -50),
                new Vector2(-50, 50),
                new Vector2(50, 50)
            };
            testColliders.Add((
                Collider.CreatePolygon(new Vector2(600, 200), triangleVertices),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1),
                false
            ));

            testColliders.Add((
                Collider.CreateCapsule(new Vector2(800, 200), 100f, 40f),
                new Vector4(1, 1, 0, 1),
                new Vector4(1, 1, 0, 1),
                false
            ));

            testColliders.Add((
                Collider.CreateLine(new Vector2(1000, 150), new Vector2(1000, 250)),
                new Vector4(1, 0, 1, 1),
                new Vector4(1, 0, 1, 1),
                false
            ));
        }

        private void UpdateMouseColliderShape()
        {
            Vector2 mousePos = InputManager.MousePosition;
            switch (currentCursorShapeIndex)
            {
                case 0: // Circle
                    mouseCollider = Collider.CreateCircle(mousePos, 25f);
                    break;
                case 1: // Rectangle
                    mouseCollider = Collider.CreateRectangle(mousePos, new Vector2(50, 50));
                    break;
                case 2: // Triangle
                    Vector2[] mouseTriangle = new[]
                    {
                        new Vector2(0, -25),
                        new Vector2(-25, 25),
                        new Vector2(25, 25)
                    };
                    mouseCollider = Collider.CreatePolygon(mousePos, mouseTriangle);
                    break;
                case 3: // Capsule
                    mouseCollider = Collider.CreateCapsule(mousePos, 50f, 20f);
                    break;
                case 4: // Line
                    Vector2 endPoint = mousePos + new Vector2(0, 50);
                    mouseCollider = Collider.CreateLine(mousePos, endPoint);
                    break;
            }
        }

        public override void Update(double deltaTime)
        {
            UpdateMouseColliderShape();
            rotationAngle += (float)deltaTime * 0.5f;

            HandleShapeSwitching();
            HandleDragging();
            HandleToggles();

            for (int i = 0; i < testColliders.Count; i++)
            {
                var (collider, color, originalColor, isDragging) = testColliders[i];
                bool isColliding = collider.CollidesWith(mouseCollider);
                color = isColliding || isDragging ? Vector4.One : originalColor;
                testColliders[i] = (collider, color, originalColor, isDragging);
            }

            base.Update(deltaTime); // Updates UI elements
        }

        private void HandleShapeSwitching()
        {
            for (int i = 0; i < shapeNames.Length; i++)
            {
                if (InputManager.IsKeyPressed((Key)((int)Key.Number1 + i)))
                {
                    currentCursorShapeIndex = i;
                    UpdateMouseColliderShape();
                }
            }
        }

        private void HandleDragging()
        {
            if (InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                for (int i = 0; i < testColliders.Count; i++)
                {
                    var (collider, color, originalColor, isDragging) = testColliders[i];
                    if (!isDragging && collider.CollidesWith(mouseCollider))
                    {
                        isDragging = true;
                        dragStartPosition = InputManager.MousePosition;
                        testColliders[i] = (collider, color, originalColor, isDragging);
                    }
                    else if (isDragging)
                    {
                        Vector2 delta = InputManager.MousePosition - dragStartPosition;
                        if (collider.Type == Collider.ShapeType.Line)
                        {
                            Vector2 newStart = collider.Position + delta;
                            Vector2 newEnd = collider.EndPoint + delta;
                            testColliders[i] = (Collider.CreateLine(newStart, newEnd), color, originalColor, isDragging);
                        }
                        else
                        {
                            collider.SetPosition(collider.Position + delta);
                            testColliders[i] = (collider, color, originalColor, isDragging);
                        }
                        dragStartPosition = InputManager.MousePosition;
                    }
                }
            }
            else
            {
                for (int i = 0; i < testColliders.Count; i++)
                {
                    var (collider, color, originalColor, isDragging) = testColliders[i];
                    if (isDragging)
                    {
                        testColliders[i] = (collider, color, originalColor, false);
                    }
                }
            }
        }

        private void HandleToggles()
        {
            if (InputManager.IsKeyPressed(Key.B))
                showBoundingBoxes = !showBoundingBoxes;
            if (InputManager.IsKeyPressed(Key.F))
                useFilledShapes = !useFilledShapes;
        }

        public override void Draw(double deltaTime)
        {
            // 1. Draw the background grid
            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            for (int x = 0; x < ScreenWidth; x += 50)
            {
                ShapeRenderer.DrawLine(
                    new Vector2(x, 0),
                    new Vector2(x, ScreenHeight),
                    new Vector4(0.2f, 0.2f, 0.2f, 0.3f)
                );
            }
            for (int y = 0; y < ScreenHeight; y += 50)
            {
                ShapeRenderer.DrawLine(
                    new Vector2(0, y),
                    new Vector2(ScreenWidth, y),
                    new Vector4(0.2f, 0.2f, 0.2f, 0.3f)
                );
            }
            ShapeRenderer.End();

            // 2. Draw the test colliders (excluding the cursor)
            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            foreach (var (collider, color, _, _) in testColliders)
            {
                DrawCollider(collider, color);
                if (showBoundingBoxes)
                    DrawBoundingBox(collider, new Vector4(0.5f, 0.5f, 0.5f, 0.3f));
            }
            ShapeRenderer.End();

            // 3. Draw the cursor shape text
            SpriteBatch.Begin(Camera, ScreenWidth, ScreenHeight);
            SpriteBatch.DrawText(fontRenderer,
                $"Cursor Shape: {shapeNames[currentCursorShapeIndex]} - Mode: {(useFilledShapes ? "Filled" : "Line")}",
                new Vector2(50, 50),
                fontStyle);
            SpriteBatch.End();

            // 4. Draw UI elements (buttons, etc.)
            base.Draw(deltaTime); // This renders the UIManager contents (e.g., "Back" button)

            // 5. Draw the cursor last to ensure it appears on top
            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);
            DrawCollider(mouseCollider, cursorColor);
            ShapeRenderer.End();
        }

        private void DrawCollider(Collider collider, Vector4 color)
        {
            if (useFilledShapes)
            {
                switch (collider.Type)
                {
                    case Collider.ShapeType.Circle:
                        ShapeRenderer.DrawCircle(collider.Position, collider.Radius, color, true);
                        break;
                    case Collider.ShapeType.Rectangle:
                        Vector2 rectPos = collider.Position - (collider.Size * 0.5f);
                        ShapeRenderer.DrawRectangle(rectPos, collider.Size, color, true);
                        break;
                    case Collider.ShapeType.Polygon:
                        ShapeRenderer.DrawPolygon(collider._absoluteVertices, color, true);
                        break;
                    case Collider.ShapeType.Capsule:
                        Vector2 capStart = collider.Position;
                        Vector2 capEnd = collider.Position + new Vector2(collider.Size.X, 0);
                        ShapeRenderer.DrawCircle(capStart, collider.Radius, color, true);
                        ShapeRenderer.DrawCircle(capEnd, collider.Radius, color, true);
                        Vector2 rectStart = capStart + new Vector2(0, -collider.Radius);
                        ShapeRenderer.DrawRectangle(rectStart, new Vector2(collider.Size.X, collider.Size.Y), color, true);
                        break;
                    case Collider.ShapeType.Line:
                        Vector2 direction = collider.EndPoint - collider.Position;
                        float length = direction.Length();
                        Vector2 normalizedDir = Vector2.Normalize(direction);
                        Vector2 perpendicular = new Vector2(-normalizedDir.Y, normalizedDir.X);
                        float thickness = 10f;
                        Vector2[] vertices = new[]
                        {
                            collider.Position + (perpendicular * thickness / 2),
                            collider.Position - (perpendicular * thickness / 2),
                            collider.EndPoint - (perpendicular * thickness / 2),
                            collider.EndPoint + (perpendicular * thickness / 2)
                        };
                        ShapeRenderer.DrawPolygon(vertices, color, true);
                        break;
                }
            }
            else
            {
                switch (collider.Type)
                {
                    case Collider.ShapeType.Circle:
                        ShapeRenderer.DrawCircle(collider.Position, collider.Radius, color, false);
                        break;
                    case Collider.ShapeType.Rectangle:
                        Vector2 rectPos = collider.Position - (collider.Size * 0.5f);
                        ShapeRenderer.DrawRectangle(rectPos, collider.Size, color, false);
                        break;
                    case Collider.ShapeType.Polygon:
                        ShapeRenderer.DrawPolygon(collider._absoluteVertices, color, false);
                        break;
                    case Collider.ShapeType.Capsule:
                        Vector2 capStart = collider.Position;
                        Vector2 capEnd = collider.Position + new Vector2(collider.Size.X, 0);
                        ShapeRenderer.DrawCircle(capStart, collider.Radius, color, false);
                        ShapeRenderer.DrawCircle(capEnd, collider.Radius, color, false);
                        Vector2 rectStart = capStart + new Vector2(0, -collider.Radius);
                        ShapeRenderer.DrawRectangle(rectStart, new Vector2(collider.Size.X, collider.Size.Y), color, false);
                        break;
                    case Collider.ShapeType.Line:
                        ShapeRenderer.DrawLine(collider.Position, collider.EndPoint, color, 4f);
                        ShapeRenderer.DrawCircle(collider.Position, 6f, color, true);
                        ShapeRenderer.DrawCircle(collider.EndPoint, 6f, color, true);
                        break;
                }
            }
        }

        private void DrawBoundingBox(Collider collider, Vector4 color)
        {
            Vector2 min = collider.Position;
            Vector2 max = collider.Position;

            switch (collider.Type)
            {
                case Collider.ShapeType.Circle:
                    min -= new Vector2(collider.Radius);
                    max += new Vector2(collider.Radius);
                    break;
                case Collider.ShapeType.Rectangle:
                    min -= collider.Size * 0.5f;
                    max += collider.Size * 0.5f;
                    break;
                case Collider.ShapeType.Polygon:
                    foreach (var vertex in collider._absoluteVertices)
                    {
                        min = Vector2.Min(min, vertex);
                        max = Vector2.Max(max, vertex);
                    }
                    break;
                case Collider.ShapeType.Capsule:
                    min -= new Vector2(0, collider.Radius);
                    max += new Vector2(collider.Size.X, collider.Radius);
                    break;
                case Collider.ShapeType.Line:
                    min = Vector2.Min(collider.Position, collider.EndPoint);
                    max = Vector2.Max(collider.Position, collider.EndPoint);
                    break;
            }

            ShapeRenderer.DrawRectangle(min, max - min, color, false);
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
        }
    }
}
