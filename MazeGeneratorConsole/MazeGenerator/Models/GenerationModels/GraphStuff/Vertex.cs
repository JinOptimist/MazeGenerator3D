﻿using MazeGenerator.Models.MazeModels;
using System;
using System.Collections.Generic;

namespace MazeGenerator.Models.GenerationModels.GraphStuff
{
    public class Vertex
    {
        public Graph Graph { get; private set; }

        public BuildingState BuildingState { get; set; }

        public int X => Cell.X;
        public int Y => Cell.Y;
        public int Z => Cell.Z;
        public InnerPart InnerPart => Cell.InnerPart;

        public BuildingState State
        {
            get
            {
                return Cell.State;
            }
            set
            {
                Cell.State = value;
            }
        }

        /// <summary>
        /// Where have I already gone from the current Vertex
        /// </summary>
        public List<Edge> DoneSteps { get; private set; } = new List<Edge>();

        /// <summary>
        /// Where can I go from the current Vertex
        /// </summary>
        private List<Edge> _possibleExitSteps = new List<Edge>();

        /// <summary>
        /// Where from I can step to the current Vertex
        /// </summary>
        public List<Edge> EnteringEdges { get; private set; } = new List<Edge>();

        public CellForGeneration Cell { get; private set; }

        public List<Edge> GetPossibleExitSteps => _possibleExitSteps;

        public void ClearPossibleExitSteps()
        {
            foreach (var edge in _possibleExitSteps)
            {
                Graph.Edges.Remove(edge);
            }
            
            _possibleExitSteps.Clear();
        }

        public void AddPossibleExitSteps(Edge edge)
        {
            _possibleExitSteps.Add(edge);
            Graph.Edges.Add(edge);
        }

        internal void RemovePossibleExitSteps(Edge edge)
        {
            _possibleExitSteps.Remove(edge);
            Graph.Edges.Remove(edge);
        }

        internal void AddRangePossibleExitSteps(IEnumerable<Edge> edges)
        {
            _possibleExitSteps.AddRange(edges);
            Graph.Edges.AddRange(edges);
        }

        public Vertex(Graph graph, CellForGeneration cell)
        {
            Graph = graph;
            Cell = cell;
        }
    }
}