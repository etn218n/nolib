namespace Nolib.Node.Generic
{
    public class EmptyNode<T> : INode<T>
    {
        private INode<T> parent;

        INode<T> INode<T>.Parent
        {
            get => parent;
            set => parent = value;
        }
    }
}