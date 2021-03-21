using System;
using System.Linq;
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
                    //// skip the head node in selector breadcrumb as that is always parentGraphNode
                    //var node = selectorGraph.Children.First().Value;

                    // walk selector breadcrumb adding nodes which are not present in parentGraphNode
                    AddChildren(parentGraphNode, selectorGraph); 
                }

            }

            return parentGraphNode;
        }

        private void AddChildren(IGraphNode parentGraphNode, IGraphNode selectorGraph)
        {
            var childNode = selectorGraph.Children.First();
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
