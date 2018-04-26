using System;
using FTGridBuilding.FlowTileUtils;
using FTGridBuilding.GridBuilding;
using FTGridBuilding.LPModel;

namespace FTGridBuilding
{
   class Program
    {
        static void Main(string[] args)
        {
            GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, 5, 10);

            for (int row = 0; row < gridBuilder.gridDimension; row++) 
            {
                for (int col = 0; col < gridBuilder.gridDimension; col++) 
                {
                    FlowTile tile = gridBuilder.AskUserForTile(row, col);
                    gridBuilder.AddTile(row, col, tile);
                }
            }

            TileGrid tileGrid = gridBuilder.GetTileGrid();
            Console.WriteLine(tileGrid.GetFlowTile(4, 4).Flux.TopEdge + " " + tileGrid.GetFlowTile(4, 4).Flux.RightEdge + " " +
                tileGrid.GetFlowTile(4, 4).Flux.BottomEdge + " " + tileGrid.GetFlowTile(4, 4).Flux.LeftEdge);
            tileGrid.WriteToXML(@"C:\Users\Felix Liu\source\repos\FTGridbuilding\TileGrid.xml");
        }
    }
}
