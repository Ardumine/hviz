using System.Drawing;

namespace AStar
{
    public static class PositionExtensions
    {
        public static Point ToPoint(this Position position)
        {
            return new Point(position.Y, position.X);
        }
        
        public static Position ToPosition(this Point point)
        {
            return new Position(point.Y, point.X);
        }
    }
}