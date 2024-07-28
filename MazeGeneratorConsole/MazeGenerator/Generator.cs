using MazeGenerator.Models.GenerationModels;
using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace MazeGenerator
{
    public class Generator
    {
        private ChunkForGeneration _chunk;
        private GenerationWeights _weightsForGeneration;
        private Random _random;

        private const int MIN_LENGTH = 3;
        private const int MIN_WIDTH = 3;

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
            int? seed = null)
        {
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

        public Chunk GenerateChunk(ChunkGeneratorConfig config)
        {
            _random = new Random(config.Seed);
            _weightsForGeneration = config.WeightsForGeneration;
            _chunk = new ChunkForGeneration
            {
                Width = config.Width,
                Height = config.Height,
                Legnth = config.Legnth,
            };

            BuildFullWalls();
            BuildCorridors(config.StartPoint);
            BuildExit(config.EndPoint);

            var chunk = new Chunk
            {
                Length = _chunk.Legnth,
                Width = _chunk.Width,
                Height = _chunk.Height,
                Seed = config.Seed,
                Cells = _chunk.Cells.Select(x =>
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
            return chunk;
        }

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

        private void BuildFullWalls()
        {
            for (int z = 0; z < _chunk.Height; z++)
            {
                for (int y = 0; y < _chunk.Width; y++)
                {
                    for (int x = 0; x < _chunk.Legnth; x++)
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

        private void BuildCorridors(Vector2? startPoint)
        {
            var miner = new Miner();
            var highestLevelZ = _chunk.Height - 1;
            var startingCell = startPoint.HasValue
                ? _chunk[startPoint.Value.X, startPoint.Value.Y, highestLevelZ]
                : _random.GetRandomFrom(_chunk
                    .Cells
                    .Where(x => x.Z == highestLevelZ)
                    .ToList());
            startingCell.InnerPart = InnerPart.Start;

            miner.CurrentCell = startingCell;
            miner.CurrentCell.State = BuildingState.Visited;

            while (_chunk.Cells.Any(x => x.State == BuildingState.Visited))
            {
                miner.CurrentCell.State = BuildingState.Visited;
                var cellsAvailableToStep = GetCellsAvailableToStep(miner)
                    .ToList();
                if (cellsAvailableToStep.Count() == 0)
                {
                    miner.CurrentCell.State = BuildingState.Finished;
                    var visitedCells = _chunk.Cells
                        .Where(x => x.State == BuildingState.Visited)
                        .ToList();
                    if (visitedCells.Count == 0)
                    {
                        break;
                    }
                    miner.CurrentCell = _random.GetRandomFrom(visitedCells);
                    continue;
                }
                // TODO use a weight for each option when call random
                var cellToStep = _random.GetRandomFromByWeight(cellsAvailableToStep);
                var movmentVector = cellToStep - miner.CurrentCell;
                // if z == 0 it means that miner moving on the same level
                if (movmentVector.Z == 0)
                {
                    BreakWallsBetweenCells(miner.CurrentCell, cellToStep);
                    miner.CurrentCell = cellToStep;
                }
                else // if z != 0 it means that we a build a stair
                {
                    /// Scheme 1 of the breaken walls to the stair
                    /// _ 2→3
                    ///   ↑
                    /// @→1 _
                    /// Stair will be on firt cell (cell1)

                    /// Scheme 2 of the breaken walls to the stair
                    /// @→1 _
                    ///   ↓
                    /// _ 2→3
                    /// Stair will be on second cell (cell2)

                    var vectorToTheCell1 = new Vector3(
                        movmentVector.X / 2,
                        movmentVector.Y / 2,
                        0);
                    var cell1 = GetCellByDirection(miner.CurrentCell, vectorToTheCell1)!;
                    BreakWallsBetweenCells(miner.CurrentCell, cell1);
                    cell1.State = BuildingState.Finished;
                    // Scheme 1. Step up. Build stair on cell1
                    if (movmentVector.Z > 0)
                    {
                        cell1.InnerPart = ChooseStairByVector(vectorToTheCell1);
                    }

                    var vectorToTheCell2 = new Vector3(
                        movmentVector.X / 2,
                        movmentVector.Y / 2,
                        movmentVector.Z);
                    var cell2 = GetCellByDirection(miner.CurrentCell, vectorToTheCell2)!;
                    BreakWallsBetweenCells(cell1, cell2);
                    cell2.State = BuildingState.Finished;
                    // Scheme 2. Step down. Build stair on cell2
                    if (movmentVector.Z < 0)
                    {
                        // To build stair down we build stair to up but in reverse direction
                        var reverseVectorToCell1 = -vectorToTheCell1;
                        cell2.InnerPart = ChooseStairByVector(reverseVectorToCell1);
                    }

                    var cell3 = GetCellByDirection(miner.CurrentCell, movmentVector)!;
                    BreakWallsBetweenCells(cell2, cell3);
                    cell3.State = BuildingState.Visited;

                    miner.CurrentCell = cell3;
                }
            }
        }

        private InnerPart ChooseStairByVector(Vector3 vectorToTheCell1)
        {
            if (vectorToTheCell1.X == 1)
            {
                return InnerPart.StairFromWestToEast;
            }
            if (vectorToTheCell1.X == -1)
            {
                return InnerPart.StairFromEastToWest;
            }

            if (vectorToTheCell1.Y == 1)
            {
                return InnerPart.StairFromSouthToNorth;
            }
            if (vectorToTheCell1.Y == -1)
            {
                return InnerPart.StairFromNorthToSouth;
            }

            throw new Exception("Unexpected vector to building stairs");
        }

        private void BreakWallsBetweenCells(CellForGeneration currentCell, CellForGeneration cellToStep)
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

        private IEnumerable<OptionWithWeight<CellForGeneration>> GetCellsAvailableToStep(Miner miner)
        {
            var centralCell = miner.CurrentCell;
            var availableDirectionOnTheSameLevel =
                GetAvailableDirectionsOnTheSameLevel(centralCell);

            foreach (var vectorToCell in availableDirectionOnTheSameLevel)
            {
                var cell = GetCellByDirection(centralCell, vectorToCell)!;
                yield return new OptionWithWeight<CellForGeneration>
                {
                    Option = cell,
                    Weight = _weightsForGeneration
                        .CalculateWeightForStep(vectorToCell, miner.LastStepDirection)
                };

                var cellOnTheLevelAbove = GetCellOnTheLevelAbove(centralCell, vectorToCell);
                if (cellOnTheLevelAbove != null)
                {
                    yield return new OptionWithWeight<CellForGeneration>
                    {
                        Option = cellOnTheLevelAbove,
                        Weight = _weightsForGeneration.
                            CalculateWeightForStair(_chunk, cellOnTheLevelAbove)
                    };
                }

                var cellOnTheLevelBelow = GetCellOnTheLevelBelow(centralCell, vectorToCell);
                if (cellOnTheLevelBelow != null)
                {
                    yield return new OptionWithWeight<CellForGeneration>
                    {
                        Option = cellOnTheLevelBelow,
                        Weight = _weightsForGeneration.
                            CalculateWeightForStair(_chunk, cellOnTheLevelBelow)
                    };
                }
            }
        }

        /// <summary>
        /// Ceck can we add cell on the level above to available cells.
        /// If yes, return the cell
        /// If no, return the null
        /// </summary>
        /// <param name="centralCell"></param>
        /// <param name="vectorToCell1"></param>
        /// <returns></returns>
        private CellForGeneration? GetCellOnTheLevelBelow(CellForGeneration centralCell, Vector3 vectorToCell1)
        {
            /// @→1 _
            ///   ↓
            /// _ 2→3

            var vectorToCell2 = new Vector3(
                   vectorToCell1.X,
                   vectorToCell1.Y,
                   vectorToCell1.Z - 1);
            var cell2 = GetCellByDirection(centralCell, vectorToCell2);
            if (cell2?.State != BuildingState.New)
            {
                return null;
            }

            var vectorToCell3 = new Vector3(
                vectorToCell1.X * 2,
                vectorToCell1.Y * 2,
                vectorToCell1.Z - 1);
            var cell3 = GetCellByDirection(centralCell, vectorToCell3);

            if (cell3?.State != BuildingState.New)
            {
                return null;
            }

            return cell3;
        }

        /// <summary>
        /// Ceck can we add cell on the level above to available cells.
        /// If yes, return the cell
        /// If no, return the null
        /// </summary>
        /// <param name="centralCell"></param>
        /// <param name="vectorToCell1"></param>
        /// <returns></returns>
        private CellForGeneration? GetCellOnTheLevelAbove(CellForGeneration centralCell, Vector3 vectorToCell1)
        {
            /// _ 2→3
            ///   ↑
            /// @→1 _

            var vectorToCell2 = new Vector3(
                   vectorToCell1.X,
                   vectorToCell1.Y,
                   vectorToCell1.Z + 1);
            var cell2 = GetCellByDirection(centralCell, vectorToCell2);
            if (cell2?.State != BuildingState.New)
            {
                return null;
            }

            var vectorToCell3 = new Vector3(
                vectorToCell1.X * 2,
                vectorToCell1.Y * 2,
                vectorToCell1.Z + 1);
            var cell3 = GetCellByDirection(centralCell, vectorToCell3);

            if (cell3?.State != BuildingState.New)
            {
                return null;
            }

            return cell3;
        }

        private IEnumerable<Vector3> GetAvailableDirectionsOnTheSameLevel(CellForGeneration centralCell)
        {
            if (_chunk[centralCell.X - 1, centralCell.Y, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(-1, 0, 0);
            }
            if (_chunk[centralCell.X + 1, centralCell.Y, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(1, 0, 0);
            }
            if (_chunk[centralCell.X, centralCell.Y - 1, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(0, -1, 0);
            }
            if (_chunk[centralCell.X, centralCell.Y + 1, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(0, 1, 0);
            }
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

        private Wall AllWalls()
                => Wall.North | Wall.East | Wall.South | Wall.West | Wall.Roof | Wall.Floor;

        private Wall BreakWall(Wall currentWall, Wall wallToBreak)
        {
            return currentWall & ~wallToBreak;
        }

        private CellForGeneration? GetCellByDirection(CellForGeneration centralCell, Vector3 vector3)
            => _chunk[centralCell.X + vector3.X, centralCell.Y + vector3.Y, centralCell.Z + vector3.Z];
    }
}
