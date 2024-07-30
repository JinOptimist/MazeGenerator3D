using MazeGenerator.Models.MazeModels;
using System.Linq;

namespace MazeGenerator.Models.GenerationModels
{
    public class ChunkForGeneration : BaseChunk<CellForGeneration>
    {
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
