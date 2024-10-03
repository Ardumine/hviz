using System;

namespace AStar.Heuristics
{
    public class EuclideanNoSQR : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var heuristicEstimate = 2;
            var h = (int)(heuristicEstimate * (Math.Pow((source.X - destination.X), 2) + Math.Pow((source.Y - destination.Y), 2)));
            return h;
        }
    }
}