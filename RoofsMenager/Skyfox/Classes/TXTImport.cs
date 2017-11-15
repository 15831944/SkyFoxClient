using System;
using System.Collections;
using System.Drawing;
using System.IO;
using CADImport;
using Skyfox.Classes;

namespace CADImportNetDemos.CADImportDemo.TXTImport
{	
	#region Help
	/// <summary>
	/// Represents a class which imports entities to TXT file
	/// </summary>
	#endregion
	public class TXTImporter
	{
		private CADImage image;
		private CADIterate cadParams;
		private ArrayList textFile;

		#region Help
		/// <summary>
		/// Creates an instanse of <see cref="CADImportNetDemos.CADImportDemo.TXTImport.TXTImporter"/>TXTImporter class
		/// </summary>
		#endregion
		public TXTImporter()
		{
			textFile = new ArrayList();
		}

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

        #region Help
        /// <summary>
        /// Displays a limited list of entities and their properties from a loaded CADImage
        /// </summary>
        /// <param name="entCount">A limit of entities number</param>
        #endregion
        public string EntStructure(int entCount)
        {
            string text = "";
            if (image != null)
            {
                textFile.Clear();
                cadParams = new CADIterate();
                cadParams.matrix = new CADMatrix();
                cadParams.matrix[0, 0] = 1;
                cadParams.matrix[1, 1] = 1;
                cadParams.matrix[2, 2] = 1;
                if (image.CurrentLayout.Entities.Count < entCount)
                    entCount = image.CurrentLayout.Entities.Count;
                for (int i = 0; i < entCount; i++)
                {
                    ReadCADEntities(image.CurrentLayout.Entities[i]);
                    for (int j = 0; j < textFile.Count; j++)
                        text += (string)textFile[j] + "\r\n";
                    text += "\r\n";
                    textFile.Clear();
                }
            }
            return text;
        }

		#region Help
		/// <summary>
		/// Creates a file with specified name which represents a list of entities and their properties of loaded CADImage
		/// </summary>
		/// <param name="aFileName">Name of TXT file to save</param>
		#endregion
		public void CreateTXTRecords(string aFileName)
		{
			if(image == null) return;
			textFile.Clear();
			StreamWriter wr1 = File.CreateText(aFileName);
			cadParams = new CADIterate();
			cadParams.matrix = new CADMatrix();
			cadParams.matrix[0, 0] = 1;
			cadParams.matrix[1, 1] = 1;
			cadParams.matrix[2, 2] = 1;
			image.Converter.AutoInsert = true;			// to get all the elements inside of inserts
			image.Converter.Iterate(new CADEntityProc(ReadCADEntities),null, cadParams);
			for(int i = 0; i < textFile.Count; i++)
				wr1.WriteLine((string)textFile[i]);
			wr1.Close();
		}
		
		private bool ReadCADEntities(CADEntity entity)
		{												
			string tmp = CADImport.FaceModule.CADImportFace.SetName(entity);
			textFile.Add(string.Format("ClassName = CADImport.CAD{0}; Entity name = {1}", tmp, tmp));
			CADConst.DoScale2D(ref cadParams);			// calculates 2d scale and rotation			
			CADLayer l = CADConst.EntLayer(entity, cadParams.insert);
			if(l != null) 
				textFile.Add(string.Format("Layer = {0}", l.Name));			
			Color c = CADConst.EntColor(entity, cadParams.insert);
			if(c == CADConst.clNone)
				textFile.Add("Color = black/white");
			else
				textFile.Add("Color = " + c.Name);						
			switch(entity.EntType)
			{
				case EntityType.Line:
				case EntityType.Point:
					ImportLine(entity as CADLine);
					break;
				case EntityType.Solid:
					ImportSolid(entity as CADSolid);
					break;
				case EntityType.Ellipse:
					ImportEllipse(entity as CADEllipse);
					break;
				case EntityType.Arc:
					ImportArc(entity as CADArc);
					break;
				case EntityType.Circle:
					ImportCircle(entity as CADCircle);
					break;
				case EntityType.Spline:
				case EntityType.Leader:
					ImportSpline(entity as CADSpline);
					break;
				case EntityType.Polyline:
				case EntityType.LWPolyline:
					ImportPolyLine(entity as CADPolyLine);
					break;				
				case EntityType.Text:
				case EntityType.Attdef:
				case EntityType.Attrib:
					ImportText(entity as CADText);			
					break;
				case EntityType.Hatch:				
					ImportHatch(entity as CADHatch);
					break;
			}
            return true;
		}
		
