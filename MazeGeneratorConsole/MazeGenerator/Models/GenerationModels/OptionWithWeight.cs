namespace MazeGenerator.Models.GenerationModels
{
    public class OptionWithWeight<T>
    {
        public T Option { get; set; }
        public double Weight { get; set; }
    }
}
