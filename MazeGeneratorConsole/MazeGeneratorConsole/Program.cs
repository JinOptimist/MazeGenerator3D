using MazeGenerator;
using MazeGeneratorConsole;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(3, 3, 3, 42);

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();