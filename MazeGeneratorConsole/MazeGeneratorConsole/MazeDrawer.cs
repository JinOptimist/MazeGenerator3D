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
        public const char START = 'S';
        public const char EXIT = 'X';
        public const char DOT = '.';

        public const int MARGIN = 2;

        public void ClearDraw(Maze maze)
        {
            var rowsDrawedPrevChunk = 0;
            var drawedLevels = 0;
            for (int chunkIndex = 0; chunkIndex < maze.Chunks.Count; chunkIndex++)
            {
                var chunk = maze.Chunks[chunkIndex];
                rowsDrawedPrevChunk += ChunkDraw(chunk, rowsDrawedPrevChunk, chunkIndex, drawedLevels);
                drawedLevels += chunk.Height;
            }
            Console.WriteLine($"Seed: {maze.Seed}");
        }

        private int ChunkDraw(Chunk chunk, int rowsDrawedPrevChunks, int chunkIndex, int drawedLevels)
        {
            var rowsDrawedCurentChunk = 0;
            for (int z = 0; z < chunk.Height; z++)
            {
                DrawOneLevel(chunk, z, rowsDrawedPrevChunks, drawedLevels);
                rowsDrawedCurentChunk += chunk.Width + MARGIN;
            }

            rowsDrawedCurentChunk += 1;
            Console.SetCursorPosition(0, rowsDrawedCurentChunk + rowsDrawedPrevChunks - 1);
            Console.WriteLine($" ------------------------ END Chunk {chunkIndex} ----------");
            return rowsDrawedCurentChunk;
        }

        private void DrawOneLevel(Chunk chunk,
            int level,
            int rowsDrawedPrevChunks,
            int drawedLevels)
        {
            var oneLevelHeightCells = chunk.Width + MARGIN;
            var levelMargin = level * oneLevelHeightCells + rowsDrawedPrevChunks;
            Console.SetCursorPosition(0, MARGIN - 2 + levelMargin);
            Console.Write($"Lvl - {level} [{chunk.Length}, {chunk.Width}]. " +
                $"Full Lvl - {drawedLevels + level}");

            //Draw upper border
            for (int x = 0; x < chunk.Length * 2; x++)
            {
                var yMargin = MARGIN - 1 + levelMargin;
                Console.SetCursorPosition(MARGIN + x, yMargin);
                Console.Write(SOUTH);
            }

            //Draw easten border
            for (int y = 0; y < chunk.Width; y++)
            {
                var xMargin = chunk.Length * 2 + MARGIN;
                var yMargin = (chunk.Width - y - 1) + MARGIN + levelMargin;
                Console.SetCursorPosition(xMargin, yMargin);
                Console.Write(WEST);
            }

            // Draw cells
            for (int y = 0; y < chunk.Width; y++)
            {
                for (int x = 0; x < chunk.Length; x++)
                {
                    var xMargin = x * 2 + MARGIN;
                    var yMargin = (chunk.Width - y - 1) + MARGIN + levelMargin;
                    var cell = chunk[x, y, level]!;
                    var walls = cell.Wall;

                    Console.SetCursorPosition(xMargin + 1, yMargin);
                    if (cell.InnerPart != InnerPart.None)
                    {
                        switch (cell.InnerPart)
                        {
                            case InnerPart.StairFromSouthToNorth:
                            case InnerPart.StairFromNorthToSouth:
                            case InnerPart.StairFromWestToEast:
                            case InnerPart.StairFromEastToWest:
                                Console.Write(STAIR);
                                break;
                            case InnerPart.Start:
                                Console.Write(START);
                                break;
                            case InnerPart.Exit:
                                Console.Write(EXIT);
                                break;
                            default:
                                break;
                        }
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

            Console.SetCursorPosition(chunk.Length * 2 + MARGIN, chunk.Width + MARGIN + levelMargin);
        }

        [Obsolete]
        public void FullDraw(Chunk maze)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var widthInCell = maze.Width * 3 + MARGIN;

            for (int y = 0; y < maze.Width; y++)
            {
                for (int x = 0; x < maze.Length; x++)
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

            Console.SetCursorPosition(maze.Length * 3 + MARGIN * 2, maze.Width * 3 + MARGIN * 2);
        }

    }
}
