using System.Collections.Generic;

namespace AStar.Collections.PathFinder
{
    internal interface IModelAGraph<T>
    {
        bool HasOpenNodes { get; }
        List<T> GetSuccessors(T node);
        T GetParent(T node);
        void OpenNode(T node);
        T GetOpenNodeWithSmallestF();
    }
}