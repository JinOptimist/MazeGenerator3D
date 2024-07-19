namespace MazeGenerator.Models
{
    public class Cell
    {
        public int X {  get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public Wall Wall { get; set; }
    }
}
