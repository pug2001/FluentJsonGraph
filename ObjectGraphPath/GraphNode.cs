using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectGraphPath
{
    public class GraphNode<T>: IGraphNode
    {
        public GraphNode()
        {
        }

        public IGraphNode Parent { get; set; }
        public string[] PrimitiveProperties()
        {
            return typeof(T).GetProperties()
                        .Where(x=>x.GetGetMethod().ReturnType.IsPrimitive)
                        .Select(x=>x.Name)
                        .ToArray();
        }

        public string[] NavigationProperties()
        {
            return typeof(T).GetProperties()
                .Where(x => !x.GetGetMethod().ReturnType.IsPrimitive)
                .Select(x => x.Name)
                .ToArray();
        }

        public string[] NavigationPropertiesToSerialize()
        {
            return Children.Keys.ToArray();
        }
        public Dictionary<string,IGraphNode> Children { get; } = new Dictionary<string,IGraphNode>();
        public Type GetGenericType => typeof(T);
        string IGraphNode.NavigationName { get; set; }
    }

}
