using MazeGenerator.Models.GenerationModels;
using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MazeGenerator.Generators
{
    public abstract class BaseGenerator
    {
        private const int MIN_LENGTH = 3;
        private const int MIN_WIDTH = 3;

        protected Random _random;
        protected ChunkForGeneration _chunk;
        protected GenerationWeights _weightsForGeneration;

        protected Action<Chunk> _debugDrawer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxLength">Axe X</param>
        /// <param name="maxWidth">Axe Y</param>
        /// <param name="height">Axe Z</param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="weights"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Maze Generate(int maxLength = 6, int maxWidth = 7, int height = 5,
            int chunkHeight = 3,
            Vector2? startPoint = null,
            Vector2? endPoint = null,
            GenerationWeights? weights = null,
            int? seed = null,
            Action<Chunk> debugDrawer = null)
        {
            _debugDrawer = debugDrawer;

            var defaultGeneratorConfig = new MazeGeneratorConfig
            {
                Legnth = maxLength,
                Width = maxWidth,
                Height = height,
                StartPoint = startPoint,
                EndPoint = endPoint,
                ChunkHeight = chunkHeight,
                WeightsForGeneration = weights ?? new GenerationWeights(),
                Seed = seed ?? DateTime.Now.Millisecond,
            };

            return Generate(defaultGeneratorConfig);
        }

        public Maze Generate(MazeGeneratorConfig config)
        {
            var chunkCount = CalcChunkCount(config);
            var chunks = new List<Chunk>();
            Vector2? lastExit = null;
            for (int chunkNumber = chunkCount - 1; chunkNumber >= 0; chunkNumber--)
            {
                var configForNextChunk = BuildConfigForChunk(
                    config,
                    chunkNumber,
                    lastExit,
                    chunkCount);

                var chunk = GenerateChunk(configForNextChunk);
                chunks.Add(chunk);

                var exit = chunk.GetExitCell();
                // Each chunk start counting Z index from Zero
                lastExit = new Vector2(exit.X, exit.Y);
            }

            BuildPathwayBetweenChunks(chunks);

            var maze = new Maze
            {
                Chunks = chunks,
                FullHeight = chunks.Sum(x => x.Height),
                MaxLength = chunks.Max(x => x.Length),
                MaxWidth = chunks.Max(x => x.Width),
                Seed = config.Seed,
            };
            return maze;
        }

        protected abstract void BuildCorridors();

        private ChunkGeneratorConfig BuildConfigForChunk(
            MazeGeneratorConfig initialConfig,
            int chunkNumber,
            Vector2? lastExit,
            int chunkCount)
        {
            var chunkHeight = CalcChunkHeight(
                    initialConfig.ChunkHeight,
                    chunkNumber,
                    initialConfig.Height);

            var configForNextChunk = new ChunkGeneratorConfig
            {
                Seed = initialConfig.Seed,
                Legnth = Math.Max(
                    initialConfig.Legnth - chunkNumber,
                    MIN_LENGTH),
                Width = Math.Max(
                    initialConfig.Width - chunkNumber,
                    MIN_WIDTH),
                Height = chunkHeight,
                StartPoint = lastExit ?? initialConfig.StartPoint,
                WeightsForGeneration = initialConfig.WeightsForGeneration,
            };

            return configForNextChunk;
        }

        private Chunk GenerateChunk(ChunkGeneratorConfig config)
        {
            _random = new Random(config.Seed);
            _weightsForGeneration = config.WeightsForGeneration;
            _chunk = new ChunkForGeneration
            {
                Width = config.Width,
                Height = config.Height,
                Length = config.Legnth,
                Seed = config.Seed,
            };

            BuildFullWalls();
            BuildStart(config.StartPoint);
            BuildCorridors();
            BuildExit(config.EndPoint);

            return MapToChunk(_chunk);
        }

        private void BuildStart(Vector2? startPoint)
        {
            var highestLevelZ = _chunk.Height - 1;
            var startingCell = startPoint.HasValue
                ? _chunk[startPoint.Value.X, startPoint.Value.Y, highestLevelZ]
                : _random.GetRandomFrom(_chunk
                    .Cells
                    .Where(x => x.Z == highestLevelZ)
                    .ToList());
            startingCell.InnerPart = InnerPart.Start;
        }

        private int CalcChunkCount(MazeGeneratorConfig config)
        {
            return config.Height % config.ChunkHeight == 0
                ? config.Height / config.ChunkHeight
                : config.Height / config.ChunkHeight + 1;
        }

        private int CalcChunkHeight(int height, int chunkNumber, int fullHeight)
        {
            if (height * (chunkNumber + 1) > fullHeight)
            {
                return fullHeight % height;
            }
            else
            {
                return height;
            }
        }

        protected void BuildFullWalls()
        {
            for (int z = 0; z < _chunk.Height; z++)
            {
                for (int y = 0; y < _chunk.Width; y++)
                {
                    for (int x = 0; x < _chunk.Length; x++)
                    {
                        var cell = new CellForGeneration()
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            State = BuildingState.New,
                            Wall = AllWalls()
                        };
                        _chunk.Cells.Add(cell);
                    }
                }
            }
        }

        private void BuildPathwayBetweenChunks(List<Chunk> chunks)
        {
            // Get all exists except last one and break floor
            chunks
                .Select(x => x.GetExitCell())
                .OrderBy(x => x.Z)
                .Reverse()
                .Skip(1)
                .ToList()
                .ForEach(cell =>
                {
                    cell.Wall = BreakWall(cell.Wall, Wall.Floor);
                    cell.InnerPart = InnerPart.ExitFromChunk;
                });

            // for each start break roof
            chunks
                .Select(x => x.GetStartCell())
                .ToList()
                .ForEach(x => x.Wall = BreakWall(x.Wall, Wall.Roof));
        }

        private Wall AllWalls()
                => Wall.North | Wall.East | Wall.South | Wall.West | Wall.Roof | Wall.Floor;

        protected Wall BreakWall(Wall currentWall, Wall wallToBreak)
        {
            return currentWall & ~wallToBreak;
        }

        protected void BreakWallsBetweenCells(CellForGeneration currentCell, CellForGeneration cellToStep)
        {
            var movmentDirection = cellToStep - currentCell;
            if (movmentDirection.Length() != 1)
            {
                throw new Exception("We can break walls only between nearest cells");
            }
            if (movmentDirection.X == 1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.East);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.West);
                return;
            }
            if (movmentDirection.X == -1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.West);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.East);
                return;
            }
            if (movmentDirection.Y == 1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.North);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.South);
                return;
            }
            if (movmentDirection.Y == -1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.South);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.North);
                return;
            }
            if (movmentDirection.Z == 1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.Roof);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.Floor);
                return;
            }
            if (movmentDirection.Z == -1)
            {
                currentCell.Wall = BreakWall(currentCell.Wall, Wall.Floor);
                cellToStep.Wall = BreakWall(cellToStep.Wall, Wall.Roof);
                return;
            }
        }

        protected InnerPart ChooseStairByVector(Vector3 vectorToTheCell1)
        {
            if (vectorToTheCell1.X == 1)
            {
                return InnerPart.StairUpOnEast;
            }
            if (vectorToTheCell1.X == -1)
            {
                return InnerPart.StairUpOnWest;
            }

            if (vectorToTheCell1.Y == 1)
            {
                return InnerPart.StairUpOnNorth;
            }
            if (vectorToTheCell1.Y == -1)
            {
                return InnerPart.StairUpOnSouth;
            }

            throw new Exception("Unexpected vector to building stairs");
        }

        protected InnerPart ChooseStairByVectorV2ForGraph(Vector3 vectorToTheCell1)
        {
            if (vectorToTheCell1.X != 0)
            {
                if (vectorToTheCell1.Z * vectorToTheCell1.X == -1)
                {
                    // If we go to West (X: -1) and up (Z: +1)
                    // or we go to East (X: +1) and down (Z: -1)
                    return InnerPart.StairUpOnWest;
                }
                else
                {
                    // If we go to West (X: -1) and down (Z: +1)
                    // or we go to East (X: +1) and up (Z: -1)
                    return InnerPart.StairUpOnEast;
                }
            }

            if (vectorToTheCell1.Y != 0)
            {
                if (vectorToTheCell1.Z * vectorToTheCell1.Y == -1)
                {
                    // If we go to South (Y: -1) and up (Z: +1)
                    // or we go to North (Y: +1) and down (Z: -1)
                    return InnerPart.StairUpOnSouth;
                }
                else
                {
                    // If we go to South (Y: -1) and down (Z: -1)
                    // or we go to North (Y: +1) and up (Z: +1)
                    return InnerPart.StairUpOnNorth;
                }
            }

            throw new Exception("Unexpected vector to building stairs");
        }

        protected Chunk MapToChunk(ChunkForGeneration chunk)
            => new Chunk
            {
                Length = chunk.Length,
                Width = chunk.Width,
                Height = chunk.Height,
                Seed = chunk.Seed,
                Cells = chunk.Cells.Select(x =>
                    // Build Cell base on CellForGeneration for maze.Cells
                    new Cell
                    {
                        X = x.X,
                        Y = x.Y,
                        Z = x.Z,
                        Wall = x.Wall,
                        InnerPart = x.InnerPart,
                    })
                .ToList()
            };

        private void BuildExit(Vector2? endPoint)
        {
            var lowestLevelZ = 0;
            CellForGeneration endCell;
            if (endPoint.HasValue)
            {
                endCell = _chunk[endPoint.Value.X, endPoint.Value.Y, lowestLevelZ];
            }
            else
            {
                var allEmptyCellsFromLastLevel = _chunk.Cells
                    .Where(x =>
                        x.InnerPart == InnerPart.None
                        && x.State == BuildingState.Finished
                        && x.Z == lowestLevelZ);
                var deadEnds = allEmptyCellsFromLastLevel
                    .Where(cell => GetNearCellsSameLevel(cell)
                        .Where(nearCell => nearCell != null)
                        .Count() == 1)
                    .ToList();
                if (deadEnds.Any())
                {
                    endCell = _random.GetRandomFrom(deadEnds);
                }
                else
                {
                    // Maybe we haven't deadends.
                    // Then build exist on random cell on the last floor
                    endCell = _random.GetRandomFrom(allEmptyCellsFromLastLevel.ToList());
                }
            }

            endCell.InnerPart = InnerPart.Exit;
        }

        private IEnumerable<CellForGeneration> GetNearCellsSameLevel(CellForGeneration centralCell)
        {
            if (centralCell.Wall.HasFlag(Wall.West))
            {
                yield return _chunk[centralCell.X - 1, centralCell.Y, centralCell.Z];
            }
            if (centralCell.Wall.HasFlag(Wall.East))
            {
                yield return _chunk[centralCell.X + 1, centralCell.Y, centralCell.Z];
            }
            if (centralCell.Wall.HasFlag(Wall.South))
            {
                yield return _chunk[centralCell.X, centralCell.Y - 1, centralCell.Z];
            }
            if (centralCell.Wall.HasFlag(Wall.North))
            {
                yield return _chunk[centralCell.X, centralCell.Y + 1, centralCell.Z];
            }
        }

    }
}
