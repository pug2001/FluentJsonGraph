using System;
using System.Collections.Generic;

namespace ObjectGraphPath
{
    public interface IGraphNode
    {
        IGraphNode Parent { get; set; }
        string[] PrimitiveProperties();
        string[] NavigationProperties();
        string[] NavigationPropertiesToSerialize();
        Dictionary<string, IGraphNode> Children { get; }
        Type GetGenericType { get; }
        string NavigationName { get; set; }
    }
}