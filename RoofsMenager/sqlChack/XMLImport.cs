using System;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;
using CADImport;
using CADImportNetDemos.CADImportDemo;
using sqlChack;

namespace CADImportNetDemos.CADImportDemo.XMLImport
{	
	#region Help
	/// <summary>
	/// Represents a class which imports entities to TXT file
	/// </summary>
	#endregion
	public class XMLImporter
	{
		private XmlDocument doc;
		private XmlNode nodeEntity;
		private XmlNode nodeEntities;
		private XmlNode nodeParam;
		private XmlNode baseNode;
		private XmlAttribute attr;		
		private CADImage image;
		private CADIterate cadParams;

		#region Help
		/// <summary>
		/// Gets or sets <see cref="CADImport.CADImage">CADImage</see> object where new entities are added
		/// </summary>
		#endregion
		public CADImage Image
		{
			get
			{
				return this.image;
			}
			set
			{
				this.image = value;
			}
		}

		private void CreateXmlDocument()
		{
			if(doc != null)
			{				
				doc.RemoveAll();
				doc = null;
			}
			doc = new XmlDocument();			
		}

		#region Help
		/// <summary>
		/// Creates a file with specified name which represents a list of entities and their properties of loaded CADImage
		/// </summary>
		/// <param name="fileName">Name of XML file to save</param>
		/// <param name="sourceFile">Name of loaded CADImage file</param>
		#endregion
		public void CreateXMLRecords(string fileName, string sourceFile)
		{
			if(image == null) 
				return;				
			CreateXmlDocument();
			cadParams = new CADIterate();
			cadParams.matrix = new CADMatrix();
			cadParams.matrix[0, 0] = 1;
			cadParams.matrix[1, 1] = 1;
			cadParams.matrix[2, 2] = 1;
			image.Converter.AutoInsert = true; 			
			CreateXmlDocument();
			baseNode = doc.CreateElement("File");			
			attr = doc.CreateAttribute("FileName");		
			attr.Value = sourceFile;
			baseNode.Attributes.Append(attr);			
			nodeEntities = doc.CreateElement("Entities");			
			baseNode.AppendChild(nodeEntities);			
			image.Converter.Iterate(new CADEntityProc(ReadCADEntitiesParamsToXML),null, cadParams);		
			doc.AppendChild(baseNode);
			doc.Save(fileName);			
			doc.RemoveAll();
		}		
		
		private bool ReadCADEntitiesParamsToXML(CADEntity entity)
		{								
			string str = CADImport.FaceModule.CADImportFace.SetName(entity);			
			Color cl;
			double val;
			nodeEntity = doc.CreateElement("Entity");			
			attr = doc.CreateAttribute("ClassName");		
			attr.Value = string.Format("CADImport.CAD{0}", str);
			nodeEntity.Attributes.Append(attr);
			attr = doc.CreateAttribute("EntityName");		
			attr.Value = str;
			nodeEntity.Attributes.Append(attr);
			cl = CADConst.EntColor(entity, cadParams.insert);
			attr = doc.CreateAttribute("Color");		
			attr.Value = ApplicationConstants.GetColor(cl);
			nodeEntity.Attributes.Append(attr);
			attr = doc.CreateAttribute("LineWeight");				
			val =  CADConst.EntLineWeight(entity, cadParams.insert);
			if(val == 0)
				val = image.NullWidth;
			attr.Value = val.ToString();
			nodeEntity.Attributes.Append(attr);
			CADConst.DoScale2D(ref cadParams);		// calculates 2d scale and rotation
			CADLayer layer = CADConst.EntLayer(entity, cadParams.insert);
			nodeParam = doc.CreateElement("Layer");
			str = "0";
			cl = CADConst.clNone;
			if(layer != null)			
			{
				str = layer.Name;			
				cl = layer.Color;
			}
			attr = doc.CreateAttribute("Name");		
			attr.Value = str;
			nodeParam.Attributes.Append(attr);
			attr = doc.CreateAttribute("Color");		
			attr.Value = ApplicationConstants.GetColor(cl);
			nodeParam.Attributes.Append(attr);
			nodeEntity.AppendChild(nodeParam);						
			switch(entity.EntType)
			{
				case EntityType.Line:
                    ImportLineXML(entity as CADLine);
                    break;
				case EntityType.Point:
                    ImportPointXML(entity as CADPoint);
                    break;
				case EntityType.Solid:
					ImportSolidXML(entity as CADSolid);
					break;
				case EntityType.Ellipse:
					ImportEllipseXML(entity as CADEllipse);
					break;
				case EntityType.Arc:
					ImportArcXML(entity as CADArc);
					break;
				case EntityType.Circle:
					ImportCircleXML(entity as CADCircle);
					break;
				case EntityType.Spline:
				case EntityType.Leader:
					ImportSplineXML(entity as CADSpline);
					break;
				case EntityType.Polyline:
				case EntityType.LWPolyline:
					ImportPolyLineXML(entity as CADPolyLine);
					break;
				case EntityType.Attdef:
				case EntityType.Attrib:
					ImportAttdefXML(entity as CADAttdef);
					break;
				case EntityType.Text:
					ImportTextXML(entity as CADText);			
					break;
				case EntityType.Hatch:				
					ImportHatchXML(entity as CADHatch);
					break;
			}
			nodeEntities.AppendChild(nodeEntity);
            return true;							
		}

