namespace MazeGenerator.Models.MazeModels
{
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Wall Wall { get; set; }

        public InnerPart InnerPart { get; set; }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]: {InnerPart}";
        }
    }
}
