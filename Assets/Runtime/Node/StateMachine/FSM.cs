using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Nolib.DataStructure;

namespace Nolib.Node
{
    public class FSM : INode
    {
        #region Fields & Properties
        protected readonly string name;
        protected readonly CircularBuffer<INode> nodeStack;
        protected readonly Dictionary<INode, List<ITransition>> transitionMap;
        protected readonly Dictionary<INode, List<ITransition>> anyNodeTransitionMap;

        protected INode parent;
        protected INode selectorNode;
        protected INode exitNode;
        protected INode currentNode;
        protected INode traceBackNode;
        
        protected bool isCompleted;
        protected int maxStackSize;
        protected List<ITransition> currentTransitionSet;

        public string Name => name;
        public int StateCount => transitionMap.Keys.Count - 1;
        public int MaxStackSize => maxStackSize;
        public bool IsCompleted => isCompleted;
        public INode CurrentNode => currentNode;
        public IReadOnlyCollection<INode> Nodes => transitionMap.Keys;
        public IReadOnlyCollection<ITransition> CurrentTransitionSet => currentTransitionSet;
        public IReadOnlyCollection<ITransition> TransitionsFrom(INode node) => transitionMap[node];
        
        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }

        public FSM(string name = "FSM", int maxStackSize = 20)
        {
            this.name = name;
            this.maxStackSize = maxStackSize;
            
            nodeStack = new CircularBuffer<INode>(maxStackSize);
            transitionMap = new Dictionary<INode, List<ITransition>>();
            anyNodeTransitionMap = new Dictionary<INode, List<ITransition>>();
            currentTransitionSet = new List<ITransition>();
            
            selectorNode  = new EmptyNode();
            traceBackNode = new EmptyNode();
            exitNode      = new ActionNode { EnterAction = () => isCompleted = true };
            currentNode   = selectorNode;
            
            AddNode(selectorNode);
            TransitionToNode(selectorNode);
        }
        #endregion

        #region Run FSM
        public void Start(INode entryNode = null)
        {
            isCompleted = false;

            if (entryNode != null && transitionMap.ContainsKey(entryNode))
                SetEntryNode(entryNode);
            
            var qualifiedTransition = CheckForQualifiedTransition(transitionMap[selectorNode]);

            if (qualifiedTransition != null)
                TransitionToNode(qualifiedTransition.Destination);
        }

        public NodeStatus Tick(float deltaTime)
        {
            var qualifiedTransition = CheckForQualifiedTransition(currentTransitionSet);
            
            if (qualifiedTransition != null)
            {
                if (qualifiedTransition.Destination == traceBackNode && nodeStack.Count >= 2)
                {
                    nodeStack.PopTail();
                    TransitionToNode(nodeStack.PopTail());
                }
                else
                {
                    TransitionToNode(qualifiedTransition.Destination);
                }
            }
            
            currentNode.OnTick(deltaTime);
            
            return NodeStatus.Running;
        }

        public void PreTick(float deltaTime) => currentNode.OnPreTick(deltaTime);
        public void PostTick(float deltaTime) => currentNode.OnPostTick(deltaTime);
        public void Update(float deltaTime) => currentNode.OnUpdate(deltaTime);
        public void FixedUpdate(float deltaTime) => currentNode.OnFixedUpdate(deltaTime);
        public void LateUpdate(float deltaTime) => currentNode.OnLateUpdate(deltaTime);
        #endregion

        #region Node Callbacks
        NodeStatus INode.OnTick(float deltaTime) => Tick(deltaTime);
        void INode.OnPreTick(float deltaTime) => PreTick(deltaTime);
        void INode.OnPostTick(float deltaTime) => PostTick(deltaTime);
        void INode.OnEnter() => Start();
        void INode.OnUpdate(float deltaTime) => Update(deltaTime);
        void INode.OnFixedUpdate(float deltaTime) => FixedUpdate(deltaTime);
        void INode.OnLateUpdate(float deltaTime) => LateUpdate(deltaTime);
        void INode.OnExit()
        {
            nodeStack.Clear();
            TransitionToNode(selectorNode);
        }
        #endregion

        #region Node Operations
        public bool Contains(INode node)
        {
            if (transitionMap.ContainsKey(node))
                return true;
            
            var subFSMs = from n in Nodes
                          where n is FSM
                          select n as FSM;

            return subFSMs.Any(sub => sub.Contains(node));
        }

        protected void AddNode(INode node)
        {
            if (!node.IsChildOf(this))
                node.Attach(this);
            
            if (transitionMap.Count == 1)
                SetEntryNode(node);
            
            if (!transitionMap.ContainsKey(node))
                transitionMap.Add(node, new List<ITransition>());
        }
        
