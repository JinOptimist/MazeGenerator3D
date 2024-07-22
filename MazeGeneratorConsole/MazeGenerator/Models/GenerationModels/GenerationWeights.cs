using System;
using System.Linq;
using System.Numerics;

namespace MazeGenerator.Models.GenerationModels
{
    public class GenerationWeights
    {
        private double _stairNotFirstWeight;
        /// <summary>
        /// Must be less the 1.0
        /// </summary>
        public double StairNotFirstWeight
        {
            get => _stairNotFirstWeight;
            set
            {
                if (value > 1.0)
                {
                    throw new ArgumentException("Must be less the 1.0");
                }
                _stairNotFirstWeight = value;
            }
        }
        public double StairFirstWeight { get; set; } = 4;
        public double StepForwardWeight { get; set; } = 50;
        public double StepRandomWeight { get; set; } = 0.01;

        public GenerationWeights() {
            StairNotFirstWeight = .3;
        }

        public GenerationWeights(
            double stairFirstWeight, double stairNotFirstWeight,
            double stepForwardWeight, double stepRandomWeight)
        {
            StairFirstWeight = stairFirstWeight;
            StairNotFirstWeight = stairNotFirstWeight;
            StepForwardWeight = stepForwardWeight;
            StepRandomWeight = stepRandomWeight;
        }

        public double CalculateWeightForStep(
            Vector3 curentStepDirection,
            Vector3 lastStepDirection)
            => curentStepDirection.X == lastStepDirection.X && curentStepDirection.Y == lastStepDirection.Y
                ? StepForwardWeight
                : StepRandomWeight;

        public double CalculateWeightForStair(MazeForGeneration maze, CellForGeneration cell)
        {
            var countOfStairsOnTheFloor = maze.Cells.Count(c => c.Z == cell.Z && c.IsStair);
            return countOfStairsOnTheFloor == 1
                ? StairFirstWeight
                : Math.Pow(StairNotFirstWeight, countOfStairsOnTheFloor);
        }

        public static GenerationWeights FullRandom()
            => new GenerationWeights(1, 1, 1, 1);

        public static GenerationWeights GenericBuilding()
            => new GenerationWeights(10, .1, 100, 1);

        public static GenerationWeights StairsEveryWhere()
            => new GenerationWeights(100, 1, 1, .1);
    }
}
