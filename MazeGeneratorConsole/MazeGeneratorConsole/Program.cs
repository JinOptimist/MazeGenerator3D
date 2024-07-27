using MazeGenerator;
using MazeGenerator.Models.GenerationModels;
using MazeGeneratorConsole;
using System.Numerics;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(7, 6, 10, 
    startPoint: new Vector2(0, 0), 
    weights: GenerationWeights.GenericBuilding(),
    seed: 100);//seed: 42

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();