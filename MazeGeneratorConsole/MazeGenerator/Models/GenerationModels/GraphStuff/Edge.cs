using System.Numerics;
using System.Xml;

namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Edge
    {
        public Vertex From { get; private set; }
        public Vertex To { get; private set; }

        public Vector3 Direction => To.Cell - From.Cell;
        
        // public double Complexity { get; set; }

        public Edge(Vertex parent, Vertex child)
        {
            From = parent;
            To = child;
        }

        public override bool Equals(object obj)
        {
            var edge = obj as Edge;
            if (edge is null)
            {
                return false;
            }

            return From == edge.From
                && To == edge.To;
        }

        public override int GetHashCode()
        {
            return From.GetHashCode() ^ To.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{Direction.X}, {Direction.Y}, {Direction.Z}] " +
                $": [{From.X}, {From.Y}, {From.Z}] " +
                $"=> [{To.X}, {To.Y}, {To.Z}]";
        }
    }
}