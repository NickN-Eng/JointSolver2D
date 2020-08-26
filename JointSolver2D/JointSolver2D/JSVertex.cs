
namespace JointSolver2D
{
    public class JSVertex
    {
        /// <summary>
        /// The position of this node
        /// </summary>
        public Vector2d Position;

        /// <summary>
        /// The node which this vertex is connected to.
        /// If this is empty, this will be populated by an existing node when added to the JointAnalysis model.
        /// </summary>
        public JSNode Node;

        /// <summary>
        /// The parent bar.
        /// </summary>
        public JSBar Bar;

        /// <summary>
        /// Create a vertex without a node.
        /// The node will be populated when it is added to the JointAnalysis model.
        /// </summary>
        public JSVertex(Vector2d position, JSBar bar)
        {
            Position = position;
            Bar = bar;
        }

        /// <summary>
        /// Create a vertex to a node.
        /// </summary>
        public JSVertex(JSNode node, JSBar bar)
        {
            Position = node.Position;
            Node = node;
            Bar = bar;
        }

        public JSVertex GetOtherVertex()
        {
            //Compact form without error checking
            //if (Bar.StartVertex != this) return Bar.StartVertex;
            //else return Bar.EndVertex;

            //Verboose form with error checks
            bool isStart = Bar.StartVertex == this;
            bool isEnd = Bar.EndVertex == this;
            if (!isStart && !isEnd) throw new System.Exception("Bar nodes do not match either vertex");
            if (isStart && isEnd) throw new System.Exception("Bar nodes match either vertex");
            if (isStart) return Bar.EndVertex;
            else return Bar.StartVertex;
        }
    }

}
