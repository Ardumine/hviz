using System;

namespace AStar.Heuristics
{
    public class Manhattan : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var heuristicEstimate = 2;
            var h = heuristicEstimate * (Math.Abs(source.X - destination.X) + Math.Abs(source.Y - destination.Y));
            return h;
        }
    }
}