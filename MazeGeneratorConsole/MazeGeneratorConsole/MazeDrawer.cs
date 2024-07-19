using MazeGenerator.Models.MazeModels;

namespace MazeGeneratorConsole
{
    public class MazeDrawer
    {
        public const char NORTH = '‾';
        public const char SOUTH = '_';
        public const char WEST = '|';
        public const char EAST = '|';
        public const char STAIR = '^';
        public const char DOT = '.';

        public const int MARGIN = 2;

        public void FullDraw(Maze maze)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var widthInCell = maze.Width * 3 + MARGIN;

            for (int y = 0; y < maze.Width; y++)
            {
                for (int x = 0; x < maze.Legnth; x++)
                {
                    var xMargin = x * 3 + MARGIN;
                    var yMargin = (maze.Width - y - 1) * 3 + MARGIN;
                    var walls = maze[x, y, 0]!.Wall;

                    if (walls.HasFlag(Wall.North))
                    {
                        Console.SetCursorPosition(xMargin + 1, yMargin + 0);
                        Console.Write(NORTH);
                    }
                    if (walls.HasFlag(Wall.East))
                    {
                        Console.SetCursorPosition(xMargin + 2, yMargin + 1);
                        Console.Write(EAST);
                    }
                    if (walls.HasFlag(Wall.South))
                    {
                        Console.SetCursorPosition(xMargin + 1, yMargin + 2);
                        Console.Write(SOUTH);
                    }
                    if (walls.HasFlag(Wall.West))
                    {
                        Console.SetCursorPosition(xMargin + 0, yMargin + 1);
                        Console.Write(WEST);
                    }

                    Console.SetCursorPosition(xMargin + 0, yMargin + 0);
                    Console.Write(DOT);
                    Console.SetCursorPosition(xMargin + 0, yMargin + 2);
                    Console.Write(DOT);
                    Console.SetCursorPosition(xMargin + 2, yMargin + 0);
                    Console.Write(DOT);
                    Console.SetCursorPosition(xMargin + 2, yMargin + 2);
                    Console.Write(DOT);
                }
            }

            Console.SetCursorPosition(maze.Legnth * 3 + MARGIN * 2, maze.Width * 3 + MARGIN * 2);
        }

        public void ClearDraw(Maze maze)
        {
            for (int z = 0; z < maze.Height; z++)
            {
                DrawOneLevel(maze, z);
            }
        }

        private void DrawOneLevel(Maze maze, int level)
        {
            var oneLevelHeightCells = maze.Height + MARGIN;
            var levelMargin = level * oneLevelHeightCells;
            Console.SetCursorPosition(0, MARGIN - 2 + levelMargin);
            Console.Write($"Level - {level}");

            //Draw upper border
            for (int x = 0; x < maze.Legnth * 2; x++)
            {
                var yMargin = MARGIN - 1 + levelMargin;
                Console.SetCursorPosition(MARGIN + x, yMargin);
                Console.Write(SOUTH);
            }

            //Draw easten border
            for (int y = 0; y < maze.Width; y++)
            {
                var xMargin = maze.Width * 2 + MARGIN;
                var yMargin = (maze.Width - y - 1) + MARGIN + levelMargin;
                Console.SetCursorPosition(xMargin, yMargin);
                Console.Write(WEST);
            }

            // Draw cells
            for (int y = 0; y < maze.Width; y++)
            {
                for (int x = 0; x < maze.Legnth; x++)
                {
                    var xMargin = x * 2 + MARGIN;
                    var yMargin = (maze.Width - y - 1) + MARGIN + levelMargin;
                    var cell = maze[x, y, level]!;
                    var walls = cell.Wall;

                    Console.SetCursorPosition(xMargin + 1, yMargin);
                    if (cell.InnerPart != InnerPart.None)
                    {
                        Console.Write(STAIR);
                    }
                    else if (walls.HasFlag(Wall.South))
                    {
                        Console.Write(SOUTH);
                    }

                    Console.SetCursorPosition(xMargin + 0, yMargin);
                    if (walls.HasFlag(Wall.West))
                    {
                        Console.Write(WEST);
                    }
                    else
                    {
                        Console.Write(SOUTH);
                    }
                }
            }

            Console.SetCursorPosition(maze.Legnth * 2 + MARGIN, maze.Width + MARGIN + levelMargin);
        }
    }
}
