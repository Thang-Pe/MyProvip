using netDxf.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static netDxf.Entities.HatchBoundaryPath;

namespace MyProvip
{
    internal class XmlGenerator
    {
        public void GeranateXMlFromJson(string jsonFilePath, string xmlFilePath)
        {
            try
            {
                //var paths = JsonConvert.DeserializeObject<List<List<MyJsonElement>>>(jsonFilePath);
                var paths = JsonConvert.DeserializeObject<
                                List<Dictionary<string, Dictionary<string, List<MyJsonElement>>>>>(jsonFilePath);


                string jsonInputEquip = File.ReadAllText(@"E:\CSharp\test29-7\test\MyProvip\MyProvip\bin\Debug\equipment_with_nozzle.json");

                ExportResult resultEquinLine = JsonConvert.DeserializeObject<ExportResult>(jsonInputEquip);

                var rnd2 = new RandomTwoDigit();
                int xmpCounter = 70;
                Dictionary<string, List<MyJsonElement>> loopMap = new Dictionary<string, List<MyJsonElement>>();

                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xmlDeclaration);

                // Root PlantModel
                XmlElement plantModel = doc.CreateElement("PlantModel");
                plantModel.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                plantModel.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                doc.AppendChild(plantModel);

                // PlantInformation
                XmlElement plantInfo = doc.CreateElement("PlantInformation");
                plantInfo.SetAttribute("SchemaVersion", "3.3.3");
                plantInfo.SetAttribute("OriginatingSystem", "AVEVA DIAGRAMS");
                plantInfo.SetAttribute("Date", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                plantInfo.SetAttribute("Time", DateTime.UtcNow.ToString("HH:mm:ss.fffffffZ"));
                plantInfo.SetAttribute("Is3D", "no");
                plantInfo.SetAttribute("Units", "Millimetre");
                plantInfo.SetAttribute("Discipline", "PID");

                XmlElement unitsOfMeasure = doc.CreateElement("UnitsOfMeasure");
                unitsOfMeasure.SetAttribute("Area", "sq mm");
                unitsOfMeasure.SetAttribute("Angle", "deg");
                unitsOfMeasure.SetAttribute("Distance", "mm");
                unitsOfMeasure.SetAttribute("Temperature", "degC");
                unitsOfMeasure.SetAttribute("Pressure", "Picopascal");
                unitsOfMeasure.SetAttribute("Volume", "cc");
                unitsOfMeasure.SetAttribute("Weight", "mg");
                plantInfo.AppendChild(unitsOfMeasure);

                plantModel.AppendChild(plantInfo);

                // Extent
                XmlElement extentDraw = doc.CreateElement("Extent");
                XmlElement minDraw = doc.CreateElement("Min");
                minDraw.SetAttribute("X", "0");
                minDraw.SetAttribute("Y", "0");
                XmlElement maxDraw = doc.CreateElement("Max");
                maxDraw.SetAttribute("X", "1189");
                maxDraw.SetAttribute("Y", "841");
                extentDraw.AppendChild(minDraw);
                extentDraw.AppendChild(maxDraw);
                plantModel.AppendChild(extentDraw);

                // Drawing
                XmlElement drawing = doc.CreateElement("Drawing");
                drawing.SetAttribute("Name", "T0-01-2012-0001_Page-1");
                drawing.SetAttribute("Type", "PID");
                drawing.SetAttribute("Revision", "");
                drawing.SetAttribute("Title", "");

                XmlElement presentationDraw = doc.CreateElement("Presentation");
                presentationDraw.SetAttribute("Layer", "");
                presentationDraw.SetAttribute("LineType", "");
                presentationDraw.SetAttribute("LineWeight", "");
                presentationDraw.SetAttribute("R", "0");
                presentationDraw.SetAttribute("G", "0");
                presentationDraw.SetAttribute("B", "0");
                drawing.AppendChild(presentationDraw);

                XmlElement drawExtent = doc.CreateElement("Extent");
                XmlElement drawMin = doc.CreateElement("Min");
                drawMin.SetAttribute("X", "0");
                drawMin.SetAttribute("Y", "0");
                XmlElement drawMax = doc.CreateElement("Max");
                drawMax.SetAttribute("X", "1189");
                drawMax.SetAttribute("Y", "841");
                drawExtent.AppendChild(drawMin);
                drawExtent.AppendChild(drawMax);
                drawing.AppendChild(drawExtent);

                // GenericAttributes
                XmlElement genericAttributesDraw = doc.CreateElement("GenericAttributes");
                genericAttributesDraw.SetAttribute("Number", "15");

                void AddGA(string name, string value, string format = "string")
                {
                    XmlElement ga = doc.CreateElement("GenericAttribute");
                    ga.SetAttribute("Name", name);
                    ga.SetAttribute("Value", value);
                    ga.SetAttribute("Format", format);
                    genericAttributesDraw.AppendChild(ga);
                }

                AddGA("Name", "T0-01-2012-0001");
                AddGA("Type", "SCDIAG");
                AddGA("Lock", "false");
                AddGA("Owner", "SCHEMATICS-PIPING-TEST01");
                AddGA("Description", "");
                AddGA("Function", "");
                AddGA("Url", "");
                AddGA("Exfile", "10000");
                AddGA("Pvno", "134");
                AddGA("Arno", "0");
                AddGA("Number", "1");
                AddGA("Schtype", "0");
                AddGA("Schfformat", "2");
                AddGA("Scsysd", "");
                AddGA("Sterefarray", "Sample_Stencil");

                drawing.AppendChild(genericAttributesDraw);

                // DrawingBorder
                XmlElement drawingBorder = doc.CreateElement("DrawingBorder");
                XmlElement borderPresentation = doc.CreateElement("Presentation");
                borderPresentation.SetAttribute("Layer", "");
                borderPresentation.SetAttribute("LineType", "");
                borderPresentation.SetAttribute("LineWeight", "");
                borderPresentation.SetAttribute("R", "0");
                borderPresentation.SetAttribute("G", "0");
                borderPresentation.SetAttribute("B", "0");
                drawingBorder.AppendChild(borderPresentation);

                XmlElement borderExtent = doc.CreateElement("Extent");
                XmlElement borderMin = doc.CreateElement("Min");
                borderMin.SetAttribute("X", "0");
                borderMin.SetAttribute("Y", "0");
                XmlElement borderMax = doc.CreateElement("Max");
                borderMax.SetAttribute("X", "1189");
                borderMax.SetAttribute("Y", "841");
                borderExtent.AppendChild(borderMin);
                borderExtent.AppendChild(borderMax);
                drawingBorder.AppendChild(borderExtent);

                drawing.AppendChild(drawingBorder);

                plantModel.AppendChild(drawing);

                // ======================================================

                List<EquipmentWrapper> equipmentList = resultEquinLine.Equipments;
                // Equipment
                foreach (var item in equipmentList)
                {
                    // Instrument
                    if (item.Equipment.Type == "Instrument")
                    {
                        var (part1, part2) = BlockConfigStorage.SplitEquipment(item.Equipment.Text);
                        if (part2 != null)
                        {
                            string value = part1 ?? ""; // XL
                            string key = part2; // P12532B

                            if (!loopMap.ContainsKey(key))
                                loopMap[key] = new List<MyJsonElement>();

                            loopMap[key].Add(item.Equipment);
                        }

                    }
                    // Equipment and Rest
                    else if (item.Equipment.Type == "Equipment")
                    {
                        string tagName = "E" + rnd2.Next();

                        XmlElement equipment = doc.CreateElement("Equipment");
                        equipment.SetAttribute("ID", "XMP_" + rnd2.Next());
                        equipment.SetAttribute("TagName", tagName);
                        equipment.SetAttribute("ComponentClass", "Equipment");
                        equipment.SetAttribute("ComponentName", "Tank");
                        plantModel.AppendChild(equipment);

                        // GenericAttributes for Equipment
                        XmlElement genericAttributesEquip = doc.CreateElement("GenericAttributes");
                        genericAttributesEquip.SetAttribute("Number", "77");
                        genericAttributesEquip.SetAttribute("Set", "Tank");
                        equipment.AppendChild(genericAttributesEquip);

                        AddGenericAttribute(doc, genericAttributesEquip, "Name", $"{tagName}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Text", "[namn][\n][desc]", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "ActType", "SCEQUI", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Area", "0", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Description", "TANK", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Diarefarray", "T0-01-2012-0001", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Distag", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Function", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Inprtref", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Ispec", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Letter", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Location", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Lock", "false", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Number", "0", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Ouprtref", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Owner", "", "SCHEMATICS-PIPING-TEST01", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Pagearray", "1", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Position", "W 319150mm N 296950mm U 101470mm", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Prefix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Purpose", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Recomment", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "REDATE", "14/08/2025", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Restatus", "1", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Reuser", "SYSTEM", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Sclore", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Scsysf", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Spref", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Suffix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Type", "SCEQUI", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Is primary", "True", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Use filter from defaults", "True", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Default Catalogue Search filter", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Use sel.table from defaults", "True", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Default Selection Table", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Component Class", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Component name", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Component Type", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Description", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Discipline", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Maximum Design Pressure", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Minimum Design Pressure", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Maximum Design Temperature", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Minimum Design Temperature", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source data ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Manufacturer", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Material Description", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Material", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Model Number", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID attribute", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID context", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID context", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID context", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID context", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Source system ID context", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Revision", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Specification", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Status", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Stocknum", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Supplier", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Tag", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Last Transaction Company", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Last Transaction Date", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Last Transaction Person", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Last Transaction Time", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Last Transaction Type", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Weight", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Desref", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Scaref", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "ABOR", "T0-01-2012-0001", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "LBOR", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "CONNECTIONS", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "ConnectionPoints", "5", "", "string");

                        var element = item.Equipment;
                        for (int i = 0; i < element.SubEntities.Count; i++)
                        {
                            if (element.SubEntities[i].Type == "Line")
                            {
                                CreateLineElement(doc, equipment, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Polyline")
                            {
                                CreatePolylineElement(doc, equipment, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Circle")
                            {
                                CreateCircleElement(doc, equipment, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Arc")
                            {
                                CreateArcAsPolylineElement(doc, equipment, element, i);
                            }
                        }

                        if (!string.IsNullOrEmpty(element.Text))
                        {
                            CreateTagElement(doc, equipment, element);
                        }

                        XmlElement persistentIdEqui = doc.CreateElement("PersistentID");
                        persistentIdEqui.SetAttribute("Identifier", "" + rnd2.Next());
                        persistentIdEqui.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                        equipment.AppendChild(persistentIdEqui);

                        // Thêm Extent cho Equipment
                        XmlElement extentEqui = doc.CreateElement("Extent");
                        XmlElement minEqui = doc.CreateElement("Min");
                        minEqui.SetAttribute("X", "93");
                        minEqui.SetAttribute("Y", "227.999995");
                        XmlElement maxEqui = doc.CreateElement("Max");
                        maxEqui.SetAttribute("X", "478");
                        maxEqui.SetAttribute("Y", "511");
                        extentEqui.AppendChild(minEqui);
                        extentEqui.AppendChild(maxEqui);
                        equipment.AppendChild(extentEqui);

                        // Thêm Presentation
                        XmlElement presentationEqui = doc.CreateElement("Presentation");
                        presentationEqui.SetAttribute("Layer", "0");
                        presentationEqui.SetAttribute("LineType", "Solid");
                        presentationEqui.SetAttribute("LineWeight", "0.25");
                        presentationEqui.SetAttribute("R", "0.541176");
                        presentationEqui.SetAttribute("G", "0.168627");
                        presentationEqui.SetAttribute("B", "0.886275");
                        equipment.AppendChild(presentationEqui);

                        // Position
                        XmlElement positionEqui = doc.CreateElement("Position");
                        XmlElement locationEqui = doc.CreateElement("Location");
                        locationEqui.SetAttribute("X", element.xCenter.ToString());
                        locationEqui.SetAttribute("Y", element.yCenter.ToString());
                        locationEqui.SetAttribute("Z", "0");
                        XmlElement axisEqui = doc.CreateElement("Axis");
                        axisEqui.SetAttribute("X", "0");
                        axisEqui.SetAttribute("Y", "0");
                        axisEqui.SetAttribute("Z", "1");
                        XmlElement referenceEqui = doc.CreateElement("Reference");
                        referenceEqui.SetAttribute("X", "1");
                        referenceEqui.SetAttribute("Y", "0");
                        referenceEqui.SetAttribute("Z", "0");
                        positionEqui.AppendChild(locationEqui);
                        positionEqui.AppendChild(axisEqui);
                        positionEqui.AppendChild(referenceEqui);
                        equipment.AppendChild(positionEqui);

                        // Nozzle
                        if (item.Nozzle != null)
                        {
                            int nozzleIndex = 1;
                            foreach (var nozzle in item.Nozzle)
                            {
                                string tagNameNozzle = tagName + "/N" + nozzleIndex;
                                XmlElement nozzleElement = doc.CreateElement("Nozzle");
                                nozzleElement.SetAttribute("ID", "XMP_" + rnd2.Next());
                                nozzleElement.SetAttribute("TagName", tagNameNozzle);
                                nozzleElement.SetAttribute("ComponentClass", "Nozzle");
                                nozzleElement.SetAttribute("ComponentName", "Tank." + rnd2.Next());
                                equipment.AppendChild(nozzleElement);

                                // GenericAttributes for nozzleElement
                                XmlElement genericAttributesNozzle = doc.CreateElement("GenericAttributes");
                                genericAttributesNozzle.SetAttribute("Number", "66");
                                genericAttributesNozzle.SetAttribute("Set", "Tank." + rnd2.Next());
                                nozzleElement.AppendChild(genericAttributesNozzle);

                                AddGenericAttribute(doc, genericAttributesNozzle, "Name", $"{tagNameNozzle}", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Text", "{after(namn,'/')}", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "ActType", "SCNOZZ", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Bore", "100", "mm", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Catref", "300lb_Ansi_Flanged/AAZFBD0NN", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Cref", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Description", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Diarefarray", "T0-01-2012-0001", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Ductheight", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Ductshape", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Ductwidth", "0", "mm", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Duty", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Function", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Inprtref", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Ispec", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Lock", "false", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Nspec", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Ouprtref", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Owner", "", $"{tagName}", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Pagearray", "1", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Pressure", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Purpose", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Scgtype", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Temperature", "100000", "degC", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Type", "SCNOZZ", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Component Class", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Component name", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Component Type", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Description", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Height of Nozzle", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source data ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Manufacturer", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Material Description", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Material", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Model Number", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Nominal Diameter", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Type of Nozzle", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID attribute", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID context", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID context", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID context", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID context", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Source system ID context", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Rating", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Revision", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Specification", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Status", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Stocknum", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Supplier", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Tag", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Last Transaction Company", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Last Transaction Date", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Last Transaction Person", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Last Transaction Time", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Last Transaction Type", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Weight", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Desref", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Scaref", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Diagxpos", "0", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "Diagypos", "0", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "ABOR", "TI-107 /T0-01-2012-0001", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "LBOR", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "CONNECTIONS", "", "", "string");
                                AddGenericAttribute(doc, genericAttributesNozzle, "ConnectionPoints", "3", "", "string");

                                var elementNozzle = nozzle;
                                for (int i = 0; i < elementNozzle.SubEntities.Count; i++)
                                {
                                    if (elementNozzle.SubEntities[i].Type == "Line")
                                    {
                                        CreateLineElement(doc, nozzleElement, elementNozzle, i);
                                    }
                                    else if (elementNozzle.SubEntities[i].Type == "Polyline")
                                    {
                                        CreatePolylineElement(doc, nozzleElement, elementNozzle, i);
                                    }
                                    else if (elementNozzle.SubEntities[i].Type == "Circle")
                                    {
                                        CreateCircleElement(doc, nozzleElement, elementNozzle, i);
                                    }
                                }

                                XmlElement persistentIdNozzle = doc.CreateElement("PersistentID");
                                persistentIdNozzle.SetAttribute("Identifier", "" + rnd2.Next());
                                persistentIdNozzle.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                                nozzleElement.AppendChild(persistentIdNozzle);

                                // Thêm Extent cho nozzleElement
                                XmlElement extentNozzle = doc.CreateElement("Extent");
                                XmlElement minNozzle = doc.CreateElement("Min");
                                minNozzle.SetAttribute("X", "93");
                                minNozzle.SetAttribute("Y", "227.999995");
                                XmlElement maxNozzle = doc.CreateElement("Max");
                                maxNozzle.SetAttribute("X", "478");
                                maxNozzle.SetAttribute("Y", "511");
                                extentNozzle.AppendChild(minNozzle);
                                extentNozzle.AppendChild(maxNozzle);
                                nozzleElement.AppendChild(extentNozzle);

                                // Thêm Presentation
                                XmlElement presentationNozzle = doc.CreateElement("Presentation");
                                presentationNozzle.SetAttribute("Layer", "0");
                                presentationNozzle.SetAttribute("LineType", "Solid");
                                presentationNozzle.SetAttribute("LineWeight", "0.25");
                                presentationNozzle.SetAttribute("R", "0.541176");
                                presentationNozzle.SetAttribute("G", "0.168627");
                                presentationNozzle.SetAttribute("B", "0.886275");
                                nozzleElement.AppendChild(presentationNozzle);

                                // Position
                                XmlElement positionNozzle = doc.CreateElement("Position");
                                XmlElement locationNozzle = doc.CreateElement("Location");
                                locationNozzle.SetAttribute("X", element.xCenter.ToString());
                                locationNozzle.SetAttribute("Y", element.yCenter.ToString());
                                locationNozzle.SetAttribute("Z", "0");
                                XmlElement axisNozzle = doc.CreateElement("Axis");
                                axisNozzle.SetAttribute("X", "0");
                                axisNozzle.SetAttribute("Y", "0");
                                axisNozzle.SetAttribute("Z", "1");
                                XmlElement referenceNozzle = doc.CreateElement("Reference");
                                referenceNozzle.SetAttribute("X", "1");
                                referenceNozzle.SetAttribute("Y", "0");
                                referenceNozzle.SetAttribute("Z", "0");
                                positionNozzle.AppendChild(locationNozzle);
                                positionNozzle.AppendChild(axisNozzle);
                                positionNozzle.AppendChild(referenceNozzle);
                                nozzleElement.AppendChild(positionNozzle);

                                nozzleIndex++;
                            }
                        }

                    }
                }

                foreach (var kv in loopMap)
                {
                    string loopTag = kv.Key; // 1113
                    string loopXmpId = "XMP_" + (xmpCounter++); // XMP_71

                    // Start InstrumentLoop
                    XmlElement instrumentLoop = doc.CreateElement("InstrumentLoop");
                    instrumentLoop.SetAttribute("ID", loopXmpId);
                    instrumentLoop.SetAttribute("TagName", loopTag);
                    plantModel.AppendChild(instrumentLoop);

                    // PersistentID
                    XmlElement persistentIdLoop = doc.CreateElement("PersistentID");
                    persistentIdLoop.SetAttribute("Identifier", $"{rnd2.Next()}");
                    persistentIdLoop.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                    instrumentLoop.AppendChild(persistentIdLoop);

                    // GenericAttributes for Equipment
                    XmlElement genericAttributesLoop = doc.CreateElement("GenericAttributes");
                    genericAttributesLoop.SetAttribute("Number", "14");
                    genericAttributesLoop.SetAttribute("Set", "");
                    instrumentLoop.AppendChild(genericAttributesLoop);

                    AddGenericAttribute(doc, genericAttributesLoop, "Block", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagBlock", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Function", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagFunction", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Prefix", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagPrefix", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Type", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagType", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Number", $"{loopTag}", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagSequenceNo", $"{loopTag}", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Suffix", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "TagSuffix", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "Grid_Reference", "", "", "string");
                    AddGenericAttribute(doc, genericAttributesLoop, "ReferenceObject", "NO", "", "string");

                    // End InstrumentLoop

                    // Start InstrumentComponent
                    foreach (var comp in kv.Value)
                    {
                        string compXmpId = "XMP_" + (xmpCounter++);
                        var (xl, code) = BlockConfigStorage.SplitEquipment(comp.Text);

                        XmlElement component = doc.CreateElement("InstrumentComponent");
                        component.SetAttribute("ID", compXmpId);
                        component.SetAttribute("TagName", $"{xl}-{code}");
                        component.SetAttribute("ComponentName", comp.LocalName);
                        component.SetAttribute("StockNumber", comp.LocalName);
                        component.SetAttribute("ComponentClass", comp.Type);

                        // Thêm Extent cho Equipment
                        XmlElement extentEqui = doc.CreateElement("Extent");
                        XmlElement minEqui = doc.CreateElement("Min");
                        minEqui.SetAttribute("X", "93");
                        minEqui.SetAttribute("Y", "227.999995");
                        minEqui.SetAttribute("Z", "0");
                        XmlElement maxEqui = doc.CreateElement("Max");
                        maxEqui.SetAttribute("X", "478");
                        maxEqui.SetAttribute("Y", "511");
                        maxEqui.SetAttribute("Z", "0");
                        extentEqui.AppendChild(minEqui);
                        extentEqui.AppendChild(maxEqui);
                        component.AppendChild(extentEqui);

                        // Thêm Presentation
                        XmlElement presentationEqui = doc.CreateElement("Presentation");
                        presentationEqui.SetAttribute("Layer", "AS_INST");
                        presentationEqui.SetAttribute("LineType", "Solid");
                        presentationEqui.SetAttribute("LineWeight", "0.25");
                        presentationEqui.SetAttribute("R", "0");
                        presentationEqui.SetAttribute("G", "0.600000");
                        presentationEqui.SetAttribute("B", "0.447059");
                        component.AppendChild(presentationEqui);

                        // Position
                        XmlElement positionEqui = doc.CreateElement("Position");
                        XmlElement locationEqui = doc.CreateElement("Location");
                        locationEqui.SetAttribute("X", $"{comp.xCenter}");
                        locationEqui.SetAttribute("Y", $"{comp.yCenter}");
                        locationEqui.SetAttribute("Z", "0");
                        XmlElement axisEqui = doc.CreateElement("Axis");
                        axisEqui.SetAttribute("X", "0");
                        axisEqui.SetAttribute("Y", "0");
                        axisEqui.SetAttribute("Z", "1");
                        XmlElement referenceEqui = doc.CreateElement("Reference");
                        referenceEqui.SetAttribute("X", "1");
                        referenceEqui.SetAttribute("Y", "0");
                        referenceEqui.SetAttribute("Z", "0");
                        positionEqui.AppendChild(locationEqui);
                        positionEqui.AppendChild(axisEqui);
                        positionEqui.AppendChild(referenceEqui);
                        component.AppendChild(positionEqui);

                        // Scale
                        XmlElement scale = doc.CreateElement("Scale");
                        scale.SetAttribute("X", "1");
                        scale.SetAttribute("Y", "1");
                        scale.SetAttribute("Z", "1");
                        component.AppendChild(scale);

                        // PersistentID
                        string Identifier = rnd2.Next().ToString();
                        XmlElement persistentIdEqui = doc.CreateElement("PersistentID");
                        persistentIdEqui.SetAttribute("Identifier", $"{Identifier}");
                        persistentIdEqui.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                        component.AppendChild(persistentIdEqui);

                        // GenericAttributes for Equipment
                        XmlElement genericAttributesEquip = doc.CreateElement("GenericAttributes");
                        genericAttributesEquip.SetAttribute("Number", "57");
                        genericAttributesEquip.SetAttribute("Set", $"{comp.LocalName}");
                        component.AppendChild(genericAttributesEquip);

                        AddGenericAttribute(doc, genericAttributesEquip, "Block", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "TagBlock", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Function", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "TagFunction", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Prefix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Type", $"{xl}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "TagType", $"{xl}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Number", $"{code}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "TagSequenceNo", $"{code}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Suffix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Instrument_Description", $"{comp.LocalName}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Handle", $"{Identifier}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Symbol_Name", $"{comp.LocalName}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Pattern_Item", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Prefix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Type", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Numbers", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Suffix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Conditioner", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Loop_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Prefix_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Suffix_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Panel_Type_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Alarm_Handles", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Alarm_Labels", ",,,", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Suffix_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Function_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Loop_Handle", "BE7E", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Prefix_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Type_Handle", "BE7B", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Block_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "AType", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Closure", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "PDMS_Type", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Prefix_Handle", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "SppPrefix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "SppType", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "BlSppNumberock", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "SppSuffix", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "SppBlock", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "SppFunction", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "CLASSNAME", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "TAGFORMAT", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "AREAPATH", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "INTERLOCK", "N", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_GENERAL_Description", $"{comp.LocalName}", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_GENERAL_Equipment_Detail", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_INSTRUMENT_TAG_IS", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_INSTRUMENT_TAG_LIMIT_SWITCH", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_INSTRUMENT_TAG_NUMBER", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_INSTRUMENT_TAG_TRAIN", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "DS_INSTRUMENT_TAG_TRAIN_NUMBER", "?-?", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Grid_Reference", "E2", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "Process_Owner", "", "", "string");
                        AddGenericAttribute(doc, genericAttributesEquip, "ReferenceObject", "NO", "", "string");

                        var element = comp;
                        for (int i = 0; i < element.SubEntities.Count; i++)
                        {
                            if (element.SubEntities[i].Type == "Line")
                            {
                                CreateLineElement(doc, component, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Polyline")
                            {
                                CreatePolylineElement(doc, component, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Circle")
                            {
                                CreateCircleElement(doc, component, element, i);
                            }
                            else if (element.SubEntities[i].Type == "Arc")
                            {
                                CreateArcAsPolylineElement(doc, component, element, i);
                            }
                        }

                        // Text
                        if (!string.IsNullOrEmpty(element.Text))
                        {
                            CreateTagElement(doc, component, element);
                        }


                        // Thêm Association tham chiếu về InstrumentLoop
                        XmlElement assoc = doc.CreateElement("Association");
                        assoc.SetAttribute("Type", "is a part of");
                        assoc.SetAttribute("ItemID", loopXmpId); // tham chiếu đúng loop
                        assoc.SetAttribute("TagName", loopTag);
                        component.AppendChild(assoc);

                        // Association Loop
                        XmlElement associationLoop = doc.CreateElement("Association");
                        associationLoop.SetAttribute("Type", "is a collection including");
                        associationLoop.SetAttribute("ItemID", $"{loopXmpId}");
                        associationLoop.SetAttribute("TagName", $"{xl}-{code}");
                        instrumentLoop.AppendChild(associationLoop);

                        plantModel.AppendChild(component);
                    }
                    // End InstrumentComponent
                }

                foreach (var equipDict in paths) // List cấp 1
                {
                    foreach (var equip in equipDict) // Key = "4\"-WPO-0648-D-SB-N0"
                    {
                        string equipmentName = equip.Key;
                        string[] parts = equipmentName.Split('-');
                        string result = parts.OrderByDescending(p => p.Length).FirstOrDefault();
                        //string result4 = parts.FirstOrDefault(p => p.Length == 4);

                        // PipingNetworkSystem
                        XmlElement pipingSystem = doc.CreateElement("PipingNetworkSystem");
                        pipingSystem.SetAttribute("ID", $"XMP_{rnd2.Next()}");
                        pipingSystem.SetAttribute("TagName", $"{equipmentName}");
                        pipingSystem.SetAttribute("Specification", $"{result}");
                        pipingSystem.SetAttribute("ComponentClass", "PipingSystem");
                        plantModel.AppendChild(pipingSystem);

                        // Thêm Extent
                        XmlElement extent = doc.CreateElement("Extent");
                        XmlElement min = doc.CreateElement("Min");
                        min.SetAttribute("X", "93");
                        min.SetAttribute("Y", "227.999995");
                        XmlElement max = doc.CreateElement("Max");
                        max.SetAttribute("X", "478");
                        max.SetAttribute("Y", "511");
                        extent.AppendChild(min);
                        extent.AppendChild(max);
                        pipingSystem.AppendChild(extent);

                        // Thêm Presentation
                        XmlElement presentation = doc.CreateElement("Presentation");
                        presentation.SetAttribute("Layer", "AS_");
                        presentation.SetAttribute("LineType", "Solid");
                        presentation.SetAttribute("LineWeight", "0.25");
                        presentation.SetAttribute("R", "0.541176");
                        presentation.SetAttribute("G", "0.168627");
                        presentation.SetAttribute("B", "0.886275");
                        pipingSystem.AppendChild(presentation);

                        // Thêm PersistentID
                        XmlElement persistentId = doc.CreateElement("PersistentID");
                        persistentId.SetAttribute("Identifier", $"{result}");
                        persistentId.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                        pipingSystem.AppendChild(persistentId);

                        // Thêm GenericAttributes
                        XmlElement genericAttributes = doc.CreateElement("GenericAttributes");
                        genericAttributes.SetAttribute("Set", "PipingSystem");
                        genericAttributes.SetAttribute("Number", "28");
                        pipingSystem.AppendChild(genericAttributes);

                        // Thêm các GenericAttribute
                        AddGenericAttribute(doc, genericAttributes, "DS_GENERAL_Description", "SECONDARY PIPE", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "DS_GENERAL_Equipment_Detail", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "DS__SPEC_PRESSURE", "6.0", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "DS_PIPE_SPEC_PRESSURETEMPERATURE", "([PRESSURE]/[TEMPERATURE])", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "DS_PIPE_SPEC_TEMPERATURE", "&quot;&quot;", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Size", "0", "in", "string");
                        AddGenericAttribute(doc, genericAttributes, "Fluid", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Number", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Specification", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Insulation_Table", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Area", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Tracing_Size", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Insulation_Index", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field7", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field9", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field4", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Tracing_Type", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field10", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field5", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Sheet_No", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Design_Temp", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Insulation_Condition", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Paint_Code", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Design_Pressure", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Tracing_No", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "Proj_Def_Field6", "", "", "string");
                        AddGenericAttribute(doc, genericAttributes, "ReferenceObject", "NO", "", "string");

                        var branches = equip.Value; // Dictionary<string, List<MyJsonElement>>

                        foreach (var branch in branches)
                        {
                            string branchName = branch.Key;
                            List<MyJsonElement> elements = branch.Value;
                            //foreach (var elem in elements)
                            //{
                                foreach (var path in elements)
                                {
                                    XmlElement segment = doc.CreateElement("PipingNetworkSegment");
                                    segment.SetAttribute("ID", "XMP_" + rnd2.Next());
                                    segment.SetAttribute("TagName", equipmentName);
                                    segment.SetAttribute("Specification", $"{result}");
                                    segment.SetAttribute("ComponentClass", "PipeLine");
                                    pipingSystem.AppendChild(segment);

                                    // Thêm Extent cho segment
                                    XmlElement segExtent = doc.CreateElement("Extent");
                                    XmlElement segMin = doc.CreateElement("Min");
                                    segMin.SetAttribute("X", "93");
                                    segMin.SetAttribute("Y", "227.999995");
                                    XmlElement segMax = doc.CreateElement("Max");
                                    segMax.SetAttribute("X", "378");
                                    segMax.SetAttribute("Y", "500");
                                    segExtent.AppendChild(segMin);
                                    segExtent.AppendChild(segMax);
                                    segment.AppendChild(segExtent);

                                    // Thêm Presentation cho segment
                                    XmlElement segPresentation = doc.CreateElement("Presentation");
                                    segPresentation.SetAttribute("Layer", "AS_PIPE");
                                    segPresentation.SetAttribute("LineType", "Solid");
                                    segPresentation.SetAttribute("LineWeight", "0.25");
                                    segPresentation.SetAttribute("R", "0.541176");
                                    segPresentation.SetAttribute("G", "0.168627");
                                    segPresentation.SetAttribute("B", "0.886275");
                                    segment.AppendChild(segPresentation);

                                    // Thêm PersistentID cho segment
                                    string identifier = rnd2.Next().ToString();
                                    XmlElement segPersistentId = doc.CreateElement("PersistentID");
                                    segPersistentId.SetAttribute("Identifier", identifier);
                                    segPersistentId.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                                    segment.AppendChild(segPersistentId);

                                    // Thêm GenericAttributes cho segment
                                    XmlElement segGenericAttributes = doc.CreateElement("GenericAttributes");
                                    segGenericAttributes.SetAttribute("Number", "39");
                                    segGenericAttributes.SetAttribute("Set", "PipeLine");
                                    segment.AppendChild(segGenericAttributes);

                                    string source = rnd2.Next().ToString();

                                    AddGenericAttribute(doc, segGenericAttributes, "BranchId", "2180", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Source", source, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Destination", identifier, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Connected_PDMS_Type_From", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Connected_PDMS_Type_To", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Connected_Handle_From", source, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Connected_Handle_To", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "OwnerId", result, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Size", "2", "in", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Fluid", "VG", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Number", result, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Specification", result, "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Insulation_Table", "N", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Area", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Tracing_Size", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Insulation_Index", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field7", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field9", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field4", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Tracing_Type", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field10", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field5", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Sheet_No", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Design_Temp", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Insulation_Condition", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Paint_Code", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Design_Pressure", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Tracing_No", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field8", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Proj_Def_Field6", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "DS_GENERAL_Description", "SECONDARY PIPE", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "DS_GENERAL_Equipment_Detail", "", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "DS_PIPE_SPEC_PRESSURE", "1.80", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "DS_PIPE_SPEC_PRESSURETEMPERATURE", "(1.80/65.0)", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "DS_PIPE_SPEC_TEMPERATURE", "65.0", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Dual_Flow", "No", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "Main", "YES", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "ActualFrom", "D-3103 N6_,DRUM, SYRINGE", "", "string");
                                    AddGenericAttribute(doc, segGenericAttributes, "ActualTo", "", "", "string");

                                    XmlElement nominalDiameter = doc.CreateElement("NominalDiameter");
                                    nominalDiameter.SetAttribute("Value", "300");
                                    nominalDiameter.SetAttribute("Units", "mm");
                                    segment.AppendChild(nominalDiameter);


                                //foreach (var element in path.TY)
                                //{
                                    var element = path; 
                                    switch (element.Type)
                                    {
                                            case "Line":
                                                XmlElement component = doc.CreateElement("Component");
                                                component.SetAttribute("ID", "XMP_" + rnd2.Next());
                                                component.SetAttribute("ComponentClass", "CenterLine");
                                                segment.AppendChild(component);

                                                // Extent
                                                XmlElement extentComponent = doc.CreateElement("Extent");
                                                XmlElement minComponent = doc.CreateElement("Min");
                                                minComponent.SetAttribute("X", Math.Min(element.xP1, element.xP2).ToString());
                                                minComponent.SetAttribute("Y", Math.Min(element.yP1, element.yP2).ToString());
                                                XmlElement maxComponent = doc.CreateElement("Max");
                                                maxComponent.SetAttribute("X", Math.Max(element.xP1, element.xP2).ToString());
                                                maxComponent.SetAttribute("Y", Math.Max(element.yP1, element.yP2).ToString());
                                                extentComponent.AppendChild(minComponent);
                                                extentComponent.AppendChild(maxComponent);
                                                component.AppendChild(extentComponent);

                                                // Presentation
                                                XmlElement presentationComponent = doc.CreateElement("Presentation");
                                                presentationComponent.SetAttribute("Layer", "Connector");
                                                presentationComponent.SetAttribute("LineType", "Solid");
                                                presentationComponent.SetAttribute("LineWeight", "0.8");
                                                presentationComponent.SetAttribute("R", "0.541176");
                                                presentationComponent.SetAttribute("G", "0.168627");
                                                presentationComponent.SetAttribute("B", "0.886275");
                                                component.AppendChild(presentationComponent);

                                                // PersistentID
                                                XmlElement persistentIdComponent = doc.CreateElement("PersistentID");
                                                persistentIdComponent.SetAttribute("Identifier", "" + rnd2.Next());
                                                persistentIdComponent.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                                                component.AppendChild(persistentIdComponent);

                                                XmlElement polylineComponent = doc.CreateElement("PolyLine");
                                                polylineComponent.SetAttribute("NumPoints", "2");
                                                component.AppendChild(polylineComponent);
                                                // Thêm Presentation cho component
                                                XmlElement polylinePresentation = doc.CreateElement("Presentation");
                                                polylinePresentation.SetAttribute("Layer", "Connector");
                                                polylinePresentation.SetAttribute("LineType", "Solid");
                                                polylinePresentation.SetAttribute("LineWeight", "0.8");
                                                polylinePresentation.SetAttribute("R", "0.541176");
                                                polylinePresentation.SetAttribute("G", "0.168627");
                                                polylinePresentation.SetAttribute("B", "0.886275");
                                                polylineComponent.AppendChild(polylinePresentation);
                                                // Extent
                                                XmlElement extentPolyline = doc.CreateElement("Extent");
                                                XmlElement minPolyline = doc.CreateElement("Min");
                                                minPolyline.SetAttribute("X", Math.Min(element.xP1, element.xP2).ToString());
                                                minPolyline.SetAttribute("Y", Math.Min(element.yP1, element.yP2).ToString());
                                                XmlElement maxPolyline = doc.CreateElement("Max");
                                                maxPolyline.SetAttribute("X", Math.Max(element.xP1, element.xP2).ToString());
                                                maxPolyline.SetAttribute("Y", Math.Max(element.yP1, element.yP2).ToString());
                                                extentPolyline.AppendChild(minPolyline);
                                                extentPolyline.AppendChild(maxPolyline);
                                                polylineComponent.AppendChild(extentPolyline);
                                                // Thêm Coordinate cho PolyLine
                                                XmlElement coordinate1 = doc.CreateElement("Coordinate");
                                                coordinate1.SetAttribute("X", element.xP1.ToString());
                                                coordinate1.SetAttribute("Y", element.yP1.ToString());
                                                coordinate1.SetAttribute("Z", "0");
                                                polylineComponent.AppendChild(coordinate1);
                                                XmlElement coordinate2 = doc.CreateElement("Coordinate");
                                                coordinate2.SetAttribute("X", element.xP2.ToString());
                                                coordinate2.SetAttribute("Y", element.yP2.ToString());
                                                coordinate2.SetAttribute("Z", "0");
                                                polylineComponent.AppendChild(coordinate2);


                                                break;
                                            default:
                                                string tagNameValv = "V" + rnd2.Next();
                                                XmlElement pipingComponent = doc.CreateElement("PipingComponent");
                                                pipingComponent.SetAttribute("ID", "XMP_" + rnd2.Next());
                                                pipingComponent.SetAttribute("TagName", tagNameValv);
                                                pipingComponent.SetAttribute("Specification", "A3B/VH80");
                                                pipingComponent.SetAttribute("ComponentClass", "Valve");
                                                pipingComponent.SetAttribute("ComponentName", "Valve." + rnd2.Next());
                                                segment.AppendChild(pipingComponent);

                                                // Thêm Extent cho pipingComponent
                                                XmlElement segExtentPipComponent = doc.CreateElement("Extent");
                                                XmlElement segMinPipComponent = doc.CreateElement("Min");
                                                segMinPipComponent.SetAttribute("X", "318");
                                                segMinPipComponent.SetAttribute("Y", "463");
                                                XmlElement segMaxPipComponent = doc.CreateElement("Max");
                                                segMaxPipComponent.SetAttribute("X", "330");
                                                segMaxPipComponent.SetAttribute("Y", "469");
                                                segExtentPipComponent.AppendChild(segMinPipComponent);
                                                segExtentPipComponent.AppendChild(segMaxPipComponent);
                                                pipingComponent.AppendChild(segExtentPipComponent);

                                                // Thêm Presentation cho pipingComponent
                                                XmlElement segPresentationPipComponent = doc.CreateElement("Presentation");
                                                segPresentationPipComponent.SetAttribute("Layer", "0");
                                                segPresentationPipComponent.SetAttribute("LineType", "Solid");
                                                segPresentationPipComponent.SetAttribute("LineWeight", "0.25");
                                                segPresentationPipComponent.SetAttribute("R", "0.541176");
                                                segPresentationPipComponent.SetAttribute("G", "0.168627");
                                                segPresentationPipComponent.SetAttribute("B", "0.886275");
                                                pipingComponent.AppendChild(segPresentationPipComponent);

                                                // Position
                                                XmlElement segPositionPipComponent = doc.CreateElement("Position");
                                                XmlElement locationPipCom = doc.CreateElement("Location");
                                                locationPipCom.SetAttribute("X", element.xCenter.ToString());
                                                locationPipCom.SetAttribute("Y", element.yCenter.ToString());
                                                locationPipCom.SetAttribute("Z", "0");
                                                XmlElement axis = doc.CreateElement("Axis");
                                                axis.SetAttribute("X", "0");
                                                axis.SetAttribute("Y", "0");
                                                axis.SetAttribute("Z", "1");
                                                XmlElement reference = doc.CreateElement("Reference");
                                                reference.SetAttribute("X", "1");
                                                reference.SetAttribute("Y", "0");
                                                reference.SetAttribute("Z", "0");
                                                segPositionPipComponent.AppendChild(locationPipCom);
                                                segPositionPipComponent.AppendChild(axis);
                                                segPositionPipComponent.AppendChild(reference);
                                                pipingComponent.AppendChild(segPositionPipComponent);


                                                // Thêm GenericAttributes cho pipingComponent
                                                XmlElement segGenericAttributesPipComponent = doc.CreateElement("GenericAttributes");
                                                segGenericAttributesPipComponent.SetAttribute("Number", "81");
                                                pipingComponent.AppendChild(segGenericAttributesPipComponent);

                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Name", tagNameValv, "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Text", "[NAMN]", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "ActType", "SCVALV", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Area", "0", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Arrive", "1", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Borearray", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Crfarray", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Description", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Distag", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Failcond", "0", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Function", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Gtype", "VALV", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Inprtref", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Ispec", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Leave", "2", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Letter", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Location", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Lock", "false", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Number", "0", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Ouprtref", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Owner", "SCSEGMENT 1 of SCBRANCH /" , "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Position", "E 0mm N 0mm U 0mm", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Prefix", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Pspec", "A3B", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Ptspec", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Sclore", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Scstype", "GLOB", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Scsysf", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Spref", "A3B/VH80", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Suffix", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Tspec", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Type", "SCVALV", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Component Class", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Component Name", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Component Type", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Connection Type", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Description", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Fabrication Category", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source data ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Inside Diameter", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "ISO Symbol", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Manufacturer", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Material Description", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Material", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Model Number", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Nominal Diameter", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Operator Type", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Outside Diameter", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID attribute", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID context", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID context", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID context", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID context", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Source system ID context", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Rating", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Revision", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Instrumentation Link - Schematic", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Specification", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Standard", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Status", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Stocknum", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Supplier", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Tag", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Last Transaction Company", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Last Transaction Date", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Last Transaction Person", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Last Transaction Time", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Last Transaction Type", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Wall Thickness", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Weight", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Desref", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Scaref", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "ABOR", "80", "mm", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "LBOR", "80", "mm", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "CONNECTIONS", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "ConnectionPoints", "3", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "Specification", "", "", "string");
                                                AddGenericAttribute(doc, segGenericAttributesPipComponent, "A3B/VH80", "", "", "string");

                                                // Thêm PersistentID cho pipingComponent
                                                XmlElement segPersistentIdPipComponent = doc.CreateElement("PersistentID");
                                                segPersistentIdPipComponent.SetAttribute("Identifier", "AA7C");
                                                segPersistentIdPipComponent.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                                                pipingComponent.AppendChild(segPersistentIdPipComponent);

                                                for (int i = 0; i < element.SubEntities.Count; i++)
                                                {
                                                    if (element.SubEntities[i].Type == "Line")
                                                    {
                                                        CreateLineElement(doc, pipingComponent, element, i);
                                                    }
                                                    else if (element.SubEntities[i].Type == "Polyline")
                                                    {
                                                        CreatePolylineElement(doc, pipingComponent, element, i);
                                                    }
                                                    else if (element.SubEntities[i].Type == "Circle")
                                                    {
                                                        CreateCircleElement(doc, pipingComponent, element, i);
                                                    }
                                                    else if (element.SubEntities[i].Type == "Arc")
                                                    {
                                                        CreateArcAsPolylineElement(doc, pipingComponent, element, i);
                                                    }
                                                }

                                                // Text
                                                if (!string.IsNullOrEmpty(element.Text))
                                                {
                                                    CreateTagElement(doc, pipingComponent, element);
                                                }

                                                // ConnectionPoint
                                                XmlElement connectionPoints = doc.CreateElement("ConnectionPoints");
                                                connectionPoints.SetAttribute("NumPoints", "3");

                                                for (int i = 0; i < 3; i++)
                                                {
                                                    XmlElement node = doc.CreateElement("Node");
                                                    XmlElement position = doc.CreateElement("Position");
                                                    XmlElement location = doc.CreateElement("Location");
                                                    location.SetAttribute("X", element.xCenter.ToString());
                                                    location.SetAttribute("Y", element.yCenter.ToString());
                                                    location.SetAttribute("Z", "0");
                                                    XmlElement axisPip = doc.CreateElement("Axis");
                                                    axisPip.SetAttribute("X", "0");
                                                    axisPip.SetAttribute("Y", "0");
                                                    axisPip.SetAttribute("Z", "1");
                                                    XmlElement referencePip = doc.CreateElement("Reference");
                                                    referencePip.SetAttribute("X", "1");
                                                    referencePip.SetAttribute("Y", "0");
                                                    referencePip.SetAttribute("Z", "0");
                                                    position.AppendChild(location);
                                                    position.AppendChild(axisPip);
                                                    position.AppendChild(referencePip);
                                                    if (i > 0)
                                                    {
                                                        XmlElement nominalDiameterPip = doc.CreateElement("NominalDiameter");
                                                        nominalDiameterPip.SetAttribute("Value", "20");
                                                        nominalDiameterPip.SetAttribute("Units", "mm");
                                                        node.AppendChild(nominalDiameterPip);
                                                    }
                                                    node.AppendChild(position);

                                                    connectionPoints.AppendChild(node);
                                                }

                                                pipingComponent.AppendChild(connectionPoints);
                                                break;

                                        //    default:
                                        //        string tagNameFitt = "F" + rnd2.Next();

                                        //        XmlElement pipingComponentF = doc.CreateElement("PipingComponent");
                                        //        pipingComponentF.SetAttribute("ID", "XMP_" + rnd2.Next());
                                        //        pipingComponentF.SetAttribute("TagName", tagNameFitt);
                                        //        pipingComponentF.SetAttribute("Specification", "A3B/VH80");
                                        //        pipingComponentF.SetAttribute("ComponentClass", "Reducer");
                                        //        pipingComponentF.SetAttribute("ComponentName", "Reducer." + rnd2.Next());
                                        //        segment.AppendChild(pipingComponentF);

                                        //        // Thêm Extent cho pipingComponent
                                        //        XmlElement segExtentPipComponentF = doc.CreateElement("Extent");
                                        //        XmlElement segMinPipComponentF = doc.CreateElement("Min");
                                        //        segMinPipComponentF.SetAttribute("X", "318");
                                        //        segMinPipComponentF.SetAttribute("Y", "463");
                                        //        XmlElement segMaxPipComponentF = doc.CreateElement("Max");
                                        //        segMaxPipComponentF.SetAttribute("X", "330");
                                        //        segMaxPipComponentF.SetAttribute("Y", "469");
                                        //        segExtentPipComponentF.AppendChild(segMinPipComponentF);
                                        //        segExtentPipComponentF.AppendChild(segMaxPipComponentF);
                                        //        pipingComponentF.AppendChild(segExtentPipComponentF);

                                        //        // Thêm Presentation cho pipingComponent
                                        //        XmlElement segPresentationPipComponentF = doc.CreateElement("Presentation");
                                        //        segPresentationPipComponentF.SetAttribute("Layer", "0");
                                        //        segPresentationPipComponentF.SetAttribute("LineType", "Solid");
                                        //        segPresentationPipComponentF.SetAttribute("LineWeight", "0.25");
                                        //        segPresentationPipComponentF.SetAttribute("R", "0.541176");
                                        //        segPresentationPipComponentF.SetAttribute("G", "0.168627");
                                        //        segPresentationPipComponentF.SetAttribute("B", "0.886275");
                                        //        pipingComponentF.AppendChild(segPresentationPipComponentF);

                                        //        // Position
                                        //        XmlElement segPositionPipComponentF = doc.CreateElement("Position");
                                        //        XmlElement locationPipComF = doc.CreateElement("Location");
                                        //        locationPipComF.SetAttribute("X", element.xCenter.ToString());
                                        //        locationPipComF.SetAttribute("Y", element.yCenter.ToString());
                                        //        locationPipComF.SetAttribute("Z", "0");
                                        //        XmlElement axisF = doc.CreateElement("Axis");
                                        //        axisF.SetAttribute("X", "0");
                                        //        axisF.SetAttribute("Y", "0");
                                        //        axisF.SetAttribute("Z", "1");
                                        //        XmlElement referenceF = doc.CreateElement("Reference");
                                        //        referenceF.SetAttribute("X", "1");
                                        //        referenceF.SetAttribute("Y", "0");
                                        //        referenceF.SetAttribute("Z", "0");
                                        //        segPositionPipComponentF.AppendChild(locationPipComF);
                                        //        segPositionPipComponentF.AppendChild(axisF);
                                        //        segPositionPipComponentF.AppendChild(referenceF);
                                        //        pipingComponentF.AppendChild(segPositionPipComponentF);


                                        //        // Thêm GenericAttributes cho pipingComponent
                                        //        XmlElement segGenericAttributesPipComponentF = doc.CreateElement("GenericAttributes");
                                        //        segGenericAttributesPipComponentF.SetAttribute("Number", "81");
                                        //        pipingComponentF.AppendChild(segGenericAttributesPipComponentF);

                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Name", tagNameFitt, "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Text", "[NAMN]", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "ActType", "SCFITT", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Area", "0", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Arrive", "1", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Borearray", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Crfarray", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Description", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Distag", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Failcond", "0", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Function", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Gtype", "VALV", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Inprtref", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Ispec", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Leave", "2", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Letter", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Location", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Lock", "false", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Number", "0", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Ouprtref", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Owner", "SCSEGMENT 1 of SCBRANCH /", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Position", "E 0mm N 0mm U 0mm", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Prefix", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Pspec", "A3B", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Ptspec", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Sclore", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Scstype", "GLOB", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Scsysf", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Spref", "A3B/VH80", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Suffix", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Tspec", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Type", "SCVALV", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Component Class", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Component Name", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Component Type", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Connection Type", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Description", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Fabrication Category", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source data ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Inside Diameter", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "ISO Symbol", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Manufacturer", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Material Description", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Material", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Model Number", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Nominal Diameter", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Operator Type", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Outside Diameter", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID attribute", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID context", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID context", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID context", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID context", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Source system ID context", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Rating", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Revision", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Instrumentation Link - Schematic", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Specification", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Standard", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Status", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Stocknum", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Supplier", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Tag", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Last Transaction Company", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Last Transaction Date", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Last Transaction Person", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Last Transaction Time", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Last Transaction Type", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Wall Thickness", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Weight", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Desref", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Scaref", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "ABOR", "80", "mm", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "LBOR", "80", "mm", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "CONNECTIONS", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "ConnectionPoints", "3", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "Specification", "", "", "string");
                                        //        AddGenericAttribute(doc, segGenericAttributesPipComponentF, "A3B/VH80", "", "", "string");

                                        //        // Thêm PersistentID cho pipingComponent
                                        //        XmlElement segPersistentIdPipComponentF = doc.CreateElement("PersistentID");
                                        //        segPersistentIdPipComponentF.SetAttribute("Identifier", "AA7C");
                                        //        segPersistentIdPipComponentF.SetAttribute("Context", "T0-01-2012-0001_Page-1");
                                        //        pipingComponentF.AppendChild(segPersistentIdPipComponentF);

                                        //        for (int i = 0; i < element.SubEntities.Count; i++)
                                        //        {
                                        //            if (element.SubEntities[i].Type == "Line")
                                        //            {
                                        //                CreateLineElement(doc, pipingComponentF, element, i);
                                        //            }
                                        //            else if (element.SubEntities[i].Type == "Polyline")
                                        //            {
                                        //                CreatePolylineElement(doc, pipingComponentF, element, i);
                                        //            }
                                        //            else if (element.SubEntities[i].Type == "Circle")
                                        //            {
                                        //                CreateCircleElement(doc, pipingComponentF, element, i);
                                        //            }
                                        //            else if (element.SubEntities[i].Type == "Arc")
                                        //            {
                                        //                CreateArcAsPolylineElement(doc, pipingComponentF, element, i);
                                        //            }
                                        //        }

                                        //        // Text
                                        //        if (!string.IsNullOrEmpty(element.Text))
                                        //        {
                                        //            CreateTagElement(doc, pipingComponentF, element);
                                        //        }

                                        //        // ConnectionPoint
                                        //        XmlElement connectionPointsF = doc.CreateElement("ConnectionPoints");
                                        //        connectionPointsF.SetAttribute("NumPoints", "3");


                                        //        for (int i = 0; i < 3; i++)
                                        //        {
                                        //            XmlElement node = doc.CreateElement("Node");
                                        //            XmlElement position = doc.CreateElement("Position");
                                        //            XmlElement location = doc.CreateElement("Location");
                                        //            location.SetAttribute("X", element.xCenter.ToString());
                                        //            location.SetAttribute("Y", element.yCenter.ToString());
                                        //            location.SetAttribute("Z", "0");
                                        //            XmlElement axisPip = doc.CreateElement("Axis");
                                        //            axisPip.SetAttribute("X", "0");
                                        //            axisPip.SetAttribute("Y", "0");
                                        //            axisPip.SetAttribute("Z", "1");
                                        //            XmlElement referencePip = doc.CreateElement("Reference");
                                        //            referencePip.SetAttribute("X", "1");
                                        //            referencePip.SetAttribute("Y", "0");
                                        //            referencePip.SetAttribute("Z", "0");
                                        //            position.AppendChild(location);
                                        //            position.AppendChild(axisPip);
                                        //            position.AppendChild(referencePip);
                                        //            if (i > 0)
                                        //            {
                                        //                XmlElement nominalDiameterPip = doc.CreateElement("NominalDiameter");
                                        //                nominalDiameterPip.SetAttribute("Value", "20");
                                        //                nominalDiameterPip.SetAttribute("Units", "mm");
                                        //                node.AppendChild(nominalDiameterPip);
                                        //            }
                                        //            node.AppendChild(position);

                                        //            connectionPointsF.AppendChild(node);
                                        //        }

                                        //        pipingComponentF.AppendChild(connectionPointsF);

                                        //break;
                                        //}
                                    }

                                //}

                            }
                        }


                    }
                }



                doc.Save(xmlFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private XmlElement CreateLineElement(XmlDocument doc, XmlElement pipingComponentF, MyJsonElement element, int i)
        {
            // Line
            XmlElement line = doc.CreateElement("Line");
            pipingComponentF.AppendChild(line);

            // Presentation
            XmlElement linePipComponent = doc.CreateElement("Presentation");
            linePipComponent.SetAttribute("Layer", "0");
            linePipComponent.SetAttribute("LineType", "Solid");
            linePipComponent.SetAttribute("LineWeight", "0.8");
            linePipComponent.SetAttribute("R", "0.541176");
            linePipComponent.SetAttribute("G", "0.168627");
            linePipComponent.SetAttribute("B", "0.886275");
            line.AppendChild(linePipComponent);

            // Extent
            XmlElement linePip = doc.CreateElement("Extent");

            XmlElement minlinePip = doc.CreateElement("Min");
            minlinePip.SetAttribute("X", Math.Min(element.SubEntities[i].xP1, element.SubEntities[i].xP2).ToString());
            minlinePip.SetAttribute("Y", Math.Min(element.SubEntities[i].yP1, element.SubEntities[i].yP2).ToString());

            XmlElement maxlinePip = doc.CreateElement("Max");
            maxlinePip.SetAttribute("X", Math.Max(element.SubEntities[i].xP1, element.SubEntities[i].xP2).ToString());
            maxlinePip.SetAttribute("Y", Math.Max(element.SubEntities[i].yP1, element.SubEntities[i].yP2).ToString());

            linePip.AppendChild(minlinePip);
            linePip.AppendChild(maxlinePip);
            line.AppendChild(linePip);

            // Coordinate 1
            XmlElement coordinate1 = doc.CreateElement("Coordinate");
            coordinate1.SetAttribute("X", element.SubEntities[i].xP1.ToString());
            coordinate1.SetAttribute("Y", element.SubEntities[i].yP1.ToString());
            coordinate1.SetAttribute("Z", "0");
            line.AppendChild(coordinate1);

            // Coordinate 2
            XmlElement coordinate2 = doc.CreateElement("Coordinate");
            coordinate2.SetAttribute("X", element.SubEntities[i].xP2.ToString());
            coordinate2.SetAttribute("Y", element.SubEntities[i].yP2.ToString());
            coordinate2.SetAttribute("Z", "0");
            line.AppendChild(coordinate2);

            return line;
        }

        private XmlElement CreatePolylineElement(XmlDocument doc, XmlElement polylineElement, MyJsonElement element, int i)
        {
            var vertices = new List<VertexDto>(element.SubEntities[i].Vertices);

            bool isClosed = element.SubEntities[i].IsClosedPolyline;

            // Nếu điểm đầu != điểm cuối → thêm điểm đầu vào cuối
            if (isClosed && vertices.Count > 1 &&
                (vertices.First().X != vertices.Last().X || vertices.First().Y != vertices.Last().Y))
            {
                vertices.Add(new VertexDto
                {
                    X = vertices.First().X,
                    Y = vertices.First().Y
                });
            }

            XmlElement polyline = doc.CreateElement("PolyLine");
            polyline.SetAttribute("NumPoints", vertices.Count.ToString());
            polylineElement.AppendChild(polyline);

            // Presentation
            XmlElement polylinePipComponent = doc.CreateElement("Presentation");
            polylinePipComponent.SetAttribute("Layer", "0");
            polylinePipComponent.SetAttribute("LineType", "Solid");
            polylinePipComponent.SetAttribute("LineWeight", "0.8");
            polylinePipComponent.SetAttribute("R", "0.541176");
            polylinePipComponent.SetAttribute("G", "0.168627");
            polylinePipComponent.SetAttribute("B", "0.886275");
            polyline.AppendChild(polylinePipComponent);

            // Extent
            XmlElement extentPolylinePip = doc.CreateElement("Extent");
            XmlElement minPolylinePip = doc.CreateElement("Min");
            minPolylinePip.SetAttribute("X", vertices.Min(v => v.X).ToString());
            minPolylinePip.SetAttribute("Y", vertices.Min(v => v.Y).ToString());
            XmlElement maxPolylinePip = doc.CreateElement("Max");
            maxPolylinePip.SetAttribute("X", vertices.Max(v => v.X).ToString());
            maxPolylinePip.SetAttribute("Y", vertices.Max(v => v.Y).ToString());
            extentPolylinePip.AppendChild(minPolylinePip);
            extentPolylinePip.AppendChild(maxPolylinePip);
            polyline.AppendChild(extentPolylinePip);

            // Coordinates
            foreach (var vertex in vertices)
            {
                XmlElement coordinate = doc.CreateElement("Coordinate");
                coordinate.SetAttribute("X", vertex.X.ToString());
                coordinate.SetAttribute("Y", vertex.Y.ToString());
                coordinate.SetAttribute("Z", "0");
                polyline.AppendChild(coordinate);
            }

            return polyline;
        }

        private XmlElement CreateCircleElement(XmlDocument doc, XmlElement circleElement, MyJsonElement element, int i)
        {
            // Circle
            XmlElement circle = doc.CreateElement("Circle");
            circle.SetAttribute("Radius", element.SubEntities[i].radius.ToString());
            //circle.SetAttribute("Filled", "None");
            circleElement.AppendChild(circle);

            // Thêm Presentation cho pipingComponent
            XmlElement polylinePipComponent = doc.CreateElement("Presentation");
            polylinePipComponent.SetAttribute("Layer", "0");
            polylinePipComponent.SetAttribute("LineType", "Solid");
            polylinePipComponent.SetAttribute("LineWeight", "0.8");
            polylinePipComponent.SetAttribute("Color", "0");
            polylinePipComponent.SetAttribute("R", "0.541176");
            polylinePipComponent.SetAttribute("G", "0.168627");
            polylinePipComponent.SetAttribute("B", "0.886275");
            circle.AppendChild(polylinePipComponent);

            // Extent
            XmlElement extentPolylinePip = doc.CreateElement("Extent");
            XmlElement minPolylinePip = doc.CreateElement("Min");
            minPolylinePip.SetAttribute("X", (element.SubEntities[i].xCenter - element.SubEntities[i].radius).ToString());
            minPolylinePip.SetAttribute("Y", (element.SubEntities[i].yCenter - element.SubEntities[i].radius).ToString());
            XmlElement maxPolylinePip = doc.CreateElement("Max");
            maxPolylinePip.SetAttribute("X", (element.SubEntities[i].xCenter + element.SubEntities[i].radius).ToString());
            maxPolylinePip.SetAttribute("Y", (element.SubEntities[i].xCenter + element.SubEntities[i].radius).ToString());
            extentPolylinePip.AppendChild(minPolylinePip);
            extentPolylinePip.AppendChild(maxPolylinePip);
            circle.AppendChild(extentPolylinePip);

            // Position
            XmlElement position = doc.CreateElement("Position");
            XmlElement location = doc.CreateElement("Location");
            location.SetAttribute("X", element.SubEntities[i].xCenter.ToString());
            location.SetAttribute("Y", element.SubEntities[i].yCenter.ToString());
            location.SetAttribute("Z", "0");
            XmlElement axisPip = doc.CreateElement("Axis");
            axisPip.SetAttribute("X", "0");
            axisPip.SetAttribute("Y", "0");
            axisPip.SetAttribute("Z", "1");
            XmlElement referencePip = doc.CreateElement("Reference");
            referencePip.SetAttribute("X", "1");
            referencePip.SetAttribute("Y", "0");
            referencePip.SetAttribute("Z", "0");
            position.AppendChild(location);
            position.AppendChild(axisPip);
            position.AppendChild(referencePip);
            circle.AppendChild(position);

            return circle;
        }

        private XmlElement CreateArcAsPolylineElement(XmlDocument doc, XmlElement arcElement, MyJsonElement element, int i)
        {
            int segments = 32;

            double a1 = element.SubEntities[i].StartAngle % 360.0; 
            if(a1 < 0) a1 += 360.0; 
            double a2 = element.SubEntities[i].EndAngle % 360.0;
            if (a2 < 0) a2 += 360.0;

            double sweep = a2 - a1;
            if (sweep < 0) sweep += 360.0;

            var pts = new List<(double X, double Y)>();
            for (int k = 0; k <= segments; k++)
            {
                double angle = (a1 + sweep * k / segments) * Math.PI / 180.0; // Chuyển đổi độ sang radian
                double x = element.SubEntities[i].xCenter + element.SubEntities[i].radius * Math.Cos(angle);
                double y = element.SubEntities[i].yCenter + element.SubEntities[i].radius * Math.Sin(angle);
                pts.Add((x, y));
            }

            // extent 
            double minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X);
            double minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);


            // Polyline
            XmlElement arc = doc.CreateElement("PolyLine");
            arc.SetAttribute("NumPoints", (pts.Count).ToString());
            arcElement.AppendChild(arc);

            // Thêm Presentation cho pipingComponent
            XmlElement polylinePipComponent = doc.CreateElement("Presentation");
            polylinePipComponent.SetAttribute("Layer", "0");
            polylinePipComponent.SetAttribute("LineType", "Solid");

            polylinePipComponent.SetAttribute("LineWeight", "0.8");
            polylinePipComponent.SetAttribute("R", "0.541176");
            polylinePipComponent.SetAttribute("G", "0.168627");
            polylinePipComponent.SetAttribute("B", "0.886275");
            arc.AppendChild(polylinePipComponent);

            // Extent
            XmlElement extentPolylinePip = doc.CreateElement("Extent");
            XmlElement minPolylinePip = doc.CreateElement("Min");
            minPolylinePip.SetAttribute("X", minX.ToString());
            minPolylinePip.SetAttribute("Y", minY.ToString());
            XmlElement maxPolylinePip = doc.CreateElement("Max");
            maxPolylinePip.SetAttribute("X", maxX.ToString());
            maxPolylinePip.SetAttribute("Y", maxY.ToString());
            extentPolylinePip.AppendChild(minPolylinePip);
            extentPolylinePip.AppendChild(maxPolylinePip);
            arc.AppendChild(extentPolylinePip);

            // Coordinates
            foreach (var p in pts)
            {
                XmlElement coordinate = doc.CreateElement("Coordinate");
                coordinate.SetAttribute("X", p.X.ToString());
                coordinate.SetAttribute("Y", p.Y.ToString());
                coordinate.SetAttribute("Z", "0");
                arc.AppendChild(coordinate);
            }

            return arc;
        }

        private XmlElement CreateTagElement(XmlDocument doc, XmlElement pipingComponentF, MyJsonElement element)
        {
            // Text
            XmlElement text = doc.CreateElement("Text");
            XmlAttribute attr = doc.CreateAttribute("String");
            attr.InnerXml = element.Text;
            text.Attributes.Append(attr);
            text.SetAttribute("Font", "ARIAL");
            text.SetAttribute("Justification", "CenterCenter");
            text.SetAttribute("Width", "27.716041");
            text.SetAttribute("Height", "2.577517");
            text.SetAttribute("TextAngle", $"{element.rotation}");
            text.SetAttribute("SlantAngle", "0");
            pipingComponentF.AppendChild(text);

            // Presentation
            XmlElement linePipComponent = doc.CreateElement("Presentation");
            linePipComponent.SetAttribute("Layer", "0");
            linePipComponent.SetAttribute("LineType", "Solid");
            linePipComponent.SetAttribute("LineWeight", "0.25");
            linePipComponent.SetAttribute("R", "1");
            linePipComponent.SetAttribute("G", "1");
            linePipComponent.SetAttribute("B", "1");
            text.AppendChild(linePipComponent);

            // Extent
            XmlElement linePip = doc.CreateElement("Extent");

            XmlElement minlinePip = doc.CreateElement("Min");
            minlinePip.SetAttribute("X", Math.Min(element.xP1, element.xP2).ToString());
            minlinePip.SetAttribute("Y", Math.Min(element.yP1, element.yP2).ToString());

            XmlElement maxlinePip = doc.CreateElement("Max");
            maxlinePip.SetAttribute("X", Math.Max(element.xP1, element.xP2).ToString());
            maxlinePip.SetAttribute("Y", Math.Max(element.yP1, element.yP2).ToString());

            linePip.AppendChild(minlinePip);
            linePip.AppendChild(maxlinePip);
            text.AppendChild(linePip);

            // Position
            float xCenterTransform = element.rotation > 0 ? element.xCenter + 6 : element.xCenter;
            float yCenterTransform = element.yCenter;

            string localName = element.LocalName.ToLower();

            if (!localName.Contains("fileld_in")
                && !localName.Contains("interl")
                && !localName.Equals("tag_block")) 
            {
                yCenterTransform -= 5;
            }

            XmlElement position = doc.CreateElement("Position");
            XmlElement location = doc.CreateElement("Location");
            location.SetAttribute("X", $"{xCenterTransform}");
            location.SetAttribute("Y", $"{yCenterTransform}");
            location.SetAttribute("Z", "0");
            XmlElement axisPip = doc.CreateElement("Axis");
            axisPip.SetAttribute("X", "0");
            axisPip.SetAttribute("Y", "0");
            axisPip.SetAttribute("Z", "1");
            XmlElement referencePip = doc.CreateElement("Reference");
            referencePip.SetAttribute("X", "1");
            referencePip.SetAttribute("Y", "0");
            referencePip.SetAttribute("Z", "0");
            position.AppendChild(location);
            position.AppendChild(axisPip);
            position.AppendChild(referencePip);
            text.AppendChild(position);

            return text;
        }

        private void AddGenericAttribute(XmlDocument doc, XmlElement parent, string name, string value, string units, string format)
        {
            XmlElement attr = doc.CreateElement("GenericAttribute");
            attr.SetAttribute("Name", name);
            attr.SetAttribute("Value", value);
            if (!string.IsNullOrEmpty(units))
                attr.SetAttribute("Units", units);
            attr.SetAttribute("Format", format);
            parent.AppendChild(attr);
        }
    }

    public class MyJsonElement
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
        public int rotation { get; set; }
        public float radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public bool IsClosedPolyline { get; set; } = false; 
        public List<MyJsonElement> SubEntities { get; set; }
        public List<VertexDto> Vertices { get; set; }
    }

    public class ExportResult
    {
        public List<Dictionary<string, Dictionary<string, List<MyJsonElement>>>> Branches { get; set; }
        public List<EquipmentWrapper> Equipments { get; set; }
        public List<MyJsonElement> InstrumentLines { get; set; }
    }

    public class EquipmentWrapper
    {
        public MyJsonElement Equipment { get; set; }
        public List<MyJsonElement> Nozzle { get; set; }
    }

    public class RandomTwoDigit
    {
        private List<int> numbers;
        private int index = 0;

        public RandomTwoDigit()
        {
            Random rand = new Random();
            numbers = Enumerable.Range(100, 9999)
                                .OrderBy(x => rand.Next())
                                .ToList();
        }

        public int Next()
        {
            if (index >= numbers.Count) throw new InvalidOperationException("Hết số!");
            return numbers[index++];
        }
    }
}
