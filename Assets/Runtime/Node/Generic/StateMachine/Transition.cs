using System;
using UnityEngine.Events;

namespace Nolib.Node.Generic
{
    public interface ITransition<T>
    {
        INode<T> Source { get; }
        INode<T> Destination { get; }
        ICondition Condition { get; }

        void ActivateCondition();
        void DeactivateCondition();
    }
    
    public class Transition<T> : ITransition<T>
    {
        public INode<T> Source { get; }
        public INode<T> Destination { get; }
        public ICondition Condition { get; }

        public static Transition<T> Empty = new Transition<T>(null, null, () => false);
        
        public Transition(INode<T> source, INode<T> destination, Func<bool> predicate)
        {
            Source      = source;
            Destination = destination;
            Condition   = new PollCondition(predicate);
        }
        
        public Transition(INode<T> source, INode<T> destination, UnityEvent unityEvent)
        {
            Source      = source;
            Destination = destination;
            Condition   = new UnityEventCondition(unityEvent);
        }
        
        public Transition(INode<T> source, INode<T> destination, UnityEvent<T> unityEvent)
        {
            Source      = source;
            Destination = destination;
            Condition   = new UnityEventCondition<T>(unityEvent);
        }

        public Transition(INode<T> source, INode<T> destination, ICondition condition)
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
}