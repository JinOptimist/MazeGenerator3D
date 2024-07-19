namespace MazeGenerator.Models.GenerationModels
{
    public class Miner
    {
        public CellForGeneration CurrentCell { get; set; }
        public Direction LastStepDirection { get; set; }

        public int X => CurrentCell.X;
        public int Y => CurrentCell.Y;
        public int Z => CurrentCell.Z;
    }
}
