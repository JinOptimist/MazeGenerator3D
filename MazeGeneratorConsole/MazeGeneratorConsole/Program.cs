using MazeGenerator;
using MazeGeneratorConsole;
using System.Numerics;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(5, 4, 3, startPoint: new Vector3(0, 0, 0), seed: 42);

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();