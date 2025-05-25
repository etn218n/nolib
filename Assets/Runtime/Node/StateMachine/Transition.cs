using System;
using UnityEngine.Events;

namespace Nolib.Node
{
    public interface ITransition
    {
        INode Source { get; }
        INode Destination { get; }
        ICondition Condition { get; }

        void ActivateCondition();
        void DeactivateCondition();
    }
    
    public class Transition : ITransition
    {
        public INode Source { get; }
        public INode Destination { get; }
        public ICondition Condition { get; }

        public static Transition Empty = new Transition(null, null, () => false);
        
        public Transition(INode source, INode destination, Func<bool> predicate)
        {
            Source      = source;
            Destination = destination;
            Condition   = new PollCondition(predicate);
        }
        
        public Transition(INode source, INode destination, UnityEvent unityEvent)
        {
            Source      = source;
            Destination = destination;
            Condition   = new UnityEventCondition(unityEvent);
        }

        public Transition(INode source, INode destination, ICondition condition)
        {
            Source      = source;
            Destination = destination;
            Condition   = condition;
        }

        public void ActivateCondition()
        {
            Condition.Activate();
        }
        
        public void DeactivateCondition()
        {
            Condition.Deactivate();
        }
    }

    public class Transition<T> : ITransition
    {
        public INode Source { get; }
        public INode Destination { get; }
        public ICondition Condition { get; }

        public Transition(INode source, INode destination, UnityEvent<T> unityEvent)
        {
            Source      = source;
            Destination = destination;
            Condition   = new UnityEventCondition<T>(unityEvent);
        }
        
        public void ActivateCondition()
        {
            Condition.Activate();
        }

        public void DeactivateCondition()
        {
            Condition.Deactivate();
        }
    }
}