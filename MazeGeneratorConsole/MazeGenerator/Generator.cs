using MazeGenerator.Models;
using MazeGenerator.Models.GenerationModels;

namespace MazeGenerator
{
    public class Generator
    {
        private Maze _maze;
        private List<CellForGeneration> _cells = new();
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
            _maze = new Maze
            {
                Width = config.Width,
                Height = config.Height,
                Legnth = config.Legnth,
            };

            BuildFullWalls();
            BuildCorridors(config.IsLongCorridors);

            // Build Cell base on CellForGeneration for maze.Cells
            return _maze;
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
                        _cells.Add(cell);
                    }
                }
            }
        }

        private void BuildCorridors(bool isLongCorridors)
        {
            var miner = new Miner();
            var startingCell = _random.GetRandomFrom(_cells);
            miner.CurrentCell = startingCell;
            miner.LastStepDirection = Direction.North;

            while (true)
            {
                var cells = GetCellsWhereMinerCanStepInto(miner);

            }

        }

        private IEnumerable<CellForGeneration> GetCellsWhereMinerCanStepInto(Miner miner)
        {
            var cellsOnTheSameLevel = GetNearCellsOnTheSameLevel(miner.CurrentCell)
                .Where(x => x.State == BuildingState.New);

            var canStepUp = _cells
                .FirstOrDefault(cell =>
                    cell.X == miner.X
                    && cell.Y == miner.Y
                    && cell.Z == miner.Z + 1)
                ?.State == BuildingState.New;
            // If cell over the Miner is new, we can break the floor on it
            if (canStepUp)
            {

            }

            return null;
        }

        private IEnumerable<CellForGeneration> GetNearCellsOnTheSameLevel(CellForGeneration centralCell)
        {
            return _cells
                .Where(cell =>
                    Math.Abs(
                    cell.X - centralCell.X) == 1
                        && cell.Y == centralCell.Y
                        && cell.Z == centralCell.Z
                    || cell.X == centralCell.X
                        && Math.Abs(cell.Y - centralCell.Y) == 1
                        && cell.Z == centralCell.Z);
        }

        private Wall AllWalls()
            => Wall.North | Wall.East | Wall.South | Wall.West | Wall.Roof | Wall.Floor;
    }
}
