using MazeGenerator;
using MazeGenerator.Models.GenerationModels;
using MazeGeneratorConsole;
using System.Numerics;

// Console.WriteLine("Start");
var builder = new GeneratorBaseOnGraph();
var maze = builder.Generate(4, 5, 3, 
    startPoint: new Vector2(0, 0), 
    weights: GenerationWeights.FullRandom(),
    seed: 985);//seed: 42

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();