using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Nolib.DataStructure;

namespace Nolib.Node.Generic
{
    public class FSM<T> : INode<T>
    {
        #region Fields & Properties
        protected readonly string name;
        protected readonly CircularBuffer<INode<T>> nodeStack;
        protected readonly Dictionary<INode<T>, List<ITransition<T>>> transitionMap;
        protected readonly Dictionary<INode<T>, List<ITransition<T>>> anyNodeTransitionMap;

        protected INode<T> parent;
        protected INode<T> selectorNode;
        protected INode<T> exitNode;
        protected INode<T> currentNode;
        protected INode<T> traceBackNode;
        
        protected bool isCompleted;
        protected int maxStackSize;
        protected List<ITransition<T>> currentTransitionSet;

        public string Name => name;
        public int StateCount => transitionMap.Keys.Count - 1;
        public int MaxStackSize => maxStackSize;
        public bool IsCompleted => isCompleted;
        public INode<T> CurrentNode => currentNode;
        public IReadOnlyCollection<INode<T>> Nodes => transitionMap.Keys;
        public IReadOnlyCollection<ITransition<T>> CurrentTransitionSet => currentTransitionSet;
        public IReadOnlyCollection<ITransition<T>> TransitionsFrom(INode<T> node) => transitionMap[node];
        
        INode<T> INode<T>.Parent
        {
            get => parent;
            set => parent = value;
        }

        public FSM(string name = "FSM", int maxStackSize = 20)
        {
            this.name = name;
            this.maxStackSize = maxStackSize;
            
            nodeStack = new CircularBuffer<INode<T>>(maxStackSize);
            transitionMap = new Dictionary<INode<T>, List<ITransition<T>>>();
            anyNodeTransitionMap = new Dictionary<INode<T>, List<ITransition<T>>>();
            currentTransitionSet = new List<ITransition<T>>();
            
            selectorNode  = new EmptyNode<T>();
            traceBackNode = new EmptyNode<T>();
            exitNode      = new ActionNode<T> {EnterAction = _ => isCompleted = true};
            currentNode   = selectorNode;
            
            AddNode(selectorNode);
            TransitionToNode(selectorNode, default);
        }
        #endregion

        #region Run FSM
        public void Start(T context, INode<T> entryNode = null)
        {
            isCompleted = false;

            if (entryNode != null && transitionMap.ContainsKey(entryNode))
                SetEntryNode(entryNode);
            
            var qualifiedTransition = CheckForQualifiedTransition(transitionMap[selectorNode]);

            if (qualifiedTransition != null)
                TransitionToNode(qualifiedTransition.Destination, context);
        }

        public NodeStatus Tick(T context, float deltaTime)
        {
            var qualifiedTransition = CheckForQualifiedTransition(currentTransitionSet);
            
            if (qualifiedTransition != null)
            {
                if (qualifiedTransition.Destination == traceBackNode && nodeStack.Count >= 2)
                {
                    nodeStack.PopTail();
                    TransitionToNode(nodeStack.PopTail(), context);
                }
                else
                {
                    TransitionToNode(qualifiedTransition.Destination, context);
                }
            }
            
            currentNode.OnTick(context, deltaTime);
            
            return NodeStatus.Running;
        }

        public void PreTick(T context, float deltaTime) => currentNode.OnPreTick(context, deltaTime);
        public void PostTick(T context, float deltaTime) => currentNode.OnPostTick(context, deltaTime);
        public void Update(T context, float deltaTime) => currentNode.OnUpdate(context, deltaTime);
        public void FixedUpdate(T context, float deltaTime) => currentNode.OnFixedUpdate(context, deltaTime);
        public void LateUpdate(T context, float deltaTime) => currentNode.OnLateUpdate(context, deltaTime);
        #endregion

