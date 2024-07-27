namespace MazeGenerator.Models.MazeModels
{
    public enum InnerPart
    {
        None,
        /// <summary>
        /// Y axe +1
        /// </summary>
        StairFromSouthToNorth,
        /// <summary>
        /// Y axe -1
        /// </summary>
        StairFromNorthToSouth,
        /// <summary>
        /// X axe +1
        /// </summary>
        StairFromWestToEast,
        /// <summary>
        /// X axe -1
        /// </summary>
        StairFromEastToWest,
        
        Start,
        /// <summary>
        /// Exit from maze
        /// </summary>
        Exit,
        ExitFromChunk,
    }
}
