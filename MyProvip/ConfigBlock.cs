using Aveva.CIE.Foundation.Basic.Algorithms;
using Aveva.CIE.Foundation.Basic.Geometry;
using Aveva.Core.Geometry;
using Microsoft.Office.Interop.Visio;
using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static netDxf.Entities.HatchBoundaryPath;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using DxfLine = netDxf.Entities.Line;
using Visio = Microsoft.Office.Interop.Visio;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace MyProvip
{
    public partial class btnBranches : Form
    {
        private HashSet<string> allCadBlocks = new HashSet<string>();
        private Dictionary<string, BlockInfo> blockDictionary = new Dictionary<string, BlockInfo>();

        public HashSet<CadEntity> allLines = new HashSet<CadEntity>();
        public HashSet<CadEntity> allInstrumentLines = new HashSet<CadEntity>();
        public HashSet<CadEntity> BlockProcessName = new HashSet<CadEntity>();

        public List<DiagLine> lines;
        private Dictionary<string, List<List<CadEntity>>> scplinBranches;

        private List<CadEntity> listComponent = new List<CadEntity>();
        private List<CadEntity> listEquipment = new List<CadEntity>();
        private List<CadEntity> listTags = new List<CadEntity>();

        private DxfDocument _currentDxf;
        private IplReadCad _iplReadCad = new IplReadCad();
        private IplReOrder _iplReOrder;
        private ExportResult _currentData;

        private bool isUpdatingUI = false;

        public Dictionary<string, string> textOfBlock = new Dictionary<string, string>();
        public List<Dictionary<string, Dictionary<string, List<EntityDto>>>> restructuredData;

        private Dictionary<string, Dictionary<string, List<MyJsonElement>>> pipelines = new Dictionary<string, Dictionary<string, List<MyJsonElement>>>();

        private List<MyJsonElement> allEntities = new List<MyJsonElement>();
        private HashSet<string> selectedEntity = new HashSet<string>();

        private ListViewItem draggedItem;

        // transform
        private float scale = 1.0f;
        private float offsetX = 0;
        private float offsetY = 0;

        // insert mode
        private bool isDrawingLine = false;
        private PointF lineStartPoint;
        private List<PointF[]> drawnLines = new List<PointF[]>(); 
        private PointF currentMousePos;
        // NEW: Variables for 2-click line drawing
        private bool isFirstClick = true;
        private PointF firstPoint;

        public btnBranches()
        {
            InitializeComponent();
            cbbBlockDiag.Items.AddRange(new string[] { 
                "Arrow", 
                "Connector", 
                "Equipment",
                "Filter", 
                "Instrument", 
                "Nozzle",
                "Reduce", 
                "Tag", 
                "Pump", 
                "Valve",
                "Joint", 
                "Other" 
            });
            InitListViews();

        }

        private void ConfigBlock_Load(object sender, EventArgs e)
        {
            //_currentDxf = DxfDocument.Load("C:\\Users\\Administrator\\Desktop\\IMPORT-VISIO4.dxf");
            //if (_currentDxf == null || !_currentDxf.Entities.Inserts.Any())
            //{
            //    MessageBox.Show("❌ Không có block (Insert) nào trong file DXF.", "Lỗi");
            //    return;
            //}
            //LoadCadBlocks(_currentDxf);
            //UpdateListViews();
            //InitHandleWithQuadTree();
        }

        private void LoadCadBlocks(DxfDocument dxf)
        {
            try
            {
                allCadBlocks.Clear();
                blockDictionary.Clear();

                int countBlock = 1;

                foreach (var insert in dxf.Entities.Inserts)
                {
                    string blockName = insert.Block?.Name;
                    if (string.IsNullOrEmpty(blockName)) continue;

                    string lowerName = blockName.ToLower();

                    // Map keywords to block types
                    var blockTypeMap = new Dictionary<string, string>
                    {
                        { "tag", "Tag" },
                        { "tank", "Equipment" },
                        { "pump", "Pump" },
                        { "joint", "Joint" },
                        { "reduce", "Reduce" },
                        { "arrow", "Arrow" },
                        { "valve", "Valve" },
                        { "strainer", "Valve" },
                        { "instrument", "Instrument" },
                        { "connector", "Connector" },
                        { "nozzle", "Nozzle" }
                    };

                    string blockType = "Other";
                    var match = blockTypeMap.FirstOrDefault(k => lowerName.Contains(k.Key));
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        blockType = match.Value;
                    }

                    var blockInfo = new BlockInfo(insert, blockType);

                    if (!allCadBlocks.Contains(blockName))
                    {
                        allCadBlocks.Add(blockName);
                        blockDictionary[blockName] = blockInfo;
                    }

                    //========================

                    CadEntity blockEntity = new CadEntity(blockInfo)
                    {
                        LocalName = $"b_{countBlock}"
                    };
                    if (blockType == "Equipment" || blockType == "Instrument" || blockType == "Pump")
                    {
                        listEquipment.Add(blockEntity);
                    }
                    else if(blockType == "Tag")
                    {
                        listTags.Add(blockEntity);
                    }
                    else
                    {
                        BlockProcessName.Add(blockEntity);
                    }
                    countBlock++;
                }

                var rawLines = new List<DiagLine>();
                foreach (var item in dxf.Entities.Lines)
                {
                    if (item.Layer.Name.ToLower() == "instrument" || item.Layer.Name.ToLower() == "instrument line")
                    {
                        var dl = item.ToDiagLine(); 
                        CadEntity lineEn = new CadEntity(dl);
                        allInstrumentLines.Add(lineEn);
                        continue;
                    }

                    var diaLine = item.ToDiagLine();
                    double length = Math.Sqrt(Math.Pow(diaLine.P2.X - diaLine.P1.X, 2) + Math.Pow(diaLine.P2.Y - diaLine.P1.Y, 2));
                    if (length > 1)
                    {
                        rawLines.Add(diaLine);
                    }
                }

                int countLines = 1;
                foreach (var item in rawLines)
                {
                    //var diagLine = item.ToDiagLine();
                    CadEntity lineEn = new CadEntity(item)
                    {
                        LocalName = $"l_{countLines}",
                    };
                    allLines.Add(lineEn);
                    countLines++;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc DXF: " + ex.Message);
            }
        }

        private void InitHandleWithQuadTree()
        {
            lines = new List<DiagLine>();
            DiagLine line = null;
            string blockName = null;
            BlockInfo blockInfo = null;

            foreach (var enti in allLines)
            {
                line = enti.Getobject() as DiagLine;

                if(!string.IsNullOrEmpty(enti.LocalName))
                {
                    line.LocalName = enti.LocalName;
                    //line.P1 = enti.Box.P1;
                    //line.P2 = enti.Box.P2;
                }
                line.UpdateBox();
                lines.Add(line);
            }

            foreach (var entry in BlockProcessName)
            {
                blockName = entry.LocalName;
                blockInfo = (BlockInfo)entry.Getobject();

                Box2 boundingBox = blockInfo.BoundingBox;

                if (boundingBox == null) continue;

                float minX = (float)Math.Round(Math.Min(boundingBox.P1.X, boundingBox.P2.X), 6, MidpointRounding.AwayFromZero);
                float maxX = (float)Math.Round(Math.Max(boundingBox.P1.X, boundingBox.P2.X), 6, MidpointRounding.AwayFromZero);
                float minY = (float)Math.Round(Math.Min(boundingBox.P1.Y, boundingBox.P2.Y), 6, MidpointRounding.AwayFromZero);
                float maxY = (float)Math.Round(Math.Max(boundingBox.P1.Y, boundingBox.P2.Y), 6, MidpointRounding.AwayFromZero);

                Point2 bottomLeft = new Point2(minX, minY);
                Point2 bottomRight = new Point2(maxX, minY);
                Point2 topRight = new Point2(maxX, maxY);
                Point2 topLeft = new Point2(minX, maxY);

                DiagLine bottomLine = new DiagLine(bottomLeft, bottomRight) { LocalName = blockName, IsBoundingLine = true };
                DiagLine rightLine = new DiagLine(bottomRight, topRight) { LocalName = blockName, IsBoundingLine = true };
                DiagLine topLine = new DiagLine(topRight, topLeft) { LocalName = blockName, IsBoundingLine = true };
                DiagLine leftLine = new DiagLine(topLeft, bottomLeft) { LocalName = blockName, IsBoundingLine = true };

                bottomLine.UpdateBox();
                rightLine.UpdateBox();
                topLine.UpdateBox();
                leftLine.UpdateBox();

                lines.Add(bottomLine);
                lines.Add(rightLine);
                lines.Add(topLine);
                lines.Add(leftLine); 
            }

            Box2 region = GetLargestInsertBoundingBox();

            OderPipeline.InitializeQuadTree(lines, region);
        }

        enum SearchDirection { Forward, Backward }

        private void HandleReOrderBranches()
        {
            try
            {
                var usedLocalNames = new HashSet<string>();

                List<string> OnpipeBlock = new List<string> { "Valve", "Reduce", "Filter", "Flange", "Arrow", "Connector", "Joint"};

                var unprocessedBlocks = BlockProcessName.Where(enti =>
                {
                    var block = enti.Getobject() as BlockInfo;
                    return block != null && !usedLocalNames.Contains(enti.LocalName) && OnpipeBlock.Contains(block.BlockType);
                }).ToList();

                var unprocessedLines = allLines
                    .Where(enti => enti.Getobject() is DiagLine)
                    .ToList();

                const double SAME_DIRECTION_THRESHOLD = 10.0;
                const double REVERSE_DIRECTION_THRESHOLD = 170.0;
                const double RIGHT_TURN_THRESHOLD = 90.0;
                const double END_POINT_TOLERANCE = 4.0;
                const double TOLERANCE = 0.1;

                // Dictionary để lưu branches theo SCPLIN
                scplinBranches = new Dictionary<string, List<List<CadEntity>>>();
                var tempBranches = new List<List<CadEntity>>();

                while (unprocessedBlocks.Any(block => !usedLocalNames.Contains(block.LocalName)))
                {
                    var currentBranch = new List<CadEntity>();
                    var currentEntity = unprocessedBlocks.FirstOrDefault(block => !usedLocalNames.Contains(block.LocalName));

                    if (currentEntity == null) break;

                    var anchorEntity = currentEntity;
                    Vector2D? currentDirection = null;
                    string branchScplinName = null;

                    for (SearchDirection searchDir = SearchDirection.Forward;
                         searchDir <= SearchDirection.Backward;
                         searchDir++)
                    {
                        if (searchDir == SearchDirection.Forward)
                        {
                            // Lần đầu: Thêm anchor entity và tìm về phía trước
                            currentBranch.Add(currentEntity);
                            unprocessedBlocks.Remove(currentEntity);
                            usedLocalNames.Add(currentEntity.LocalName);
                            currentDirection = null;
                        }
                        else
                        {
                            // Lần thứ 2: Quay lại anchor và tìm về phía sau
                            currentEntity = anchorEntity;
                            currentDirection = null;
                        }

                        while (true)
                        {
                            if (currentEntity.Box == null) //{(698.4919 352.4671) (749.3705 391.8176)}
                            {
                                break;
                            }                            

                            var boxEntity3 = IplReOrder.HandleCorrectBox(currentEntity, 3);

                            // Text for entity
                            _iplReadCad.ProcessTagsInBox(listTags, currentEntity,boxEntity3, textOfBlock);

                            // Text for Scplin
                            string foundScplinName = _iplReadCad.FindScplinNameFromTags(listTags, boxEntity3, textOfBlock);
                            if (!string.IsNullOrEmpty(foundScplinName) && string.IsNullOrEmpty(branchScplinName))
                            {
                                branchScplinName = foundScplinName;
                            }
                            var boxEntity1 = IplReOrder.HandleCorrectBox(currentEntity, 0);
                            List<DiagLine> interSectionLine = OderPipeline.InterSectionLine(boxEntity1); // {(697.4919 351.4671) (750.3705 392.8176)}

                            if (interSectionLine.Count == 0)
                            {
                                break;
                            }

                            if (currentEntity.LocalName == "b_36")
                            {
                                Debug.WriteLine("Vo day 7");
                            }

                            DiagLine foundNextBoundingLine = null;

                            if (currentDirection != null)
                            {
                                if (currentEntity.LocalName.StartsWith("l_"))
                                {
                                    foundNextBoundingLine = interSectionLine
                                    .Where(line => line.LocalName != currentEntity.LocalName &&
                                                   !usedLocalNames.Contains(line.LocalName))
                                    .Select(line =>
                                    {
                                        var tol = line.Box.Center;
                                        var froml = currentEntity.Box.Center;
                                        var dir = froml.GetDirectionToVector(tol).Normalize();
                                        var cur = currentDirection.Value.Normalize();

                                        var dot = dir.Dot(cur);

                                        bool isSameDirection = dot > 1 - TOLERANCE;   // gần 1 = cùng hướng
                                        bool isRightAngle = Math.Abs(dot) < TOLERANCE; // gần 0 = vuông góc

                                        return new { Line = line, isSameDirection, isRightAngle };
                                    })
                                    .OrderBy(x => x.isSameDirection ? 0 : (x.isRightAngle ? 1 : 2)) // ưu tiên hướng thẳng, sau đó mới rẽ
                                    .FirstOrDefault(x => x.isSameDirection || x.isRightAngle)
                                    ?.Line;
                                }else
                                {
                                    foundNextBoundingLine = interSectionLine
                                        .Where(line => line.LocalName != currentEntity.LocalName && !usedLocalNames.Contains(line.LocalName))
                                        .Select(line =>
                                        {
                                            var tol = line.Box.Center;
                                            var froml = currentEntity.Box.Center;
                                            var dir = froml.GetDirectionToVector(tol);

                                            bool isExactMatch = Math.Abs(dir.X - currentDirection.Value.X) < TOLERANCE &&
                                                Math.Abs(dir.Y - currentDirection.Value.Y) < TOLERANCE;

                                            var angle = Math.Abs(currentDirection.Value.AngleTo(dir));

                                            return new { Line = line, IsExactMatch = isExactMatch, Angle = angle };
                                        })
                                        .OrderBy(x => x.IsExactMatch)
                                        .ThenBy(x => x.Angle)
                                        .FirstOrDefault()?.Line;
                                }

                            }
                            else
                            {
                                foundNextBoundingLine = interSectionLine
                                    .FirstOrDefault(line => line.LocalName != currentEntity.LocalName &&
                                    !usedLocalNames.Contains(line.LocalName));
                            }

                            if (foundNextBoundingLine == null)
                            {
                                break;
                            }

                            CadEntity getEntity = null;
                            if (foundNextBoundingLine.LocalName.StartsWith("l_"))
                            {
                                getEntity = unprocessedLines
                                    .FirstOrDefault(li => li.LocalName == foundNextBoundingLine.LocalName &&
                                                         !usedLocalNames.Contains(li.LocalName));
                            }
                            else
                            {
                                getEntity = unprocessedBlocks
                                    .FirstOrDefault(li => li.LocalName == foundNextBoundingLine.LocalName &&
                                                         !usedLocalNames.Contains(li.LocalName));
                            }

                            if (getEntity == null) //{(720.0924 370.4646) (722.0924 393.6629)}
                            {
                                break;
                            }

                            var from = currentEntity.Box.Center;
                            var to = getEntity.Box.Center;
                            var direction = from.GetDirectionToVector(to);
                            var dirWithAnchor = from.GetDirectionToVector(anchorEntity.Box.Center);

                            if (direction.IsZero)
                            {
                                Debug.WriteLine($"Direction is zero between {currentEntity.LocalName} and {getEntity.LocalName}");
                                break;
                            }

                            if (currentDirection == null)
                            {
                                currentDirection = IplReOrder.DetermineInitialDirection(direction);

                                if (IplReOrder.ShouldInsertAtBeginning(direction))
                                {
                                    currentBranch.Insert(0, getEntity);
                                    currentDirection = direction;
                                }
                                else
                                {
                                    currentBranch.Add(getEntity);
                                    currentDirection = direction;
                                }
                            }
                            else
                            {
                                double angle = Math.Abs(currentDirection.Value.AngleTo(direction));
                                double angleWithAnchor = Math.Abs(currentDirection.Value.AngleTo(dirWithAnchor));

                                if (angle < SAME_DIRECTION_THRESHOLD) // 10
                                {
                                    if (searchDir == SearchDirection.Forward)
                                    {
                                        currentBranch.Add(getEntity);
                                    }
                                    else 
                                    {
                                        currentBranch.Insert(0, getEntity);
                                    }

                                    currentDirection = direction;
                                }
                                else if (angle > REVERSE_DIRECTION_THRESHOLD) // 170
                                {
                                    currentBranch.Insert(0, getEntity);
                                    currentDirection = direction.Reverse();
                                }
                                else if (angle < RIGHT_TURN_THRESHOLD)
                                {
                                    bool isEndPoint = IplReOrder.IsIntersectionAtEndPoint(currentEntity, getEntity, END_POINT_TOLERANCE);
                                    if (isEndPoint)
                                    {
                                        currentBranch.Add(getEntity);
                                        currentDirection = direction;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    bool isEndPoint = IplReOrder.IsIntersectionAtEndPoint(currentEntity, getEntity, END_POINT_TOLERANCE);
                                    if (isEndPoint)
                                    {
                                        currentBranch.Add(getEntity);
                                        currentDirection = direction;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            currentEntity = getEntity;
                            usedLocalNames.Add(currentEntity.LocalName);
                            unprocessedBlocks.Remove(currentEntity);

                        }
                    }

                    if (currentBranch.Count > 0)
                    {
                        // Validate branch
                        for (int i = currentBranch.Count - 2; i >= 0; i--)
                        {
                            if (currentBranch[i].Type == EntityType.Line &&
                                currentBranch[i + 1].Type == EntityType.Line)
                            {
                                var line1 = (DiagLine)currentBranch[i].Getobject();
                                var line2 = (DiagLine)currentBranch[i + 1].Getobject();

                                if (line1.P2.GetDistanceTo(line2.P1) < END_POINT_TOLERANCE)
                                {
                                    line1.P2 = line2.P2;
                                    currentBranch.RemoveAt(i + 1);
                                }
                                else if (line1.P1.GetDistanceTo(line2.P2) < END_POINT_TOLERANCE)
                                {
                                    line1.P1 = line2.P1;
                                    currentBranch.RemoveAt(i + 1);
                                }
                            }
                        }

                        // Nếu tìm thấy SCPLIN name từ tag
                        if (!string.IsNullOrEmpty(branchScplinName))
                        {
                            if (!scplinBranches.ContainsKey(branchScplinName))
                                scplinBranches[branchScplinName] = new List<List<CadEntity>>();

                            scplinBranches[branchScplinName].Add(currentBranch);
                        }
                        else
                        {
                            // Lưu tạm branch chưa xác định
                            tempBranches.Add(currentBranch);
                        }
                    }
                }

                // Xử lý các branch tạm thời - kiểm tra va chạm với các branch đã phân loại
                _iplReadCad.ProcessTempBranches(tempBranches, scplinBranches);

                if (scplinBranches.Count > 0 || tempBranches.Count > 0)
                {
                    restructuredData = _iplReadCad.CreateScplinStructuredJson(scplinBranches, tempBranches, textOfBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình xử lý HandleReOrderBranches: {ex.Message}\n{ex.StackTrace}", "Lỗi");
            }
        }

        private void InitListViews()
        {
            listViewPipelines.View = View.Details;
            listViewPipelines.FullRowSelect = true;
            listViewPipelines.Columns.Add("Pipeline", -2);

            listViewBranches.View = View.Details;
            listViewBranches.FullRowSelect = true;
            listViewBranches.Columns.Add("Branch", -2);

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("Merge", null, (s, e) =>
            {
                if (listViewBranches.SelectedItems.Count < 2) return;

                var firstSelected = listViewBranches.SelectedItems[0];

                if (firstSelected.Tag != null && firstSelected.Tag is BranchTag tagFirst)
                {
                    string pipelineName = tagFirst.Pipeline;
                    string firstBranchName = tagFirst.Branch;

                    // Tạo copy danh sách selected items (tránh modify khi loop)
                    var itemsToMerge = listViewBranches.SelectedItems.Cast<ListViewItem>().ToList();

                    foreach (var item in itemsToMerge)
                    {
                        if (item == firstSelected) continue;

                        if (item.Tag is BranchTag tagOther)
                        {
                            string otherBranchName = tagOther.Branch;

                            var elements = pipelines[pipelineName][otherBranchName];

                            pipelines[pipelineName][firstBranchName].AddRange(elements);

                            pipelines[pipelineName].Remove(otherBranchName);

                            listViewBranches.Items.Remove(item);
                        }
                    }
                }
            });

            var moveMenu = new ToolStripMenuItem("Move");
            contextMenu.Items.Add(moveMenu);

            // Gán sự kiện Opening để build lại submenu mỗi lần chuột phải
            contextMenu.Opening += (s, e) =>
            {
                moveMenu.DropDownItems.Clear(); // clear submenu cũ

                foreach (var pipelineName in pipelines.Keys)
                {
                    var pipelineItem = new ToolStripMenuItem(pipelineName);
                    pipelineItem.Click += (sender2, e2) =>
                    {
                        if (listViewBranches.SelectedItems.Count == 0) return;

                        string newPipeline = pipelineName;
                        if (!pipelines.ContainsKey(newPipeline))
                            pipelines[newPipeline] = new Dictionary<string, List<MyJsonElement>>();

                        var itemsToMove = listViewBranches.SelectedItems.Cast<ListViewItem>().ToList();

                        foreach (var branchItem in itemsToMove)
                        {
                            if (!(branchItem.Tag is BranchTag tag)) continue;

                            string oldPipeline = tag.Pipeline;
                            string branchName = tag.Branch;

                            if (oldPipeline == newPipeline) continue;

                            var branchEntities = pipelines[oldPipeline][branchName];

                            pipelines[oldPipeline].Remove(branchName);

                            string newBranchName = branchName;
                            if (pipelines[newPipeline].ContainsKey(newBranchName))
                            {
                                int maxNumber = 0;
                                foreach (var name in pipelines[newPipeline].Keys)
                                {
                                    if (name.StartsWith("SCBRAN "))
                                    {
                                        if (int.TryParse(name.Substring(7), out int num) && num > maxNumber)
                                            maxNumber = num;
                                    }
                                }
                                newBranchName = $"SCBRAN {maxNumber + 1}";
                            }

                            pipelines[newPipeline][newBranchName] = branchEntities;

                            listViewBranches.Items.Remove(branchItem);
                        }

                        RefreshBranchesView(newPipeline, null);
                        drawingPanel.Invalidate();
                    };
                    moveMenu.DropDownItems.Add(pipelineItem);
                }
            };

            listViewBranches.ContextMenuStrip = contextMenu;

            listViewElementOfBranch.View = View.Details;
            listViewElementOfBranch.FullRowSelect = true;
            listViewElementOfBranch.Columns.Add("Sequence", 70);
            listViewElementOfBranch.Columns.Add("Type", 60);
            listViewElementOfBranch.Columns.Add("PClass", 60);
            listViewElementOfBranch.Columns.Add("Bore 1", 60);
            listViewElementOfBranch.Columns.Add("Bore 2", 60);
            listViewElementOfBranch.Columns.Add("Bore 3", 60);
        }

        private void btnBrowserCad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "AutoCad Files|*.dxf|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _currentDxf = DxfDocument.Load(ofd.FileName);
                    if (_currentDxf == null || !_currentDxf.Entities.Inserts.Any())
                    {
                        MessageBox.Show("❌ There are no blocks (Inserts) in the DXF file.", "Lỗi");
                        return;
                    }
                    LoadCadBlocks(_currentDxf);
                    UpdateListViews();
                    InitHandleWithQuadTree();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if(BlockProcessName.Count == 0 && allLines.Count == 0)
                {
                    MessageBox.Show("❌ No blocks or lines to process.", "Error");
                    return;
                }
                HandleReOrderBranches();
                ExportEquipmentWithNozzle_ByBoundingBox();

                MessageBox.Show($"✅ Export success all entities", "OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error");
            }
        }

        private void UpdateListViews()
        {
            lvCad.Items.Clear();
            imageListBlocks.Images.Clear();
            imageListBlocks.ImageSize = new Size(64, 64);
            lvCad.LargeImageList = imageListBlocks;

            HashSet<string> addedTypes = new HashSet<string>();
            lbDiag.Items.Clear();

            int imageIndex = 0;
            foreach (var pair in blockDictionary)
            {
                string blockName = pair.Key;
                BlockInfo blockInfo = pair.Value;

                Bitmap bmp = _iplReadCad.RenderBlockAsBitmap(blockInfo);
                imageListBlocks.Images.Add(bmp);

                ListViewItem item = new ListViewItem(blockName, imageIndex++);
                item.Tag = blockInfo; 
                lvCad.Items.Add(item);

                if (!addedTypes.Contains(blockInfo.BlockType))
                {
                    lbDiag.Items.Add(blockInfo.BlockType);
                    addedTypes.Add(blockInfo.BlockType);
                }
            }
        }

        private void cbbBlockDiag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isUpdatingUI) return;

            isUpdatingUI = true;
            try
            {
                if (lvCad.SelectedItems.Count == 0 || cbbBlockDiag.SelectedItem == null) return;

                string selectedBlock = lvCad.SelectedItems[0].Text;
                string selectedType = cbbBlockDiag.SelectedItem.ToString();

                if (blockDictionary.ContainsKey(selectedBlock))
                {
                    string oldBlockType = blockDictionary[selectedBlock].BlockType;
                    blockDictionary[selectedBlock].BlockType = selectedType;
                    UpdateListViews();

                    var entitiesToMove = BlockProcessName
                        .Concat(listEquipment)
                        .Where(entry =>
                        {
                            var blockInfo = entry.Getobject() as BlockInfo;
                            return blockInfo?.BlockName == selectedBlock;
                        })
                        .ToList();

                    foreach (var entity in entitiesToMove)
                    {
                        var blockInfo = entity.Getobject() as BlockInfo;
                        if (blockInfo != null)
                            blockInfo.BlockType = selectedType;

                        if (oldBlockType == "Instrument" || oldBlockType == "Equipment" || oldBlockType == "Pump")
                            listEquipment.Remove(entity);
                        else if (oldBlockType == "Tag")
                            listTags.Remove(entity);
                        else
                            BlockProcessName.Remove(entity);

                        if (selectedType == "Instrument" || selectedType == "Equipment" || selectedType == "Pump")
                            listEquipment.Add(entity);
                        else if (selectedType == "Tag" )
                            listTags.Add(entity);
                        else
                            BlockProcessName.Add(entity);
                    }
                }
            }
            finally
            {
                isUpdatingUI = false;
            }
        }

        private void lbDiag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbDiag.SelectedItem == null) return;

            string selectedType = lbDiag.SelectedItem.ToString();

            lvCad.Items.Clear();
            imageListBlocks.Images.Clear();
            lvCad.LargeImageList = imageListBlocks;

            int imageIndex = 0;

            foreach (var pair in blockDictionary)
            {
                if (pair.Value.BlockType == selectedType)
                {
                    Bitmap bmp = _iplReadCad.RenderBlockAsBitmap(pair.Value);
                    imageListBlocks.Images.Add(bmp);

                    ListViewItem item = new ListViewItem(pair.Key, imageIndex++);
                    item.Tag = pair.Value;
                    lvCad.Items.Add(item);
                }
            }
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            UpdateListViews();
        }

        private void ConfigBlock_FormClosed(object sender, FormClosedEventArgs e)
        {
            var saveDict = blockDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BlockType);
            //BlockConfigStorage.Save(saveDict);
        }

        private void btnTest1_Click(object sender, EventArgs e)
        {
            string xmlOutputPath = "output.xml";
            var merged = BuildCurrentExportResult();
            string currentJson = JsonConvert.SerializeObject(merged, Formatting.Indented);

            XmlGenerator xmlGenerator = new XmlGenerator();
            xmlGenerator.GeranateXMlFromJson(currentJson, xmlOutputPath);

            MessageBox.Show("XML file generated successfully!");
        }

        private void lvCad_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (isUpdatingUI) return;
            if (!e.IsSelected) return;

            isUpdatingUI = true;
            try
            {
                string selectedBlock = e.Item.Text;

                if (blockDictionary.TryGetValue(selectedBlock, out BlockInfo blockInfo))
                {
                    cbbBlockDiag.SelectedItem = blockInfo.BlockType;
                }
            }
            finally
            {
                isUpdatingUI = false;
            }
        }

        private void listViewPipelines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewPipelines.SelectedItems.Count == 0) return;

            string pipelineName = listViewPipelines.SelectedItems[0].Text;

            listViewBranches.Items.Clear();
            selectedEntity.Clear();

            foreach (var branch in pipelines[pipelineName])
            {
                var item = new ListViewItem(branch.Key); // SCBRAN 1, SCBRAN 2...
                item.Tag = new BranchTag
                {
                    Pipeline = pipelineName,
                    Branch = branch.Key
                };
                listViewBranches.Items.Add(item);
                foreach (var entity in branch.Value)
                {
                    selectedEntity.Add(entity.LocalName);
                }
            }

            listViewElementOfBranch.Items.Clear();
            drawingPanel.Invalidate();
        }

        private void listViewBranches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewBranches.SelectedItems.Count == 0) return;

            var tag = (dynamic)listViewBranches.SelectedItems[0].Tag;
            string pipelineName = tag.Pipeline;
            string branchName = tag.Branch;

            var elements = pipelines[pipelineName][branchName];

            listViewElementOfBranch.Items.Clear();
            selectedEntity.Clear();
            int sequence = 1;

            foreach (var entity in elements)
            {
                string type = string.IsNullOrEmpty(entity.Type) ? "Line" : entity.Type;

                var item = new ListViewItem(sequence.ToString());
                item.SubItems.Add(type);
                item.Tag = entity;

                selectedEntity.Add(entity.LocalName);
                listViewElementOfBranch.Items.Add(item);
                sequence++;
            }
            drawingPanel.Invalidate();
        }

        private void listViewElementOfBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedEntity.Clear();
            if (listViewElementOfBranch.SelectedItems.Count == 0) return;

            var selectedItem = listViewElementOfBranch.SelectedItems[0];
            var entity = selectedItem.Tag as MyJsonElement;

            if (entity != null)
            {
                selectedEntity.Add(entity.LocalName);

                // Tính toán vị trí center của entity để focus
                PointF centerPoint = GetEntityCenter(entity);

                // Tùy chọn: Auto-pan đến entity được chọn
                PanToEntity(entity);

                // Vẽ lại panel để highlight entity được chọn
                drawingPanel.Invalidate();
            }
        }

        // Load branches
        private void btnLoadBranches_Click(object sender, EventArgs e)
        {
            string jsonInput = File.ReadAllText(@"E:\CSharp\test29-7\test\MyProvip\MyProvip\bin\Debug\All_Entities.json");
            LoadFromJson(jsonInput);
            LoadDrawFromJson(jsonInput);
        }

        private void btnSaveAsJson_Click(object sender, EventArgs e)
        {
            try
            {
                // Build lại object từ pipelines
                var exportResult = BuildCurrentExportResult();

                // Serialize sang JSON
                string jsonOutput = JsonConvert.SerializeObject(exportResult, Formatting.Indented);

                // Lưu file (ví dụ cho save lại ngay file cũ)
                string filePath = @"E:\CSharp\test29-7\test\MyProvip\MyProvip\bin\Debug\All_Entities.json";
                File.WriteAllText(filePath, jsonOutput);

                MessageBox.Show("Pipelines saved success!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when save pipelines: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void listViewElementOfBranch_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListViewItem))) return;

            Point cp = listViewElementOfBranch.PointToClient(new Point(e.X, e.Y));
            ListViewItem targetItem = listViewElementOfBranch.GetItemAt(cp.X, cp.Y);

            if (targetItem == null || draggedItem == null) return;

            int targetIndex = targetItem.Index;

            listViewElementOfBranch.Items.Remove(draggedItem);

            listViewElementOfBranch.Items.Insert(targetIndex, draggedItem);
            draggedItem.Selected = true;

            UpdateSequenceNumbers();

            if (listViewBranches.SelectedItems.Count > 0)
            {
                var tag = (dynamic)listViewBranches.SelectedItems[0].Tag;
                string pipelineName = tag.Pipeline;
                string branchName = tag.Branch;

                // Lấy lại danh sách entity theo thứ tự mới trong ListView
                var reordered = new List<MyJsonElement>();
                foreach (ListViewItem item in listViewElementOfBranch.Items)
                {
                    if (item.Tag is MyJsonElement entity)
                        reordered.Add(entity);
                }

                // Cập nhật vào pipelines
                pipelines[pipelineName][branchName] = reordered;
            }
        }

        private void listViewElementOfBranch_ItemDrag(object sender, ItemDragEventArgs e)
        {
            draggedItem = (ListViewItem)e.Item;
            DoDragDrop(draggedItem, DragDropEffects.Move);
        }

        private void listViewElementOfBranch_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listViewElementOfBranch_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            foreach (var entity in allEntities)
            {
                DrawEntity(g, entity);
            }

            // Draw manually drawn lines (màu đen)
            using (Pen blackPen = new Pen(Color.Black, 2))
            {
                foreach (var line in drawnLines)
                {
                    if (line.Length >= 2)
                    {
                        g.DrawLine(blackPen, line[0], line[1]);

                        // Vẽ điểm đầu và cuối
                        using (Brush pointBrush = new SolidBrush(Color.Black))
                        {
                            g.FillEllipse(pointBrush, line[0].X - 2, line[0].Y - 2, 4, 4);
                            g.FillEllipse(pointBrush, line[1].X - 2, line[1].Y - 2, 4, 4);
                        }
                    }
                }
            }

            // Draw preview line khi đang vẽ
            if (isDrawingLine && lineStartPoint != PointF.Empty)
            {
                using (Pen previewPen = new Pen(Color.Gray, 1))
                {
                    previewPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(previewPen, lineStartPoint, currentMousePos);
                }

                // Vẽ điểm start
                using (Brush startBrush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(startBrush, lineStartPoint.X - 3, lineStartPoint.Y - 3, 6, 6);
                }
            }
        }

        private void drawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (isDrawingLine && e.Button == MouseButtons.Left)
            {
                HandleLineDrawing(e.Location);
            }
            else if (!isDrawingLine)
            {
                HandleSelectClick(e.Location);
            }
        }

        private void drawingPanel_MouseWheel(object sender, MouseEventArgs e)
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

        private void btnDrawLine_Click(object sender, EventArgs e)
        {
            isDrawingLine = true;
            this.Cursor = Cursors.Cross;
            // Reset nếu đang vẽ line dở dang
            if (isDrawingLine)
            {
                isDrawingLine = false;
            }
            isDrawingLine = true;
        }

        private void drawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawingLine)
            {
                currentMousePos = e.Location;
                drawingPanel.Invalidate(); // Vẽ lại để show preview
            }
        }











        // =============== Handle Functions ==============

        private void HandleLineDrawing(PointF clickPoint)
        {
            if (isFirstClick)
            {
                // First click - lưu điểm đầu
                firstPoint = clickPoint;
                lineStartPoint = clickPoint;
                isFirstClick = false;
                Console.WriteLine($"First point: {clickPoint.X}, {clickPoint.Y}");
            }
            else
            {
                // Second click - hoàn thành line
                PointF secondPoint = clickPoint;

                // Thêm line vào danh sách
                drawnLines.Add(new PointF[] { firstPoint, secondPoint });

                Console.WriteLine($"Line completed: ({firstPoint.X},{firstPoint.Y}) to ({secondPoint.X},{secondPoint.Y})");

                // Reset để vẽ line tiếp theo
                isFirstClick = true;

                // Vẽ lại
                drawingPanel.Invalidate();
            }
        }

        // Helper method để lấy tọa độ trung tâm của entity
        private PointF GetEntityCenter(MyJsonElement entity)
        {
            switch (entity.Type)
            {
                case "Line":
                    return new PointF((entity.xP1 + entity.xP2) / 2, (entity.yP1 + entity.yP2) / 2);
                case "Circle":
                case "Arc":
                case "Valve":
                default:
                    return new PointF(entity.xCenter, entity.yCenter);
                case "Polyline":
                    if (entity.Vertices != null && entity.Vertices.Count > 0)
                    {
                        float avgX = (float)entity.Vertices.Average(v => v.X);
                        float avgY = (float)entity.Vertices.Average(v => v.Y);
                        return new PointF(avgX, avgY);
                    }
                    return new PointF(entity.xCenter, entity.yCenter);
            }
        }

        // Helper method để pan view đến entity được chọn
        private void PanToEntity(MyJsonElement entity)
        {
            PointF center = GetEntityCenter(entity);

            // Tính toán offset mới để đưa entity vào giữa màn hình
            offsetX = (drawingPanel.Width / (2 * scale)) - center.X;
            offsetY = (drawingPanel.Height / (2 * scale)) - center.Y;
        }

        private void HandleSelectClick(PointF location)
        {
            const double tolerance = 10.0;
            selectedEntity.Clear();

            foreach (var entity in allEntities)
            {
                if (IsEntityClicked(entity, location, tolerance))
                {
                    selectedEntity.Add(entity.LocalName);
                    break;
                }
            }

            drawingPanel.Invalidate();
        }

        private void LoadDrawFromJson(string path)
        {
            try
            {
                var rootData = JsonConvert.DeserializeObject<ExportResult>(path);

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

                // Load InstrumentLines
                if (rootData.InstrumentLines != null)
                {
                    foreach (var instrumentLine in rootData.InstrumentLines)
                    {
                        allEntities.Add(instrumentLine);
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

        private void DrawEntity(Graphics g, MyJsonElement entity)
        {
            Pen pen;

            if (selectedEntity.Contains(entity.LocalName))
            {
                pen = new Pen(Color.Red, 2);
            }
            else if (entity.Type == "InstrumentLine")
            {
                pen = new Pen(Color.Blue, 1);
            }
            else
            {
                pen = new Pen(Color.Black, 1); // mặc định
            }

            Brush textBrush = Brushes.Blue;
            System.Drawing.Font textFont = new System.Drawing.Font("Arial", 4);

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

                case "InstrumentLine":
                    var pI1 = WorldToScreen(entity.xP1, entity.yP1);
                    var pI2 = WorldToScreen(entity.xP2, entity.yP2);

                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; // Dotted line for InstrumentLine
                    g.DrawLine(pen, pI1, pI2);
                    break;

                default:
                    //var valveCenter = WorldToScreen(entity.xCenter, entity.yCenter);
                    //float valveSize = 10 * scale;

                    //// Apply rotation if needed
                    //g.TranslateTransform(valveCenter.X, valveCenter.Y);
                    //g.RotateTransform((float)entity.rotation);

                    ////// Draw text if exists
                    //if (!string.IsNullOrEmpty(entity.Text))
                    //{
                    //    //var textSize = g.MeasureString(entity.Text, textFont);
                    //    //g.DrawString(entity.Text, textFont, textBrush, -textSize.Width / 2, valveSize / 2 + 5);

                    //    string text = entity.Text.Replace("&#xA;", "\n");

                    //    StringFormat format = new StringFormat
                    //    {
                    //        Alignment = StringAlignment.Center,
                    //        LineAlignment = StringAlignment.Near
                    //    };

                    //    RectangleF layoutRect = new RectangleF(
                    //        -valveSize,                // trái
                    //        valveSize / 2 + 5,         // trên
                    //        valveSize * 2,             // rộng
                    //        100                        // cao (tùy chỉnh)
                    //    );

                    //    g.DrawString(text, textFont, textBrush, layoutRect, format);

                    //}

                    //g.ResetTransform();

                    //// Draw sub-entities
                    //if (entity.SubEntities != null)
                    //{
                    //    foreach (var subEntity in entity.SubEntities)
                    //    {
                    //        DrawEntity(g, subEntity);
                    //    }
                    //}

                    break;
            }

            pen.Dispose();
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



        private void LoadFromJson(string jsonInput)
        {
            _currentData = JsonConvert.DeserializeObject<ExportResult>(jsonInput);
            pipelines.Clear();

            if (_currentData?.Branches != null)
            {
                foreach (var pipelineObj in _currentData.Branches)
                {
                    foreach (var pipelineName in pipelineObj.Keys)
                    {
                        pipelines[pipelineName] = pipelineObj[pipelineName];
                    }
                }

            }

            LoadPipelinesToListView();
        }

        private void LoadPipelinesToListView()
        {
            listViewPipelines.Items.Clear();

            foreach (var kvp in pipelines)
            {
                var item = new ListViewItem(kvp.Key);
                listViewPipelines.Items.Add(item);
            }
        }

        private ExportResult BuildCurrentExportResult()
        {
            // NOTE: Branches trong JSON là List<Dictionary<...>>
            // ta gói "pipelines" (Dictionary<...>) vào 1 phần tử của List.
            var branchesList = new List<Dictionary<string, Dictionary<string, List<MyJsonElement>>>>()
            {
                pipelines.ToDictionary(k => k.Key, v => v.Value) // shallow copy là đủ
            };

            return new ExportResult
            {
                Branches = branchesList,
                Equipments = _currentData?.Equipments ?? new List<EquipmentWrapper>(),
                InstrumentLines = _currentData?.InstrumentLines ?? new List<MyJsonElement>()
            };
        }


        private void RefreshBranchesView(string pipelineName, string branchToSelect = null)
        {
            listViewBranches.Items.Clear();
            listViewElementOfBranch.Items.Clear();
            selectedEntity.Clear();

            if (!pipelines.ContainsKey(pipelineName)) return;

            foreach (var branch in pipelines[pipelineName])
            {
                var item = new ListViewItem(branch.Key);
                item.Tag = new { Pipeline = pipelineName, Branch = branch.Key };
                listViewBranches.Items.Add(item);
            }

            // Auto chọn branch cụ thể (nếu có)
            if (!string.IsNullOrEmpty(branchToSelect))
            {
                foreach (ListViewItem item in listViewBranches.Items)
                {
                    if (item.Text == branchToSelect)
                    {
                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                        break;
                    }
                }
            }
        }




        private Box2 GetLargestInsertBoundingBox()
        {
            Box2 largestBox = null;
            double maxArea = 0;

            foreach (var item in blockDictionary)
            {
                Box2 currentBox = item.Value.GetInsertBoundingBox();
                if (currentBox == null) continue;

                double currentArea = (currentBox.P2.X - currentBox.P1.X) * (currentBox.P2.Y - currentBox.P1.Y);
                if (currentArea > maxArea)
                {
                    maxArea = currentArea;
                    largestBox = currentBox;
                }
            }

            return largestBox;
        }

        private void ExportEquipmentWithNozzle_ByBoundingBox()
        {
            try
            {
                var blockMap = BlockProcessName
                .Where(b => b.Getobject() is BlockInfo)
                .ToDictionary(b => b.LocalName, b => (BlockInfo)b.Getobject());

                var exportList = new List<object>();

                foreach (var equip in listEquipment)
                {
                    var eInfo = equip.Getobject() as BlockInfo;
                    if (eInfo == null) continue;

                    var searchBox = IplReOrder.HandleCorrectBox(equip);

                    var hits = OderPipeline.InterSectionLine(searchBox);
                    var touchingBlocksNames = hits
                        .Where(line => line.IsBoundingLine && line.LocalName.StartsWith("b_"))
                        .Select(line => line.LocalName)
                        .Distinct()
                        .ToList();

                    var nozzles = new List<EntityDto>();

                    foreach (var bn in touchingBlocksNames)
                    {
                        var cadEntity = BlockProcessName
                                .FirstOrDefault(b => b.LocalName == bn);

                        if (cadEntity == null) continue;

                        if (!(cadEntity.Getobject() is BlockInfo bInfo)) continue;

                        if (!string.Equals(bInfo.BlockType, "Nozzle", StringComparison.OrdinalIgnoreCase))
                            continue;

                        nozzles.Add(new EntityDto
                        {
                            LocalName = bn,
                            Type = bInfo.BlockType,
                            xCenter = (float)bInfo.BoundingBox.Center.X,
                            yCenter = (float)bInfo.BoundingBox.Center.Y,
                            rotation = (int)bInfo.Insert.Rotation,
                            SubEntities = _iplReadCad.getSubEntities(cadEntity)
                        });

                        BlockProcessName.Remove(cadEntity);
                    }

                    float xPos = 0;
                    float yPos = 0;
                    string textAttribute = "";
                    if (eInfo.Insert.Attributes.Count > 0)
                    {
                        foreach (var att in eInfo.Insert.Attributes)
                        {
                            if (textAttribute.Length > 0)
                                textAttribute += "&#xA;";

                            textAttribute += att.Value;
                        }
                    }

                    if (eInfo.BoundingBox == null)
                    {
                        xPos = (float)eInfo.Insert.Position.X;
                        yPos = (float)eInfo.Insert.Position.Y;
                    }
                    else
                    {
                        Point2 center = eInfo.GetCenter();
                        xPos = (float)center.X;
                        yPos = (float)center.Y;
                    }

                    exportList.Add(new
                    {
                        Equipment = new EntityDto
                        {
                            LocalName = eInfo.BlockName,
                            Type = eInfo.BlockType,
                            Text = textAttribute,
                            xCenter = xPos,
                            yCenter = yPos,
                            rotation = (int)eInfo.Insert.Rotation,
                            SubEntities = _iplReadCad.getSubEntities(equip)
                        },
                        Nozzle = nozzles
                    });
                }

                // ====== Thêm phần allInstrumentLines vào JSON ======
                var instrumentDtos = allInstrumentLines
                    .Select(l =>
                    {
                        var line = l.Getobject() as DiagLine;
                        if (line == null) return null;
                        return new EntityDto
                        {
                            Type = "InstrumentLine",
                            xP1 = (float)line.P1.X,
                            yP1 = (float)line.P1.Y,
                            xP2 = (float)line.P2.X,
                            yP2 = (float)line.P2.Y
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                var finalExport = new
                {
                    Branches = restructuredData,
                    Equipments = exportList,
                    InstrumentLines = instrumentDtos,
                };

                var json = JsonConvert.SerializeObject(finalExport, Formatting.Indented);
                File.WriteAllText("All_Entities.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi export equipment với nozzle: {ex.Message}", "Lỗi");
            }
        }

        private void UpdateSequenceNumbers()
        {
            int seq = 1;
            foreach (ListViewItem item in listViewElementOfBranch.Items)
            {
                item.SubItems[0].Text = seq.ToString(); // cột Sequence là SubItem[0]
                seq++;
            }
        }

    }

    public class BranchTag
    {
        public string Pipeline { get; set; }
        public string Branch { get; set; }
    }


    public class BlockInfo
    {
        public netDxf.Entities.Insert Insert { get; set; }
        public string BlockType { get; set; }
        public string BlockName => Insert.Block?.Name ?? "Unknown";
        public string LocalName { get; set; }
        public bool IsProcessed { get; set; } = false;
        public Box2 BoundingBox { get; private set; }
        public BlockInfo(netDxf.Entities.Insert insert, string blockType)
        {
            Insert = insert;
            BlockType = blockType;
            BoundingBox = GetInsertBoundingBox();
        }
        public Box2 Box()
        {
            if (BoundingBox == null)
            {
                return GetInsertBoundingBox();
            }
            else
            {
                return BoundingBox;
            }
        }

        public Point2 GetCenter()
        {
            if (BoundingBox == null)
                return null; 

            double centerX = (Math.Min(BoundingBox.P1.X, BoundingBox.P2.X) + Math.Max(BoundingBox.P1.X, BoundingBox.P2.X)) / 2.0;
            double centerY = (Math.Min(BoundingBox.P1.Y, BoundingBox.P2.Y) + Math.Max(BoundingBox.P1.Y, BoundingBox.P2.Y)) / 2.0;

            return new Point2(centerX, centerY);
        }

        public Box2 GetInsertBoundingBox()
        {
            Box2 circleBox = GetCircleBoundingBox();
            Box2 lineBox = GetLineBoundingBox();

            if (circleBox != null && lineBox != null)
                return Box2.UnionFast(circleBox, lineBox);
            if (circleBox != null)
                return circleBox;
            if (lineBox != null)
                return lineBox.Expand(1);
            return null;

        }

        public Box2 GetCircleBoundingBox()
        {
            var circles = Insert.Block.Entities.OfType<Circle>().ToList();
            if (circles.Count == 0)
                return null;

            Box2 boundingBox = null;
            foreach (var circle in circles)
            {
                Vector2 insertPos = new Vector2(Insert.Position.X, Insert.Position.Y);
                Vector2 circleLocalPos = new Vector2(circle.Center.X, circle.Center.Y);
                Vector2 circleWorldPos = insertPos + circleLocalPos * new Vector2(Insert.Scale.X, Insert.Scale.Y);

                double radius = circle.Radius * Math.Max(Insert.Scale.X, Insert.Scale.Y);

                Box2 box = new Box2(
                    new Point2(circleWorldPos.X - radius, circleWorldPos.Y - radius),
                    new Point2(circleWorldPos.X + radius, circleWorldPos.Y + radius)
                );

                boundingBox = boundingBox == null ? box : Box2.UnionFast(boundingBox, box);
            }
            return boundingBox;
        }

        public Box2 GetLineBoundingBox()
        {
            var insertPos = new Vector2(Insert.Position.X, Insert.Position.Y);
            var scale = new Vector2(Insert.Scale.X, Insert.Scale.Y);
            double angleRad = Insert.Rotation * Math.PI / 180.0;
            Func<Vector2, Vector2> rotate = v =>
                new Vector2(
                    v.X * Math.Cos(angleRad) - v.Y * Math.Sin(angleRad),
                    v.X * Math.Sin(angleRad) + v.Y * Math.Cos(angleRad)
                );

            Box2 boundingBox = null;

            // Lines
            var lines = Insert.Block.Entities.OfType<netDxf.Entities.Line>();
            foreach (var line in lines)
            {
                Vector2 startLocal = new Vector2(line.StartPoint.X, line.StartPoint.Y);
                Vector2 endLocal = new Vector2(line.EndPoint.X, line.EndPoint.Y);

                Vector2 startGlobal = insertPos + rotate(startLocal * scale);
                Vector2 endGlobal = insertPos + rotate(endLocal * scale);

                Box2 box = new Box2(
                    new Point2(Math.Min(startGlobal.X, endGlobal.X), Math.Min(startGlobal.Y, endGlobal.Y)),
                    new Point2(Math.Max(startGlobal.X, endGlobal.X), Math.Max(startGlobal.Y, endGlobal.Y))
                );

                boundingBox = boundingBox == null ? box : Box2.UnionFast(boundingBox, box);
            }

            // Arcs
            var arcs = Insert.Block.Entities.OfType<netDxf.Entities.Arc>();
            foreach (var arc in arcs)
            {
                Vector2 centerLocal = new Vector2(arc.Center.X, arc.Center.Y);
                Vector2 centerGlobal = insertPos + rotate(centerLocal * scale);
                double radius = arc.Radius * Math.Max(scale.X, scale.Y);

                double startAngle = arc.StartAngle + Insert.Rotation;
                double endAngle = arc.EndAngle + Insert.Rotation;

                var arc2 = new Arc2(
                    new Point2(centerGlobal.X, centerGlobal.Y),
                    radius,
                    startAngle,
                    endAngle - startAngle
                );
                Box2 box = arc2.Box(BoundingBoxHint.FAST);

                boundingBox = boundingBox == null ? box : Box2.UnionFast(boundingBox, box);
            }

            // Polyline2D
            var polylines = Insert.Block.Entities.OfType<netDxf.Entities.Polyline2D>();
            foreach (var polyline in polylines)
            {
                var vertices = polyline.Vertexes.ToList();
                if (vertices.Count == 0) continue;

                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                foreach (var v in vertices)
                {
                    Vector2 ptLocal = new Vector2(v.Position.X, v.Position.Y);
                    Vector2 ptGlobal = insertPos + rotate(ptLocal * scale);

                    minX = Math.Min(minX, ptGlobal.X);
                    minY = Math.Min(minY, ptGlobal.Y);
                    maxX = Math.Max(maxX, ptGlobal.X);
                    maxY = Math.Max(maxY, ptGlobal.Y);
                }

                Box2 box = new Box2(new Point2(minX, minY), new Point2(maxX, maxY));
                boundingBox = boundingBox == null ? box : Box2.UnionFast(boundingBox, box);
            }

            return boundingBox;
        }

    }
}
