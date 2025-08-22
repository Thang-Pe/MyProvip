using Aveva.CIE.Foundation.Basic.Geometry;
using Aveva.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace MyProvip
{
    public struct Vector2D
    {
        public double X { get; }
        public double Y { get; }

        public Vector2D(double x, double y)
        {
            double length = Math.Sqrt(x * x + y * y);
            if (length <= 1e-10) // Tránh chia cho 0
            {
                X = 0;
                Y = 0;
            }
            else
            {
                X = x / length;
                Y = y / length;
            }
        }

        private Vector2D(double x, double y, bool normalize)
        {
            X = x;
            Y = y;
        }

        public double Length => Math.Sqrt(X * X + Y * Y);
        public bool IsZero => Length < 1e-10;

        public double AngleTo(Vector2D other)
        {
            if (IsZero || other.IsZero)
                return 0;
            double dot = X * other.X + Y * other.Y; // Tích vô hướng
            double det = X * other.Y - Y * other.X; // Định thức

            dot = Math.Max(-1.0, Math.Min(1.0, dot)); // Giới hạn giá trị dot để tránh NaN
            return Math.Atan2(det, dot); // Trả về góc theo radian
        }

        public double AngleToAbs(Vector2D other)
        {
            return Math.Abs(AngleTo(other));
        }

        public static Vector2D Right => new Vector2D(1, 0, true);
        public static Vector2D Up => new Vector2D(0, 1, true);
        public static Vector2D Down => new Vector2D(0, -1, true);
        public static Vector2D Left => new Vector2D(-1, 0, true);

        public Vector2D Reverse() => new Vector2D(-X, -Y, true);

        public override string ToString() => $"({X:F3}, {Y:F3})";

    }

    public static class GeometryExtension
    {
        public static Vector2D Normalize(this Vector2D v)
        {
            var length = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            if (length == 0) return new Vector2D(0, 0);
            return new Vector2D(v.X / length, v.Y / length);
        }

        public static double Dot(this Vector2D a, Vector2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector2D GetDirectionToVector(this Point2 from, Point2 to)
        {
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            return new Vector2D(dx, dy);
        }

        public static bool IsAbove(this Point2 upper, Point2 lower)
        {
            return upper.Y > lower.Y;
        }

        public static double GetDistanceTo(this Point2 a, Point2 b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static Box2 Expand(this Box2 box, double val)
        {
            double x1 = box.P1.X - val;
            double y1 = box.P1.Y - val;
            double x2 = box.P2.X + val;
            double y2 = box.P2.Y + val;
            return new Box2(x1, y1, x2 - x1, y2 - y1);
        }
        public static Position ToPosition(this Point2 point)
        {
            try
            {
                return Position.Create(point.X, point.Y, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ToPosition: {ex.Message}");
                throw;
            }

        }
        public static Direction GetDirectionTo(this Point2 point, Point2 otherpoint)
        {
            return Aveva.Core.Geometry.Direction.Create(point.ToPosition(), otherpoint.ToPosition());

        }

    }

    internal class IplReOrder
    {
        // Helper Method
        public static Vector2D DetermineInitialDirection(Vector2D direction)
        {
            // Tính góc với các hướng cơ bản
            var angles = new Dictionary<Vector2D, double>
            {
                [Vector2D.Right] = direction.AngleToAbs(Vector2D.Right),
                [Vector2D.Up] = direction.AngleToAbs(Vector2D.Up),
                [Vector2D.Left] = direction.AngleToAbs(Vector2D.Left),
                [Vector2D.Down] = direction.AngleToAbs(Vector2D.Down)
            };

            // Trả về hướng có góc nhỏ nhất
            return angles.OrderBy(x => x.Value).First().Key;
        }

        public static bool ShouldInsertAtBeginning(Vector2D direction)
        {
            // Ngưỡng có thể điều chỉnh (0.707 ≈ cos(45°))
            const double threshold = 0.707;

            // Nếu thành phần X âm mạnh (Left) HOẶC thành phần Y âm mạnh (Down)
            return direction.X < -threshold || direction.Y < -threshold;
        }

        public static bool IsIntersectionAtEndPoint(CadEntity currentEntity, CadEntity getEntity, double tolerance)
        {
            var currentEndpoints = GetEndPoints(currentEntity);
            var getEndpoints = GetEndPoints(getEntity);

            if (currentEndpoints == null || getEndpoints == null) return false;

            foreach (var currentPoint in currentEndpoints)
            {
                foreach (var getPoint in getEndpoints)
                {
                    double distance = currentPoint.GetDistanceTo(getPoint);
                    if (distance <= tolerance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<Point2> GetEndPoints(CadEntity entity)
        {
            var endpoints = new List<Point2>();
            var getObj = entity.Getobject();

            if (getObj is DiagLine diagLine)
            {
                endpoints.Add(diagLine.P1);
                endpoints.Add(diagLine.P2);
            }
            else if (getObj is BlockInfo block)
            {
                var box = block.BoundingBox;
                if (box != null)
                {
                    //    double minX = Math.Min(box.P1.X, box.P2.X);
                    //    double maxX = Math.Max(box.P1.X, box.P2.X);
                    //    double minY = Math.Min(box.P1.Y, box.P2.Y);
                    //    double maxY = Math.Max(box.P1.Y, box.P2.Y);

                    //    endpoints.Add(new Point2(minX, minY)); // Bottom-left
                    //    endpoints.Add(new Point2(maxX, minY)); // Bottom-right
                    //    endpoints.Add(new Point2(maxX, maxY)); // Top-right
                    //    endpoints.Add(new Point2(minX, maxY)); // Top-left

                    endpoints.Add(box.Center);
                }
            }

            return endpoints;
        }

        public static Box2 HandleCorrectBox(CadEntity currentEntity, int numExpand = 1)
        {
            try
            {
                if (currentEntity.Box == null) return null;
                Point2 point1 = new Point2(currentEntity.Box.P1.X, currentEntity.Box.P1.Y);
                Point2 point2 = new Point2(currentEntity.Box.P2.X, currentEntity.Box.P2.Y);

                Point2 minPoint = new Point2(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y));
                Point2 maxPoint = new Point2(Math.Max(point1.X, point2.X), Math.Max(point1.Y, point2.Y));

                Box2 boundingBox = new Box2(minPoint, maxPoint);

                Box2 boxxx = boundingBox.Expand(numExpand);

                return boxxx;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error in handleCorrectBox function about: {e.Message}");
                return null;
            }
        }
    }
}
