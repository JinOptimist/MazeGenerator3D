namespace MazeGenerator
{
    public class GeneratorConfig
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
       

        public bool IsLongCorridors { get; set; }
        public int RandomSeed { get; set; }
    }
}
