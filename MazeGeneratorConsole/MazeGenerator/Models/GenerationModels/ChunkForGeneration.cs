using System.Collections.Generic;
using System.Linq;

namespace MazeGenerator.Models.GenerationModels
{
    public class ChunkForGeneration
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

        public List<CellForGeneration> Cells { get; set; } = new List<CellForGeneration>();

        public CellForGeneration this[int x,int y, int z]
        {
            get
            {
                return Cells.FirstOrDefault(cell =>
                    cell.X == x
                    && cell.Y == y
                    && cell.Z == z);
            }
        }

        public CellForGeneration this[float x, float y, float z]
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
