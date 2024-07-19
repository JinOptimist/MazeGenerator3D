﻿using MazeGenerator.Models.GenerationModels;

namespace MazeGenerator.Models
{
    public class Maze
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
        public virtual List<Cell> Cells { get; set; } = new();

        public Cell? this[int x, int y, int z]
        {
            get
            {
                return Cells.FirstOrDefault(cell =>
                    cell.X == x
                    && cell.Y == y
                    && cell.Z == z);
            }
        }
    }
}
