using Aveva.CIE.Foundation.Basic.Collections;
using Aveva.CIE.Foundation.Basic.Geometry;
using Aveva.CIE.Foundation.Basic.Geometry.PMRQuadTree;
using Aveva.Core.Geometry;
using netDxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DxfLine = netDxf.Entities.Line;

namespace MyProvip
{
    public enum EntityType
    {
        Line,
        Circle,
        Arc,
        Polyline,
        Text,
        BlockReference,
        Dimension,
        Hatch,
        Solid,
        Ellipse,
        Spline,
        Point,
        Blockinfor,
        Connector
    }
    public class CadEntity
    {
        public EntityType Type { get; set; }
        public string LocalName { get; set; }
        public object EntityObject { get; set; }
        public Box2 Box { get; set; }

        public string GetBlockType()
        {
            if (EntityObject is BlockInfo blockInfo)
            {
                return blockInfo.BlockType.ToString();
            }
            return string.Empty;
        }

        public CadEntity(object obj)
        {
            if (obj.GetType() == typeof(BlockInfo))
            {
                Type = EntityType.Blockinfor;
                Box = (obj as BlockInfo).GetInsertBoundingBox();
            }
            else if (obj.GetType() == typeof(DiagLine))
            {
                Type = EntityType.Line;
                Box = obj.GetType() == typeof(DiagLine) ? (obj as DiagLine).UpdateBox() : new Box2();
            }
            EntityObject = obj;
            
        }
        public object Getobject()
        {
            if (Type == EntityType.Blockinfor)
            {
                BlockInfo blockInfo = EntityObject as BlockInfo;
                return blockInfo;
            }
            else if (Type == EntityType.Line)
            {
                DiagLine Line = EntityObject as DiagLine;
                return Line;
            }
            else { return null; }
        }
    }

    public class EntityDto
    {
        public string LocalName { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public float xP1 { get; set; }
        public float yP1 { get; set; }
        public float xP2 { get; set; }
        public float yP2 { get; set; }
        public float xCenter { get; set; }
        public float yCenter { get; set; }
        public int rotation {  get; set; }
        public float Radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public bool IsClosedPolyline { get; set; }
        public List<EntityDto> SubEntities { get; set; }
        public List<VertexDto> Vertices { get; set; }

    }

    public class VertexDto
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Branch
    {
        public Dictionary<string, List<EntityDto>> BranchData { get; set; }
    }

    public class DiagLine : IPMRQuadTreeSegment
    {
        public bool IsBlankable { get; set; }

        public Point2 P1 { get; set; }

        public Point2 P2 { get; set; }
        public string LocalName { get; set; }
        public bool IsProcessed { get; set; } = false;
        public Box2 Box { get; set; }
        public bool IsBoundingLine { get; set; } = false;

        private Box2 _cachedBoundingBox;

        public DiagLine(Point2 p1, Point2 p2, bool isBlankable = false)
        {
            P1 = p1;
            P2 = p2;
            IsBlankable = isBlankable;
            LocalName = string.Empty;
            UpdateBox();
        }
        public Box2 UpdateBox()
        {
            return UpdateBox(1);
        }

        public Box2 UpdateBox(double val = 0)
        {
            double minX = Math.Min(P1.X, P2.X) - val;
            double minY = Math.Min(P1.Y, P2.Y) - val;
            double maxX = Math.Max(P1.X, P2.X) + val;
            double maxY = Math.Max(P1.Y, P2.Y) + val;

            Box = new Box2(minX, minY, maxX - minX, maxY - minY);
            return Box;
        }
        public Box2 ToBox2()
        {
            if (_cachedBoundingBox == null)
            {
                _cachedBoundingBox = new Box2(P1, P2);
            }
            return _cachedBoundingBox;
        }
        public Direction Direction()
        {
            Direction direction = Aveva.Core.Geometry.Direction.Create( Position.Create(P1.X, P1.Y, 0), Position.Create(P2.X, P2.Y, 0));
            return direction;
        }
        public double Angle(DiagLine otherline)
        {
            Direction direction1 = Aveva.Core.Geometry.Direction.Create(Position.Create(P1.X, P1.Y, 0), Position.Create(P2.X, P2.Y, 0));
            Direction direction2 = Aveva.Core.Geometry.Direction.Create(Position.Create(otherline.P1.X, otherline.P1.Y, 0), Position.Create(otherline.P2.X, otherline.P2.Y, 0));
            return direction1.Angle(direction2);
        }

    }

    public static class  OderPipeline
    {
        public static PMRQuadTree QuadTree { get; private set; }

        private const int OPTIMAL_MAX_SEGMENTS_PER_NODE = 20;
        private const int OPTIMAL_MAX_DEPTH = 8;

        public static void InitializeQuadTree(List<DiagLine> lines, Box2 region)
        {
            QuadTree = new PMRQuadTree(OPTIMAL_MAX_SEGMENTS_PER_NODE, region, OPTIMAL_MAX_DEPTH);
            foreach (var DiagLine in lines)
            {
                QuadTree.AddSegment(DiagLine);
            }
        }

        public static List<DiagLine> InterSectionLine(Box2 QueryBox)
        {
            try
            {
                if (QuadTree == null)
                    throw new InvalidOperationException("QuadTree not initialized");
                if (QueryBox == null)
                    throw new ArgumentNullException(nameof(QueryBox), "QueryBox cannot be null");

                QuadTree.GetIntersectedSegments(QueryBox, out Aveva.CIE.Foundation.Basic.Collections.ISet<IPMRQuadTreeSegment> Intersect);
                List<DiagLine> result = Intersect.OfType<DiagLine>().ToList();
                return result;

            }catch(Exception ex)
            {
                Console.WriteLine($"Error during intersection: {ex.Message}");
                return new List<DiagLine>();
            }

}
        public static Point2 ToPoint2(this Vector3 vector3)
        {
            return new Point2(vector3.X, vector3.Y);
        }
        public static DiagLine ToDiagLine (this DxfLine dxfLine)
        {
            DiagLine line = new DiagLine(dxfLine.StartPoint.ToPoint2(), dxfLine.EndPoint.ToPoint2());
            return line;
        }
    }
}
