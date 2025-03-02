using System;
using System.Collections.Generic;
using System.Numerics;
using SDNGame.Core;
using SDNGame.Input;
using SDNGame.Rendering.Fonts;
using SDNGame.Rendering.Shapes;
using SDNGame.Scenes;
using SDNGame.Scenes.Transitioning;
using SDNGame.UI;
using Silk.NET.Input;
using Button = SDNGame.UI.Button;

namespace SDNGame.Core.GameScenes
{
    public class SoftBodyScene : Scene
    {
        private FontRenderer fontRenderer;
        private TextStyle titleStyle;
        private TextStyle buttonStyle;
        private TextStyle infoStyle;
        private UIManager uiManager => UIManager;

        private List<Particle> particles;
        private List<Spring> springs;
        private float springConstant = 4500f;
        private const float Gravity = 10000f;
        private const float Damping = 0.95f;
        private const float RestLength = 50f;
        private Vector2 groundPosition;
        private Particle draggedParticle;

        public SoftBodyScene(Game game) : base(game)
        {
            particles = new List<Particle>();
            springs = new List<Spring>();
            groundPosition = new Vector2(ScreenWidth / 2, ScreenHeight - 50);
            draggedParticle = null;
        }

        public override void LoadContent()
        {
            fontRenderer = new FontRenderer(Gl, SpriteBatch);
            fontRenderer.LoadFont("Assets/Fonts/HubotSans-Black.ttf", "HubotSans");

            titleStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 32f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            buttonStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Center
            };

            infoStyle = new TextStyle
            {
                FontFamily = "HubotSans",
                FontSize = 20f,
                Color = new Vector4(1, 1, 1, 1),
                Alignment = TextAlignment.Left
            };

            CreateSoftBody(new Vector2(ScreenWidth / 2, ScreenHeight / 4), 16);

            var titleLabel = new Label(fontRenderer,
                new Vector2(ScreenWidth / 2, 50),
                "2D Soft Body Simulator",
                titleStyle);
            uiManager.AddElement(titleLabel);

            var stiffnessLabel = new Label(fontRenderer,
                new Vector2(50, 100),
                $"Stiffness: {springConstant:F0} (Up/Down to adjust)",
                infoStyle);
            uiManager.AddElement(stiffnessLabel);

            var backButton = new Button(fontRenderer,
                new Vector2(50, ScreenHeight - 60),
                new Vector2(100, 40),
                "Back",
                buttonStyle);
            backButton.OnClick += () =>
            {
                var outgoing = new FadeTransition(Game, 0.5f, false);
                var incoming = new FadeTransition(Game, 0.5f, true);
                Game.SetScene(new MainMenuScene(Game), outgoing, incoming);
            };
            uiManager.AddElement(backButton);
        }

        private void CreateSoftBody(Vector2 center, int particleCount)
        {
            particles.Clear();
            springs.Clear();

            // grid arrangement
            int gridSize = (int)Math.Sqrt(particleCount);
            if (gridSize * gridSize < particleCount) gridSize++;
            for (int y = 0; y < gridSize && particles.Count < particleCount; y++)
            {
                for (int x = 0; x < gridSize && particles.Count < particleCount; x++)
                {
                    Vector2 offset = new Vector2(x - (gridSize - 1) / 2f, y - (gridSize - 1) / 2f) * RestLength;
                    particles.Add(new Particle(center + offset));
                }
            }

            // connect particles
            for (int i = 0; i < particles.Count; i++)
            {
                int x = i % gridSize;
                int y = i / gridSize;

                if (x < gridSize - 1)
                    springs.Add(new Spring(particles[i], particles[i + 1], RestLength, springConstant));
                if (y < gridSize - 1)
                    springs.Add(new Spring(particles[i], particles[i + gridSize], RestLength, springConstant));
                if (x < gridSize - 1 && y < gridSize - 1)
                {
                    float diagonalLength = Vector2.Distance(particles[i].Position, particles[i + gridSize + 1].Position);
                    springs.Add(new Spring(particles[i], particles[i + gridSize + 1], diagonalLength, springConstant));

                    diagonalLength = Vector2.Distance(particles[i + 1].Position, particles[i + gridSize].Position);
                    springs.Add(new Spring(particles[i + 1], particles[i + gridSize], diagonalLength, springConstant));
                }
            }
        }

        private void UpdateSpringConstants()
        {
            foreach (var spring in springs)
            {
                spring.SpringConstant = springConstant;
            }
        }

