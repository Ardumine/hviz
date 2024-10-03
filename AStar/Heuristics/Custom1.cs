using System;

namespace AStar.Heuristics
{
    public class Custom1 : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var heuristicEstimate = 2;
            var dxy = new Position(Math.Abs(destination.X - source.X), Math.Abs(destination.Y - source.Y));
            var Orthogonal = Math.Abs(dxy.X - dxy.Y);
            var Diagonal = Math.Abs(((dxy.X + dxy.Y) - Orthogonal) / 2);
            var h = heuristicEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);
            return h;
        }
    }
}