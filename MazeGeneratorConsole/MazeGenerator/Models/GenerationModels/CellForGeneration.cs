using System.Numerics;

namespace MazeGenerator.Models.GenerationModels
{
    public class CellForGeneration : Cell
    {
        public BuildingState State {  get; set; }

        public static Vector3 operator -(CellForGeneration cell1, CellForGeneration cell2)
        {
            return new Vector3(
                cell1.X - cell2.X,
                cell1.Y - cell2.Y,
                cell1.Z - cell2.Z);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]: {State}";
        }
    }
}
