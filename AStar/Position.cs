using System;

namespace AStar
{
    /// <summary>
    /// A point in a matrix. P(row, column)
    /// </summary>
    public readonly struct Position
    {
        /// <summary>
        /// The row in the matrix
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The column in the matrix
        /// </summary>
        public int Y { get; }

        public Position(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public bool IsDiagonalTo(Position other)
        {
            // return Row - other.Row != 0 ||
            //     Column - other.Column != 0;
            
            return X != other.X &&
                Y != other.Y;
        }
        
        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object? other)
        {
            if (other is Position otherPoint)
            {
                return X == otherPoint.X && Y == otherPoint.Y;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"[{X}.{Y}]";
        }
    }
}