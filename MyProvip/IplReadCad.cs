using Aveva.CIE.Foundation.Basic.Geometry;
using netDxf;
using netDxf.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using entitype = netDxf.Entities.EntityType;

namespace MyProvip
{
    public class IplReadCad
    {
        public netDxf.Entities.Insert Insert;

        public PointF TransformToImage(Vector3 point, double minX, double minY, double scale, double offsetX, double offsetY, int imageSize)
        {
            float x = (float)((point.X - minX) * scale + offsetX);
            float y = (float)((point.Y - minY) * scale + offsetY);
            // Gốc toạ độ GDI ở top-left nên phải đảo trục Y
            return new PointF(x, imageSize - y);
        }

        public List<Vector2> GetEntityPoints(EntityObject entity)
        {
            var points = new List<Vector2>();

            switch (entity.Type)
            {
                case entitype.Line:
                    var line = (Line)entity;
                    points.Add(new Vector2(line.StartPoint.X, line.StartPoint.Y));
                    points.Add(new Vector2(line.EndPoint.X, line.EndPoint.Y));
                    break;

                case entitype.Circle:
                    var circle = (Circle)entity;
                    double radius = circle.Radius;
                    points.Add(new Vector2(circle.Center.X - radius, circle.Center.Y - radius));
                    points.Add(new Vector2(circle.Center.X + radius, circle.Center.Y + radius));
                    break;

                case entitype.Polyline2D:
                    var polyline = (Polyline2D)entity;
                    points.AddRange(polyline.Vertexes.Select(v => new Vector2(v.Position.X, v.Position.Y)));
                    break;

                case entitype.Arc:
                    var arc = (Arc)entity;
                    double cx = arc.Center.X;
                    double cy = arc.Center.Y;
                    double r = arc.Radius;

                    // Calculate bounding box based on the arc's start and end angles
                    double startAngle = arc.StartAngle * Math.PI / 180.0;
                    double endAngle = arc.EndAngle * Math.PI / 180.0;

                    // Add the arc's center and radius to determine bounding points
                    var arcPoints = new List<Vector2>
                        {
                            new Vector2(cx - r, cy - r), // Top-left corner of bounding box
                            new Vector2(cx + r, cy + r)  // Bottom-right corner of bounding box
                        };

                    // Optionally, sample points along the arc for more precise bounding
                    int segments = 36; // Number of points to sample along the arc
                    double deltaAngle = (endAngle - startAngle) / segments;

                    for (int i = 0; i <= segments; i++)
                    {
                        double angle = startAngle + i * deltaAngle;
                        double x = cx + r * Math.Cos(angle);
                        double y = cy + r * Math.Sin(angle);
                        arcPoints.Add(new Vector2(x, y));
                    }

                    return arcPoints;


                case entitype.Spline:
                    var spline = (Spline)entity;
                    points.AddRange(spline.PolygonalVertexes(36).Select(v => new Vector2(v.X, v.Y)));
                    break;

                    // Add cases for other entity types as needed
            }

            return points;
        }

