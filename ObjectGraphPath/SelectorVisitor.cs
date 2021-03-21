using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ObjectGraphPath
{
    public interface ISelectorVisitor
    {

        IGraphNode BuildGraph(Expression selectors);
    }
    public class SelectorVisitor<T>:ExpressionVisitor, ISelectorVisitor
    {
        private IGraphNode _selectEndNode;
        private IGraphNode _lastGraphNode;
        private readonly ILogger<SelectorVisitor<T>> _log;

        public SelectorVisitor(ILogger<SelectorVisitor<T>> log)
        {
            _log = log;
        }

        public IGraphNode BuildGraph(Expression selector)
        {
            Visit(selector);
            return FindRootNode();
        }

        private IGraphNode FindRootNode()
        {
            if (_lastGraphNode.Parent == null)
            {
                return _lastGraphNode;
            }
            var ret = _lastGraphNode;
            do
            {
                ret = ret.Parent;
            } while (ret.Parent != null);

            return ret;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name != "Select")
            {
                var message = $"Method is not select in selector \"{node}\"";
                throw new Exception(message);
            }

            if (node.Arguments.Count != 2)
            {
                var message = $"Method call \"select\" does not have 2 parameters in selector \"{node}\"";
                throw new Exception(message);
            }

            Visit(node.Arguments[0]);
            var parent = _selectEndNode;
            _lastGraphNode = null;
            _selectEndNode = null;
            Visit(node.Arguments[1]);
            var childNode = _lastGraphNode.Children.First().Value; // skip first node as that is x in x.NavigationProperty
            if (!string.IsNullOrWhiteSpace(childNode.NavigationName) )
            {
                parent.Children.Add(childNode.NavigationName, childNode);
            }
            return node;
        }
        //private Expression[] VisitArguments(IArgumentProvider nodes)
        //{
        //    return ExpressionVisitorUtils.VisitArguments(this, nodes);
        //}


        protected override Expression VisitMember(MemberExpression node)
        {
            var propertyType = node.Type;
            if (!propertyType.IsPrimitive)
            {
                _log.LogDebug($"node={node}");
                if (_lastGraphNode?.Children.ContainsKey(node.Member.Name)??false)
                {
                    _lastGraphNode = _lastGraphNode.Children[node.Member.Name];
                }
                else
                {
                    var graphNode = GraphNodeFactory(propertyType);
                    graphNode.NavigationName = node.Member.Name;
                    if (_lastGraphNode == null)
                    {
                        _selectEndNode = graphNode;
                    }
                    else
                    {
                        _lastGraphNode.Children.Add(node.Member.Name, graphNode);
                    }

                    _lastGraphNode = graphNode;
                }
            }
            var res =  base.VisitMember(node);
            return res;
        }

        private IGraphNode GraphNodeFactory(Type propertyType)
        {
            var graphNodeType = typeof(GraphNode<>).MakeGenericType(new Type[] {propertyType});
            var graphNode = (IGraphNode) Activator.CreateInstance(graphNodeType);
            graphNode.Parent = _lastGraphNode;
            return graphNode;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            var graphNode = GraphNodeFactory(p.Type);
            if (!string.IsNullOrWhiteSpace(_lastGraphNode.NavigationName))
            {
                graphNode.Children.Add(_lastGraphNode.NavigationName, _lastGraphNode);
                _lastGraphNode = graphNode;
            }

            var res = base.VisitParameter(p);

            return res;
        }

    }
}
