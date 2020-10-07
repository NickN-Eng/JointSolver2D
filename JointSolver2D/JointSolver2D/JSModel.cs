using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace JointSolver2D
{
    /// <summary>
    /// Contains a collection of Nodes, Bars and Loads, which can be fed to a solver.
    /// Joins bars with unconnected nodes etc...
    /// </summary>
    public class JSModel
    {
        public SolverBase Solver;

        public List<JSNode> Nodes = new List<JSNode>();
        public List<JSBar> Bars =   new List<JSBar>();
        public List<JSPointLoad> Loads = new List<JSPointLoad>();

        public double EA_default = 210e9 * (0.01 * 0.01);

        /// <summary> Time taken to solve for bar forces in miliseconds. </summary>
        public double SolveDuration_Force;

        /// <summary> Time taken to solve for bar deflections in miliseconds. </summary>
        public double SolveDuration_Deflection;

        public JSModel(SolverBase solver = null)
        {
            if (solver == null)
                Solver = new SparseArray_Solver();
            else
                Solver = solver;
        }

        /// <summary>
        /// Combine Nodes, Bars and Loads to form a model.
        /// Where bars are not connected to a node, this method will find an existing node to connect to, or create a new analysis node.
        /// Adding items in multiple batches has NOT been tested!
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="nodes"></param>
        /// <param name="pLoads"></param>
        public void AddItems(IList<JSBar> bars, IList<JSNode> nodes, IList<JSPointLoad> pLoads = null)
        {
            //A set of nodes, used to find existing nodes for un-noded bars
            var nodeSet = new HashSet<JSNode>(Nodes);

            //Add nodes
            if (nodes != null)
            {
                //Step 1: Register each node
                foreach (var node in nodes)
                {
                    nodeSet.Add(node);
                    node.Vertices.Clear();
                }
            }

            //Add bars
            if(bars != null)
            {
                //Step 2: Register each bar. For each start/end vertex:
                // - Add to the NodeSet if the vertices has a node]
                // - Otherwise, cache the vertex/position to populate the node at the next step
                var vertexesNeedingNodes = new List<JSVertex>(); //The cache of vertexes requiring nodes
                foreach (var bar in bars)
                {


                    //For the start vertex
                    var startNode = bar.StartVertex.Node;
                    if (startNode != null)
                    {
                        nodeSet.Add(startNode);
                        startNode.Vertices.Add(bar.StartVertex);
                    }
                    else
                        vertexesNeedingNodes.Add(bar.StartVertex);

                    //For the end vertex
                    var endNode = bar.EndVertex.Node;
                    if (endNode != null)
                    {
                        nodeSet.Add(endNode);
                        endNode.Vertices.Add(bar.EndVertex);
                    }
                    else
                        vertexesNeedingNodes.Add(bar.EndVertex);

                    Bars.Add(bar);
                }

                //Step 3: Find appropriate nodes to populate un-noded vertices
                foreach (var vert in vertexesNeedingNodes)
                {
                    FindOrCreateNodeForVertex(vert, nodeSet);
                }
            }

            Nodes = nodeSet.ToList();

            //Step 4: Add point loads
            if (pLoads != null)
            {
                Loads.AddRange(pLoads);
            }
        }

        /// <summary>
        /// An error check to find if nodes have the same position
        /// </summary>
        /// <returns></returns>
        public IList<JSNode> FindNodesWithClashingPositions()
        {
            var allClashingNodes = new HashSet<JSNode>();
            foreach (var node in Nodes)
            {
                foreach (var otherNode in Nodes)
                {
                    if (node == otherNode) continue;
                    if (PointsAreClose(node.Position, otherNode.Position))
                    {
                        allClashingNodes.Add(node);
                    }
                }
            }
            return allClashingNodes.ToList();
        }

        private void FindOrCreateNodeForVertex(JSVertex vertex, HashSet<JSNode> nodesSet)
        {
            var pos = vertex.Position;
            foreach (var node in nodesSet)
            {
                if (PointsAreClose(node.Position, pos))
                {
                    vertex.Node = node;
                    node.Vertices.Add(vertex);
                    return;
                }
            }
            var newNode = new JSNode(vertex.Position);
            newNode.Vertices.Add(vertex);
            nodesSet.Add(newNode);
            return;
        }

        /// <summary>
        /// Two positions are the same if the delta x & y are both less than this value
        /// </summary>
        public const double PointTolerance = 0.001;
        private static bool PointsAreClose(Vector2d point1, Vector2d point2)
        {
            var delta = point1 - point2;
            if (Math.Abs(delta.X) < PointTolerance && Math.Abs(delta.Y) < PointTolerance)
                return true;
            return false;
        }

        public void Solve()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Solver.SolveForces(this);
            stopWatch.Stop();
            SolveDuration_Force = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();
            stopWatch.Start();
            Solver.SolveDeflections(this);
            stopWatch.Stop();
            SolveDuration_Deflection = stopWatch.ElapsedMilliseconds;

            return;
        }



        public string WriteReport()
        {
            var strBuilder = new StringBuilder();

            strBuilder.Append($"Force analysis completed in {SolveDuration_Force} milliseconds" + Environment.NewLine);
            strBuilder.Append($"Deflection analysis completed in {SolveDuration_Deflection} milliseconds" + Environment.NewLine);
            strBuilder.Append(Environment.NewLine);
            strBuilder.Append("Bar forces as follows:" + Environment.NewLine);

            for (int i = 0; i < Bars.Count; i++)
            {
                var bar = Bars[i];
                strBuilder.Append($" - Bar {bar.Number}: Force={bar.ForceResult.ToString("F2")}" + Environment.NewLine);
            }

            strBuilder.Append(Environment.NewLine);
            strBuilder.Append("Node reactions as follows:" + Environment.NewLine);


            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                var curString = $" - Node {node.Number}:";

                if (node.AppliedForces != Vector2d.Zero)
                    curString += $" AppliedForce={node.AppliedForces.ToString_Short()}";

                if (node.XRestrained || node.YRestrained)
                    curString += $" Reaction={node.ReactionResult.ToString_Short()}";

                if (!node.XRestrained || !node.YRestrained)
                    curString += $" Deflection={node.Deflection.ToString_Short("G2")}";

                strBuilder.Append(curString + Environment.NewLine);
            }

            strBuilder.Append(Environment.NewLine);
            strBuilder.Append("Matrix in the form [A X = B] as follows:" + Environment.NewLine);
            strBuilder.Append(Solver.GetDebugString_MatrixAXB());

            return strBuilder.ToString();
        }
    }

}
