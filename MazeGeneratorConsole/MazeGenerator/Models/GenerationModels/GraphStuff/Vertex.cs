using System.Collections.Generic;

namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Vertex
    {
        public Graph Graph { get; private set; }
        public List<Vertex> Parents { get; private set; } = new List<Vertex>();
        public List<Vertex> Children { get; private set; } = new List<Vertex>();
        public List<Edge> Edges { get; private set; } = new List<Edge>();

        public CellForGeneration Cell {  get; private set; }

        public Vertex(Graph graph, CellForGeneration cell)
        {
            Graph = graph;
            Cell = cell;
        }
    }
}