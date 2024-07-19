namespace MazeGenerator.Models.GenerationModels
{
    public class MazeForGeneration
    {
        /// <summary>
        /// X-axe
        /// </summary>
        public int Legnth { get; set; }

        /// <summary>
        /// Y-axe
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Z-axe
        /// </summary>
        public int Height { get; set; }

        public List<CellForGeneration> CellsForGeneration { get; set; } = new();

        public Cell this[int x,int y, int z]
        {
            get
            {
                return _
            }
        }
    }
}
