using System;

namespace AStar.Heuristics
{
    public class Euclidean : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var heuristicEstimate = 2;
            var h = (int)(heuristicEstimate * Math.Sqrt(Math.Pow((source.X - destination.X), 2) + Math.Pow((source.Y - destination.Y), 2)));
            return h;
        }
    }
}