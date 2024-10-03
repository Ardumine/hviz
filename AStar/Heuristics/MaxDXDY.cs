using System;

namespace AStar.Heuristics
{
    public class MaxDXDY : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var heuristicEstimate = 2;
            var h = heuristicEstimate * (Math.Max(Math.Abs(source.X - destination.X), Math.Abs(source.Y - destination.Y)));
            return h;
        }
    }
}