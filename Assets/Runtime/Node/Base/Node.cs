using UnityEngine;

namespace Nolib.Node
{
    public enum NodeStatus
    {
        Running,
        Success,
        Failure
    }

    public interface INode
    {
        public INode Parent { get; protected set; }

        public bool HasParent => Parent != null;
        
        public void Attach(INode node)
        {
            if (HasParent)
                return;

            Parent = node;
        }

        public void Detach(INode node)
        { 
            if (!HasParent || Parent != node)
                return;

            Parent = null;
        }
        
        public bool IsChildOf(INode node)
        {
            return Parent == node;
            
        }
        protected internal void OnEnter() { }
        protected internal void OnUpdate(float deltaTime) { }
        protected internal void OnFixedUpdate(float deltaTime) { }
        protected internal void OnLateUpdate(float deltaTime) { }
        protected internal void OnExit() { }
        protected internal void OnPreTick(float deltaTime) { }
        protected internal void OnPostTick(float deltaTime) { }
        protected internal NodeStatus OnTick(float deltaTime) { return  NodeStatus.Failure; }
    }

    public abstract class Node : INode
    {
        private INode parent;

        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }

        void INode.OnEnter() => OnEnter();
        void INode.OnUpdate(float deltaTime) => OnUpdate(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => OnFixedUpdate(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => OnLateUpdate(deltaTime);
        void INode.OnExit() => OnExit();
        void INode.OnPreTick(float deltaTime) => OnPreTick(deltaTime);
        void INode.OnPostTick(float deltaTime) => OnPostTick(deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => OnTick(deltaTime);
        
        protected internal virtual void OnEnter() { }
        protected internal virtual void OnUpdate(float deltaTime) { }
        protected internal virtual void OnFixedUpdate(float deltaTime) { }
        protected internal virtual void OnLateUpdate(float deltaTime) { }
        protected internal virtual void OnExit() { }
        protected internal virtual void OnPreTick(float deltaTime) { }
        protected internal virtual void OnPostTick(float deltaTime) { }
        protected internal virtual NodeStatus OnTick(float deltaTime) { return NodeStatus.Failure; }
    }

    public abstract class MonoBehaviourNode : MonoBehaviour, INode
    {
        private INode parent;

        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }
        
        void INode.OnEnter() => OnEnter();
        void INode.OnUpdate(float deltaTime) => OnUpdate(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => OnFixedUpdate(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => OnLateUpdate(deltaTime);
        void INode.OnExit() => OnExit();
        void INode.OnPreTick(float deltaTime) => OnPreTick(deltaTime);
        void INode.OnPostTick(float deltaTime) => OnPostTick(deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => OnTick(deltaTime);
        
        protected internal virtual void OnEnter() { }
        protected internal virtual void OnUpdate(float deltaTime) { }
        protected internal virtual void OnFixedUpdate(float deltaTime) { }
        protected internal virtual void OnLateUpdate(float deltaTime) { }
        protected internal virtual void OnExit() { }
        protected internal virtual void OnPreTick(float deltaTime) { }
        protected internal virtual void OnPostTick(float deltaTime) { }
        protected internal virtual NodeStatus OnTick(float deltaTime) { return NodeStatus.Failure; }
    }
    
    public abstract class ScriptableObjectNode : ScriptableObject, INode
    {
        private INode parent;

        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }
        
        void INode.OnEnter() => OnEnter();
        void INode.OnUpdate(float deltaTime) => OnUpdate(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => OnFixedUpdate(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => OnLateUpdate(deltaTime);
        void INode.OnExit() => OnExit();
        void INode.OnPreTick(float deltaTime) => OnPreTick(deltaTime);
        void INode.OnPostTick(float deltaTime) => OnPostTick(deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => OnTick(deltaTime);
        
        protected internal virtual void OnEnter() { }
        protected internal virtual void OnUpdate(float deltaTime) { }
        protected internal virtual void OnFixedUpdate(float deltaTime) { }
        protected internal virtual void OnLateUpdate(float deltaTime) { }
        protected internal virtual void OnExit() { }
        protected internal virtual void OnPreTick(float deltaTime) { }
        protected internal virtual void OnPostTick(float deltaTime) { }
        protected internal virtual NodeStatus OnTick(float deltaTime) { return NodeStatus.Failure; }
    }
}
