using System;

namespace Nolib.Node.Generic
{
    public class ActionNode<T> : INode<T>
    {
        protected INode<T> parent;
        
        public Action<T> EnterAction = context => { };
        public Action<T> ExitAction  = context => { };
        
        public Action<T, float> UpdateAction      = (context, deltaTime) => { };
        public Action<T, float> FixedUpdateAction = (context, deltaTime) => { };
        public Action<T, float> LateUpdateAction  = (context, deltaTime) => { };
        
        public Func<T, float, NodeStatus> TickAction = (context, deltaTime) => NodeStatus.Failure;
        public Action<T, float> PreTickAction  = (context, deltaTime) => { };
        public Action<T, float> PostTickAction = (context, deltaTime) => { };
        
        INode<T> INode<T>.Parent
        {
            get => parent;
            set => parent = value;
        }

        void INode<T>.OnEnter(T context) => EnterAction(context);
        void INode<T>.OnExit(T context) => ExitAction(context);
        void INode<T>.OnUpdate(T context, float deltaTime) => UpdateAction(context, deltaTime);
        void INode<T>.OnFixedUpdate(T context, float deltaTime) => FixedUpdateAction(context, deltaTime);
        void INode<T>.OnLateUpdate(T context, float deltaTime) => LateUpdateAction(context, deltaTime);
        NodeStatus INode<T>.OnTick(T context, float deltaTime) => TickAction(context, deltaTime);
        void INode<T>.OnPreTick(T context, float deltaTime) => PreTickAction(context, deltaTime);
        void INode<T>.OnPostTick(T context, float deltaTime) => PostTickAction(context, deltaTime);
    }
}