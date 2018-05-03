﻿using System;
using System.Collections.Generic;
using FTGridBuilding.FlowTileUtils;
using FTGridBuilding.GridBuilding;
using FTGridBuilding.LPModel;

namespace FTGridBuilding
{
   class Program
    {
        static void Main(string[] args)
        {
            float baseFlux = 2f;
            GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, baseFlux, 5, 10);
            foreach (int[] tile in gridBuilder.AskUserForObstacle())
            {
                gridBuilder.AddObstacle(tile[0], tile[1]);
            }

            for (int row = 0; row < gridBuilder.gridDimension; row++) 
            {
                for (int col = 0; col < gridBuilder.gridDimension; col++) 
                {
                    FlowTile tile = gridBuilder.AskUserForTile(row, col);
                    gridBuilder.AddTile(row, col, tile);
                }
            }

            TileGrid tileGrid = gridBuilder.GetTileGrid();
            tileGrid.WriteToXML("/home/felix/FTGridBuilding/TileGridHorisontal.xml");
            //tileGrid.WriteToXML(@"C:\Users\Felix Liu\source\repos\FTGridbuilding\Tilings\Curve.xml");
        }
    }
}
