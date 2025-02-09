﻿using MazeGenerator.Models.GenerationModels;
using MazeGenerator.Models.GenerationModels.GraphStuff;
using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MazeGenerator.Generators
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

            var lastStepDirection = new Vector3();
            var currentVertex = _graph.Root;
            while (currentVertex != null)
            {
                if (_debugDrawer != null)
                {
                    var chunk = MapToChunk(_chunk);
                    _debugDrawer(chunk);
                    // Console.ReadLine();
                }
                currentVertex.FilterPossibleStep();

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

                //currentVertex.GetPossibleExitSteps
                var possibleStepsWithWeight = 
                    BuildPossibleStepsWithWeight(currentVertex, lastStepDirection)
                    .ToList();
                var edgeToStep = _random.GetRandomFromByWeight(possibleStepsWithWeight);
                // var edgeToStep = _random.GetRandomFrom(currentVertex.GetPossibleExitSteps);
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

                    var stair = ChooseStairByVectorV2ForGraph(edgeToStep.Direction);
                    if (edgeToStep.Direction.Z == 1)
                    {
                        middleVertext.Cell.InnerPart = stair;
                    }
                    else
                    {
                        edgeToStep.To.Cell.InnerPart = stair;
                    }

                    UpdatePossibleEdgesForStair(edgeToStep);
                }

                currentVertex = edgeToStep.To;
                lastStepDirection = edgeToStep.Direction;
            }
        }

        private IEnumerable<OptionWithWeight<Edge>> BuildPossibleStepsWithWeight(Vertex currentVertex, Vector3 lastStepDirection)
        {
            foreach (var edge in currentVertex.GetPossibleExitSteps)
            {
                if (edge.Direction.Z == 0)
                {
                    yield return new OptionWithWeight<Edge>
                    {
                        Option = edge,
                        Weight = _weightsForGeneration
                        .CalculateWeightForStep(edge.Direction, lastStepDirection)
                    };
                }
                else
                {
                    var zOfTheLevel = (int)(edge.Direction.Z + edge.Direction.Z);
                    yield return new OptionWithWeight<Edge>
                    {
                        Option = edge,
                        Weight = _weightsForGeneration.
                            CalculateWeightForStair(_chunk, zOfTheLevel)
                    };
                }
            }
        }

        private void BuildBigRoom(int size)
        {
            var x = _random.Next(0, _chunk.Length - size - 1);
            var y = _random.Next(0, _chunk.Width - size - 1);
        }

        private void UpdatePossibleEdgesForStair(Edge edgeToStep)
        {
            var vertexAfterStep = edgeToStep.To;
            vertexAfterStep.ClearPossibleExitSteps();

            var nextStairInTheSameDirection = GetCellByDirection(vertexAfterStep, edgeToStep.Direction);
            if (nextStairInTheSameDirection != null
                && nextStairInTheSameDirection.InnerPart == InnerPart.None
                && nextStairInTheSameDirection.State == BuildingState.New)
            {
                var edge = new Edge(vertexAfterStep, nextStairInTheSameDirection);
                if (IsEdgeAllowed(edge))
                {
                    vertexAfterStep.AddPossibleExitSteps(edge);
                }
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
                var edge = new Edge(vertexAfterStep, cellOnTheSameDirectioButOnTheSameLevel);
                if (IsEdgeAllowed(edge))
                {
                    vertexAfterStep.AddPossibleExitSteps(edge);
                }
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

                edges = edges.Where(IsEdgeAllowed);

                vertex.AddRangePossibleExitSteps(edges);
            }
        }

        private bool IsEdgeAllowed(Edge edge)
        {
            if (edge.Direction.Z == 0)
            {
                return true;
            }

            // if we try build stair to end of the maze, remove this edge
            var lastX = _chunk.Length - 1;
            var lastY = _chunk.Width - 1;
            if (edge.Direction.X == 1 && edge.To.X == lastX)
            {
                return false;
            }
            if (edge.Direction.X == -1 && edge.To.X == 0)
            {
                return false;
            }
            if (edge.Direction.Y == 1 && edge.To.Y == lastY)
            {
                return false;
            }
            if (edge.Direction.Y == -1 && edge.To.X == 0)
            {
                return false;
            }

            return true;
        }

        private Vertex GetMiddleVertex(Edge edge)
            => _graph[edge.To.X, edge.To.Y, edge.From.Z];

        private Vertex? GetCellByDirection(Vertex vertex, Vector3 vector)
            => _graph[vertex.X + vector.X, vertex.Y + vector.Y, vertex.Z + vector.Z];
    }
}
