﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Numerics;

using static FTGridBuilding.Settings;

namespace FTGridBuilding.FlowTileUtils
{   
    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    public class FlowTile
    {  
        public Flux Flux;
        public CornerVelocities CornerVelocities;
        public int GridSize;
        private int StreamFunctionGridSize;
        
        /// <summary>
        /// The velocity grid is GridSize * GridSize large and holds a velocity in every point. The index x=0, y=0
        /// represents the bottom left corner and the first index represents y-coordinates, i.e rows.
        /// </summary>
        private Vector2[,] VelocityGrid;
        
        
        /// <summary>
        /// The StreamFunctionGrid is only for internal use to calculate the velocites. The size of the StreamFunctionGrid
        /// is (GridSize + 1) * (GridSize + 1) since it represents points between the points of Velocity. A point at index [y, x]
        /// in the StreamFunctionGrid represtens a physical point at (x-0.5, y-0.5).
        /// </summary>
        private float[,] StreamFunctionGrid;

        /// <summary>
        /// The control points are the points given to the Bezier interpolation function.
        /// </summary>
        private Vector3[,] ControlPoints;

        public FlowTile(int gridSize, Flux flux, CornerVelocities cornerVelocities)
        {
            /*
            if (flux.LeftEdge + flux.BottomEdge != flux.RightEdge + flux.TopEdge)
            {
                throw new ArgumentException("The net flux in and out a flow tile must be zero");
            }
            */
            GridSize = gridSize;
            StreamFunctionGridSize = GridSize + 1;
            StreamFunctionGrid = new float[StreamFunctionGridSize, StreamFunctionGridSize];
            Flux = flux;
            CornerVelocities = cornerVelocities;
            VelocityGrid = new Vector2[GridSize, GridSize];
            ControlPoints = new Vector3[4,4];
            GenerateStreamFunctionGrid();
            GenerateVelocityGrid();
        }

        public FlowTile(int gridSize)
        {
            Flux = new Flux(0, 0, 0, 0);
            CornerVelocities = new CornerVelocities(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) );
            GridSize = gridSize;
            StreamFunctionGridSize = GridSize + 1;
            StreamFunctionGrid = new float[StreamFunctionGridSize, StreamFunctionGridSize];
            VelocityGrid = new Vector2[GridSize, GridSize];
            ControlPoints = new Vector3[4, 4];
            GenerateStreamFunctionGrid();
            GenerateVelocityGrid();
                    
        }

        private void GenerateStreamFunctionGrid()
        {
            //Biggest index in VelocityGrid
            int max = GridSize - 1;
            SetControlPoint(x : -0.5, y : -0.5 , value: 0.0);
            SetControlPoint(x : max + 0.5, y : -0.5, value: Flux.BottomEdge);
            SetControlPoint(x : -0.5, y : max + 0.5, value: -Flux.LeftEdge);
            SetControlPoint(x : max + 0.5, y : max + 0.5 , 
                              value: GetControlPoint(x: -0.5, y: max + 0.5).Z + Flux.TopEdge);

            //set points around the corners
            //bottom left
            SetControlPoint(x: -0.5, y: 0.5, value: GetControlPoint(x: -0.5, y: -0.5).Z + CornerVelocities.BottomLeft.X);
            SetControlPoint(x: 0.5, y: -0.5, value: GetControlPoint(x: -0.5, y: -0.5).Z - CornerVelocities.BottomLeft.Y);
            SetControlPoint(x: 0.5, y: 0.5, value: GetControlPoint(x: 0.5, y: -0.5).Z + CornerVelocities.BottomLeft.X);

            //top left
            SetControlPoint(x: -0.5, y: max - 0.5, value: GetControlPoint(x: -0.5, y: max + 0.5).Z - CornerVelocities.TopLeft.X);
            SetControlPoint(x: 0.5, y: max + 0.5, value: GetControlPoint(x: -0.5, y: max + 0.5).Z - CornerVelocities.TopLeft.Y);
            SetControlPoint(x: 0.5, y: max - 0.5, value: GetControlPoint(x: 0.5, y: max + 0.5).Z - CornerVelocities.TopLeft.X);

            //top right
            SetControlPoint(x: max + 0.5, y: max - 0.5, value: GetControlPoint(x: max + 0.5, y: max + 0.5).Z + CornerVelocities.TopRight.X);
            SetControlPoint(x: max - 0.5, y: max + 0.5, value: GetControlPoint(x: max + 0.5, y: max + 0.5).Z - CornerVelocities.TopRight.Y);
            SetControlPoint(x: max - 0.5, y: max - 0.5, value: GetControlPoint(x: max - 0.5, y: max + 0.5).Z + CornerVelocities.TopRight.X);

            //bottom right
            SetControlPoint(x: max + 0.5, y: 0.5, value: GetControlPoint(x: max + 0.5, y: -0.5).Z + CornerVelocities.BottomRight.X);
            SetControlPoint(x: max - 0.5, y: -0.5, value: GetControlPoint(x: max + 0.5, y: 0.5).Z - CornerVelocities.BottomRight.Y);
            SetControlPoint(x: max - 0.5, y: 0.5, value: GetControlPoint(x: max - 0.5, y: -0.5).Z + CornerVelocities.BottomRight.X);

            Vector3[,] streamVector3s = BezierInterpolation.Bezier3D(ControlPoints, StreamFunctionGridSize);
            foreach (var streamVector in streamVector3s)
            {
                SetStreamFunction(streamVector);
            }
        }

