using UnityEngine;

namespace Nolib.Node.Generic
{
    public enum NodeStatus
    {
        Running,
        Success,
        Failure
    }

    public interface INode<T>
    {
        public INode<T> Parent { get; protected set; }

        public bool HasParent => Parent != null;
        
        public void Attach(INode<T> node)
        {
            if (HasParent)
                return;

            Parent = node;
        }

        public void Detach(INode<T> node)
        { 
            if (!HasParent || Parent != node)
                return;

            Parent = null;
        }
        
        public bool IsChildOf(INode<T> node)
        {
            return Parent == node;
            
        }
        protected internal void OnEnter(T context) { }
        protected internal void OnUpdate(T context, float deltaTime) { }
        protected internal void OnFixedUpdate(T context, float deltaTime) { }
        protected internal void OnLateUpdate(T context, float deltaTime) { }
        protected internal void OnExit(T context) { }
        protected internal void OnPreTick(T context, float deltaTime) { }
        protected internal void OnPostTick(T context, float deltaTime) { }
        protected internal NodeStatus OnTick(T context, float deltaTime) { return  NodeStatus.Failure; }
    }

    public abstract class Node<T> : INode<T>
    {
        private INode<T> parent;

        INode<T> INode<T>.Parent
        {
            get => parent;
            set => parent = value;
        }

        void INode<T>.OnEnter(T context) => OnEnter(context);
        void INode<T>.OnUpdate(T context, float deltaTime) => OnUpdate(context, deltaTime);
        void INode<T>.OnFixedUpdate(T context, float deltaTime) => OnFixedUpdate(context, deltaTime);
        void INode<T>.OnLateUpdate(T context, float deltaTime) => OnLateUpdate(context, deltaTime);
        void INode<T>.OnExit(T context) => OnExit(context);
        void INode<T>.OnPreTick(T context, float deltaTime) => OnPreTick(context, deltaTime);
        void INode<T>.OnPostTick(T context, float deltaTime) => OnPostTick(context, deltaTime);
        NodeStatus INode<T>.OnTick(T context, float deltaTime) => OnTick(context, deltaTime);
        
        protected internal virtual void OnEnter(T context) { }
        protected internal virtual void OnUpdate(T context, float deltaTime) { }
        protected internal virtual void OnFixedUpdate(T context, float deltaTime) { }
        protected internal virtual void OnLateUpdate(T context, float deltaTime) { }
        protected internal virtual void OnExit(T context) { }
        protected internal virtual void OnPreTick(T context, float deltaTime) { }
        protected internal virtual void OnPostTick(T context, float deltaTime) { }
        protected internal virtual NodeStatus OnTick(T context, float deltaTime) { return NodeStatus.Failure; }
    }

    public abstract class MonoBehaviourNode<T> : MonoBehaviour, INode<T>
    {
        private INode<T> parent;

        INode<T> INode<T>.Parent
        {
            get => parent;
            set => parent = value;
        }
        
        void INode<T>.OnEnter(T context) => OnEnter(context);
        void INode<T>.OnUpdate(T context, float deltaTime) => OnUpdate(context, deltaTime);
        void INode<T>.OnFixedUpdate(T context, float deltaTime) => OnFixedUpdate(context, deltaTime);
        void INode<T>.OnLateUpdate(T context, float deltaTime) => OnLateUpdate(context, deltaTime);
        void INode<T>.OnExit(T context) => OnExit(context);
        void INode<T>.OnPreTick(T context, float deltaTime) => OnPreTick(context, deltaTime);
        void INode<T>.OnPostTick(T context, float deltaTime) => OnPostTick(context, deltaTime);
        NodeStatus INode<T>.OnTick(T context, float deltaTime) => OnTick(context, deltaTime);
        
        protected internal virtual void OnEnter(T context) { }
        protected internal virtual void OnUpdate(T context, float deltaTime) { }
        protected internal virtual void OnFixedUpdate(T context, float deltaTime) { }
        protected internal virtual void OnLateUpdate(T context, float deltaTime) { }
        protected internal virtual void OnExit(T context) { }
        protected internal virtual void OnPreTick(T context, float deltaTime) { }
        protected internal virtual void OnPostTick(T context, float deltaTime) { }
        protected internal virtual NodeStatus OnTick(T context, float deltaTime) { return NodeStatus.Failure; }
    }
}
