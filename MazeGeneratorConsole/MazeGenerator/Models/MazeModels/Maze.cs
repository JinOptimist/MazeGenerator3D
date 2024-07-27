using System.Collections.Generic;

namespace MazeGenerator.Models.MazeModels
{
    public class Maze
    {
        public List<Chunk> Chunks {  get; set; } = new List<Chunk>();

        /// <summary>
        /// X-axe
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Y-axe
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// Z-axe
        /// </summary>
        public int FullHeight { get; set; }

        /// <summary>
        /// Seed base on what maze was builded
        /// </summary>
        public int Seed { get; set; }
    }
}