        private void GenerateVelocityGrid()
        {
             for(int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    VelocityGrid[y, x] = CalculateVeloctiy(x, y);
                }
            }   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">Between 0 and 1</param>
        /// <param name="y">Between 0 and 1</param>
        /// <returns></returns>
		public Vector2 Velocity(float x, float y)
		{
			if (x < 0 || x > 1)
			{
				throw new ArgumentException("x-coordinate out of bounds");
			}

			if (y < 0 || y > 1)
			{
				throw new ArgumentException("y-coordinate out of bounds");
			}

			if (x > 0.99)
			{
				x = GridSize - 1;
			}
			else
			{
				x = (float)Math.Floor(x * GridSize);
			}

			if (y > 0.99)
			{
				y = GridSize - 1;
			}
			else
			{
				y = (float) Math.Floor(y * GridSize);
			}
			return Velocity((int) x, (int) y);
		}

        public Vector2 Velocity(int x, int y)
        {
            return VelocityGrid[y, x];
        }
        
        /// <summary>
        /// Sets the control point at the point (x,y)
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <param name="value"></param>
        void SetControlPoint(double x, double y, double value)
        {
            int n = StreamFunctionGridSize;
            int i = CoordinateToIndex(y);
            int j = CoordinateToIndex(x);
            if (i != 0 && i != 1)
            {
                i = i - n + 4;
            }

            if (j != 0 && j != 1)
            {
                j = j - n + 4;
            }
            ControlPoints[i, j] = new Vector3((float) x, (float)y, (float)value);
        }
        
        
        /// <summary>
        /// Returns the ControlPoint at the point (x,y)
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        Vector3 GetControlPoint(double x, double y)
        {
            int n = StreamFunctionGridSize;
            int i = CoordinateToIndex(y);
            int j = CoordinateToIndex(x);
            if (i != 0 && i != 1)
            {
                i = i - n + 4;
            }

            if (j != 0 && j != 1)
            {
                j = j - n + 4;
            }
            return ControlPoints[i, j];
        }
        
        private Vector2 CalculateVeloctiy(int x, int y)
        {
            int n = GridSize;
            if (x == -0.5 && y == -0.5)
            {
                return CornerVelocities.BottomLeft;
            }

            if (x == 0 && y == n + 0.5)
            {
                return CornerVelocities.TopLeft;
            }

            if (x == n + 0.5 && y == -0.5)
            {
                return CornerVelocities.BottomRight;
            }

            if (x == n + 0.5 && y == n + 0.5)
            {
                return CornerVelocities.TopRight;
            }
            Vector2 velocity = new Vector2 ();
            velocity.X = (StreamFunction(x - 0.5, y - 0.5) + StreamFunction(x + 0.5, y - 0.5))/2 
                         - (StreamFunction(x - 0.5, y + 0.5) + StreamFunction(x + 0.5, y + 0.5))/2;
            velocity.Y = (StreamFunction(x + 0.5, y + 0.5) + StreamFunction(x + 0.5, y - 0.5))/2 
                         - (StreamFunction(x - 0.5, y + 0.5) + StreamFunction(x - 0.5, y - 0.5))/2;
            return velocity;
        }
        
        /// <summary>
        /// Returns the float value of the stream function at point x and y.
        /// The arguments x and y are the physical coordinates from where the StreamFunction should be retrived.
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        private float StreamFunction(double x, double y)
        {
            return StreamFunctionGrid[CoordinateToIndex(y), CoordinateToIndex(x)];
        }
        