		private void ImportHatch(CADHatch sender)
		{
			CADHatchStyle style; DPoint p;		
			textFile.Add(string.Format("HatchName = {0}", sender.HatchName));			
			style = sender.HatchStyle;
			textFile.Add(string.Format("HatchStyle = {0}",  ApplicationConstants.hatchStyle[(int)style]));			
			textFile.Add(string.Format("Color = {0}", sender.Color.Name));			
			p = cadParams.matrix.PtXMat(sender.Elevation); 
			textFile.Add(string.Format("Elevation: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));							
			p = cadParams.matrix.PtXMat(sender.Extrusion); 
			textFile.Add(string.Format("Extrusion: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));					
		}

		private void ImportSolid(CADSolid sender)
		{
			DPoint p = cadParams.matrix.PtXMat(sender.Point);
			textFile.Add(string.Format("P1: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));			
			p = cadParams.matrix.PtXMat(sender.Point1);
			textFile.Add(string.Format("P2: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));						
			p = cadParams.matrix.PtXMat(sender.Point3);
			textFile.Add(string.Format("P3: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));						
			p = cadParams.matrix.PtXMat(sender.Point2);
			textFile.Add(string.Format("P4: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));									
		}

		private void ImportLine(CADLine sender)
		{
			DPoint p = cadParams.matrix.PtXMat(sender.Point);
			textFile.Add(string.Format("Begin point: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));
			p = cadParams.matrix.PtXMat(sender.Point1);
			textFile.Add(string.Format("End point: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));				
		}

		private void ImportEllipse(CADEllipse sender)
		{
			DPoint p = cadParams.matrix.PtXMat(sender.Point);
			textFile.Add(string.Format("Center of Ellipse: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));
			textFile.Add(string.Format("Start Angle: {0}", sender.StartAngle));
			textFile.Add(string.Format("End Angle: {0}", sender.EndAngle));	
			textFile.Add(string.Format("Rx: {0}", sender.Radius));			
			textFile.Add(string.Format("Ry: {0}", sender.Radius * sender.Ratio));						
		}
		
		private void ImportArc(CADArc sender)
		{
			DPoint p = cadParams.matrix.PtXMat(sender.Point);
			textFile.Add(string.Format("Center of RoundArc: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));
			textFile.Add(string.Format("Start Angle: {0}", sender.StartAngle));
			textFile.Add(string.Format("End Angle: {0}", sender.EndAngle));	
			textFile.Add(string.Format("Rx: {0}", sender.Radius));						
		}

		private void ImportCircle(CADCircle sender)
		{
			DPoint p = cadParams.matrix.PtXMat(sender.Point);
			textFile.Add(string.Format("Center of Circle: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));
			textFile.Add(string.Format("Rx: {0}", sender.Radius));
		}

		private void ImportPolyLine(CADPolyLine sender)
		{
			DPoint p; CADVertex vertex;			
			textFile.Add("Points of PolyLine: ");								
			for(int i = 0; i < sender.Count; i++)
			{								
				vertex = ((CADVertex)sender.Entities[i]);
				p = cadParams.matrix.PtXMat(vertex.Point);
				textFile.Add(string.Format("P{0}: X = {1} Y = {2} Z = {3}", i + 1, p.X, p.Y, p.Z));				
			}						
		}			

		private void ImportText(CADText sender)
		{			
			CADConst.DoScale2D(ref cadParams);			
			DPoint tmp = sender.StartPoint;			
			DPoint p = cadParams.matrix.PtXMat(tmp);
			textFile.Add(string.Format("Start point: X = {0} Y = {1} Z = {2}", p.X, p.Y, p.Z));	
			textFile.Add(string.Format("Text: {0}", sender.Text));
			textFile.Add(string.Format("Angle: {0}", sender.Rotation + cadParams.angle));															
			DPoint d1 = new DPoint(sender.Box.left, sender.Box.top, sender.Box.z1);
			DPoint d2 = new DPoint(sender.Box.right, sender.Box.bottom, sender.Box.z2);
			Point p1 = image.GDIPainter.GetPoint(d1);
            Point p2 = image.GDIPainter.GetPoint(d2);
            Point p3 = image.GDIPainter.GetPoint(p);
			int left = p3.X;
			int top = p3.Y;
			int width = (p2.X - p1.X);
			int height = (p2.Y - p1.Y);
			textFile.Add(string.Format("Box size: Left = {0} Top = {1} Widht = {2} Height = {3}", p.X, p.Y, Math.Abs(sender.Box.Width), Math.Abs(sender.Box.Height)));				
		}

		private void ImportSpline(CADSpline sender)
		{
			DPoint p;			
			int cn = sender.FitCount;
			if(cn > 0)
			{
				textFile.Add("Fit points of Spline:");
				for(int i = 0; i < cn; i++)
				{					
					p = cadParams.matrix.PtXMat((DPoint)sender.Fit[i]);
					textFile.Add(string.Format("P{0}: X = {1} Y = {2} Z = {3}", i + 1, p.X, p.Y, p.Z));
				}
			}
			cn = sender.ControlCount;
			if(cn > 0)
			{
				textFile.Add("Control points of Spline:");				
				for(int i = 0; i < cn; i++)
				{					
					p = cadParams.matrix.PtXMat((DPoint)sender.Controls[i]);
					textFile.Add(string.Format("P{0}: X = {1} Y = {2} Z = {3}", i + 1, p.X, p.Y, p.Z));
				}
			}			                                     
		}		
	}
}
