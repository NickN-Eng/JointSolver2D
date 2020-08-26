
namespace JointSolver2D
{
    public class JSBar
    {
        public JSVertex StartVertex;
        public JSVertex EndVertex;

        /// <summary>
        /// The bar number (zero based index).
        /// Also the index of vector x to which Rx of this node corresponds (where the bar/node forces are solved by the system of equations Ax = B).
        /// </summary>
        public int Number;

        private double? _Length;
        /// <summary>
        /// The length of this bar
        /// </summary>
        public double Length
        {
            get
            {
                if (!_Length.HasValue)
                    _Length = (EndPosition - StartPosition).Length();
                return _Length.Value;
            }
        }

        public Vector2d StartPosition => StartVertex.Node.Position;
        public Vector2d EndPosition => EndVertex.Node.Position;

        public Vector2d MidPoint => (StartPosition + EndPosition) / 2;

        /// <summary>
        /// The internal force in this bar where TENSION is +ve. (populated after a JointAnalysis).
        /// </summary>
        public double ForceResult;

        /// <summary>
        /// Create a bar between 2 nodes.
        /// </summary>
        public JSBar (JSNode startNode, JSNode endNode)
        {
            StartVertex = new JSVertex(startNode, this);
            EndVertex = new JSVertex(endNode, this);
        }

        /// <summary>
        /// Create a bar between 2 positions (unconnected to nodes).
        /// Node connections will be formed when this bar is added to a JointAnalysis model.
        /// </summary>
        public JSBar(Vector2d startPoint, Vector2d endPoint)
        {
            StartVertex = new JSVertex(startPoint, this);
            EndVertex = new JSVertex(endPoint, this);
        }

        /// <summary>
        /// Create a bar between a node and a point positions (unconnected to nodes).
        /// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        /// </summary>
        public JSBar(JSNode startNode, Vector2d endPoint)
        {
            StartVertex = new JSVertex(startNode, this);
            EndVertex = new JSVertex(endPoint, this);
        }

        /// <summary>
        /// Create a bar between a node and a point positions (unconnected to nodes).
        /// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        /// </summary>
        public JSBar(Vector2d startPoint, JSNode endNode)
        {
            StartVertex = new JSVertex(startPoint, this);
            EndVertex = new JSVertex(endNode, this);
        }

        protected internal void Reset(int newNumber = -1)
        {
            Number = newNumber;
            ForceResult = 0;
        }
    }

}
