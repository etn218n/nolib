using System;
using UnityEngine.Events;

namespace Nolib.Node.Generic
{
    public enum NodeType { Regular, Selector, Any, Exit, Previous }
    
    public class RegularSourceNode<T>
    {
        private FSM<T> fsm;
        private INode<T> source;
        
        public RegularSourceNode(FSM<T> fsm, INode<T> source)
        {
            this.fsm = fsm;
            this.source = source;
        }

        public RegularDestinationNode<T> To(INode<T> destination)
        {
            return new RegularDestinationNode<T>(fsm, source, destination, NodeType.Regular);
        }
        
        public ExitNode<T> ToExitNode()
        {
            return new ExitNode<T>(fsm, source);
        }
        
        public PreviousNode<T> ToPreviousNode()
        {
            return new PreviousNode<T>(fsm, source);
        }
    }
    
    public class RegularDestinationNode<T>
    {
        private FSM<T> fsm;
        private INode<T> source;
        private INode<T> destination;
        private NodeType sourceType;
        
        public RegularDestinationNode(FSM<T> fsm, INode<T> source, INode<T> destination, NodeType sourceType)
        {
            this.fsm = fsm;
            this.source = source;
            this.destination = destination;
            this.sourceType  = sourceType;
        }
        
        public void When(Func<bool> predicate)
        {
            switch (sourceType)
            {
                case NodeType.Any: fsm.AddTransitionFromAnyNode(destination, predicate); break;
                case NodeType.Regular: fsm.AddTransition(source, destination, predicate); break;
                case NodeType.Selector: fsm.AddTransitionFromSelectorNode(destination, predicate); break;
            }
        }
        
        public void When(UnityEvent unityEvent)
        {
            switch (sourceType)
            {
                case NodeType.Any: fsm.AddTransitionFromAnyNode(destination, unityEvent); break;
                case NodeType.Regular: fsm.AddTransition(source, destination, unityEvent); break;
                case NodeType.Selector: fsm.AddTransitionFromSelectorNode(destination, unityEvent); break;
            }
        }
        
        public void When(UnityEvent<T> unityEvent)
        {
            switch (sourceType)
            {
                case NodeType.Any: fsm.AddTransitionFromAnyNode(destination, unityEvent); break;
                case NodeType.Regular: fsm.AddTransition(source, destination, unityEvent); break;
                case NodeType.Selector: fsm.AddTransitionFromSelectorNode(destination, unityEvent); break;
            }
        }
    }
    
    public class AnyNode<T>
    {
        private FSM<T> fsm;

        public AnyNode(FSM<T> fsm)
        {
            this.fsm = fsm;
        }

        public RegularDestinationNode<T> To(INode<T> destination)
        {
            return new RegularDestinationNode<T>(fsm, null, destination, NodeType.Any);
        }
    }
    
    public class SelectorNode<T>
    {
        private FSM<T> fsm;

        public SelectorNode(FSM<T> fsm)
        {
            this.fsm = fsm;
        }

        public RegularDestinationNode<T> To(INode<T> destination)
        {
            return new RegularDestinationNode<T>(fsm, null, destination, NodeType.Selector);
        }
    }
    
    public class ExitNode<T>
    {
        private FSM<T> fsm;
        private INode<T> source;
        
        public ExitNode(FSM<T> fsm, INode<T> source)
        {
            this.fsm    = fsm;
            this.source = source;
        }

        public void When(Func<bool> predicate)
        {
            fsm.AddTransitionToExitNode(source, predicate);
        }
        
        public void When(UnityEvent unityEvent)
        {
            fsm.AddTransitionToExitNode(source, unityEvent);
        }
        
        public void When(UnityEvent<T> unityEvent)
        {
            fsm.AddTransitionToExitNode(source, unityEvent);
        }
    }
    
    public class PreviousNode<T>
    {
        private FSM<T> fsm;
        private INode<T> source;
        
        public PreviousNode(FSM<T> fsm, INode<T> source)
        {
            this.fsm    = fsm;
            this.source = source;
        }

        public void When(Func<bool> predicate)
        {
            fsm.AddTransitionToPreviousNode(source, predicate);
        }
        
        public void When(UnityEvent unityEvent)
        {
            fsm.AddTransitionToPreviousNode(source, unityEvent);
        }
        
        public void When(UnityEvent<T> unityEvent)
        {
            fsm.AddTransitionToPreviousNode(source, unityEvent);
        }
    }
}