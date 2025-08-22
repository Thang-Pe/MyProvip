using netDxf;
using netDxf.Entities;
using netDxf.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Newtonsoft.Json;
using System.IO;

namespace MyProvip
{
    public partial class ManualElementForm : Form
    {
        private List<MyJsonElement> allEntities = new List<MyJsonElement>();
        private MyJsonElement selectedEntity = null;

        // transform
        private float scale = 1.0f;
        private float offsetX = 0;
        private float offsetY = 0;

        // insert mode
        private bool insertMode = false;
        private string currentInsertType = "Line";

        private Panel drawingPanel;

        public ManualElementForm()
        {
            InitializeComponent();

            InitializeComponent();

            // Create main panel
            drawingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Create toolbar
            Panel toolbar = CreateToolbar();

            this.Controls.Add(drawingPanel);
            this.Controls.Add(toolbar);

            drawingPanel.Paint += Panel1_Paint;
            drawingPanel.MouseClick += Panel1_MouseClick;
            drawingPanel.MouseWheel += Panel1_MouseWheel;
        }

        private Panel CreateToolbar()
        {
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.LightGray
            };

            Button btnLoadJson = new Button
            {
                Text = "Load JSON",
                Location = new Point(10, 10),
                Size = new Size(80, 30)
            };
            btnLoadJson.Click += BtnLoadJson_Click;

            Button btnSaveJson = new Button
            {
                Text = "Save JSON",
                Location = new Point(100, 10),
                Size = new Size(80, 30)
            };
            btnSaveJson.Click += BtnSaveJson_Click;

            Button btnInsertLine = new Button
            {
                Text = "Insert Line",
                Location = new Point(190, 10),
                Size = new Size(80, 30)
            };
            btnInsertLine.Click += (s, e) => SetInsertMode("Line");

            Button btnInsertCircle = new Button
            {
                Text = "Insert Circle",
                Location = new Point(280, 10),
                Size = new Size(80, 30)
            };
            btnInsertCircle.Click += (s, e) => SetInsertMode("Circle");

            Button btnInsertValve = new Button
            {
                Text = "Insert Valve",
                Location = new Point(370, 10),
                Size = new Size(80, 30)
            };
            btnInsertValve.Click += (s, e) => SetInsertMode("Valve");

            Button btnSelect = new Button
            {
                Text = "Select",
                Location = new Point(460, 10),
                Size = new Size(80, 30)
            };
            btnSelect.Click += (s, e) => SetInsertMode("Select");

            Button btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(550, 10),
                Size = new Size(80, 30)
            };
            btnDelete.Click += BtnDelete_Click;

            toolbar.Controls.AddRange(new Control[] {
                btnLoadJson, btnSaveJson, btnInsertLine, btnInsertCircle,
                btnInsertValve, btnSelect, btnDelete
            });

