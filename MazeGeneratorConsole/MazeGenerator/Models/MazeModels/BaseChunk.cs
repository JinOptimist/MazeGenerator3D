using System.Collections.Generic;
using System.Linq;

namespace MazeGenerator.Models.MazeModels
{
    public abstract class BaseChunk<T> where T : Cell
    {
        /// <summary>
        /// X-axe
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Y-axe
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Z-axe
        /// </summary>
        public int Height { get; set; }

        public virtual List<T> Cells { get; set; } = new List<T>();

        /// <summary>
        /// Seed base on what chunk was builded
        /// </summary>
        public int Seed { get; set; }

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

        public T GetExitCell()
            => Cells.Single(x => x.InnerPart == InnerPart.Exit);

        public T GetStartCell()
            => Cells.Single(x => x.InnerPart == InnerPart.Start);

        public override string ToString()
        {
            var exist = GetExitCell();
            return $"Chunk [{Length}, {Width}, {Height}] => Exit {exist}";
        }
    }
}
