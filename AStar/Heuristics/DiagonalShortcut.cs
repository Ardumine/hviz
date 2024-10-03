using System;

namespace AStar.Heuristics
{
    public class DiagonalShortcut : ICalculateHeuristic
    {
        public int Calculate(Position source, Position destination)
        {
            var hDiagonal = Math.Min(Math.Abs(source.X - destination.X),
                Math.Abs(source.Y - destination.Y));
            var hStraight = (Math.Abs(source.X - destination.X) + Math.Abs(source.Y - destination.Y));
            var heuristicEstimate = 2;
            var h = (heuristicEstimate * 2) * hDiagonal + heuristicEstimate * (hStraight - 2 * hDiagonal);
            return h;
        }
    }
}