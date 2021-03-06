﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Numerics;
using System.Xml;
using Random = System.Random;

using FTGridBuilding.LPModel;
using FTGridBuilding.FlowTileUtils;
using static FTGridBuilding.Settings;
using lpsolve55;


namespace FTGridBuilding.GridBuilding
{
    public class GridBuilder
    {

        private int minXFlux;
        private int maxXFlux;
        private int minYFlux;
        private int maxYFlux;
        public float BaseFlux;
        private List<Vector2> allowedVelocities = new List<Vector2>();

        private TileGrid tileGrid;

        public int gridDimension;

        public int innerTileGridDimension;

        private int?[,][] boundaryConditions;

        public List<int[]> Obstacles = new List<int[]>();

        public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, int gridDimensionIn,
            int innerTileGridDimensionIn, Vector2[] allowedVelocitiesIn)
        {
            minXFlux = minXFluxIn;
            maxXFlux = maxXFluxIn;
            minYFlux = minYFluxIn;
            maxYFlux = maxYFluxIn;
            gridDimension = gridDimensionIn;

            innerTileGridDimension = innerTileGridDimensionIn;
            
            foreach (Vector2 cornerVelocity in allowedVelocitiesIn)
            {
                allowedVelocities.Add(cornerVelocity);
            }

            tileGrid = new TileGrid(gridDimension);
        }
        
