using MazeGenerator;
using MazeGenerator.Models.GenerationModels;
using MazeGeneratorConsole;
using System.Numerics;

// Console.WriteLine("Start");
var builder = new GeneratorBaseOnGraph();
var maze = builder.Generate(3, 3, 3, 
    startPoint: new Vector2(0, 0), 
    weights: GenerationWeights.FullRandom(),
    seed: 109);//seed: 42

var drawer = new MazeDrawer();
drawer.ClearDraw(maze);
Console.ReadLine();