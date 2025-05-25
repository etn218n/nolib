namespace Nolib.Node
{
    public class EmptyNode : INode
    {
        private INode parent;

        INode INode.Parent
        {
            get => parent;
            set => parent = value;
        }
    }
}