        public GridBuilder(int minXFluxIn, int maxXFluxIn, int minYFluxIn, int maxYFluxIn, float BaseFluxIn, int gridDimensionIn,
            int innerTileGridDimensionIn)
        {
            minXFlux = minXFluxIn;
            maxXFlux = maxXFluxIn;
            minYFlux = minYFluxIn;
            maxYFlux = maxYFluxIn;
            BaseFlux = BaseFluxIn;

            gridDimension = gridDimensionIn;

            innerTileGridDimension = innerTileGridDimensionIn;
            
            allowedVelocities.Add(Vector2.Zero);
            
            tileGrid = new TileGrid(gridDimension);
            
            /*
            boundaryConditions = new int?[gridDimension, gridDimension][];
            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    boundaryConditions[row, col] = new int?[4];
                    if (row == 0)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Top] = 0;
                    }

                    if (row == gridDimension - 1 && col >= gridDimension - 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Bottom] = -4;
                    }

                    if (col == 0 && row < 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Left] = 4;
                    }

                    if (col == gridDimension - 1)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Right] = 0;
                    }

                    if (row == 2 && col < gridDimension - 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Top] = 0;
                    }

                    if (col == 0 && row >= 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Left] = 0;
                    }

                    if (row == gridDimension - 1 && col < gridDimension - 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Bottom] = 0;
                    }

                    if (col == gridDimension - 2 && row >= 2)
                    {
                        boundaryConditions[row, col][(int)LPSolve.Direction.Left] = 0;
                    }

                }
            }
            */
            
            
            
            //Hard coded boundary conditions, maybe fix to be more user friendly later!!!!! 
            boundaryConditions = new int?[gridDimension, gridDimension][];
            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    boundaryConditions[row, col] = new int?[4];
                    if (row == 0)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Top] = 0;
                    }

                    if (col == 0)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Left] = 2;
                    }

                    if (row == gridDimension - 1)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Bottom] = 0;
                    }

                    if (col == gridDimension - 1)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Right] = 2;
                    } 
                } 
            }
            
            
            /*
            boundaryConditions = new int?[gridDimension, gridDimension][];
            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    boundaryConditions[row, col] = new int?[4];
                    if (row == 0)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Top] = 2;
                    }

                    if (col == 0)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Left] = 0;
                    }

                    if (row == gridDimension - 1)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Bottom] = 2;
                    }

                    if (col == gridDimension - 1)
                    {
                        boundaryConditions[row, col][(int) LPSolve.Direction.Right] = 0;
                    } 
                } 
            }
            */
            
            //LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid);
        }

        public void AddTile(int rowIndex, int colIndex, FlowTile flowTile)
        {
            tileGrid.AddTile(rowIndex, colIndex, flowTile);
        }

        public TileGrid GetTileGrid()
        {
            return tileGrid;
        }

        private void WriteTilesToXML(List<FlowTile> tiles, string filename)
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
            foreach (var tile in tiles)
            {
                root.AppendChild(tile.ToXmlElement(xmlDoc));
            }
            xmlDoc.Save(filename);
        }

        /// <summary>
        /// Allows users to choose a valid flow tile in a given slot by plotting all the valid flow tiles using Python's matplotlib
        /// and having the user click on which tile they want to place in the slot.
        /// </summary>
        /// <param name="row"> Row index of the tile slot </param>
        /// <param name="col"> Column index of the tile slot </param>
        public FlowTile AskUserForTile(int row, int col)
        {
            LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid, boundaryConditions);

            List<FlowTile> validTiles = ValidTiles(row, col);

            //If there is only one valid tile, there is no choice to be made so we simply add the tile to the tiling.
            if (validTiles.Count == 1) 
            {
                Console.WriteLine("There was only one valid tile, so it was placed in the spot.");
                Console.WriteLine("Top flux: {0}, Right Flux: {1}, Bottom Flux: {2}, Left Flux {3}", 
                    validTiles[0].Flux.TopEdge, validTiles[0].Flux.RightEdge, validTiles[0].Flux.BottomEdge, validTiles[0].Flux.LeftEdge);
                Console.ReadLine();
                return validTiles[0];
            }
            
            tileGrid.WriteToXML(PathToGridXML);    
            WriteTilesToXML(validTiles, PathToValidTilesXML);
            
            //Settings for the plotting script in python.
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Python;
            start.Arguments = string.Format("{0} {1} {2} {3} {4} {5}",
                PythonCreateTiling, gridDimension, row, col, PathToGridXML, PathToValidTilesXML);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            
            string result;
            //Runs the python script for plotting
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
            }

            //Process python = Process.Start(start);
            //python.WaitForExit();
            foreach (FlowTile tile in validTiles) 
            {
                Console.WriteLine("Top flux: {0}, Right Flux: {1}, Bottom Flux: {2}, Left Flux {3}", 
                    tile.Flux.TopEdge, tile.Flux.RightEdge, tile.Flux.BottomEdge, tile.Flux.LeftEdge);
            }
            //Console.WriteLine("Which tile do you want at row {0}, column {1}. Type a number:", row, col);
            //var num = Convert.ToInt32(Console.ReadLine());
                        
            //python.Close();
            
            //IMPORTANT! Frees allocated memory, removing this line will likely cause overflow errors.
            LPSolve.FreeModel();

            return validTiles[Convert.ToInt32(result)];
            }
        /// <summary>
        /// Retruns a List of row-col-pairs where obstacles should be placed. Each row-col-pair is represented by a List of integers
        /// </summary>
        /// <returns></returns>
        public int[] AskUserForObstacle()
        {
            StringBuilder eligibleStringBuilder = new StringBuilder();
            eligibleStringBuilder.Append(";");
            foreach (var rowcol in ValidObstaclePositions())
            {
                eligibleStringBuilder.Append(rowcol[0] + "," + rowcol[1] + ";");
            }
            StringBuilder obstaclesStringBuilder = new StringBuilder();
            obstaclesStringBuilder.Append(";");

            foreach (var rowcol in Obstacles)
            {
                obstaclesStringBuilder.Append(rowcol[0] + "," + rowcol[1] + ";");
            }
        
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Python;
            Console.WriteLine(string.Format("{0} {1} {2} {3}",
                PythonSetObstacle, gridDimension, obstaclesStringBuilder.ToString(), eligibleStringBuilder.ToString()));
            start.Arguments = string.Format("{0} {1} {2} {3}",
                PythonSetObstacle, gridDimension, obstaclesStringBuilder.ToString(), eligibleStringBuilder.ToString());
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
        
            string result;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
            result.Trim(Environment.NewLine.ToCharArray());
            if (result == "" || result == null || result == "q")
            {
                return null;
            }
            Console.WriteLine("hej" + result);
            return new []{Int32.Parse(result.Split(",")[0]), Int32.Parse(result.Split(",")[1])};
        }
        
        
        public TileGrid BuildRandomTileGrid()
        {
            //Clear the tilegrid.
            tileGrid = new TileGrid(gridDimension);
            
            Random RNG = new Random();
            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid, boundaryConditions);
                    List<FlowTile> validTiles = ValidTiles(row, col);
                    FlowTile newTile = validTiles[RNG.Next(0, validTiles.Count - 1)];
                    /*
                    Console.WriteLine("(" + row + ", " + col + ")");
                    Console.WriteLine("Top: " + newTile.Flux.topEdge);
                    Console.WriteLine("Right: " + newTile.Flux.rightEdge);
                    Console.WriteLine("Bottom: " + newTile.Flux.bottomEdge);
                    Console.WriteLine("Left: " + newTile.Flux.leftEdge);
                    */
                    tileGrid.AddTile(row, col, newTile);

                    LPSolve.FreeModel();
                }
            }

            return tileGrid;
        }

        public void AddObstacle(int row, int col) 
        {
            //Adds the obstacle to the boundary conditions
            Obstacles.Add(new []{row, col});
            for (int i = 0; i < 4; i++) 
            {
                boundaryConditions[row, col][i] = 0;
            }
            return;
        }

        public List<int[]> ValidObstaclePositions()
        {
            /* 
            //Save the previous boundary conditions locally to reset after each test
            int?[,][] previousBoundaryConditions = new int?[gridDimension, gridDimension][];

            for (int row = 0; row < gridDimension; row++)
            {
                for (int col = 0; col < gridDimension; col++)
                {
                    previousBoundaryConditions[row, col] = new int?[4];
                    for (int i = 0; i < 4; i++)
                    {
                        previousBoundaryConditions[row, col][i] = 
                            (boundaryConditions[row, col][i].HasValue ? (int?) boundaryConditions[row, col][i].Value : null);
                    }
                }
            }
            */

            List<int[]> validObstaclePositions = new List<int[]>();
            for (int row = 0; row < gridDimension; row++) 
            {
                for(int col = 0; col < gridDimension; col++)
                {
                    int?[] previousValues = new int?[4];
                    //Adds the obstacle to boundaryConditions.
                    for (int i = 0; i < 4; i++)
                    {
                        previousValues[i] = 
                            (boundaryConditions[row,col][i].HasValue ? (int?) boundaryConditions[row, col][i].Value : null);
                        boundaryConditions[row, col][i] = 0;
                    }
                    
                    LPSolve.BuildInitialModel(minXFlux, maxXFlux, minYFlux, maxYFlux, tileGrid, boundaryConditions);
                    if (LPSolve.IsFeasible())
                    {
                        validObstaclePositions.Add(new int[] {row, col});
                    }

                    //Free memory allocated to LP-model
                    LPSolve.FreeModel();
                    //reset boundaryConditions for next test
                    for (int i = 0; i < 4; i++) 
                    {
                        boundaryConditions[row, col][i] = (previousValues[i].HasValue ? (int?) previousValues[i].Value : null);
                    }
                }
            }

            foreach (int[] iArr in validObstaclePositions) 
            {
                Console.WriteLine("row: " + iArr[0]);
                Console.WriteLine("col: " + iArr[1] + "\n");
            }

            return validObstaclePositions;
        }

        //Determines the restrictions on the corner velocities in a given tile slot due to corner velocities having to 
        //match with neighboring tiles. Returns an array containing the valid velocities for each corner, or null if there is no retriction.
        private Vector2?[] velocityRestrictions(int rowNumber, int colNumber)
        {
            //Nullable so that we can use null to represent no restriction.
            Vector2? allowedTopLeftVelocity = null;
            Vector2? allowedBottomLeftVelocity = null;
            Vector2? allowedTopRightVelocity = null;
            Vector2? allowedBottomRightVelocity = null;
            
            bool topLeftRestricted = false;
            bool bottomLeftRestricted = false;
            bool topRightRestricted = false;
            bool bottomRightRestricted = false;
            
            //Left tile
            if(tileGrid.HasTile(rowNumber, colNumber - 1))
            {
                allowedTopLeftVelocity = tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.TopRight;
                allowedBottomLeftVelocity =
                    tileGrid.GetFlowTile(rowNumber, colNumber - 1).CornerVelocities.BottomRight; 
                
                topLeftRestricted = true;
                bottomLeftRestricted = true;
            }
            
            //Top left tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber - 1))
            {
                
                //Checks if velocity restriction has been set already, in that case we don't need to set it again, 
                //since it will be the same in a valid tiling.
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber - 1).CornerVelocities.BottomRight;
                    topLeftRestricted = true;
                }
            }
            
            //Bottom left tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber - 1))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber - 1).CornerVelocities.TopRight;
                    bottomLeftRestricted = true;
                }
            }
            
            //Top tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber))
            {
                if (!topLeftRestricted)
                {
                    allowedTopLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topLeftRestricted = true;
                }

                if (!topRightRestricted)
                {
                    allowedTopRightVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topRightRestricted = true;
                }
            }
            
            //Bottom Tile
            if (tileGrid.HasTile(rowNumber - 1, colNumber))
            {
                if (!bottomLeftRestricted)
                {
                    allowedBottomLeftVelocity = 
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.TopLeft;
                    bottomLeftRestricted = true;
                }

                if (!bottomRightRestricted)
                {
                    allowedBottomRightVelocity =
                        tileGrid.GetFlowTile(rowNumber - 1, colNumber).CornerVelocities.TopRight;
                    bottomRightRestricted = true;
                }
            }
            
            //Right Tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber))
            {
                if (!bottomRightRestricted)
                {
                    allowedBottomLeftVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    bottomRightRestricted = true;
                }

                if (!topRightRestricted)
                {
                    allowedTopRightVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber).CornerVelocities.BottomLeft;
                    topRightRestricted = true;
                }
            }
            
            //Top right tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber + 1))
            {
                if (!topRightRestricted)
                {
                    allowedTopRightVelocity = 
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber + 1).CornerVelocities.TopRight;
                }
            }
            
            //Bottom right tile
            if (tileGrid.HasTile(rowNumber + 1, colNumber - 1))
            {
                if (!bottomRightRestricted)
                {
                    allowedBottomRightVelocity =
                        tileGrid.GetFlowTile(rowNumber + 1, colNumber - 1).CornerVelocities.BottomRight;
                }
            }

            return new Vector2?[]
            {
                allowedTopLeftVelocity, allowedTopRightVelocity,
                allowedBottomRightVelocity, allowedBottomLeftVelocity
            };
        }

        //Generates all combinations of alid corner velocity combinations for a tile, given the restrictions on that tile.
        private List<CornerVelocities> cornerVelocityCombinations(Vector2?[] restrictions)
        {
            //Array of lists for valid velocities. Array index is for the 4 corners, and the list holds all the valid corner velocities.
            List<Vector2>[] iteratorVectorList = new List<Vector2>[4];
            
            for (int i = 0; i < 4; i++)
            {
                iteratorVectorList[i] = new List<Vector2>();
                //null value represents no restrictions, and we add all allowed velocities as possibilities.
                if (!restrictions[i].HasValue)
                {
                    foreach (Vector2 cornerVelocity in allowedVelocities)
                    {
                        iteratorVectorList[i].Add(cornerVelocity);
                    }
                }

                else
                {
                    iteratorVectorList[i].Add(restrictions[i].Value);
                }
            }

            List<CornerVelocities> combinationList = new List<CornerVelocities>();

            foreach (Vector2 v1 in iteratorVectorList[(int) Corner.TopLeft])
            {
                foreach (Vector2 v2 in iteratorVectorList[(int) Corner.TopRight])
                {
                    foreach (Vector2 v3 in iteratorVectorList[(int) Corner.BottomLeft])
                    {
                        foreach (Vector2 v4 in iteratorVectorList[(int) Corner.BottomRight])
                        {
                            combinationList.Add(new CornerVelocities(v1, v2, v3, v4));
                        }
                    }
                }
            }

            return combinationList;
        }
        
        /// <summary>
        /// Finds the valid flow tiles to be put in a position given by row and column indices.
        /// The constraints are that edge fluxes should match and that the field needs to be divergence free.
        /// </summary>
        /// <param name="rowNumber">Row index in grid</param>
        /// <param name="colNumber">Column index in grid</param>
        /// <returns>List of valid tiles to put in that position</returns>
        /// <exception cref="ArgumentException">
        /// If there already is a tile in the given position this exception is thrown.
        /// </exception>
        private List<FlowTile> ValidTiles(int rowNumber, int colNumber)
        {
            if (tileGrid.HasTile(rowNumber, colNumber))
            {
                throw new ArgumentException("Tile already exists on that position");
            }
            
            int[][] validFluxRanges = new int[4][];
            for (int i = 0; i < 4; i++)
            {
                validFluxRanges[i] = new int[2];
            }
            
            /*
            int[] validTopFluxRange = new int[2];
            int[] validBottomFluxRange = new int[2];
            int[] validLeftFluxRange = new int[2];
            int[] validRightFluxRange = new int[2];
            */
            
            //Finds the restrictions on corner velocities
            Vector2?[] restrictions = velocityRestrictions(rowNumber, colNumber);
            //Finds all valid combinations of the allowed corner velocities.
            List<CornerVelocities> allowedCornerVelocities = cornerVelocityCombinations(restrictions);
            
            for (int i = 0; i < 4; i++)
            {
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, (LPSolve.Direction) i, gridDimension, false);
                validFluxRanges[i][0] = LPSolve.SolveModel();
                LPSolve.SetEdgeToSolve(rowNumber, colNumber, (LPSolve.Direction) i, gridDimension, true);
                validFluxRanges[i][1] = LPSolve.SolveModel(); 
            }
            
            List<FlowTile> currentValidTiles = new List<FlowTile>();

            //Create all possible FlowTiles given the bounds on flows. This set still needs to be filtered
            for (int i = validFluxRanges[0][0]; i <= validFluxRanges[0][1]; i++)
            {
                for (int j = validFluxRanges[1][0]; j <= validFluxRanges[1][1]; j++)
                {
                    for (int k = validFluxRanges[2][0]; k <= validFluxRanges[2][1]; k++)
                    {
                        for (int l = validFluxRanges[3][0]; l <= validFluxRanges[3][1]; l++)
                        {
                            foreach (CornerVelocities cornerVelocities in allowedCornerVelocities)
                            {

                                Flux flux = new Flux();
                                flux.TopEdge = i;
                                flux.RightEdge = j;
                                flux.BottomEdge = k;
                                flux.LeftEdge = l;

                                currentValidTiles.Add(new FlowTile(innerTileGridDimension, flux, cornerVelocities));
                            }
                        }
                    }
                }
            }
            List<FlowTile> validTiles =
                LPSolve.FilterValidTiles(currentValidTiles, rowNumber, colNumber, gridDimension);

            Console.WriteLine("final number of tiles: " + validTiles.Count);
            foreach (FlowTile tile in validTiles)
            {
                Console.WriteLine("Top: " + tile.Flux.TopEdge);
                Console.WriteLine("Right: " + tile.Flux.RightEdge);
                Console.WriteLine("Bottom: " + tile.Flux.BottomEdge);
                Console.WriteLine("Left: " + tile.Flux.LeftEdge + "\n");
            }
            

            return validTiles;
        }
        
        public static TileGrid BuildFromXML(string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReader reader = new XmlTextReader(filename);
            xmlDoc.Load(reader);
            XmlElement root = xmlDoc.DocumentElement;
            int Dimension = (int)Math.Sqrt(root.ChildNodes.Count);
            TileGrid tileGrid = new TileGrid(Dimension);
            int TileSize = (int) Math.Sqrt(root.ChildNodes[0].ChildNodes.Count);
            
            foreach (XmlElement tile in root.ChildNodes)
            {
                FlowTile flowTile = new FlowTile(TileSize);
                foreach (XmlElement velocity in tile)
                {
                    double x = double.Parse(velocity.GetAttribute("relX"));
                    double y = double.Parse(velocity.GetAttribute("relY"));
                    double vx = double.Parse(velocity.GetAttribute("vx"));
                    double vy = double.Parse(velocity.GetAttribute("vy"));
                    flowTile.SetVelocity(x, y, vx, vy);
                }
                tileGrid.AddTile(int.Parse(tile.GetAttribute("row")), int.Parse(tile.GetAttribute("col")), flowTile);
            }
            return tileGrid;
        }
    }

}