using System;

namespace ObjectGraphPath
{
    public interface IGraphNodeFactory
    {
        IGraphNode CreateGraphNode(string navigationName, Type propertyType, IGraphNode lastGraphNode);
    }
    public class GraphNodeFactory : IGraphNodeFactory
    {
        public IGraphNode CreateGraphNode(string navigationName, Type propertyType, IGraphNode lastGraphNode)
        {
            var graphNodeType = typeof(GraphNode<>).MakeGenericType(new Type[] { propertyType });
            var graphNode = (IGraphNode)Activator.CreateInstance(graphNodeType);
            graphNode.NavigationName = navigationName;
            if (lastGraphNode == null)
            {
                return graphNode;
            }
            lastGraphNode.Parent = graphNode;
            graphNode.Children.Add(lastGraphNode.NavigationName, lastGraphNode);

            return graphNode;
        }
    }
}
