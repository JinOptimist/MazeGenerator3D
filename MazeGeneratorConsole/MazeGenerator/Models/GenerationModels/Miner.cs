using System.Numerics;

namespace MazeGenerator.Models.GenerationModels
{
    public class Miner
    {
        public CellForGeneration CurrentCell { get; set; }
        public Vector3 LastStepDirection { get; set; }

        public int X => CurrentCell.X;
        public int Y => CurrentCell.Y;
        public int Z => CurrentCell.Z;
    }
}
