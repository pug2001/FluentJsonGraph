using System;
using System.Linq.Expressions;

namespace ObjectGraphPath
{
    public class GraphFactory<T>
    {
        private readonly ISelectorVisitor _visitor;

        public GraphFactory(ISelectorVisitor visitor)
        {
            _visitor = visitor;
        }
        public IGraphNode SelectorsToGraph(Expression<Func<T, object>>[] selectors)
        {
            IGraphNode parentGraphNode = null;
            foreach (var selector in selectors)
            {
                var selectorGraph = _visitor.BuildGraph(selector);
                if (parentGraphNode == null)
                {
                    parentGraphNode = selectorGraph;
                }
                else
                {
                    AddChildren(parentGraphNode, selectorGraph);
                }

            }

            return parentGraphNode;
        }

        private void AddChildren(IGraphNode parentGraphNode, IGraphNode selectorGraph)
        {
            foreach (var childNode in selectorGraph.Children)
            {
                if (parentGraphNode.Children.ContainsKey(childNode.Key))
                {
                    AddChildren(parentGraphNode.Children[childNode.Key],childNode.Value);
                }
                else
                {
                    parentGraphNode.Children.Add(childNode.Key,childNode.Value);
                }
            }
        }
    }
}
