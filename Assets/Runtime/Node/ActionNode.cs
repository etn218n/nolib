using System;

namespace Nolib.Node
{
    public class ActionNode : Node
    {
        public Action EnterAction = () => { };
        public Action ExitAction  = () => { };
        
        public Action<float> UpdateAction      = deltaTime => { };
        public Action<float> FixedUpdateAction = deltaTime => { };
        public Action<float> LateUpdateAction  = deltaTime => { };

        public Func<float, NodeStatus> TickAction = deltaTime => NodeStatus.Failure;
        public Action<float> PreTickAction  = deltaTime => { };
        public Action<float> PostTickAction = deltaTime => { };

        protected internal override void OnEnter() => EnterAction();
        protected internal override void OnExit() => ExitAction();
        protected internal override void OnUpdate(float deltaTime) => UpdateAction(deltaTime);
        protected internal override void OnFixedUpdate(float deltaTime) => FixedUpdateAction(deltaTime);
        protected internal override void OnLateUpdate(float deltaTime) => LateUpdateAction(deltaTime);
        protected internal override NodeStatus OnTick(float deltaTime) => TickAction(deltaTime);
        protected internal override void OnPreTick(float deltaTime) => PreTickAction(deltaTime);
        protected internal override void OnPostTick(float deltaTime) => PostTickAction(deltaTime);
    }
    
    public class ActionNode<T> : Node
    {
        protected T context;
        
        public Action<T> EnterAction = context => { };
        public Action<T> ExitAction  = context => { };
        
        public Action<T, float> UpdateAction      = (context, deltaTime) => { };
        public Action<T, float> FixedUpdateAction = (context, deltaTime) => { };
        public Action<T, float> LateUpdateAction  = (context, deltaTime) => { };
        
        public Func<T, float, NodeStatus> TickAction = (context, deltaTime) => NodeStatus.Failure;
        public Action<T, float> PreTickAction  = (context, deltaTime) => { };
        public Action<T, float> PostTickAction = (context, deltaTime) => { };

        public ActionNode(T context) => this.context = context;

        protected internal override void OnEnter() => EnterAction(context);
        protected internal override void OnExit() => ExitAction(context);
        protected internal override void OnUpdate(float deltaTime) => UpdateAction(context, deltaTime);
        protected internal override void OnFixedUpdate(float deltaTime) => FixedUpdateAction(context, deltaTime);
        protected internal override void OnLateUpdate(float deltaTime) => LateUpdateAction(context, deltaTime);
        protected internal override NodeStatus OnTick(float deltaTime) => TickAction(context, deltaTime);
        protected internal override void OnPreTick(float deltaTime) => PreTickAction(context, deltaTime);
        protected internal override void OnPostTick(float deltaTime) => PostTickAction(context, deltaTime);
    }
}