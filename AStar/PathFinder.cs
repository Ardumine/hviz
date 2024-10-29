using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AStar.Collections.PathFinder;
using AStar.Heuristics;
using AStar.Options;

namespace AStar
{
    public class PathFinder : IFindAPath
    {
        private const sbyte ClosedValue = 0;
        private const int DistanceBetweenNodes = 1;
        private readonly PathFinderOptions _options;
        private readonly WorldGrid _world;
        private readonly ICalculateHeuristic _heuristic;

        public PathFinder(WorldGrid worldGrid, PathFinderOptions pathFinderOptions = null)
        {
            _world = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
            _options = pathFinderOptions ?? new PathFinderOptions();
            _heuristic = HeuristicFactory.Create(_options.HeuristicFormula);
        }

        ///<inheritdoc/>
        public Point[] FindPath(Point start, Point end)
        {
            return FindPath(new Position(start.Y, start.X), new Position(end.Y, end.X))
                .Select(position => new Point(position.Y, position.X))
                .ToArray();
        }

        ///<inheritdoc/>
        public Position[] FindPath(Position start, Position end)
        {
            long nodesVisited = 0;
            IModelAGraph<PathFinderNode> graph = new PathFinderGraph(_world.Height, _world.Width, _options.UseDiagonals);

            var startNode = new PathFinderNode(position: start, g: 0, h: 2, parentNodePosition: start);
            graph.OpenNode(startNode);

            while (graph.HasOpenNodes)
            {
                var q = graph.GetOpenNodeWithSmallestF();

                if (q.Position == end)
                {
                    return OrderClosedNodesAsArray(graph, q);
                }

                if (nodesVisited > _options.SearchLimit)
                {
                    return [];
                }

                List<PathFinderNode> list = graph.GetSuccessors(q);
                for (int i = 0; i < list.Count; i++)
                {
                    PathFinderNode successor = list[i];
                    if (_world[successor.Position] == ClosedValue)
                    {
                        continue;
                    }

                    var newG = q.G + DistanceBetweenNodes;

                    if (_options.PunishChangeDirection)
                    {
                        newG += CalculateModifierToG(q, successor, end);
                    }

                    var newH = _heuristic.Calculate(successor.Position, end);
                    switch (_options.Weighting)
                    {
                        case Weighting.Negative:
                            newH += _world[successor.Position];
                            break;
                        case Weighting.Positive:
                            newH -= _world[successor.Position];
                            break;
                        case Weighting.None:
                        default:
                            break;
                    }

                    var updatedSuccessor = new PathFinderNode(
                        position: successor.Position,
                        g: newG,
                        h: newH,
                        parentNodePosition: q.Position);

                    if (BetterPathToSuccessorFound(updatedSuccessor, successor))
                    {
                        graph.OpenNode(updatedSuccessor);
                    }
                }

                nodesVisited++;
            }

            return [];
        }

        private int CalculateModifierToG(PathFinderNode q, PathFinderNode successor, Position end)
        {
            if (q.Position == q.ParentNodePosition)
            {
                return 0;
            }

            var gPunishment = Math.Abs(successor.Position.X - end.X) + Math.Abs(successor.Position.Y - end.Y);

            var successorIsVerticallyAdjacentToQ = successor.Position.X - q.Position.X != 0;

            if (successorIsVerticallyAdjacentToQ)
            {
                var qIsVerticallyAdjacentToParent = q.Position.X - q.ParentNodePosition.X == 0;
                if (qIsVerticallyAdjacentToParent)
                {
                    return gPunishment;
                }
            }

            var successorIsHorizontallyAdjacentToQ = successor.Position.X - q.Position.X != 0;

            if (successorIsHorizontallyAdjacentToQ)
            {
                var qIsHorizontallyAdjacentToParent = q.Position.X - q.ParentNodePosition.X == 0;
                if (qIsHorizontallyAdjacentToParent)
                {
                    return gPunishment;
                }
            }

            if (_options.UseDiagonals)
            {
                var successorIsDiagonallyAdjacentToQ = (successor.Position.Y - successor.Position.X) == (q.Position.Y - q.Position.X);
                if (successorIsDiagonallyAdjacentToQ)
                {
                    var qIsDiagonallyAdjacentToParent = (q.Position.Y - q.Position.X) == (q.ParentNodePosition.Y - q.ParentNodePosition.X)
                                                        && IsStraightLine(q.ParentNodePosition, q.Position, successor.Position);
                    if (qIsDiagonallyAdjacentToParent)
                    {
                        return gPunishment;
                    }
                }
            }

            return 0;
        }

        private bool IsStraightLine(Position a, Position b, Position c)
        {
            // area of triangle == 0
            return (a.Y * (b.X - c.X) + b.Y * (c.X - a.X) + c.Y * (a.X - b.X)) / 2 == 0;
        }

        private bool BetterPathToSuccessorFound(PathFinderNode updateSuccessor, PathFinderNode currentSuccessor)
        {
            return !currentSuccessor.HasBeenVisited ||
                (currentSuccessor.HasBeenVisited && updateSuccessor.F < currentSuccessor.F);
        }

        private static Position[] OrderClosedNodesAsArray(IModelAGraph<PathFinderNode> graph, PathFinderNode endNode)
        {
            var path = new Stack<Position>();

            var currentNode = endNode;

            while (currentNode.Position != currentNode.ParentNodePosition)
            {
                path.Push(currentNode.Position);
                currentNode = graph.GetParent(currentNode);
            }

            path.Push(currentNode.Position);

            return path.ToArray();
        }
    }
}
