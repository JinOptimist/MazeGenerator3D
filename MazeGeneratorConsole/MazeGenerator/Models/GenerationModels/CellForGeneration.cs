namespace MazeGenerator.Models.GenerationModels
{
    public class CellForGeneration : Cell
    {
        public BuildingState State {  get; set; }
    }

    public enum BuildingState
    {
        New,
        Visited,
        Finished
    }
}