		private void ImportHatchXML(CADHatch hatch)
		{
			nodeParam = doc.CreateElement("HatchName");		
			AddValueNode(hatch.HatchName);			
			nodeEntity.AppendChild(nodeParam);
			CADHatchStyle style = hatch.HatchStyle;
			nodeParam = doc.CreateElement("HatchStyle");		
			AddValueNode(ApplicationConstants.hatchStyle[(int)style]);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Elevation");			
			this.AddRealPointNode(hatch.Elevation);			
			nodeEntity.AppendChild(nodeParam);						
			nodeParam = doc.CreateElement("Extrusion");			
			this.AddRealPointNode(hatch.Extrusion);			
			nodeEntity.AppendChild(nodeParam);												
		}

		private void ImportTextXML(CADText text)
		{			
			CADConst.DoScale2D(ref cadParams);			
			nodeParam = doc.CreateElement("StartPoint");			
			this.AddRealPointNode(text.StartPoint);			
			nodeEntity.AppendChild(nodeParam);						
			nodeParam = doc.CreateElement("Text");		
			AddValueNode(text.Text);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Angle");		
			AddValueNode(text.Rotation.ToString());			
			nodeEntity.AppendChild(nodeParam);																						
		}

		private void ImportAttdefXML(CADAttdef attdef)
		{	
			nodeParam = doc.CreateElement("StartPoint");			
			this.AddRealPointNode(attdef.StartPoint);			
			nodeEntity.AppendChild(nodeParam);			
			nodeParam = doc.CreateElement("Text");		
			AddValueNode(attdef.Text);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Angle");		
			AddValueNode(attdef.Rotation.ToString());			
			nodeEntity.AppendChild(nodeParam);															
		}

		private void ImportPolyLineXML(CADPolyLine polyline)
		{
			XmlNode vertexNode;
			CADVertex vertex;			
			for(int i = 0; i < polyline.Count; i++)
			{
				vertexNode = doc.CreateElement(string.Format("Vertex{0}", i));		
				vertex = ((CADVertex)polyline.Entities[i]);
				nodeParam = doc.CreateElement("Bulge");		
				AddValueNode(vertex.Bulge.ToString());			
				vertexNode.AppendChild(nodeParam);
				nodeParam = doc.CreateElement("Point");			
				this.AddRealPointNode(vertex.Point);			
				vertexNode.AppendChild(nodeParam);
				nodeEntity.AppendChild(vertexNode);							
			}			
		}

