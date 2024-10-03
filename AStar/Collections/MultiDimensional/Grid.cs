using System;
using System.Collections.Generic;
using System.Drawing;

namespace AStar.Collections.MultiDimensional
{
	public class Grid<T> : IModelAGrid<T>
	{
		public readonly T[] _grid;
		public Grid(int height, int width)
		{
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}

			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			Height = height;
			Width = width;

			_grid = new T[height * width];
		}

		public int Height { get; }

		public int Width { get; }

		public IEnumerable<Position> GetSuccessorPositions(Position node, bool optionsUseDiagonals = false)
		{
			var offsets = GridOffsets.GetOffsets(optionsUseDiagonals);
            for (int i = 0; i < offsets.Count; i++)
			{
                (sbyte row, sbyte column) neighbourOffset = offsets[i];
                var successorRow = node.X + neighbourOffset.row;

				if (successorRow < 0 || successorRow >= Height)
				{
					continue;

				}

				var successorColumn = node.Y + neighbourOffset.column;

				if (successorColumn < 0 || successorColumn >= Width)
				{
					continue;
				}

				yield return new Position(successorRow, successorColumn);
			}
		}

		public T this[Point point]
		{
			get
			{
				return this[point.ToPosition()];
			}
			set
			{
				this[point.ToPosition()] = value;
			}
		}
		public T this[Position position]
		{
			get
			{
				return _grid[ConvertRowColumnToIndex(position.X, position.Y)];
			}
			set
			{
				_grid[ConvertRowColumnToIndex(position.X, position.Y)] = value;
			}
		}
		public T this[int row, int column]
		{
			get
			{
				return _grid[ConvertRowColumnToIndex(row, column)];
			}
			set
			{
				_grid[ConvertRowColumnToIndex(row, column)] = value;
			}
		}
		public T this[int idx]
		{
			get
			{
				return _grid[idx];
			}
			set
			{
				_grid[idx] = value;
			}
		}
		private int ConvertRowColumnToIndex(int row, int column)
		{
			return Width * row + column;
		}
	}
}
