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
            Console.WriteLine(System.Environment.CurrentDirectory);
            FTGridBuilding.LPModel.LPSolve.BuildInitialModel(-4, 4, -4, 4, new TileGrid(10));
        }
    }
}
