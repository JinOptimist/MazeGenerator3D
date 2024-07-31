using MazeGenerator.Generators;
using MazeGenerator.Models.GenerationModels;
using MazeGeneratorConsole;
using System.Numerics;

var drawer = new MazeDrawer();

// Console.WriteLine("Start");
var builder = new GeneratorBaseOnGraph();
var maze = builder.Generate(4, 5, 5, 
    startPoint: new Vector2(0, 0), 
    weights: GenerationWeights.FullRandom(),
    seed: 985
    // debugDrawer: drawer.TestDrawChunk
    );//seed: 42


drawer.ClearDraw(maze);
Console.ReadLine();