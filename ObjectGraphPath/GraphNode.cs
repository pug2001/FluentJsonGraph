using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(((IGraphNode)this).NavigationName);

            if (Children.Count == 1)
            {
                sb.Append(".");
                sb.Append(Children.First().Value);
            }

            if (Children.Count > 1)
            {
                sb.Append(".{");
                var children = string.Join(",", Children.Select(x => $".{x.Value}"));
                sb.Append(children);
                sb.Append("}");
            }

            return sb.ToString();
        }
    }

}