        #region Node Callbacks
        NodeStatus INode<T>.OnTick(T context, float deltaTime) => Tick(context, deltaTime);
        void INode<T>.OnPreTick(T context, float deltaTime) => PreTick(context, deltaTime);
        void INode<T>.OnPostTick(T context, float deltaTime) => PostTick(context, deltaTime);
        void INode<T>.OnEnter(T context) => Start(context);
        void INode<T>.OnUpdate(T context, float deltaTime) => Update(context, deltaTime);
        void INode<T>.OnFixedUpdate(T context, float deltaTime) => FixedUpdate(context, deltaTime);
        void INode<T>.OnLateUpdate(T context, float deltaTime) => LateUpdate(context, deltaTime);
        void INode<T>.OnExit(T context)
        {
            nodeStack.Clear();
            TransitionToNode(selectorNode, context);
        }
        #endregion

        #region Node Operations
        public bool Contains(INode<T> node)
        {
            if (transitionMap.ContainsKey(node))
                return true;
            
            var subFSMs = from n in Nodes
                          where n is FSM<T>
                          select n as FSM<T>;

            return subFSMs.Any(sub => sub.Contains(node));
        }

        protected void AddNode(INode<T> node)
        {
            if (!node.IsChildOf(this))
                node.Attach(this);
            
            if (transitionMap.Count == 1)
                SetEntryNode(node);
            
            if (!transitionMap.ContainsKey(node))
                transitionMap.Add(node, new List<ITransition<T>>());
        }
        
        protected void SetEntryNode(INode<T> node)
        {
            // override all transitions from Selector node
            transitionMap[selectorNode].Clear();
            transitionMap[selectorNode].Add(new Transition<T>(selectorNode, node, () => true));
        }

        public void RemoveNode(INode<T> node)
        {
            var foundNode = Nodes.FirstOrDefault(s => s == node);

            if (foundNode != null && foundNode != currentNode)
            {
                foundNode.Detach(this);
                transitionMap.Remove(node);
                anyNodeTransitionMap.Remove(node);

                foreach (var n in transitionMap.Keys)
                {
                    var relatedTransitions = from transition in transitionMap[n] 
                                             where transition.Destination == node 
                                             select transition;

                    foreach (var transition in relatedTransitions.ToList())
                        transitionMap[n].Remove(transition);
                }
                
                // TODO: remove node from node stack also
            }
        }

        public void SetCurrentNode(INode<T> node, T context)
        {
            if (!Contains(node))
                return;
            
            TransitionToNode(node, context);
        }

        // peek node from the tail of the stack.
        // Index 0 means the current node (tail index)
        // Index 1 means the previous node (tail - 1 index)
        public INode<T> GetNodeHistory(int index)
        {
            return nodeStack.PeekTail(index);
        }

        public RegularSourceNode<T> AddTransitionFrom(INode<T> source) => new RegularSourceNode<T>(this, source);
        public AnyNode<T> AddTransitionFromAnyNode() => new AnyNode<T>(this);
        public SelectorNode<T> AddTransitionFromSelectorNode() => new SelectorNode<T>(this);