            return toolbar;
        }

        private void SetInsertMode(string mode)
        {
            if (mode == "Select")
            {
                insertMode = false;
                this.Cursor = Cursors.Default;
            }
            else
            {
                insertMode = true;
                currentInsertType = mode;
                this.Cursor = Cursors.Cross;
            }
        }

        private void BtnLoadJson_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files|*.json|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadFromJson(ofd.FileName);
                }
            }
        }

        private void BtnSaveJson_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON Files|*.json|All Files|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SaveToJson(sfd.FileName);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEntity != null)
            {
                RemoveEntityRecursive(selectedEntity);
                selectedEntity = null;
                drawingPanel.Invalidate();
            }
        }

        private void RemoveEntityRecursive(MyJsonElement entity)
        {
            allEntities.Remove(entity);

            // Also remove from any parent's SubEntities
            foreach (var parent in allEntities.Where(e => e.SubEntities.Contains(entity)))
            {
                parent.SubEntities.Remove(entity);
            }
        }

        private void LoadFromJson(string path)
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                //var branches = JsonConvert.DeserializeObject<List<Dictionary<string, Dictionary<string, List<MyJsonElement>>>>>(jsonContent);
                var rootData = JsonConvert.DeserializeObject<ExportResult>(jsonContent);

                allEntities.Clear();

                // Load Branches and Entities
                if (rootData.Branches != null)
                {
                    foreach (var branchGroup in rootData.Branches)
                    {
                        foreach (var pipelineGroup in branchGroup)
                        {
                            foreach (var branch in pipelineGroup.Value)
                            {
                                foreach (var entity in branch.Value)
                                {
                                    allEntities.Add(entity);
                                    // Add sub-entities as well
                                    if (entity.SubEntities != null)
                                    {
                                        AddSubEntitiesRecursive(entity.SubEntities);
                                    }
                                }
                            }
                        }
                    }

                }

                // Load Equipments
                //List<EquipmentWrapper> equipmentList = rootData.Equipments;
                if (rootData.Equipments != null)
                {
                    foreach (var equipmentWrapper in rootData.Equipments)
                    {
                        // Add main equipment
                        if (equipmentWrapper.Equipment != null)
                        {
                            allEntities.Add(equipmentWrapper.Equipment);
                            if (equipmentWrapper.Equipment.SubEntities != null)
                            {
                                AddSubEntitiesRecursive(equipmentWrapper.Equipment.SubEntities);
                            }
                        }

                        // Add nozzles
                        if (equipmentWrapper.Nozzle != null)
                        {
                            foreach (var nozzle in equipmentWrapper.Nozzle)
                            {
                                allEntities.Add(nozzle);
                                if (nozzle.SubEntities != null)
                                {
                                    AddSubEntitiesRecursive(nozzle.SubEntities);
                                }
                            }
                        }
                    }
                }

                FitToScreen();
                drawingPanel.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSubEntitiesRecursive(List<MyJsonElement> subEntities)
        {
            foreach (var subEntity in subEntities)
            {
                allEntities.Add(subEntity);
                if (subEntity.SubEntities != null && subEntity.SubEntities.Count > 0)
                {
                    AddSubEntitiesRecursive(subEntity.SubEntities);
                }
            }
        }

        private void SaveToJson(string path)
        {
            try
            {
                // Group entities back into branch structure for saving
                var branchData = new Dictionary<string, Dictionary<string, List<MyJsonElement>>>();

                // Simple grouping - you may need to modify this based on your structure
                var defaultBranch = new Dictionary<string, List<MyJsonElement>>
                {
                    { "SCBRAN 1", allEntities.Where(e => e.SubEntities == null || e.SubEntities.Count == 0).ToList() }
                };
                branchData["Default Pipeline"] = defaultBranch;

                var jsonContent = JsonConvert.SerializeObject(new[] { branchData }, Formatting.Indented);
                File.WriteAllText(path, jsonContent);

                MessageBox.Show("File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JSON file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FitToScreen()
        {
            if (allEntities.Count == 0)
            {
                scale = 1.0f;
                offsetX = 0;
                offsetY = 0;
                return;
            }

            double minX = allEntities.Min(e => GetEntityMinX(e));
            double minY = allEntities.Min(e => GetEntityMinY(e));
            double maxX = allEntities.Max(e => GetEntityMaxX(e));
            double maxY = allEntities.Max(e => GetEntityMaxY(e));

            var dx = maxX - minX;
            var dy = maxY - minY;

            if (dx == 0) dx = 1;
            if (dy == 0) dy = 1;

            var sx = drawingPanel.Width / (float)dx;
            var sy = drawingPanel.Height / (float)dy;
            scale = Math.Min(sx, sy) * 0.8f; // Giảm xuống 0.8 để có margin

            // Center the drawing
            var centerX = (minX + maxX) / 2;
            var centerY = (minY + maxY) / 2;

            offsetX = (float)(drawingPanel.Width / (2 * scale) - centerX);
            offsetY = (float)(drawingPanel.Height / (2 * scale) - centerY);
        }

        private double GetEntityMinX(MyJsonElement e)
        {
            switch (e.Type)
            {
                case "Line":
                    return Math.Min(e.xP1, e.xP2);
                case "Circle":
                    return e.xCenter - e.radius;
                case "Arc":
                    return e.xCenter - e.radius;
                case "Polyline":
                    return e.Vertices?.Min(v => v.X) ?? e.xCenter;
                default:
                    return e.xCenter;
            }
        }

        private double GetEntityMinY(MyJsonElement e)
        {
            switch (e.Type)
            {
                case "Line":
                    return Math.Min(e.yP1, e.yP2);
                case "Circle":
                    return e.yCenter - e.radius;
                case "Arc":
                    return e.yCenter - e.radius;
                case "Polyline":
                    return e.Vertices?.Min(v => v.Y) ?? e.yCenter;
                default:
                    return e.yCenter;
            }
        }

        private double GetEntityMaxX(MyJsonElement e)
        {
            switch (e.Type)
            {
                case "Line":
                    return Math.Max(e.xP1, e.xP2);
                case "Circle":
                    return e.xCenter + e.radius;
                case "Arc":
                    return e.xCenter + e.radius;
                case "Polyline":
                    return e.Vertices?.Max(v => v.X) ?? e.xCenter;
                default:
                    return e.xCenter;
            }
        }

        private double GetEntityMaxY(MyJsonElement e)
        {
            switch (e.Type)
            {
                case "Line":
                    return Math.Max(e.yP1, e.yP2);
                case "Circle":
                    return e.yCenter + e.radius;
                case "Arc":
                    return e.yCenter + e.radius;
                case "Polyline":
                    return e.Vertices?.Max(v => v.Y) ?? e.yCenter;
                default:
                    return e.yCenter;
            }
        }

        private PointF WorldToScreen(double x, double y)
        {
            return new PointF(
                (float)((x + offsetX) * scale),
                drawingPanel.Height - (float)((y + offsetY) * scale)
            );
        }

        private PointF ScreenToWorld(PointF screenPoint)
        {
            double x = screenPoint.X / scale - offsetX;
            double y = (drawingPanel.Height - screenPoint.Y) / scale - offsetY;
            return new PointF((float)x, (float)y);
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            foreach (var entity in allEntities)
            {
                DrawEntity(g, entity);
            }
        }

        private void DrawEntity(Graphics g, MyJsonElement entity)
        {
            Pen pen = (entity == selectedEntity) ? new Pen(Color.Red, 2) : new Pen(Color.Black, 1);
            Brush textBrush = Brushes.Blue;
            Font textFont = new Font("Arial", 6);

            switch (entity.Type)
            {
                case "Line":
                    var p1 = WorldToScreen(entity.xP1, entity.yP1);
                    var p2 = WorldToScreen(entity.xP2, entity.yP2);
                    g.DrawLine(pen, p1, p2);
                    break;

                case "Circle":
                    var center = WorldToScreen(entity.xCenter, entity.yCenter);
                    float r = (float)(entity.radius * scale);
                    g.DrawEllipse(pen, center.X - r, center.Y - r, r * 2, r * 2);
                    break;

                case "Arc":
                    var ac = WorldToScreen(entity.xCenter, entity.yCenter);
                    float ra = (float)(entity.radius * scale);
                    float start = (float)entity.StartAngle;
                    float sweep = (float)(entity.EndAngle - entity.StartAngle);
                    if (sweep <= 0) sweep += 360;
                    g.DrawArc(pen, ac.X - ra, ac.Y - ra, ra * 2, ra * 2, start, sweep);
                    break;

                case "Polyline":
                    if (entity.Vertices != null && entity.Vertices.Count > 1)
                    {
                        var pts = entity.Vertices.Select(v => WorldToScreen(v.X, v.Y)).ToArray();
                        if (entity.IsClosedPolyline)
                            g.DrawPolygon(pen, pts);
                        else
                            g.DrawLines(pen, pts);
                    }
                    break;

                default:
                    // Draw valve as a rectangle with text
                    var valveCenter = WorldToScreen(entity.xCenter, entity.yCenter);
                    float valveSize = 20 * scale;

                    // Apply rotation if needed
                    g.TranslateTransform(valveCenter.X, valveCenter.Y);
                    g.RotateTransform((float)entity.rotation);

                    //g.DrawRectangle(pen, -valveSize / 2, -valveSize / 2, valveSize, valveSize);

                    // Draw text if exists
                    if (!string.IsNullOrEmpty(entity.Text))
                    {
                        var textSize = g.MeasureString(entity.Text, textFont);
                        g.DrawString(entity.Text, textFont, textBrush, -textSize.Width / 2, valveSize / 2 + 5);
                    }

                    g.ResetTransform();

                    // Draw sub-entities
                    if (entity.SubEntities != null)
                    {
                        foreach (var subEntity in entity.SubEntities)
                        {
                            DrawEntity(g, subEntity);
                        }
                    }
                    break;
            }

            pen.Dispose();
        }

        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (insertMode)
            {
                HandleInsertClick(e.Location);
            }
            else
            {
                HandleSelectClick(e.Location);
            }
        }

        private void HandleInsertClick(PointF location)
        {
            var worldPos = ScreenToWorld(location);

            switch (currentInsertType)
            {
                case "Line":
                    InsertLine(worldPos);
                    break;
                case "Circle":
                    InsertCircle(worldPos);
                    break;
                case "Valve":
                    InsertValve(worldPos);
                    break;
            }
        }

        private void InsertLine(PointF startPos)
        {
            var line = new MyJsonElement
            {
                LocalName = $"l_{DateTime.Now.Ticks}",
                Type = "Line",
                xP1 = startPos.X,
                yP1 = startPos.Y,
                xP2 = startPos.X + 50,
                yP2 = startPos.Y
            };

            allEntities.Add(line);
            drawingPanel.Invalidate();
        }

        private void InsertCircle(PointF centerPos)
        {
            var circle = new MyJsonElement
            {
                LocalName = $"c_{DateTime.Now.Ticks}",
                Type = "Circle",
                xCenter = centerPos.X,
                yCenter = centerPos.Y,
                radius = 25
            };

            allEntities.Add(circle);
            drawingPanel.Invalidate();
        }

        private void InsertValve(PointF centerPos)
        {
            var valve = new MyJsonElement
            {
                LocalName = $"v_{DateTime.Now.Ticks}",
                Type = "Valve",
                Text = "NEW-VALVE",
                xCenter = centerPos.X,
                yCenter = centerPos.Y,
                rotation = 0,
                SubEntities = new List<MyJsonElement>
                {
                    new MyJsonElement
                    {
                        LocalName = $"v_{DateTime.Now.Ticks}_circle",
                        Type = "Circle",
                        xCenter = centerPos.X,
                        yCenter = centerPos.Y,
                        radius = 5
                    }
                }
            };

            allEntities.Add(valve);
            // Add sub-entities to main list for selection
            AddSubEntitiesRecursive(valve.SubEntities);
            drawingPanel.Invalidate();
        }

        private void HandleSelectClick(PointF location)
        {
            const double tolerance = 10.0;
            selectedEntity = null;

            foreach (var entity in allEntities)
            {
                if (IsEntityClicked(entity, location, tolerance))
                {
                    selectedEntity = entity;
                    break;
                }
            }

            drawingPanel.Invalidate();
        }

        private bool IsEntityClicked(MyJsonElement entity, PointF location, double tolerance)
        {
            switch (entity.Type)
            {
                case "Line":
                    var p1 = WorldToScreen(entity.xP1, entity.yP1);
                    var p2 = WorldToScreen(entity.xP2, entity.yP2);
                    return PointToLineDistance(location, p1, p2) < tolerance;

                case "Circle":
                    var center = WorldToScreen(entity.xCenter, entity.yCenter);
                    float r = (float)(entity.radius * scale);
                    var dist = Distance(location, center);
                    return Math.Abs(dist - r) < tolerance;

                case "Valve":
                    var valveCenter = WorldToScreen(entity.xCenter, entity.yCenter);
                    return Distance(location, valveCenter) < tolerance;

                case "Polyline":
                    if (entity.Vertices != null && entity.Vertices.Count > 1)
                    {
                        for (int i = 0; i < entity.Vertices.Count - 1; i++)
                        {
                            var pt1 = WorldToScreen(entity.Vertices[i].X, entity.Vertices[i].Y);
                            var pt2 = WorldToScreen(entity.Vertices[i + 1].X, entity.Vertices[i + 1].Y);
                            if (PointToLineDistance(location, pt1, pt2) < tolerance)
                                return true;
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }

        private double Distance(PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double PointToLineDistance(PointF p, PointF a, PointF b)
        {
            double A = p.X - a.X;
            double B = p.Y - a.Y;
            double C = b.X - a.X;
            double D = b.Y - a.Y;

            double dot = A * C + B * D;
            double lenSq = C * C + D * D;
            double param = (lenSq != 0) ? dot / lenSq : -1;

            double xx, yy;
            if (param < 0) { xx = a.X; yy = a.Y; }
            else if (param > 1) { xx = b.X; yy = b.Y; }
            else { xx = a.X + param * C; yy = a.Y + param * D; }

            double dx = p.X - xx;
            double dy = p.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Get mouse position in world coordinates before zoom
            var worldPosBefore = ScreenToWorld(e.Location);

            // Apply zoom
            float oldScale = scale;
            if (e.Delta > 0)
                scale *= 1.1f;
            else
                scale *= 0.9f;

            // Get mouse position in world coordinates after zoom
            var worldPosAfter = ScreenToWorld(e.Location);

            // Adjust offset to keep mouse position stable
            offsetX += (float)(worldPosAfter.X - worldPosBefore.X);
            offsetY += (float)(worldPosAfter.Y - worldPosBefore.Y);

            drawingPanel.Invalidate();
        }


    }
}