        public Bitmap RenderBlockAsBitmap(BlockInfo blockInfo)
        {
            const int imageSize = 128;
            Bitmap bmp = new Bitmap(imageSize, imageSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                var insert = blockInfo.Insert;
                //if (insert.Block.Name == "BIPC CONICAL TOP TANK")
                //{
                //    MessageBox.Show("Vo day");
                //}
                var entities = insert.Block?.Entities;

                if (entities == null || entities.Count == 0)
                    return bmp;

                // Calculate bounding box for scaling
                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                foreach (var ent in entities)
                {
                    foreach (var vertex in GetEntityPoints(ent))
                    {
                        minX = Math.Min(minX, vertex.X);
                        minY = Math.Min(minY, vertex.Y);
                        maxX = Math.Max(maxX, vertex.X);
                        maxY = Math.Max(maxY, vertex.Y);
                    }
                }

                double width = maxX - minX;
                double height = maxY - minY;
                double scale = 0.9 * Math.Min(imageSize / width, imageSize / height);
                double offsetX = (imageSize - scale * width) / 2.0;
                double offsetY = (imageSize - scale * height) / 2.0;

                Pen pen = Pens.Black;

                foreach (var ent in entities)
                {
                    switch (ent.Type)
                    {
                        case entitype.Line:
                            var ln = (Line)ent;
                            if (ln.StartPoint == ln.EndPoint) break;
                            var p1 = TransformToImage(ln.StartPoint, minX, minY, scale, offsetX, offsetY, imageSize);
                            var p2 = TransformToImage(ln.EndPoint, minX, minY, scale, offsetX, offsetY, imageSize);
                            g.DrawLine(pen, p1, p2);
                            break;

                        case entitype.Circle:
                            var c = (Circle)ent;
                            if (c.Radius <= 0) break;
                            var center = TransformToImage(c.Center, minX, minY, scale, offsetX, offsetY, imageSize);
                            float radius = (float)(c.Radius * scale);
                            g.DrawEllipse(pen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                            break;

                        case entitype.Arc:
                            var arc = (Arc)ent;
                            double cx = arc.Center.X;
                            double cy = arc.Center.Y;
                            double r = arc.Radius;

                            double start = arc.StartAngle;
                            double end = arc.EndAngle;
                            if (end < start) end += 360;

                            int segments = 32;
                            double delta = (end - start) / segments;

                            var arcPoints = new List<PointF>();
                            for (int i = 0; i <= segments; i++)
                            {
                                double angle = (start + i * delta) * Math.PI / 180.0;
                                double x = cx + r * Math.Cos(angle);
                                double y = cy + r * Math.Sin(angle);
                                arcPoints.Add(TransformToImage(new Vector3(x, y, 0), minX, minY, scale, offsetX, offsetY, imageSize));
                            }

                            if (arcPoints.Count > 1)
                                g.DrawLines(pen, arcPoints.ToArray());
                            break;

                        case entitype.Polyline2D:
                            var polyline = (Polyline2D)ent;
                            var vertices = polyline.Vertexes.ToList();

                            // Ensure closed polylines are properly handled
                            if (polyline.IsClosed && vertices.Count >= 2)
                            {
                                var firstVertex = vertices.First().Position;
                                var lastVertex = vertices.Last().Position;

                                if (Math.Abs(firstVertex.X - lastVertex.X) > 1e-4 || Math.Abs(firstVertex.Y - lastVertex.Y) > 1e-4)
                                {
                                    vertices.Add(new Polyline2DVertex(firstVertex));
                                }
                            }

                            // Transform vertices to image coordinates
                            var polylinePoints = vertices
                                .Select(v => TransformToImage(new Vector3(v.Position.X, v.Position.Y, 0), minX, minY, scale, offsetX, offsetY, imageSize))
                                .ToArray();

                            // Draw the polyline
                            if (polylinePoints.Length > 1)
                            {
                                if (polyline.IsClosed)
                                {
                                    g.DrawPolygon(pen, polylinePoints);
                                }
                                else
                                {
                                    g.DrawLines(pen, polylinePoints);
                                }
                            }
                            break;


                        case entitype.Spline:
                            var spline = (Spline)ent;
                            var splinePoints = spline.PolygonalVertexes(36)
                                .Select(p => TransformToImage(new Vector3(p.X, p.Y, 0), minX, minY, scale, offsetX, offsetY, imageSize))
                                .ToArray();

                            if (splinePoints.Length > 1)
                                g.DrawLines(pen, splinePoints);
                            break;
                    }
                }
            }

            return bmp;
        }


        public void ProcessTagsInBox(List<CadEntity> listTags, CadEntity currentEntity, Box2 boxEntity, Dictionary<string, string> textOfBlock)
        {
            for (int i = listTags.Count - 1; i >= 0; i--)
            {
                var tag = listTags[i];
                var tagInfor = tag.Getobject() as BlockInfo;

                if (currentEntity.LocalName.StartsWith("l_")) continue;
                if (tagInfor?.Insert == null) continue;

                var tagPos = tagInfor.Insert.Position;
                if (boxEntity.Contains(new Point2(tagPos.X, tagPos.Y)))
                {
                    string tagText = BuildTagText(tagInfor);

                    // Remove tag khỏi list
                    listTags.RemoveAt(i);

                    if (!string.IsNullOrEmpty(tagText))
                    {
                        textOfBlock[currentEntity.LocalName] = tagText;
                        Debug.WriteLine($"✅ Thêm tag {tag.LocalName} vào block {currentEntity.LocalName} với text: {tagText}");
                    }
                }
            }
        }

        private string BuildTagText(BlockInfo tagInfor)
        {
            string tagText = "";

            if (tagInfor.Insert.Attributes.Count > 0)
            {
                foreach (var att in tagInfor.Insert.Attributes)
                {
                    if (tagText.Length > 0)
                        tagText += "&#xA;"; // xuống dòng
                    tagText += att.Value;
                }
            }

            return tagText;
        }

        // Tim SCPLIN name
        public string FindScplinNameFromTags(List<CadEntity> listTags, Box2 boxEntity, Dictionary<string, string> textOfBlock)
        {
            for (int i = listTags.Count - 1; i >= 0; i--)
            {
                var tag = listTags[i];
                var tagInfor = tag.Getobject() as BlockInfo;

                if (tagInfor?.Insert == null) continue;

                var tagPos = tagInfor.Insert.Position;
                if (boxEntity.Contains(new Point2(tagPos.X, tagPos.Y)))
                {
                    if (tagInfor.Insert.Attributes.Count > 0)
                    {
                        foreach (var att in tagInfor.Insert.Attributes)
                        {
                            string attValue = att.Value?.Trim();
                            if (!string.IsNullOrEmpty(attValue))
                            {
                                return attValue;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // Xử lý các branch tạm thời - kiểm tra va chạm
        public void ProcessTempBranches(List<List<CadEntity>> tempBranches, Dictionary<string, List<List<CadEntity>>> scplinBranches)
        {
            var branchesToRemove = new List<List<CadEntity>>();
            foreach (var tempBranch in tempBranches)
            {
                string foundScplin = FindCollisionWithExistingBranches(tempBranch, scplinBranches);

                if (!string.IsNullOrEmpty(foundScplin))
                {
                    scplinBranches[foundScplin].Add(tempBranch);
                    branchesToRemove.Add(tempBranch);
                    Debug.WriteLine($"✅ Đã thêm branch tạm vào {foundScplin} do va chạm");
                }
            }

            // Xóa các branch đã được phân loại
            foreach (var branch in branchesToRemove)
            {
                tempBranches.Remove(branch);
            }
        }

        // Kiểm tra va chạm giữa branch tạm với các branch đã phân loại
        private string FindCollisionWithExistingBranches(List<CadEntity> tempBranch, Dictionary<string, List<List<CadEntity>>> scplinBranches)
        {
            foreach (var scplinGroup in scplinBranches)
            {
                foreach (var existingBranch in scplinGroup.Value)
                {
                    if (BranchesCollide(tempBranch, existingBranch))
                    {
                        return scplinGroup.Key;
                    }
                }
            }

            return null;
        }

        // Kiểm tra 2 branch có va chạm không
        private bool BranchesCollide(List<CadEntity> branch1, List<CadEntity> branch2)
        {
            foreach (var entity1 in branch1)
            {
                foreach (var entity2 in branch2)
                {
                    if (EntitiesCollide(entity1, entity2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Kiem tra 2 entity co va cham khong
        private bool EntitiesCollide(CadEntity entity1, CadEntity entity2)
        {
            var boxEntity1 = IplReOrder.HandleCorrectBox(entity1);

            List<DiagLine> interSectionLine = OderPipeline.InterSectionLine(boxEntity1);

            DiagLine foundNextBoundingLine = null;
            if (interSectionLine.Count > 0)
            {
                foundNextBoundingLine = interSectionLine
                    .Where(line => line.LocalName == entity2.LocalName)
                    .FirstOrDefault();

                if (foundNextBoundingLine != null)
                {
                    return true;
                }
            }

            return false;
        }

        // Tạo JSON với cấu trúc SCPLIN
        public List<Dictionary<string, Dictionary<string, List<EntityDto>>>> CreateScplinStructuredJson(
            Dictionary<string, List<List<CadEntity>>> scplinBranches,
            List<List<CadEntity>> tempBranches, Dictionary<string, string> textOfBlock)
        {
            var result = new List<Dictionary<string, Dictionary<string, List<EntityDto>>>>();

            // Thêm các SCPLIN đã xác định
            foreach (var scplinGroup in scplinBranches)
            {
                var pipelineData = new Dictionary<string, Dictionary<string, List<EntityDto>>>();
                var branchDict = new Dictionary<string, List<EntityDto>>();

                int branchCounter = 1;
                foreach (var branch in scplinGroup.Value)
                {
                    string branchName = $"SCBRAN {branchCounter}";

                    var entityDtos = ConvertBranchToEntityDtos(branch, textOfBlock);
                    branchDict[branchName] = entityDtos;
                    branchCounter++;
                }

                pipelineData[scplinGroup.Key] = branchDict;
                result.Add(pipelineData);
            }

            // Thêm các branch chưa phân loại vào SCPLIN Unknown
            if (tempBranches.Count > 0)
            {
                var unknownPipelineData = new Dictionary<string, Dictionary<string, List<EntityDto>>>();
                var unknownBranchDict = new Dictionary<string, List<EntityDto>>();

                int branchCounter = 1;
                foreach (var branch in tempBranches)
                {
                    string branchName = $"SCBRAN {branchCounter}";

                    var entityDtos = ConvertBranchToEntityDtos(branch, textOfBlock);
                    unknownBranchDict[branchName] = entityDtos;
                    branchCounter++;
                }

                unknownPipelineData["SCPLIN Unknown"] = unknownBranchDict;
                result.Add(unknownPipelineData);
            }

            return result;
        }

        // Helper method chuyển đổi branch thành EntityDto
        private List<EntityDto> ConvertBranchToEntityDtos(List<CadEntity> branch, Dictionary<string, string>  textOfBlock)
        {
            return branch
                .Where(entity => !(entity.Getobject() is BlockInfo block && block.BoundingBox == null))
                .Select(entity => new EntityDto
                {
                    LocalName = entity.LocalName,
                    Text = textOfBlock.ContainsKey(entity.LocalName) ? textOfBlock[entity.LocalName] : null,
                    Type = string.IsNullOrEmpty(entity.GetBlockType()) ? "Line" : entity.GetBlockType(),
                    xP1 = entity.Getobject() is DiagLine xline1 ? (float)xline1.P1.X : 0,
                    yP1 = entity.Getobject() is DiagLine yline1 ? (float)yline1.P1.Y : 0,
                    xP2 = entity.Getobject() is DiagLine xline2 ? (float)xline2.P2.X : 0,
                    yP2 = entity.Getobject() is DiagLine yline2 ? (float)yline2.P2.Y : 0,
                    xCenter = entity.Getobject() is BlockInfo xblock ? (float)xblock.BoundingBox.Center.X : 0,
                    yCenter = entity.Getobject() is BlockInfo yblock ? (float)yblock.BoundingBox.Center.Y : 0,
                    rotation = entity.Getobject() is BlockInfo ro ? (int)ro.Insert.Rotation : 0,
                    SubEntities = getSubEntities(entity),
                }).ToList();
        }

        public List<EntityDto> getSubEntities(CadEntity entity)
        {
            var subEntities = new List<EntityDto>();
            if (entity.Getobject() is BlockInfo blockInfo)
            {
                try
                {
                    bool isSharedDisplay = blockInfo.Insert.Block.Name == "SHARED DISPLAY";
                    if (isSharedDisplay)
                    {
                        Debug.WriteLine("Vo day");
                    }

                    var insertPos = blockInfo.Insert.Position;
                    var rotationRad = blockInfo.Insert.Rotation * Math.PI / 180.0;
                    double cosA = Math.Cos(rotationRad);
                    double sinA = Math.Sin(rotationRad);

                    double scaleX = Math.Abs(blockInfo.Insert.Scale.X);
                    double scaleY = Math.Abs(blockInfo.Insert.Scale.Y);

                    if (scaleX == 0 || scaleX < 0.05) scaleX = 1;
                    if (scaleY == 0 || scaleY < 0.05) scaleY = 1;

                    double scaleAvg = (scaleX + scaleY) / 2.0;

                    // Hàm chuyển từ local sang global
                    Func<double, double, (float, float)> toGlobal = (lx, ly) =>
                    {
                        // 1. Scale local
                        double sx = lx * scaleX;
                        double sy = ly * scaleY;

                        // 2. Rotate
                        double rx = sx * cosA - sy * sinA;
                        double ry = sx * sinA + sy * cosA;

                        // 3. Translate
                        double gx = insertPos.X + rx;
                        double gy = insertPos.Y + ry;

                        if (isSharedDisplay)
                        {
                            gx -= 3;
                        }

                        return ((float)gx, (float)gy);
                    };

                    var blockEntities = blockInfo.Insert.Block.Entities;

                    foreach (var blockEntity in blockEntities)
                    {
                        if (blockEntity is netDxf.Entities.Line line)
                        {
                            var (gx1, gy1) = toGlobal(line.StartPoint.X, line.StartPoint.Y);
                            var (gx2, gy2) = toGlobal(line.EndPoint.X, line.EndPoint.Y);

                            subEntities.Add(new EntityDto
                            {
                                LocalName = $"{entity.LocalName}_line_{subEntities.Count + 1}",
                                Type = "Line",
                                xP1 = gx1,
                                yP1 = gy1,
                                xP2 = gx2,
                                yP2 = gy2,
                                xCenter = (gx1 + gx2) / 2,
                                yCenter = (gy1 + gy2) / 2,
                                rotation = (int)blockInfo.Insert.Rotation,
                                SubEntities = null
                            });
                        }
                        // Kiểm tra nếu là Polyline2D
                        else if (blockEntity is netDxf.Entities.Polyline2D polyline)
                        {
                            var vertices = polyline.Vertexes.ToList();

                            var vertexDtos = new List<VertexDto>();
                            foreach (var v in vertices)
                            {
                                var (gx, gy) = toGlobal(v.Position.X, v.Position.Y);
                                vertexDtos.Add(new VertexDto { X = gx, Y = gy });
                            }

                            subEntities.Add(new EntityDto
                            {
                                LocalName = $"{entity.LocalName}_polyline_{subEntities.Count + 1}",
                                Type = "Polyline",
                                xP1 = vertexDtos.First().X,
                                yP1 = vertexDtos.First().Y,
                                xP2 = vertexDtos.Last().X,
                                yP2 = vertexDtos.Last().Y,
                                xCenter = vertexDtos.Average(v => v.X),
                                yCenter = vertexDtos.Average(v => v.Y),
                                rotation = (int)blockInfo.Insert.Rotation,
                                IsClosedPolyline = polyline.IsClosed,
                                SubEntities = null,
                                Vertices = vertexDtos
                            });
                        }
                        else if (blockEntity is Circle circle)
                        {
                            var (gcx, gcy) = toGlobal(circle.Center.X, circle.Center.Y);
                            float radius = (float)(circle.Radius * scaleAvg);

                            subEntities.Add(new EntityDto
                            {
                                LocalName = $"{entity.LocalName}_circle_{subEntities.Count + 1}",
                                Type = "Circle",
                                xCenter = gcx,
                                yCenter = gcy,
                                rotation = (int)blockInfo.Insert.Rotation,
                                Radius = radius
                            });
                        }
                        else if (blockEntity is netDxf.Entities.Arc arc)
                        {
                            var (gcx, gcy) = toGlobal(arc.Center.X, arc.Center.Y);

                            float radius = (float)(arc.Radius * scaleAvg);
                            float blockRotation = (float)blockInfo.Insert.Rotation;

                            float startAngle = (float)arc.StartAngle + blockRotation;
                            float endAngle = (float)arc.EndAngle + blockRotation;

                            subEntities.Add(new EntityDto
                            {
                                LocalName = $"{entity.LocalName}_arc_{subEntities.Count + 1}",
                                Type = "Arc",
                                xCenter = gcx,
                                yCenter = gcy,
                                rotation = (int)blockInfo.Insert.Rotation,
                                Radius = radius,
                                StartAngle = startAngle,
                                EndAngle = endAngle
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting sub-entities for {entity.LocalName}: {ex.Message}");
                }
            }

            return subEntities;
        }


    }
}
