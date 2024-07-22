using MazeGenerator;
using MazeGenerator.Models.GenerationModels;
using MazeGeneratorConsole;
using System.Numerics;

// Console.WriteLine("Start");
var builder = new Generator();
var maze = builder.Generate(20, 10, 5, 
    startPoint: new Vector3(0, 0, 0), 
    weights: GenerationWeights.GenericBuilding(),
    seed: 42);//seed: 42

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();