        protected void SetEntryNode(INode node)
        {
            // override all transitions from Selector node
            transitionMap[selectorNode].Clear();
            transitionMap[selectorNode].Add(new Transition(selectorNode, node, () => true));
        }

        public void RemoveNode(INode node)
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

        public void SetCurrentNode(INode node)
        {
            if (!Contains(node))
                return;
            
            TransitionToNode(node);
        }

        // peek node from the tail of the stack.
        // Index 0 means the current node (tail index)
        // Index 1 means the previous node (tail - 1 index)
        public INode GetNodeHistory(int index)
        {
            return nodeStack.PeekTail(index);
        }

        public RegularSourceNode AddTransitionFrom(INode source) => new RegularSourceNode(this, source);
        public AnyNode AddTransitionFromAnyNode() => new AnyNode(this);
        public SelectorNode AddTransitionFromSelectorNode() => new SelectorNode(this);

        protected internal void AddTransition(INode source, INode destination, Func<bool> predicate)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition(source, destination, predicate));
        }

        protected internal void AddTransition(INode source, INode destination, UnityEvent unityEvent)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition(source, destination, unityEvent));
        }
        
        protected internal void AddTransition<T>(INode source, INode destination, UnityEvent<T> unityEvent)
        {
            if (!IsValidNode(source) || !IsValidNode(destination))
                return;
            
            AddNode(source);
            AddNode(destination);
            RegisterTransition(new Transition<T>(source, destination, unityEvent));
        }
        
        protected internal void AddTransitionFromAnyNode(INode destination, Func<bool> predicate)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new PollCondition(predicate));
        }

        protected internal void AddTransitionFromAnyNode(INode destination, UnityEvent unityEvent)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new UnityEventCondition(unityEvent));
        }
        
        protected internal void AddTransitionFromAnyNode<T>(INode destination, UnityEvent<T> unityEvent)
        {
            if (!IsValidNode(destination))
                return;

            AddNode(destination);
            RegisterAnyNodeTransition(destination, new UnityEventCondition<T>(unityEvent));
        }

        protected internal void AddTransitionToPreviousNode(INode source, Func<bool> predicate) => AddTransition(source, traceBackNode, predicate);
        protected internal void AddTransitionToPreviousNode(INode source, UnityEvent unityEvent) => AddTransition(source, traceBackNode, unityEvent);
        protected internal void AddTransitionToPreviousNode<T>(INode source, UnityEvent<T> unityEvent) => AddTransition(source, traceBackNode, unityEvent);

        protected internal void AddTransitionToExitNode(INode source, Func<bool> predicate) => AddTransition(source, exitNode, predicate);
        protected internal void AddTransitionToExitNode(INode source, UnityEvent unityEvent) => AddTransition(source, exitNode, unityEvent);
        protected internal void AddTransitionToExitNode<T>(INode source, UnityEvent<T> unityEvent) => AddTransition(source, exitNode, unityEvent);

        protected internal void AddTransitionFromSelectorNode(INode destination, Func<bool> predicate) => AddTransition(selectorNode, destination, predicate);
        protected internal void AddTransitionFromSelectorNode(INode destination, UnityEvent unityEvent) => AddTransition(selectorNode, destination, unityEvent);
        protected internal void AddTransitionFromSelectorNode<T>(INode destination, UnityEvent<T> unityEvent) => AddTransition(selectorNode, destination, unityEvent);
        #endregion

        #region Transition Operations
        protected void RegisterTransition(ITransition transition)
        {
            if (transition.Source == selectorNode)
            {
                transitionMap[selectorNode].Insert(0, transition);
                return;
            }
            
            transitionMap[transition.Source].Add(transition);
        }
        
        protected void RegisterAnyNodeTransition(INode destination, ICondition condition)
        {
            if (!anyNodeTransitionMap.ContainsKey(destination))
                anyNodeTransitionMap.Add(destination, new List<ITransition>());

            anyNodeTransitionMap[destination].Add(new Transition(null, destination, condition));
        }
        
        protected ITransition CheckForQualifiedTransition(List<ITransition> transitionSet)
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
        
        protected void TransitionToNode(INode node)
        {
            currentNode.OnExit();
            DeactivateConditions(currentNode);
            currentNode = node;
            ActivateConditions(currentNode);
            currentNode.OnEnter();
            
            nodeStack.PushTail(currentNode);
            currentTransitionSet = transitionMap[currentNode];
        }

        protected void ActivateConditions(INode node)
        {
            foreach (var destinationNode in anyNodeTransitionMap.Keys)
            {
                foreach (var transition in anyNodeTransitionMap[destinationNode])
                    transition.ActivateCondition();
            }
            
            foreach (var transition in transitionMap[node])
                transition.ActivateCondition();
        }
        
        protected void DeactivateConditions(INode node)
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
        protected bool IsValidNode(INode node)
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