		private void ImportSplineXML(CADSpline spline)
		{
			nodeParam = doc.CreateElement("FitPointsCount");		
			AddValueNode(spline.FitCount.ToString());			
			nodeEntity.AppendChild(nodeParam);	
			int cn = spline.FitCount;
			for(int i = 0; i < cn; i++)	
			{
				nodeParam = doc.CreateElement(string.Format("FitPoint{0}", i));			
				this.AddRealPointNode((DPoint)spline.Fit[i]);			
				nodeEntity.AppendChild(nodeParam);			
			}		
			cn = spline.ControlCount;
			for(int i = 0; i < cn; i++)	
			{
				nodeParam = doc.CreateElement(string.Format("ControlPoint{0}", i));			
				this.AddRealPointNode((DPoint)spline.Controls[i]);			
				nodeEntity.AppendChild(nodeParam);			
			}					                                 
		}

		private void ImportCircleXML(CADCircle circle)
		{
			nodeParam = doc.CreateElement("Point");			
			this.AddRealPointNode(circle.Point);			
			nodeEntity.AppendChild(nodeParam);			
		}

		private void ImportArcXML(CADArc arc)
		{
			nodeParam = doc.CreateElement("Point");			
			this.AddRealPointNode(arc.Point);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("StartAngle");		
			AddValueNode(arc.StartAngle.ToString());			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("EndAngle");		
			AddValueNode(arc.EndAngle.ToString());			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Radius");		
			AddValueNode(arc.Radius.ToString());			
			nodeEntity.AppendChild(nodeParam);						
		}

		private void ImportEllipseXML(CADEllipse ellipse)
		{
			nodeParam = doc.CreateElement("Point");			
			this.AddRealPointNode(ellipse.Point);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("StartAngle");		
			AddValueNode(ellipse.StartAngle.ToString());			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("EndAngle");		
			AddValueNode(ellipse.EndAngle.ToString());			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Rx");		
			AddValueNode(ellipse.Radius.ToString());			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Ry");		
			AddValueNode((ellipse.Radius * ellipse.Ratio).ToString());			
			nodeEntity.AppendChild(nodeParam);						
		}

		private void ImportSolidXML(CADSolid solid)
		{
			nodeParam = doc.CreateElement("Point");			
			this.AddRealPointNode(solid.Point);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Point1");			
			this.AddRealPointNode(solid.Point1);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Point2");			
			this.AddRealPointNode(solid.Point2);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("Point3");			
			this.AddRealPointNode(solid.Point3);			
			nodeEntity.AppendChild(nodeParam);
		}

		private void AddValueNode(string val)
		{
			attr = doc.CreateAttribute("Value");		
			attr.Value = val;		
			nodeParam.Attributes.Append(attr);
		}

		private void AddRealPointNode(DPoint val)
		{			
			DPoint pt = cadParams.matrix.PtXMat(val);			
			attr = doc.CreateAttribute("X");		
			attr.Value = pt.X.ToString();		
			nodeParam.Attributes.Append(attr);
			attr = doc.CreateAttribute("Y");		
			attr.Value = pt.Y.ToString();		
			nodeParam.Attributes.Append(attr);
			attr = doc.CreateAttribute("Z");		
			attr.Value = pt.Z.ToString();		
			nodeParam.Attributes.Append(attr);
		}		

		private void AddScreenPointNode(Point val)
		{						
			attr = doc.CreateAttribute("X");		
			attr.Value = val.X.ToString();		
			nodeParam.Attributes.Append(attr);
			attr = doc.CreateAttribute("Y");		
			attr.Value = val.Y.ToString();					
			nodeParam.Attributes.Append(attr);
		}		

		private void ImportLineXML(CADLine line)
		{
			nodeParam = doc.CreateElement("StartPoint");			
			this.AddRealPointNode(line.Point);			
			nodeEntity.AppendChild(nodeParam);
			nodeParam = doc.CreateElement("EndPoint");			
			this.AddRealPointNode(line.Point1);			
			nodeEntity.AppendChild(nodeParam);						
		}

        private void ImportPointXML(CADPoint point)
        {
            nodeParam = doc.CreateElement("Point");
            this.AddRealPointNode(point.Point);
            nodeEntity.AppendChild(nodeParam);
        }
	}
}