        public override void Update(double deltaTime)
        {
            float dt = (float)deltaTime;

            if (InputManager.IsKeyPressed(Key.Up))
            {
                springConstant += 100f;
                UpdateSpringConstants();
            }
            if (InputManager.IsKeyPressed(Key.Down))
            {
                springConstant = Math.Max(100f, springConstant - 100f);
                UpdateSpringConstants();
            }

            var stiffnessLabel = uiManager.Elements.Find(e => e is Label l && l.Text.StartsWith("Stiffness")) as Label;
            if (stiffnessLabel != null)
            {
                stiffnessLabel.Text = $"Stiffness: {springConstant:F0} (Up/Down to adjust)";
            }

            foreach (var particle in particles)
            {
                particle.ApplyForce(new Vector2(0, Gravity));
                particle.Update(dt, Damping);
            }

            foreach (var spring in springs)
            {
                spring.ApplyForce(dt);
            }

            Vector2 mousePos = InputManager.MousePosition;
            if (InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                if (draggedParticle == null)
                {
                    float minDistance = float.MaxValue;
                    foreach (var particle in particles)
                    {
                        float distance = Vector2.Distance(mousePos, particle.Position);
                        if (distance < 30f && distance < minDistance)
                        {
                            minDistance = distance;
                            draggedParticle = particle;
                        }
                    }
                }

                if (draggedParticle != null)
                {
                    Vector2 dragForce = (mousePos - draggedParticle.Position) * 1000f;
                    draggedParticle.ApplyForce(dragForce);
                }
            }
            else
            {
                draggedParticle = null;
            }

            foreach (var particle in particles)
            {
                try
                {
                    if (particle.Position.Y + particle.Radius > groundPosition.Y)
                    {
                        particle.Position = new Vector2(particle.Position.X, groundPosition.Y - particle.Radius);
                        particle.Velocity = new Vector2(particle.Velocity.X, -particle.Velocity.Y * 0.8f);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"Particle out of bounds: {particle.Position}");
                }
            }

            base.Update(deltaTime);
        }

        public override void Draw(double deltaTime)
        {
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

            ShapeRenderer.Begin(Camera, ScreenWidth, ScreenHeight);

            foreach (var spring in springs)
            {
                float avgVelocity = (spring.ParticleA.Velocity.Length() + spring.ParticleB.Velocity.Length()) / 2f;
                Vector4 springColor = GetColorFromVelocity(avgVelocity);
                ShapeRenderer.DrawLine(
                    spring.ParticleA.Position,
                    spring.ParticleB.Position,
                    springColor,
                    2f
                );
            }

            foreach (var particle in particles)
            {
                Vector4 particleColor = particle == draggedParticle
                    ? new Vector4(0f, 1f, 0f, 1f)
                    : GetColorFromVelocity(particle.Velocity.Length());
                ShapeRenderer.DrawCircle(
                    particle.Position,
                    particle.Radius,
                    particleColor,
                    true
                );
            }

            ShapeRenderer.DrawRectangle(
                new Vector2(0, groundPosition.Y),
                new Vector2(ScreenWidth, ScreenHeight - groundPosition.Y),
                new Vector4(0.3f, 0.3f, 0.3f, 1f),
                true
            );

            ShapeRenderer.End();

            base.Draw(deltaTime);
        }

        private Vector4 GetColorFromVelocity(float velocity)
        {
            const float maxVelocity = 500f;
            float t = Math.Clamp(velocity / maxVelocity, 0f, 1f);
            float r = t;
            float g = 0f;
            float b = 1f - t;
            return new Vector4(r, g, b, 1f);
        }

        public override void Dispose()
        {
            fontRenderer?.Dispose();
        }

        private class Particle
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Vector2 Force { get; set; }
            public float Radius { get; } = 10f;
            public float Mass { get; } = 1f;

            public Particle(Vector2 position)
            {
                Position = position;
                Velocity = Vector2.Zero;
                Force = Vector2.Zero;
            }

            public void ApplyForce(Vector2 force)
            {
                Force += force;
            }

            public void Update(float dt, float damping)
            {
                Velocity += (Force / Mass) * dt;
                Velocity *= damping;
                Position += Velocity * dt;
                Force = Vector2.Zero;
            }
        }

        private class Spring
        {
            public Particle ParticleA { get; }
            public Particle ParticleB { get; }
            public float RestLength { get; }
            public float SpringConstant { get; set; }

            public Spring(Particle a, Particle b, float restLength, float springConstant)
            {
                ParticleA = a;
                ParticleB = b;
                RestLength = restLength;
                SpringConstant = springConstant;
            }

            public void ApplyForce(float dt)
            {
                Vector2 delta = ParticleB.Position - ParticleA.Position;
                float currentLength = delta.Length();
                if (currentLength == 0) return;

                float displacement = currentLength - RestLength;
                Vector2 forceDirection = Vector2.Normalize(delta);
                Vector2 springForce = forceDirection * (displacement * SpringConstant);

                ParticleA.ApplyForce(springForce);
                ParticleB.ApplyForce(-springForce);
            }
        }
    }
}