using MazeGenerator;
using MazeGenerator.Models;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(2, 2, 1);

for (int y = 0; y < maze.Width; y++)
{
    for (int x = 0; x < maze.Legnth; x++)
    {
        var cell = maze[x, y, 0]!;
        var wallsWithRoofAndFloor = cell.Wall;
        var walls = wallsWithRoofAndFloor & ~Wall.Roof & ~Wall.Floor;
        Console.SetCursorPosition(x * 3, y * 2);
        Console.Write((int)walls);
    }
}
