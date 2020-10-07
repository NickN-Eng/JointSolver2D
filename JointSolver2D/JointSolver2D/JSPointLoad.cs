
namespace JointSolver2D
{
    /// <summary>
    /// A point load which is applied by adding it to the JointAnalysis model.
    /// </summary>
    public class JSPointLoad
    {
        public JSNode Node;
        public Vector2d Force;

        public JSPointLoad (Vector2d forces, JSNode trussNode)
        {
            Force = forces;
            Node = trussNode;
        }

        public override string ToString()
        {
            return $"Point load {Force.ToString_Short()} - at {Node?.Position.ToString_Short()}";
        }
    }
}
