using MazeGenerator.Models.GenerationModels;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MazeGenerator.Generators
{
    public class Generator : BaseGenerator
    {
        protected override void BuildCorridors()
        {
            var miner = new Miner();

            var startingCell = _chunk.GetStartCell();

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


        private CellForGeneration? GetCellByDirection(CellForGeneration centralCell, Vector3 vector3)
            => _chunk[centralCell.X + vector3.X, centralCell.Y + vector3.Y, centralCell.Z + vector3.Z];
    }
}
