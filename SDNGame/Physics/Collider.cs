using System.Numerics;

namespace SDNGame.Physics
{
    public class Collider
    {
        public enum ShapeType
        {
            Circle,
            Rectangle,
            Polygon,
            Capsule,
            Line
        }

        public ShapeType Type { get; private set; }
        public Vector2 Position { get; private set; } // Center or start point (for Line)

        // Circle properties
        public float Radius { get; private set; }

        // Rectangle and Capsule properties
        public Vector2 Size { get; private set; } // Width and height (Rectangle), Length and thickness (Capsule)

        // Polygon properties
        public Vector2[] Vertices { get; private set; } // Relative to Position
        /* private */ public Vector2[] _absoluteVertices; // Cached absolute vertices
        private bool _verticesDirty = true;

        // Line properties
        public Vector2 EndPoint { get; private set; } // For Line only

        // Reusable arrays for calculations to reduce allocations
        private static readonly List<Vector2> _axesList = new List<Vector2>(20);
        private static readonly Vector2[] _tempVertices = new Vector2[8];

        // Constructors (kept for backward compatibility)
        public Collider(Vector2 position, float radius) // Circle
        {
            Type = ShapeType.Circle;
            Position = position;
            Radius = radius;
            Size = Vector2.Zero;
            Vertices = null;
            EndPoint = Vector2.Zero;
        }

        public Collider(Vector2 position, Vector2[] vertices) // Polygon
        {
            Type = ShapeType.Polygon;
            Position = position;
            Vertices = vertices;
            _absoluteVertices = new Vector2[vertices.Length];
            UpdateAbsoluteVertices();
            Radius = 0f;
            Size = Vector2.Zero;
            EndPoint = Vector2.Zero;
        }

        public Collider(Vector2 position, float length, float thickness) // Capsule
        {
            Type = ShapeType.Capsule;
            Position = position;
            Size = new Vector2(length, thickness);
            Radius = thickness / 2f;
            Vertices = null;
            EndPoint = Vector2.Zero;
        }

        // Factory Methods
        public static Collider CreateCircle(Vector2 position, float radius)
        {
            if (radius <= 0f)
                throw new ArgumentException("Circle radius must be positive.", nameof(radius));

            Collider collider = new Collider();
            collider.Type = ShapeType.Circle;
            collider.Position = position;
            collider.Radius = radius;
            collider.Size = Vector2.Zero;
            collider.Vertices = null;
            collider.EndPoint = Vector2.Zero;
            return collider;
        }

        public static Collider CreateRectangle(Vector2 position, Vector2 size)
        {
            if (size.X <= 0f || size.Y <= 0f)
                throw new ArgumentException("Rectangle dimensions must be positive.", nameof(size));

            Collider collider = new Collider();
            collider.Type = ShapeType.Rectangle;
            collider.Position = position;
            collider.Size = size;
            collider.Radius = 0f;
            collider.Vertices = null;
            collider.EndPoint = Vector2.Zero;
            return collider;
        }

        public static Collider CreatePolygon(Vector2 position, Vector2[] vertices)
        {
            if (vertices == null || vertices.Length < 3)
                throw new ArgumentException("Polygon must have at least 3 vertices.", nameof(vertices));

            Collider collider = new Collider();
            collider.Type = ShapeType.Polygon;
            collider.Position = position;
            collider.Vertices = vertices;
            collider._absoluteVertices = new Vector2[vertices.Length];
            collider.UpdateAbsoluteVertices();
            collider.Radius = 0f;
            collider.Size = Vector2.Zero;
            collider.EndPoint = Vector2.Zero;
            return collider;
        }

        public static Collider CreateCapsule(Vector2 position, float length, float thickness)
        {
            if (length <= 0f || thickness <= 0f)
                throw new ArgumentException("Capsule dimensions must be positive.", nameof(length));

            Collider collider = new Collider();
            collider.Type = ShapeType.Capsule;
            collider.Position = position;
            collider.Size = new Vector2(length, thickness);
            collider.Radius = thickness / 2f;
            collider.Vertices = null;
            collider.EndPoint = Vector2.Zero;
            return collider;
        }

        public static Collider CreateLine(Vector2 startPoint, Vector2 endPoint)
        {
            if (startPoint == endPoint)
                throw new ArgumentException("Line endpoints must be different.", nameof(endPoint));

            Collider collider = new Collider();
            collider.Type = ShapeType.Line;
            collider.Position = startPoint;
            collider.EndPoint = endPoint;
            collider.Radius = 0f;
            collider.Size = Vector2.Zero;
            collider.Vertices = null;
            return collider;
        }

        // Default private constructor for factory methods
        private Collider() { }

        // When Position is updated, mark vertices as dirty
        public void SetPosition(Vector2 newPosition)
        {
            if (Position != newPosition)
            {
                Position = newPosition;
                _verticesDirty = true;
            }
        }

