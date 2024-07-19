using MazeGenerator.Models;
using MazeGenerator.Models.GenerationModels;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;

namespace MazeGenerator
{
    public class Generator
    {
        private MazeForGeneration _maze;
        private Random _random;

        public Maze Generate(int length = 6, int width = 7, int height = 5, int? seed = null)
        {
            var defaultGeneratorConfig = new GeneratorConfig
            {
                Legnth = length,
                Width = width,
                Height = height,
                RandomSeed = seed ?? DateTime.Now.Millisecond,
            };

            return Generate(defaultGeneratorConfig);
        }

        public Maze Generate(GeneratorConfig config)
        {
            _random = new Random(config.RandomSeed);
            _maze = new MazeForGeneration
            {
                Width = config.Width,
                Height = config.Height,
                Legnth = config.Legnth,
            };

            BuildFullWalls();
            BuildCorridors(config.IsLongCorridors);

            // Build Cell base on CellForGeneration for maze.Cells
            var maze = new Maze
            {
                Legnth = _maze.Legnth,
                Width = _maze.Width,
                Height = _maze.Height,
                Cells = _maze.Cells.Select(x =>
                    new Cell
                    {
                        X = x.X,
                        Y = x.Y,
                        Z = x.Z,
                        Wall = x.Wall,
                    })
                .ToList()
            };
            return maze;
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

        private void BuildCorridors(bool isLongCorridors)
        {
            var miner = new Miner();
            var startingCell = _random.GetRandomFrom(_maze.Cells);
            miner.CurrentCell = startingCell;
            miner.CurrentCell.State = BuildingState.Visited;
            //miner.LastStepDirection = Direction.North;

            while (_maze.Cells.Any(x => x.State == BuildingState.Visited))
            {
                miner.CurrentCell.State = BuildingState.Visited;
                var cellsAvailableToStep = GetCellsAvailableToStep(miner.CurrentCell)
                    .ToList();
                if (cellsAvailableToStep.Count == 0)
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
                var cellToStep = _random.GetRandomFrom(cellsAvailableToStep);
                var movmentVector = miner.CurrentCell - cellToStep;
                // if z == 0 it means that miner moving on the same level
                if (movmentVector.Z == 0)
                {
                    BreakWallsBetweenCells(miner.CurrentCell, cellToStep);
                    miner.CurrentCell = cellToStep;
                }
                else // if z != 0 it means that we a build a stair
                {


                }
            }

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

        private IEnumerable<CellForGeneration> GetCellsAvailableToStep(CellForGeneration centralCell)
        {
            var availableDirectionOnTheSameLevel =
                GetAvailableDirectionsOnTheSameLevel(centralCell);

            foreach (var vectorToCell in availableDirectionOnTheSameLevel)
            {
                var cell = GetCellByDirection(centralCell, vectorToCell)!;
                yield return cell;

                var cellOnTheLevelAbove = GetCellOnTheLevelAbove(centralCell, vectorToCell);
                if (cellOnTheLevelAbove != null)
                {
                    yield return cellOnTheLevelAbove;
                }

                var cellOnTheLevelBelow = GetCellOnTheLevelBelow(centralCell, vectorToCell);
                if (cellOnTheLevelBelow != null)
                {
                    yield return cellOnTheLevelBelow;
                }
            }
        }

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
