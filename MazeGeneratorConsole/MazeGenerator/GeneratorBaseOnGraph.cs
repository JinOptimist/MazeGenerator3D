using MazeGenerator.Generators;
using MazeGenerator.Models.GenerationModels;
using MazeGenerator.Models.GenerationModels.GraphStuff;
using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MazeGenerator
{
    public class GeneratorBaseOnGraph : BaseGenerator
    {
        private Graph _graph;

        protected override void BuildCorridors()
        {
            _graph = new Graph(_chunk);
            var vertices = _chunk.Cells.Select(x => new Vertex(_graph, x));
            _graph.AddRangeVertex(vertices);

            BuildPossibleEdges();

            var root = _graph.Vertices.First(x => x.InnerPart == InnerPart.Start);
            _graph.Root = root;

            var currentVertex = _graph.Root;
            while (currentVertex != null)
            {
                if (currentVertex.GetPossibleExitSteps.Any())
                {
                    currentVertex.State = BuildingState.Visited;
                }
                else
                {
                    currentVertex.State = BuildingState.Finished;
                    var allVisited = _graph
                        .Vertices
                        .Where(x => x.State == BuildingState.Visited)
                        .ToList();
                    if (!allVisited.Any())
                    {
                        break;
                    }

                    currentVertex = _random.GetRandomFrom(allVisited);
                    continue;
                }

                var edgeToStep = _random.GetRandomFrom(currentVertex.GetPossibleExitSteps);
                RemoveEdgesToTheVertext(edgeToStep.To);
                RemoveWall(edgeToStep);
                if (edgeToStep.Direction.Z != 0)
                {
                    // we try to build stairs
                    var middleVertext = GetMiddleVertex(edgeToStep);
                    middleVertext.ClearPossibleExitSteps();
                    middleVertext.State = BuildingState.Finished;
                    var stepToMiddle = currentVertex.GetPossibleExitSteps.FirstOrDefault(x => x.To == middleVertext);
                    if (stepToMiddle != null)
                    {
                        currentVertex.RemovePossibleExitSteps(stepToMiddle);
                    }

                    var stair = ChooseStairByVector(edgeToStep.Direction);
                    if (edgeToStep.Direction.Z == 1)
                    {
                        middleVertext.Cell.InnerPart = stair;
                    }
                    else
                    {
                        edgeToStep.To.Cell.InnerPart = stair;
                    }
                    
                    UpdatePossibleEdges(edgeToStep);
                }

                currentVertex = edgeToStep.To;
            }
        }

        private void UpdatePossibleEdges(Edge edgeToStep)
        {
            var vertexAfterStep = edgeToStep.To;
            vertexAfterStep.ClearPossibleExitSteps();

            var nextStairInTheSameDirection = GetCellByDirection(vertexAfterStep, edgeToStep.Direction);
            if (nextStairInTheSameDirection != null
                && nextStairInTheSameDirection.InnerPart == InnerPart.None
                && nextStairInTheSameDirection.State == BuildingState.New)
            {
                vertexAfterStep.AddPossibleExitSteps(new Edge(vertexAfterStep, nextStairInTheSameDirection));
            }

            var vectorToTheSameDirectioButOnTheSameLevel = new Vector3(
                edgeToStep.Direction.X,
                edgeToStep.Direction.Y,
                0);
            var cellOnTheSameDirectioButOnTheSameLevel = GetCellByDirection(vertexAfterStep, vectorToTheSameDirectioButOnTheSameLevel);
            if (cellOnTheSameDirectioButOnTheSameLevel != null
                && cellOnTheSameDirectioButOnTheSameLevel.InnerPart == InnerPart.None
                && cellOnTheSameDirectioButOnTheSameLevel.State == BuildingState.New)
            {
                vertexAfterStep.AddPossibleExitSteps(new Edge(vertexAfterStep, cellOnTheSameDirectioButOnTheSameLevel));
            }
        }

        private void RemoveEdgesToTheVertext(Vertex to)
        {
            var edgesLeadingToTheNewVertex = _graph
                    .Edges
                    .Where(x => x.To == to)
                    .ToList();
            foreach (var edge in edgesLeadingToTheNewVertex)
            {
                edge.From.RemovePossibleExitSteps(edge);
            }
        }

        private void RemoveWall(Edge edge)
        {
            if (edge.From.Z == edge.To.Z)
            {
                BreakWallsBetweenCells(edge.From.Cell, edge.To.Cell);
                return;
            }

            var middleVertex = GetMiddleVertex(edge);
            BreakWallsBetweenCells(edge.From.Cell, middleVertex.Cell);
            BreakWallsBetweenCells(middleVertex.Cell, edge.To.Cell);
        }

        private void BuildPossibleEdges()
        {
            foreach (var vertex in _graph.Vertices)
            {
                var possibleStepVertices = _graph.Vertices.Where(v =>
                    {
                        var xAbsDiff = Math.Abs(vertex.X - v.X);
                        var yAbsDiff = Math.Abs(vertex.Y - v.Y);
                        var zDiff = vertex.Z - v.Z;

                        return xAbsDiff + yAbsDiff == 1
                            && zDiff >= -1
                            && zDiff <= 1;
                    })
                    .Where(x => x.InnerPart == InnerPart.None);
                var edges = possibleStepVertices.Select(x => new Edge(vertex, x));
                vertex.AddRangePossibleExitSteps(edges);
            }
        }

        protected override void BuildExit(Vector2? endPoint)
        {
            var emptyCells = _chunk.Cells
                .Where(x => x.InnerPart == InnerPart.None)
                .ToList();
            var randomCell = _random.GetRandomFrom(emptyCells);
            randomCell.InnerPart = InnerPart.Exit;
        }

        private Vertex GetMiddleVertex(Edge edge)
            => _graph[edge.To.X, edge.To.Y, edge.From.Z];

        private IEnumerable<Vertex> GetNearestVerticesOnTheSameLevel(Vertex centralVertex)
        {
            var westVertex = _graph[centralVertex.X - 1, centralVertex.Y, centralVertex.Z];
            if (westVertex != null)
            {
                yield return westVertex;
            }
            var eastVertex = _graph[centralVertex.X + 1, centralVertex.Y, centralVertex.Z];
            if (eastVertex != null)
            {
                yield return eastVertex;
            }

            var southVertex = _graph[centralVertex.X, centralVertex.Y - 1, centralVertex.Z];
            if (southVertex != null)
            {
                yield return southVertex;
            }
            var northVertex = _graph[centralVertex.X, centralVertex.Y + 1, centralVertex.Z];
            if (northVertex != null)
            {
                yield return northVertex;
            }
        }

        private Vertex? GetCellByDirection(Vertex vertex, Vector3 vector)
            => _graph[vertex.X + vector.X, vertex.Y + vector.Y, vertex.Z + vector.Z];
    }
}
