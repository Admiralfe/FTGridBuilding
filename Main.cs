﻿using System;
using FTGridBuilding.FlowTileUtils;
using FTGridBuilding.GridBuilding;
using FTGridBuilding.LPModel;

namespace FTGridBuilding
{
   class Program
    {
        static void Main(string[] args)
        {
            GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, 10, 15);

            for (int row = 0; row < gridBuilder.gridDimension; row++) 
            {
                for (int col = 0; col < gridBuilder.gridDimension; col++) 
                {
                    FlowTile tile = gridBuilder.AskUserForTile(row, col);
                    gridBuilder.AddTile(row, col, tile);
                }
            }

            TileGrid tileGrid = gridBuilder.GetTileGrid();
            tileGrid.WriteToXML("~/FTGridBuilding/TileGrid.xml");
        }
    }
}