        protected internal void AddTransition(INode<T> source, INode<T> destination, Func<bool> predicate)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition<T>(source, destination, predicate));
        }

        protected internal void AddTransition(INode<T> source, INode<T> destination, UnityEvent unityEvent)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition<T>(source, destination, unityEvent));
        }
        
        protected internal void AddTransition(INode<T> source, INode<T> destination, UnityEvent<T> unityEvent)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition<T>(source, destination, unityEvent));
        }
        
        protected internal void AddTransitionFromAnyNode(INode<T> destination, Func<bool> predicate)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new PollCondition(predicate));
        }

        protected internal void AddTransitionFromAnyNode(INode<T> destination, UnityEvent unityEvent)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new UnityEventCondition(unityEvent));
        }
        
        protected internal void AddTransitionFromAnyNode(INode<T> destination, UnityEvent<T> unityEvent)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new UnityEventCondition<T>(unityEvent));
        }

        protected internal void AddTransitionToPreviousNode(INode<T> source, Func<bool> predicate) => AddTransition(source, traceBackNode, predicate);
        protected internal void AddTransitionToPreviousNode(INode<T> source, UnityEvent unityEvent) => AddTransition(source, traceBackNode, unityEvent);
        protected internal void AddTransitionToPreviousNode(INode<T> source, UnityEvent<T> unityEvent) => AddTransition(source, traceBackNode, unityEvent);

        protected internal void AddTransitionToExitNode(INode<T> source, Func<bool> predicate) => AddTransition(source, exitNode, predicate);
        protected internal void AddTransitionToExitNode(INode<T> source, UnityEvent unityEvent) => AddTransition(source, exitNode, unityEvent);
        protected internal void AddTransitionToExitNode(INode<T> source, UnityEvent<T> unityEvent) => AddTransition(source, exitNode, unityEvent);

        protected internal void AddTransitionFromSelectorNode(INode<T> destination, Func<bool> predicate) => AddTransition(selectorNode, destination, predicate);
        protected internal void AddTransitionFromSelectorNode(INode<T> destination, UnityEvent unityEvent) => AddTransition(selectorNode, destination, unityEvent);
        protected internal void AddTransitionFromSelectorNode(INode<T> destination, UnityEvent<T> unityEvent) => AddTransition(selectorNode, destination, unityEvent);
        #endregion

        #region Transition Operations
        protected void RegisterTransition(ITransition<T> transition)
        {
            if (transition.Source == selectorNode)
            {
                transitionMap[selectorNode].Insert(0, transition);
                return;
            }
            
            transitionMap[transition.Source].Add(transition);
        }
        
        protected void RegisterAnyNodeTransition(INode<T> destination, ICondition condition)
        {
            if (!anyNodeTransitionMap.ContainsKey(destination))
                anyNodeTransitionMap.Add(destination, new List<ITransition<T>>());

            anyNodeTransitionMap[destination].Add(new Transition<T>(null, destination, condition));
        }
        
        protected ITransition<T> CheckForQualifiedTransition(List<ITransition<T>> transitionSet)
        {
            foreach (var destinationNode in anyNodeTransitionMap.Keys)
            {
                if (destinationNode == currentNode)
                    continue;
                
                foreach (var transition in anyNodeTransitionMap[destinationNode])
                {
                    if (transition.Condition.IsTrue())
                        return transition;
                }
            }

            foreach (var transition in transitionSet)
            {
                if (transition.Condition.IsTrue())
                    return transition;
            }
            
            return null;
        }
        
        protected void TransitionToNode(INode<T> node, T context)
        {
            currentNode.OnExit(context);
            DeactivateConditions(currentNode);
            currentNode = node;
            ActivateConditions(currentNode);
            currentNode.OnEnter(context);
            
            nodeStack.PushTail(currentNode);
            currentTransitionSet = transitionMap[currentNode];
        }

        protected void ActivateConditions(INode<T> node)
        {
            foreach (var destinationNode in anyNodeTransitionMap.Keys)
            {
                foreach (var transition in anyNodeTransitionMap[destinationNode])
                    transition.ActivateCondition();
            }
            
            foreach (var transition in transitionMap[node])
                transition.ActivateCondition();
        }
        
        protected void DeactivateConditions(INode<T> node)
        {
            foreach (var destinationNode in anyNodeTransitionMap.Keys)
            {
                foreach (var transition in anyNodeTransitionMap[destinationNode])
                    transition.DeactivateCondition();
            }
            
            foreach (var transition in transitionMap[node])
                transition.DeactivateCondition();
        }
        #endregion
        
        #region Validations
        protected bool IsValidNode(INode<T> node)
        {
            if (node == null)
            {
                Debug.Log($"{name}: node can not be NULL.");
                return false;
            }

            if (node == this)
            {
                Debug.Log($"{name}: can not add itself.");
                return false;
            }

            if (node.HasParent && !node.IsChildOf(this))
            {
                Debug.Log($"{name}: node is already owned by other node.");
                return false;
            }

            return true;
        }
        #endregion
    }
}