        /// <summary>
        /// Returns a Vector3 representing a triplet with the x- and y-coordinate and the stream function value.
        /// The parameters x and y are the physical coordinates from where the StreamFunction vector should be retrived. 
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        private Vector3 StreamFunctionVector(double x, double y)
        {
            return new Vector3((float)x, (float)y, StreamFunction(x, y));
        }

        private void SetStreamFunction(Vector3 xys)
        {
            SetStreamFunction(xys.X, xys.Y, xys.Z);
        }

        /// <summary>
        /// Sets the stream function value at the point (x,y) to <c>value</c>.
        /// The parameters x and y are the physical coordinates where the stream function should be set.
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <param name="value">The value the set the StreamFunction to</param>
        private void SetStreamFunction(float x, float y, float value)
        {
            StreamFunctionGrid[CoordinateToIndex(y), CoordinateToIndex(x)] = value;
        }
        
        /// <summary>
        /// Returns the index in the StreamFunctionGrid corresponding to the coordinate given. Does not apply
        /// to the VelocityGrid
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int CoordinateToIndex(double coordinate)
        {
            if ((coordinate + 0.5) % 1.0 > 0.05 && (coordinate + 0.5) % 1.0 < 0.95)
            {
                throw new ArgumentException();
            }

            return Convert.ToInt32(coordinate + 0.5);
        }

        public void WriteToFile(StreamWriter writer)
        {
            for (int i = 0; i < GridSize; i++)
            {
                String rowString = "";
                for (int j = 0; j < GridSize; j++)
                {
                    try
                    {
                        var vel = Velocity(j, i);
                        rowString += string.Format("{0:N2},{1:N2};",vel.X, vel.Y);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(i);
                        Console.WriteLine(j);
                        throw;
                    } 
                }
                writer.WriteLine(rowString);
            }
        }
        
        public void WriteToFile(String filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                WriteToFile(writer);
            }
        }

        public XmlElement ToXmlElement(XmlDocument xmlDoc)
        {
            XmlElement element = xmlDoc.CreateElement("tile");
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    XmlElement velocity = xmlDoc.CreateElement("velocity");
                    velocity.SetAttribute("relX", (x/(float)GridSize).ToString());
                    velocity.SetAttribute("relY", (y/(float)GridSize).ToString());
                    velocity.SetAttribute("vx", Velocity(x, y).X.ToString());
                    velocity.SetAttribute("vy", Velocity(x, y).Y.ToString());
                    element.AppendChild(velocity);
                }
            }
            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">Between 0 and 1</param>
        /// <param name="y">Between 0 and 1</param>
        public void SetVelocity(double x, double y, double vx, double vy)
        {
            VelocityGrid[(int)(x * GridSize), (int)(y * GridSize)] = new Vector2((float)vx, (float)vy);
        }
    }
    
    public struct Flux
    {
        public int LeftEdge;
        public int RightEdge;
        public int TopEdge;
        public int BottomEdge;
        
        public Flux(int a)
        {
            LeftEdge = 0;
            RightEdge = 0;
            TopEdge = 0;
            BottomEdge = 0;
        }

        public Flux(int leftEdge, int rightEdge, int topEdge, int bottomEdge)
        {
            LeftEdge = leftEdge;
            RightEdge = rightEdge;
            TopEdge = rightEdge;
            BottomEdge = bottomEdge;
        }
    }

    public struct CornerVelocities
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
        
        public CornerVelocities(int a)
        {    
            TopLeft = new Vector2(0, 0);
            TopRight = new Vector2(0, 0);
            BottomLeft = new Vector2(0, 0);
            BottomRight = new Vector2(0, 0);
        }

        public CornerVelocities(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public Vector2 this[Corner c]
        {
            get
            {
                switch (c)
                {
                    case Corner.TopLeft:
                        return TopLeft;
                    case Corner.TopRight:
                        return TopRight;
                    case Corner.BottomRight:
                        return BottomRight;
                    case Corner.BottomLeft:
                        return BottomLeft;
                    default:
                        throw new IndexOutOfRangeException("Corner index is out of bounds. " +
                                                           "Make sure Corner enum has 4 values, one for each corner");
                }
            }

            set
            {
                switch (c)
                {
                    case Corner.TopLeft:
                        TopLeft = value;
                        break;
                    case Corner.TopRight:
                        TopRight = value;
                        break;
                    case Corner.BottomRight:
                        BottomRight = value;
                        break;
                    case Corner.BottomLeft:
                        BottomLeft = value;
                        break;
                }
            }
        }
        /// <summary>
        /// Writes
        /// </summary>
        /// <param name="filename"></param>
    }
}