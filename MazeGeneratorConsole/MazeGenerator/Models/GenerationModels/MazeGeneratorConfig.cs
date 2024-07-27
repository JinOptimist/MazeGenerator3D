using MazeGenerator.Models.GenerationModels;
using System.Numerics;

namespace MazeGenerator
{
    public class MazeGeneratorConfig
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

        /// <summary>
        /// Height of each chunks.
        /// Chunks will have different complexity
        /// </summary>
        public int ChunkHeight { get; set; }

        /// <summary>
        /// We use Vector2 cause start will be always on the first one level
        /// So Z-index will 0
        /// You can choose only X and Y
        /// </summary>
        public Vector2? StartPoint { get; set; }

        /// <summary>
        /// We use Vector2 cause exit will be always on the last one level
        /// So Z-index will Height - 1
        /// You can choose only X and Y
        /// </summary>
        public Vector2? EndPoint { get; set; }

        public GenerationWeights WeightsForGeneration { get; set; }

        public int Seed { get; set; }
    }
}