        // Update absolute vertices cache
        private void UpdateAbsoluteVertices()
        {
            if (Type == ShapeType.Polygon && _verticesDirty)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    _absoluteVertices[i] = Position + Vertices[i];
                }
                _verticesDirty = false;
            }
        }

        // Collision detection
        public bool CollidesWith(Collider other)
        {
            if (other == null) return false;

            // Update cached values before collision check
            if (Type == ShapeType.Polygon) UpdateAbsoluteVertices();
            if (other.Type == ShapeType.Polygon) other.UpdateAbsoluteVertices();

            switch (Type)
            {
                case ShapeType.Circle:
                    return CollideCircle(other);
                case ShapeType.Rectangle:
                    return CollideRectangle(other);
                case ShapeType.Polygon:
                    return CollidePolygon(other);
                case ShapeType.Capsule:
                    return CollideCapsule(other);
                case ShapeType.Line:
                    return CollideLine(other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        private bool CollideCircle(Collider other)
        {
            switch (other.Type)
            {
                case ShapeType.Circle:
                    return CircleVsCircle(this, other);
                case ShapeType.Rectangle:
                    return CircleVsRectangle(this, other);
                case ShapeType.Polygon:
                    return CircleVsPolygon(this, other);
                case ShapeType.Capsule:
                    return CircleVsCapsule(this, other);
                case ShapeType.Line:
                    return CircleVsLine(this, other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        private bool CollideRectangle(Collider other)
        {
            switch (other.Type)
            {
                case ShapeType.Circle:
                    return CircleVsRectangle(other, this);
                case ShapeType.Rectangle:
                    return RectangleVsRectangle(this, other);
                case ShapeType.Polygon:
                    return RectangleVsPolygon(this, other);
                case ShapeType.Capsule:
                    return RectangleVsCapsule(this, other);
                case ShapeType.Line:
                    return RectangleVsLine(this, other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        private bool CollidePolygon(Collider other)
        {
            switch (other.Type)
            {
                case ShapeType.Circle:
                    return CircleVsPolygon(other, this);
                case ShapeType.Rectangle:
                    return RectangleVsPolygon(other, this);
                case ShapeType.Polygon:
                    return PolygonVsPolygon(this, other);
                case ShapeType.Capsule:
                    return PolygonVsCapsule(this, other);
                case ShapeType.Line:
                    return PolygonVsLine(this, other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        private bool CollideCapsule(Collider other)
        {
            switch (other.Type)
            {
                case ShapeType.Circle:
                    return CircleVsCapsule(other, this);
                case ShapeType.Rectangle:
                    return RectangleVsCapsule(other, this);
                case ShapeType.Polygon:
                    return PolygonVsCapsule(other, this);
                case ShapeType.Capsule:
                    return CapsuleVsCapsule(this, other);
                case ShapeType.Line:
                    return CapsuleVsLine(this, other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        private bool CollideLine(Collider other)
        {
            switch (other.Type)
            {
                case ShapeType.Circle:
                    return CircleVsLine(other, this);
                case ShapeType.Rectangle:
                    return RectangleVsLine(other, this);
                case ShapeType.Polygon:
                    return PolygonVsLine(other, this);
                case ShapeType.Capsule:
                    return CapsuleVsLine(other, this);
                case ShapeType.Line:
                    return LineVsLine(this, other);
                default:
                    throw new NotSupportedException("Unknown collider type.");
            }
        }

        // Collision methods
        private static bool CircleVsCircle(Collider c1, Collider c2)
        {
            float distance = Vector2.Distance(c1.Position, c2.Position);
            return distance < c1.Radius + c2.Radius;
        }

        private static bool RectangleVsRectangle(Collider r1, Collider r2)
        {
            Vector2 r1Min = r1.Position - r1.Size * 0.5f;
            Vector2 r1Max = r1.Position + r1.Size * 0.5f;
            Vector2 r2Min = r2.Position - r2.Size * 0.5f;
            Vector2 r2Max = r2.Position + r2.Size * 0.5f;

            return r1Min.X < r2Max.X && r1Max.X > r2Min.X &&
                   r1Min.Y < r2Max.Y && r1Max.Y > r2Min.Y;
        }

        private static bool CircleVsRectangle(Collider circle, Collider rect)
        {
            Vector2 rectMin = rect.Position - rect.Size * 0.5f;
            Vector2 rectMax = rect.Position + rect.Size * 0.5f;
            float closestX = Math.Clamp(circle.Position.X, rectMin.X, rectMax.X);
            float closestY = Math.Clamp(circle.Position.Y, rectMin.Y, rectMax.Y);
            float distance = Vector2.Distance(circle.Position, new Vector2(closestX, closestY));
            return distance < circle.Radius;
        }

        private static bool CircleVsPolygon(Collider circle, Collider polygon)
        {
            Vector2 closestPoint = ClosestPointOnPolygon(circle.Position, polygon._absoluteVertices);
            return Vector2.Distance(circle.Position, closestPoint) < circle.Radius;
        }

        private static bool RectangleVsPolygon(Collider rect, Collider polygon)
        {
            Vector2[] rectVertices = GetRectangleVerticesNonAlloc(rect, _tempVertices);
            return SATCollision(rectVertices, 4, polygon._absoluteVertices, polygon._absoluteVertices.Length);
        }

        private static bool PolygonVsPolygon(Collider p1, Collider p2)
        {
            return SATCollision(p1._absoluteVertices, p1._absoluteVertices.Length,
                              p2._absoluteVertices, p2._absoluteVertices.Length);
        }

        private static bool CircleVsCapsule(Collider circle, Collider capsule)
        {
            Vector2 capsuleStart = capsule.Position;
            Vector2 capsuleEnd = capsule.Position + new Vector2(capsule.Size.X, 0);
            Vector2 closestPoint = ClosestPointOnLineSegment(capsuleStart, capsuleEnd, circle.Position);
            return Vector2.Distance(circle.Position, closestPoint) < circle.Radius + capsule.Radius;
        }

        private static bool RectangleVsCapsule(Collider rect, Collider capsule)
        {
            Vector2[] rectVertices = GetRectangleVerticesNonAlloc(rect, _tempVertices);
            Vector2 capsuleStart = capsule.Position;
            Vector2 capsuleEnd = capsule.Position + new Vector2(capsule.Size.X, 0);
            return CapsuleVsPolygon(capsuleStart, capsuleEnd, capsule.Radius, rectVertices, 4);
        }

        private static bool PolygonVsCapsule(Collider polygon, Collider capsule)
        {
            Vector2 capsuleStart = capsule.Position;
            Vector2 capsuleEnd = capsule.Position + new Vector2(capsule.Size.X, 0);
            return CapsuleVsPolygon(capsuleStart, capsuleEnd, capsule.Radius, polygon._absoluteVertices, polygon._absoluteVertices.Length);
        }

        private static bool CapsuleVsCapsule(Collider c1, Collider c2)
        {
            Vector2 start1 = c1.Position;
            Vector2 end1 = c1.Position + new Vector2(c1.Size.X, 0);
            Vector2 start2 = c2.Position;
            Vector2 end2 = c2.Position + new Vector2(c2.Size.X, 0);
            Vector2 closest1 = ClosestPointOnLineSegment(start1, end1, start2);
            Vector2 closest2 = ClosestPointOnLineSegment(start2, end2, start1);
            float distance = Vector2.Distance(closest1, closest2);
            return distance < c1.Radius + c2.Radius;
        }

        private static bool CircleVsLine(Collider circle, Collider line)
        {
            Vector2 closestPoint = ClosestPointOnLineSegment(line.Position, line.EndPoint, circle.Position);
            return Vector2.Distance(circle.Position, closestPoint) < circle.Radius;
        }

        private static bool RectangleVsLine(Collider rect, Collider line)
        {
            Vector2[] rectVertices = GetRectangleVerticesNonAlloc(rect, _tempVertices);
            return LineVsPolygon(line.Position, line.EndPoint, rectVertices, 4);
        }

        private static bool PolygonVsLine(Collider polygon, Collider line)
        {
            return LineVsPolygon(line.Position, line.EndPoint, polygon._absoluteVertices, polygon._absoluteVertices.Length);
        }

        private static bool CapsuleVsLine(Collider capsule, Collider line)
        {
            Vector2 capsuleStart = capsule.Position;
            Vector2 capsuleEnd = capsule.Position + new Vector2(capsule.Size.X, 0);
            Vector2 closestCapsule = ClosestPointOnLineSegment(capsuleStart, capsuleEnd, line.Position);
            Vector2 closestLine = ClosestPointOnLineSegment(line.Position, line.EndPoint, capsuleStart);
            float distance = Vector2.Distance(closestCapsule, closestLine);
            return distance < capsule.Radius;
        }

        private static bool LineVsLine(Collider l1, Collider l2)
        {
            Vector2 a = l1.Position, b = l1.EndPoint;
            Vector2 c = l2.Position, d = l2.EndPoint;
            float denominator = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
            if (Math.Abs(denominator) < float.Epsilon) return false; // Parallel lines
            float t = ((c.X - a.X) * (d.Y - c.Y) - (c.Y - a.Y) * (d.X - c.X)) / denominator;
            float u = ((c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X)) / denominator;
            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }

        // Helper methods
        private static Vector2[] GetRectangleVerticesNonAlloc(Collider rect, Vector2[] output)
        {
            Vector2 halfSize = rect.Size * 0.5f;
            output[0] = rect.Position + new Vector2(-halfSize.X, -halfSize.Y);
            output[1] = rect.Position + new Vector2(halfSize.X, -halfSize.Y);
            output[2] = rect.Position + new Vector2(halfSize.X, halfSize.Y);
            output[3] = rect.Position + new Vector2(-halfSize.X, halfSize.Y);
            return output;
        }

        private static Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ap = p - a;
            Vector2 ab = b - a;
            float t = Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab);
            t = Math.Clamp(t, 0f, 1f);
            return a + t * ab;
        }

        private static Vector2 ClosestPointOnPolygon(Vector2 point, Vector2[] vertices)
        {
            int vertexCount = vertices.Length;
            Vector2 closest = vertices[0];
            float minDistance = Vector2.DistanceSquared(point, closest);

            for (int i = 1; i < vertexCount; i++)
            {
                Vector2 v = vertices[i];
                float distance = Vector2.DistanceSquared(point, v);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = v;
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                int j = (i + 1) % vertexCount;
                Vector2 closestOnEdge = ClosestPointOnLineSegment(vertices[i], vertices[j], point);
                float distance = Vector2.DistanceSquared(point, closestOnEdge);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = closestOnEdge;
                }
            }
            return closest;
        }

        private static bool SATCollision(Vector2[] vertices1, int count1, Vector2[] vertices2, int count2)
        {
            _axesList.Clear();

            for (int i = 0; i < count1; i++)
            {
                Vector2 p1 = vertices1[i];
                Vector2 p2 = vertices1[(i + 1) % count1];
                Vector2 edge = p2 - p1;
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                normal = Vector2.Normalize(normal);

                bool isUnique = true;
                for (int j = 0; j < _axesList.Count; j++)
                {
                    if (Vector2.DistanceSquared(_axesList[j], normal) < 0.0001f ||
                        Vector2.DistanceSquared(_axesList[j], -normal) < 0.0001f)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                    _axesList.Add(normal);
            }

            for (int i = 0; i < count2; i++)
            {
                Vector2 p1 = vertices2[i];
                Vector2 p2 = vertices2[(i + 1) % count2];
                Vector2 edge = p2 - p1;
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                normal = Vector2.Normalize(normal);

                bool isUnique = true;
                for (int j = 0; j < _axesList.Count; j++)
                {
                    if (Vector2.DistanceSquared(_axesList[j], normal) < 0.0001f ||
                        Vector2.DistanceSquared(_axesList[j], -normal) < 0.0001f)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                    _axesList.Add(normal);
            }

            foreach (var axis in _axesList)
            {
                float min1 = float.MaxValue, max1 = float.MinValue;
                float min2 = float.MaxValue, max2 = float.MinValue;

                for (int i = 0; i < count1; i++)
                {
                    float dot = Vector2.Dot(vertices1[i], axis);
                    min1 = Math.Min(min1, dot);
                    max1 = Math.Max(max1, dot);
                }

                for (int i = 0; i < count2; i++)
                {
                    float dot = Vector2.Dot(vertices2[i], axis);
                    min2 = Math.Min(min2, dot);
                    max2 = Math.Max(max2, dot);
                }

                if (min1 > max2 || min2 > max1)
                    return false;
            }

            return true;
        }

        private static bool CapsuleVsPolygon(Vector2 capsuleStart, Vector2 capsuleEnd, float radius, Vector2[] polyVertices, int vertexCount)
        {
            Vector2 closestPoint = ClosestPointOnPolygon(capsuleStart, polyVertices);
            Vector2 closestSegment = ClosestPointOnLineSegment(capsuleStart, capsuleEnd, closestPoint);
            float distance = Vector2.Distance(closestPoint, closestSegment);

            if (distance < radius)
                return true;

            Vector2 closestToStart = ClosestPointOnPolygon(capsuleStart, polyVertices);
            if (Vector2.Distance(capsuleStart, closestToStart) < radius)
                return true;

            Vector2 closestToEnd = ClosestPointOnPolygon(capsuleEnd, polyVertices);
            if (Vector2.Distance(capsuleEnd, closestToEnd) < radius)
                return true;

            return false;
        }

        private static bool LineVsPolygon(Vector2 lineStart, Vector2 lineEnd, Vector2[] polyVertices, int vertexCount)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 p1 = polyVertices[i];
                Vector2 p2 = polyVertices[(i + 1) % vertexCount];

                Collider line1 = CreateLine(lineStart, lineEnd);
                Collider line2 = CreateLine(p1, p2);

                if (LineVsLine(line1, line2))
                    return true;
            }

            return IsPointInPolygon(lineStart, polyVertices, vertexCount);
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2[] vertices, int vertexCount)
        {
            bool inside = false;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                    (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) /
                    (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}