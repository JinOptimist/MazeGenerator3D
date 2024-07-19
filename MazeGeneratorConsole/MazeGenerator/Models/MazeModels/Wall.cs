namespace MazeGenerator.Models
{
    [Flags]
    public enum Wall
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        Roof = 16,
        Floor = 32,
    }
}
