namespace Nolib.Node
{
    public enum NodeStatus
    {
        Running,
        Success,
        Failure
    }
    
    public abstract class Node
    {
        private Node parent;

        public bool HasParent => parent != null;
        
        public void Attach(Node node)
        {
            if (HasParent)
                return;

            parent = node;
        }

        public void Detach(Node node)
        { 
            if (!HasParent || parent != node)
                return;

            parent = null;
        }
        
        public bool IsChildOf(Node node)
        {
            return parent == node;
        }

        protected internal virtual void OnEnter() { }
        protected internal virtual void OnExit() { }
        protected internal virtual void OnUpdate(float deltaTime) { }
        protected internal virtual void OnFixedUpdate(float deltaTime) { }
        protected internal virtual void OnLateUpdate(float deltaTime) { }
        protected internal virtual void OnPreTick(float deltaTime) { }
        protected internal virtual NodeStatus OnTick(float deltaTime) { return  NodeStatus.Failure; }
        protected internal virtual void OnPostTick(float deltaTime) { }
    }
}
