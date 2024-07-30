using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Graph
    {
        public List<Edge> Edges { get; private set; } = new List<Edge>();
        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public Vertex? Root { get; set; }

        public ChunkForGeneration Chunk { get; private set; }

        public Graph(ChunkForGeneration chunk)
        {
            Chunk = chunk;
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
            Vertices.Add(edge.From);
            Vertices.Add(edge.From);
        }

        public Vertex this[int x, int y, int z]
        {
            get
            {
                return Vertices.FirstOrDefault(vertex =>
                    vertex.Cell.X == x
                    && vertex.Cell.Y == y
                    && vertex.Cell.Z == z);
            }
        }

        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddRangeVertex(IEnumerable<Vertex> vertexs)
        {
            Vertices.AddRange(vertexs);
            //Edges = Edges.Distinct().ToList();
        }

        internal void AddRangeEdges(IEnumerable<Edge> edges)
        {
            Edges.AddRange(edges);
        }
    }
}
