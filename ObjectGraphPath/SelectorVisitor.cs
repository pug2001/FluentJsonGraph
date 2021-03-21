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
        private IGraphNode _lastGraphNode;
        private readonly ILogger<SelectorVisitor<T>> _log;
        private readonly IGraphNodeFactory _graphNodeFactory;

        public SelectorVisitor(ILogger<SelectorVisitor<T>> log, IGraphNodeFactory graphNodeFactory)
        {
            _log = log;
            _graphNodeFactory = graphNodeFactory;
        }

        public IGraphNode BuildGraph(Expression selector)
        {
            _lastGraphNode = null;
            Visit(selector);
            return _lastGraphNode.Children.First().Value; // skip the node which represents the x before the => in x=>x.Dependencies
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
            var parentHead = _lastGraphNode;
            _lastGraphNode = null;
            Visit(node.Arguments[1]);
            var parentTail = TailNode(parentHead);
            var childNode = _lastGraphNode.Children.First().Value.Children.First().Value; // skip 2 nodes as they are the x in x=>x.NavigationProperty
            _lastGraphNode = parentHead;
            if (!string.IsNullOrWhiteSpace(childNode.NavigationName) )
            {
                parentTail.Children.Add(childNode.NavigationName, childNode);
            }
            return node;
        }

        private IGraphNode HeadNode(IGraphNode node)
        {
            var ret = node;
            while (ret?.Parent != null)
            {
                ret = ret.Parent;
            } 

            return ret;
        }
        private static IGraphNode TailNode(IGraphNode node)
        {
            var child = node;
            while (child?.Children.Count > 0)
            {
                child = child.Children.First().Value;
            }

            return child;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var propertyType = node.Type;
            if (!propertyType.IsPrimitive)
            {
                _log.LogDebug($"node={node}");
                _lastGraphNode = _graphNodeFactory.CreateGraphNode(node.Member.Name, propertyType, _lastGraphNode);
            }
            var res =  base.VisitMember(node);
            return res;
        }


        protected override Expression VisitParameter(ParameterExpression p)
        {
            _lastGraphNode = _graphNodeFactory.CreateGraphNode(p.Name,p.Type,_lastGraphNode);

            var res = base.VisitParameter(p);

            return res;
        }

    }
}
