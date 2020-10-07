using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using js = JointSolver2D;
 
namespace JointSolver2D.Dynamo
{
    //Todo, look into default values for dynamo!!!

    /// <summary>
    /// Input data used to create a JSNode within the core library.
    /// </summary>
    public static class JSNode
    {
        [NodeCategory("Create")]
        public static js.JSNode ByCoordinates_Released(double x, double y) => new js.JSNode(new Vector2d(x, y));

        [NodeCategory("Create")]
        public static js.JSNode ByCoordinates_Restrained(double x, double y) => new js.JSNode(new Vector2d(x, y), true, true);

        [NodeCategory("Create")]
        public static js.JSNode ByCoordinates_Partial(double x, double y, bool xRestrained, bool yRestrained) => new js.JSNode(new Vector2d(x, y), xRestrained, yRestrained);
        //private Vector2d Position;

        ///// <summary>
        ///// Set to true if the X direction of this node is restrained.
        ///// </summary>
        //private bool XRestrained;

        ///// <summary>
        ///// Set to true if the Y direction of this node is restrained.
        ///// </summary>
        //private bool YRestrained;

        //public JSNode(Vector2d position, bool xRestrained = false, bool yRestrained = false)
        //{
        //    Position = position;
        //    XRestrained = xRestrained;
        //    YRestrained = yRestrained;
        //}

        //public js.JSNode CreateNode() => new js.JSNode(Position, XRestrained, YRestrained);

        //[NodeCategory("Create")]
        //public static JSNode ByCoordinates_Released(double x, double y) => new JSNode(new Vector2d(x, y));

        //[NodeCategory("Create")]
        //public static JSNode ByCoordinates_Restrained(double x, double y) => new JSNode(new Vector2d(x, y), true, true);

        //[NodeCategory("Create")]
        //public static JSNode ByCoordinates_Partial(double x, double y, bool xRestrained, bool yRestrained) => new JSNode(new Vector2d(x, y), xRestrained, yRestrained);

    }

    public static class JSBar
    {
        /// <summary>
        /// Create a bar between 2 nodes.
        /// </summary>
        public static js.JSBar ByStartNodeEndNode(js.JSNode startNode, js.JSNode endNode) => new js.JSBar(startNode, endNode);

        /// <summary>
        /// Create a bar between 2 positions (unconnected to nodes).
        /// Node connections will be formed when this bar is added to a JointAnalysis model.
        /// </summary>
        public static js.JSBar ByStartPtEndPt(Vector2d startPoint, Vector2d endPoint) => new js.JSBar(startPoint, endPoint);

        /// <summary>
        /// Create a bar between a node and a point positions (unconnected to nodes).
        /// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        /// </summary>
        public static js.JSBar ByStartNodeEndPt(js.JSNode startNode, Vector2d endPoint) => new js.JSBar(startNode, endPoint);

        /// <summary>
        /// Create a bar between a node and a point positions (unconnected to nodes).
        /// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        /// </summary>
        public static js.JSBar ByStartPtEndNode(Vector2d startPoint, js.JSNode endNode) => new js.JSBar(startPoint, endNode);

        //public JSVertex StartVertex;
        //public JSVertex EndVertex;

        ///// <summary>
        ///// Create a bar between 2 nodes.
        ///// </summary>
        //public JSBar(JSNode startNode, JSNode endNode)
        //{
        //    StartVertex = new JSVertex(startNode, this);
        //    EndVertex = new JSVertex(endNode, this);
        //}

        ///// <summary>
        ///// Create a bar between 2 positions (unconnected to nodes).
        ///// Node connections will be formed when this bar is added to a JointAnalysis model.
        ///// </summary>
        //public JSBar(Vector2d startPoint, Vector2d endPoint)
        //{
        //    StartVertex = new JSVertex(startPoint, this);
        //    EndVertex = new JSVertex(endPoint, this);
        //}

        ///// <summary>
        ///// Create a bar between a node and a point positions (unconnected to nodes).
        ///// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        ///// </summary>
        //public JSBar(JSNode startNode, Vector2d endPoint)
        //{
        //    StartVertex = new JSVertex(startNode, this);
        //    EndVertex = new JSVertex(endPoint, this);
        //}

