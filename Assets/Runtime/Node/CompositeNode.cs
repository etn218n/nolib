using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nolib.Node
{
    public abstract class CompositeNode : INode
    {
        protected List<INode> children;
        protected Func<bool> condition;
        protected bool isConditionMet;
        protected INode parent;
        
        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }

        protected CompositeNode(params INode[] children) : this(() => true, children)
        {
        }

        protected CompositeNode(Func<bool> condition, params INode[] children)
        {
            this.condition = condition;
            this.children  = new List<INode>(children.Length);

            foreach (var node in children)
            {
                if (IsValidNode(node))
                    this.children.Add(node);
            }
        }

        public void AddChildNode(INode node)
        {
            if (!IsValidNode(node))
                return;
            
            children.Add(node);
            node.Attach(this);
            OnChildNodeAdded(node);
        }
        
        public void RemoveChildNode(INode node)
        {
            if (!node.IsChildOf(this))
                return;
            
            children.Remove(node);
            node.Detach(this);
            OnChildNodeRemoved(node);
        }

        public void Start() => InternalOnEnter();
        public void Update(float deltaTime) => InternalUpdate(deltaTime);
        public void FixedUpdate(float deltaTime) => InternalFixedUpdate(deltaTime);
        public void LateUpdate(float deltaTime) => InternalLateUpdate(deltaTime);
        public void Exit() => InternalExit();
        public void PreTick(float deltaTime) => InternalPreTick(deltaTime);
        public void PostTick(float deltaTime) => InternalPostTick(deltaTime);
        public NodeStatus Tick(float deltaTime) => InternalOnTick(deltaTime);
        
        void INode.OnEnter() => InternalOnEnter();
        void INode.OnUpdate(float deltaTime) => InternalUpdate(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => InternalFixedUpdate(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => InternalLateUpdate(deltaTime);
        void INode.OnExit() => InternalExit();
        void INode.OnPreTick(float deltaTime) => InternalPreTick(deltaTime);
        void INode.OnPostTick(float deltaTime) => InternalPostTick(deltaTime);
        NodeStatus INode.OnTick(float deltaTime) => InternalOnTick(deltaTime);

        protected internal virtual void InternalOnEnter() => isConditionMet = condition.Invoke();
        protected internal virtual void InternalUpdate(float deltaTime) { }
        protected internal virtual void InternalFixedUpdate(float deltaTime) { }
        protected internal virtual void InternalLateUpdate(float deltaTime) { }
        protected internal virtual void InternalExit() { }
        protected internal virtual void InternalPreTick(float deltaTime) { }
        protected internal virtual void InternalPostTick(float deltaTime) { }
        protected internal virtual NodeStatus InternalOnTick(float deltaTime) => NodeStatus.Failure;

        protected bool IsValidNode(INode node)
        {
            if (node == null)
                throw new NullReferenceException();

            if (node == this)
                throw new InvalidOperationException();

            if (node.HasParent && !node.IsChildOf(this))
                throw new InvalidOperationException();

            if (node.IsChildOf(this))
            {
                Debug.Log("Already contained this node.");
                return false;
            }
            
            return true;
        }
        
        protected virtual void OnChildNodeAdded(INode node) { }
        protected virtual void OnChildNodeRemoved(INode node) { }
    }
}