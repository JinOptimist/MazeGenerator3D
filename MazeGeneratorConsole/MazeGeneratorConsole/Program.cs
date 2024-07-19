using MazeGenerator;
using MazeGenerator.Models;
using MazeGeneratorConsole;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(5, 5, 1);

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();