        ///// <summary>
        ///// Create a bar between a node and a point positions (unconnected to nodes).
        ///// The unconnected vertex position will be connected to a node when this bar is added to a JointAnalysis model.
        ///// </summary>
        //public JSBar(Vector2d startPoint, JSNode endNode)
        //{
        //    StartVertex = new JSVertex(startPoint, this);
        //    EndVertex = new JSVertex(endNode, this);
        //}
    }

    public static class JSLoads
    {
        public static JSPointLoad PointLoad(double Fx, double Fy, js.JSNode node) => new JSPointLoad(new Vector2d(Fx, Fy), node);
    }

    public class Analysis
    {
        //Private constructor to hide it from Dynamo
        private Analysis() { }

        public JSModel model;

        /// <summary>
        /// Do not supply the same nodes or bars to the same 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="bars"></param>
        /// <param name="pointLoads"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Analysis Execute(List<js.JSNode> nodes, List<js.JSBar> bars, List<js.JSPointLoad> pointLoads)
        {
            var analysis = new Analysis();

            var jointAnalysis = new js.JSModel();
            jointAnalysis.AddItems(bars, nodes, pointLoads);
            jointAnalysis.Solve();

            analysis.model = jointAnalysis;
            return analysis;
        }

        [NodeCategory("Query")]
        public List<double> GetAllBarForces() => model.Bars.Select(bar => bar.ForceResult).ToList();

        [NodeCategory("Query")]
        public double GetBarForce(js.JSBar bar) 
        {
            if (model.Bars.Contains(bar))
                return bar.ForceResult;

            return double.NaN;
        }

        [NodeCategory("Query")][MultiReturn("Fx", "Fy")]
        public Dictionary<string, List<double>> GetAllNodeReactions()
        {
            var fx = new List<double>();
            var fy = new List<double>();
            foreach (var node in model.Nodes)
            {
                fx.Add(node.ReactionResult.X);
                fy.Add(node.ReactionResult.Y);
            }

            return new Dictionary<string, List<double>>()
            {
                { "Fx", fx },
                { "Fy", fy }
            };
        }

        [NodeCategory("Query")][MultiReturn("Fx", "Fy")]
        public Dictionary<string, double> GetNodeReaction(js.JSNode node)
        {
            if (model.Nodes.Contains(node))
                return new Dictionary<string, double>() 
                {
                    { "Fx", node.ReactionResult.X },
                    { "Fy", node.ReactionResult.Y }
                };

            return new Dictionary<string, double>()
            {
                { "Fx", double.NaN },
                { "Fy", double.NaN }
            };
        }

        [NodeCategory("Query")]
        [MultiReturn("dx", "dy")]
        public Dictionary<string, List<double>> GetAllNodeDeflections()
        {
            var dx = new List<double>();
            var dy = new List<double>();
            foreach (var node in model.Nodes)
            {
                dx.Add(node.Deflection.X);
                dy.Add(node.Deflection.Y);
            }

            return new Dictionary<string, List<double>>()
            {
                { "dx", dx },
                { "dy", dy }
            };
        }

        [NodeCategory("Query")]
        [MultiReturn("dx", "dy")]
        public Dictionary<string, double> GetNodeDeflection(js.JSNode node)
        {
            if (model.Nodes.Contains(node))
                return new Dictionary<string, double>()
                {
                    { "dx", node.Deflection.X },
                    { "dy", node.Deflection.Y }
                };

            return new Dictionary<string, double>()
            {
                { "dx", double.NaN },
                { "dy", double.NaN }
            };
        }

        [NodeCategory("Action")]
        public string WriteDebugReport() => model.WriteReport();
    }

    public class TestItem
    {
        private double value = 0;
        private string timeUpdated;

        public double Value
        {
            get => value;
            set
            {
                this.value = value;
                TimeUpdated = DateTime.Now.ToString();
            }
        }

        public string TimeUpdated { get => timeUpdated; set => timeUpdated = value; }

        public TestItem(double value)
        {
            Value = value;

        }

        public static void UpdateValue(TestItem item, double value)
        {
            item.Value = value;
        }

        public static TestItem Update2Value(TestItem item, double value)
        {
            item.Value = value;
            return item;
        }


        public double GetValue(TestItem item, double value)
        {
            return item.Value;
        }

        public static List<TestItem> IncrementAll(List<TestItem> list)
        {
            foreach (var item in list)
            {
                item.Value += 1;
            }

            return list;
        }

        public static double ReadResult(List<TestItem> list, TestItem item)
        {
            if (list.Contains(item))
                return item.value;

            return double.NaN;
        }
    }
}
