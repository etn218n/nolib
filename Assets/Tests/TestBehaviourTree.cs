using Nolib.Node;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestBehaviourTree
    {
        [Test]
        public void Sequencer_SucceedWhenAllChildrenSucceed()
        {
            var a  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var b  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var c  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var bt = new Sequencer(a, b, c);
            bt.Start();

            Assert.IsTrue(bt.Tick() == NodeStatus.Success);
        }
        
        [Test]
        public void Sequencer_FailWhenAnyChildFail()
        {
            var a  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var b  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var c  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var bt = new Sequencer(a, b, c);
            bt.Start();

            Assert.IsTrue(bt.Tick() == NodeStatus.Failure);
        }
        
        [Test]
        public void Selector_SucceedWhenAnyChildSucceed()
        {
            var a  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var b  = new ActionNode { TickAction = deltaTime => NodeStatus.Success };
            var c  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var bt = new Selector(a, b, c);
            bt.Start();

            Assert.IsTrue(bt.Tick() == NodeStatus.Success);
        }
        
        [Test]
        public void Selector_FailWhenAllChildrenFail()
        {
            var a  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var b  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var c  = new ActionNode { TickAction = deltaTime => NodeStatus.Failure };
            var bt = new Selector(a, b, c);
            bt.Start();

            Assert.IsTrue(bt.Tick() == NodeStatus.Failure);
        }
        
        [Test]
        public void Parallel_RunAllChildren_NonBlock()
        {
            var n  = 0;
            var a  = new ActionNode { TickAction = deltaTime => { n++; return NodeStatus.Running; }};
            var b  = new ActionNode { TickAction = deltaTime => { n++; return NodeStatus.Running; }};
            var c  = new ActionNode { TickAction = deltaTime => { n++; return NodeStatus.Running; }};
            var bt = new Parallel(Parallel.TerminationPolicy.AnyFailure, a, b, c);
            bt.Start();

            Assert.IsTrue(bt.Tick() == NodeStatus.Running && n == 3);
        }
    }
}