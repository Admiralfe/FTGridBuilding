//Detta ska in i GridBuilder

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


//En ny konstruktor till FlowTile 

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

//Lägg till denna i FlowTile

/// <summary>
/// 
/// </summary>
/// <param name="x">Between 0 and 1</param>
/// <param name="y">Between 0 and 1</param>
public void SetVelocity(double x, double y, double vx, double vy)
{
VelocityGrid[(int)(x * GridSize), (int)(y * GridSize)] = new Vector2((float)vx, (float)vy);
}