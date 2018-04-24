using System.IO;

namespace FTGridBuilding
{
    public class Settings
    {
        public const string ProjectPath = "/home/felix/FTGridBuilding/";
        public const string PythonScriptPath = ProjectPath + "PythonExternal/ui.py";
        public const string PathToXMLFiles = ProjectPath + "PythonExternal/";
        public const string PathToGridXML = PathToXMLFiles + "grid.xml";
        public const string PathToValidTilesXML = PathToXMLFiles + "tiles.xml";
        
        private const string dllFileNameWindows = "lpsolve55.dll";
        private const string dllFileNameLinux = "liblpsolve55.so";
        private const string dllFlieNameOSX = ProjectPath + "liblpsolve55.bundle";
        public const string dllFileNameCurrent = dllFileNameLinux;
        //public const string Pyhthon = "/usr/bin/python3";
        public const string Python = "/usr/bin/python3";
    }
}