using System.Collections.Generic;

namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Graph
    {
        public List<Edge> Edges { get; private set; } = new List<Edge>();
        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public Vertex? Root { get; set; }

        public ChunkForGeneration Maze {  get; private set; }

        public Graph(ChunkForGeneration maze)
        {
            Maze = maze;
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
            Vertices.Add(edge.Parent);
            Vertices.Add(edge.Parent);
        }

        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
            Edges.AddRange(vertex.Edges);
        }
    }
}
