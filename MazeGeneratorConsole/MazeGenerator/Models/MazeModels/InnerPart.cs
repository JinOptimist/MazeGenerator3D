namespace MazeGenerator.Models.MazeModels
{
    public enum InnerPart
    {
        None,
        /// <summary>
        /// Y: +1 Z: +1
        /// </summary>
        StairUpOnNorth,
        /// <summary>
        /// Y: -1 Z: +1
        /// </summary>
        StairUpOnSouth,
        /// <summary>
        /// X: +1 Z: +1
        /// </summary>
        StairUpOnEast,
        /// <summary>
        /// X: -1 Z: +1
        /// </summary>
        StairUpOnWest,
        
        Start,
        /// <summary>
        /// Exit from maze
        /// </summary>
        Exit,
        ExitFromChunk,
    }
}
