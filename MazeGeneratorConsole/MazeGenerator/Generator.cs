using MazeGenerator.Models.GenerationModels;
using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace MazeGenerator
{
    public class Generator
    {
        private MazeForGeneration _maze;
        private GenerationWeights _weightsGenerator;
        private Random _random;

        public Maze Generate(int length = 6, int width = 7, int height = 5,
            Vector3? startPoint = null,
            Vector3? endPoint = null,
            GenerationWeights weights = null,
            int? seed = null)
        {
            var defaultGeneratorConfig = new GeneratorConfig
            {
                Legnth = length,
                Width = width,
                Height = height,
                StartPoint = startPoint,
                EndPoint = endPoint,
                GenerationWeights = weights ?? new GenerationWeights(),
                Seed = seed ?? DateTime.Now.Millisecond,
            };

            return Generate(defaultGeneratorConfig);
        }

        public Maze Generate(GeneratorConfig config)
        {
            _random = new Random(config.Seed);
            _weightsGenerator = config.GenerationWeights;
            _maze = new MazeForGeneration
            {
                Width = config.Width,
                Height = config.Height,
                Legnth = config.Legnth,
            };

            BuildFullWalls();
            BuildCorridors(config.StartPoint);
            BuildExit(config.EndPoint);

            // Build Cell base on CellForGeneration for maze.Cells
            var maze = new Maze
            {
                Length = _maze.Legnth,
                Width = _maze.Width,
                Height = _maze.Height,
                Seed = config.Seed,
                Cells = _maze.Cells.Select(x =>
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
            return maze;
        }

        private void BuildExit(Vector3? endPoint)
        {
            var allEmptyCells = _maze.Cells
                .Where(x =>
                    x.InnerPart == InnerPart.None
                    && x.State == BuildingState.Finished)
                .ToList();

            var endCell = endPoint.HasValue
                ? _maze[endPoint.Value.X, endPoint.Value.Y, endPoint.Value.Z]
                : _random.GetRandomFrom(allEmptyCells);
            endCell.InnerPart = InnerPart.Exit;
        }

        private void BuildFullWalls()
        {
            for (int z = 0; z < _maze.Height; z++)
            {
                for (int y = 0; y < _maze.Width; y++)
                {
                    for (int x = 0; x < _maze.Legnth; x++)
                    {
                        var cell = new CellForGeneration()
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            State = BuildingState.New,
                            Wall = AllWalls()
                        };
                        _maze.Cells.Add(cell);
                    }
                }
            }
        }

        private void BuildCorridors(Vector3? startPoint)
        {
            var miner = new Miner();
            var startingCell = startPoint.HasValue
                ? _maze[startPoint.Value.X, startPoint.Value.Y, startPoint.Value.Z]
                : _random.GetRandomFrom(_maze.Cells);
            startingCell.InnerPart = InnerPart.Start;

            miner.CurrentCell = startingCell;
            miner.CurrentCell.State = BuildingState.Visited;

            while (_maze.Cells.Any(x => x.State == BuildingState.Visited))
            {
                miner.CurrentCell.State = BuildingState.Visited;
                var cellsAvailableToStep = GetCellsAvailableToStep(miner)
                    .ToList();
                if (cellsAvailableToStep.Count() == 0)
                {
                    miner.CurrentCell.State = BuildingState.Finished;
                    var visitedCells = _maze.Cells
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
                currentCell.Wall &= ~Wall.East;
                cellToStep.Wall &= ~Wall.West;
                return;
            }
            if (movmentDirection.X == -1)
            {
                currentCell.Wall &= ~Wall.West;
                cellToStep.Wall &= ~Wall.East;
                return;
            }
            if (movmentDirection.Y == 1)
            {
                currentCell.Wall &= ~Wall.North;
                cellToStep.Wall &= ~Wall.South;
                return;
            }
            if (movmentDirection.Y == -1)
            {
                currentCell.Wall &= ~Wall.South;
                cellToStep.Wall &= ~Wall.North;
                return;
            }
            if (movmentDirection.Z == 1)
            {
                currentCell.Wall &= ~Wall.Roof;
                cellToStep.Wall &= ~Wall.Floor;
                return;
            }
            if (movmentDirection.Z == -1)
            {
                currentCell.Wall &= ~Wall.Floor;
                cellToStep.Wall &= ~Wall.Roof;
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
                    Weight = _weightsGenerator
                        .CalculateWeightForStep(vectorToCell, miner.LastStepDirection)
                };

                var cellOnTheLevelAbove = GetCellOnTheLevelAbove(centralCell, vectorToCell);
                if (cellOnTheLevelAbove != null)
                {
                    yield return new OptionWithWeight<CellForGeneration>
                    {
                        Option = cellOnTheLevelAbove,
                        Weight = _weightsGenerator.
                            CalculateWeightForStair(_maze, cellOnTheLevelAbove)
                    };
                }

                var cellOnTheLevelBelow = GetCellOnTheLevelBelow(centralCell, vectorToCell);
                if (cellOnTheLevelBelow != null)
                {
                    yield return new OptionWithWeight<CellForGeneration>
                    {
                        Option = cellOnTheLevelBelow,
                        Weight = _weightsGenerator.
                            CalculateWeightForStair(_maze, cellOnTheLevelBelow)
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
            if (_maze[centralCell.X - 1, centralCell.Y, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(-1, 0, 0);
            }
            if (_maze[centralCell.X + 1, centralCell.Y, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(1, 0, 0);
            }
            if (_maze[centralCell.X, centralCell.Y - 1, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(0, -1, 0);
            }
            if (_maze[centralCell.X, centralCell.Y + 1, centralCell.Z]?.State == BuildingState.New)
            {
                yield return new Vector3(0, 1, 0);
            }
        }

        private Wall AllWalls()
                => Wall.North | Wall.East | Wall.South | Wall.West | Wall.Roof | Wall.Floor;

        private CellForGeneration? GetCellByDirection(CellForGeneration centralCell, Vector3 vector3)
            => _maze[centralCell.X + vector3.X, centralCell.Y + vector3.Y, centralCell.Z + vector3.Z];
    }
}
