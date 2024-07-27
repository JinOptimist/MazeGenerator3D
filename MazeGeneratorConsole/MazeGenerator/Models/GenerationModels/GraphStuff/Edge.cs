namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Edge
    {
        public Vertex Parent { get; private set; }
        public Vertex Child { get; private set; }
        public double Complexity { get; set; }

        public Edge(Vertex parent, Vertex child)
        {
            Parent = parent;
            Child = child;
        }
    }
}