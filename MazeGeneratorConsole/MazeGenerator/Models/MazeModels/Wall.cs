using System;

namespace MazeGenerator.Models.MazeModels
{
    [Flags]
    public enum Wall
    {
        /// <summary>
        /// Y axe +1
        /// </summary>
        North = 1,
        /// <summary>
        /// X axe +1
        /// </summary>
        East = 2,
        /// <summary>
        /// Y axe -1
        /// </summary>
        South = 4,
        /// <summary>
        /// X axe -1
        /// </summary>
        West = 8,
        /// <summary>
        /// Z axe +1
        /// </summary>
        Roof = 16,
        /// <summary>
        /// Z axe -1
        /// </summary>
        Floor = 32,
    }
}
