using System;

namespace Nolib.Node
{
    public class ActionNode : INode
    {
        private INode parent;
        
        public Action EnterAction = () => { };
        public Action ExitAction  = () => { };
        
        public Action<float> UpdateAction      = deltaTime => { };
        public Action<float> FixedUpdateAction = deltaTime => { };
        public Action<float> LateUpdateAction  = deltaTime => { };

        public Func<float, NodeStatus> TickAction = deltaTime => NodeStatus.Failure;
        public Action<float> PreTickAction  = deltaTime => { };
        public Action<float> PostTickAction = deltaTime => { };

        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }

        void INode.OnEnter() => EnterAction();
        void INode.OnExit() => ExitAction();
        void INode.OnUpdate(float deltaTime) => UpdateAction(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => FixedUpdateAction(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => LateUpdateAction(deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => TickAction(deltaTime);
        void INode.OnPreTick(float deltaTime) => PreTickAction(deltaTime);
        void INode.OnPostTick(float deltaTime) => PostTickAction(deltaTime);
    }
    
    public class ActionNode<T> : INode
    {
        protected INode parent;
        protected T context;
        
        public Action<T> EnterAction = context => { };
        public Action<T> ExitAction  = context => { };
        
        public Action<T, float> UpdateAction      = (context, deltaTime) => { };
        public Action<T, float> FixedUpdateAction = (context, deltaTime) => { };
        public Action<T, float> LateUpdateAction  = (context, deltaTime) => { };
        
        public Func<T, float, NodeStatus> TickAction = (context, deltaTime) => NodeStatus.Failure;
        public Action<T, float> PreTickAction  = (context, deltaTime) => { };
        public Action<T, float> PostTickAction = (context, deltaTime) => { };
        
        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }

        public ActionNode(T context) => this.context = context;

        void INode.OnEnter() => EnterAction(context);
        void INode.OnExit() => ExitAction(context);
        void INode.OnUpdate(float deltaTime) => UpdateAction(context, deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => FixedUpdateAction(context, deltaTime);
        void INode.OnLateUpdate(float deltaTime) => LateUpdateAction(context, deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => TickAction(context, deltaTime);
        void INode.OnPreTick(float deltaTime) => PreTickAction(context, deltaTime);
        void INode.OnPostTick(float deltaTime) => PostTickAction(context, deltaTime);
    }
}