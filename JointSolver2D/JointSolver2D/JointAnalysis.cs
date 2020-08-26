using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace JointSolver2D
{
    public class JointAnalysis
    {
        public SolverBase Solver;

        public List<JSNode> Nodes = new List<JSNode>();
        public List<JSBar> Bars =   new List<JSBar>();
        public List<JSPointLoad> Loads = new List<JSPointLoad>();

        public JointAnalysis(SolverBase solver = null)
        {
            if (solver == null)
                Solver = new Accord_LeastSquares_Solver();
            else
                Solver = solver;
        }

        /// <summary>
        /// Combine Nodes, Bars and Loads to form a model.
        /// Where bars are not connected to a node, this method will find an existing node to connect to, or create a new analysis node.
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
            Solver.Solve(this);
            return;
        }

        //private void AddNodeXEquilibrium(JSNode node, int equationRowNo, List<Tuple<int, int, double>> equationContantsToUpdate, double[] equationResultToUpdate)
        //{
        //    foreach (var vert in node.Vertices)
        //    {
        //        var otherNode = vert.GetOtherVertex().Node;
        //        var bar = vert.Bar;
        //        var constant = (otherNode.Position.X - node.Position.X) / bar.Length;
        //        equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, bar.Number, constant));
        //    }

        //    if (node.XRestrained)
        //        equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Rx, 1));

        //    equationResultToUpdate[equationRowNo] = - node.AppliedForces.X;
        //}

        //private void AddNodeYEquilibrium(JSNode node, int equationRowNo, List<Tuple<int, int, double>> equationContantsToUpdate, double[] equationResultToUpdate)
        //{
        //    foreach (var vert in node.Vertices)
        //    {
        //        var otherNode = vert.GetOtherVertex().Node;
        //        var bar = vert.Bar;
        //        var constant = (otherNode.Position.Y - node.Position.Y) / bar.Length;
        //        equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, bar.Number, constant));
        //    }

        //    if (node.YRestrained)
        //        equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Ry, 1));

        //    equationResultToUpdate[equationRowNo] = -node.AppliedForces.Y;
        //}

        //private void AddGlobalXEquilibrium(IList<JSNode> nodes, int equationRowNo, List<Tuple<int, int, double>> equationContantsToUpdate, double[] equationResultToUpdate)
        //{
        //    double totalAppliedXForce = 0;
        //    foreach (var node in nodes)
        //    {
        //        if(node.XRestrained)
        //            equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Rx, 1));

        //        totalAppliedXForce += node.AppliedForces.X;
        //    }
        //    equationResultToUpdate[equationRowNo] = - totalAppliedXForce;
        //}

        //private void AddGlobalYEquilibrium(IList<JSNode> nodes, int equationRowNo, List<Tuple<int, int, double>> equationContantsToUpdate, double[] equationResultToUpdate)
        //{
        //    double totalAppliedYForce = 0;
        //    foreach (var node in nodes)
        //    {
        //        if (node.YRestrained)
        //            equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Ry, 1));

        //        totalAppliedYForce += node.AppliedForces.Y;
        //    }
        //    equationResultToUpdate[equationRowNo] = -totalAppliedYForce;
        //}

        //private void AddGlobalMomentEquilibrium(IList<JSNode> nodes, int equationRowNo, List<Tuple<int, int, double>> equationContantsToUpdate, double[] equationResultToUpdate)
        //{
        //    double totalAppliedMoment = 0;
        //    foreach (var node in nodes)
        //    {
        //        if (node.XRestrained)
        //            equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Rx, node.Position.Y));

        //        if (node.YRestrained)
        //            equationContantsToUpdate.Add(new Tuple<int, int, double>(equationRowNo, node.VariableNumber_Ry, node.Position.X));

        //        totalAppliedMoment += node.AppliedForces.X * node.Position.Y;
        //        totalAppliedMoment += node.AppliedForces.Y * node.Position.X;
        //    }
        //    equationResultToUpdate[equationRowNo] = -totalAppliedMoment;
        //}
    }

    //public class MathNet_RandomElimination_Solver : MathNet_Solver
    //{
    //    public Random Random = new Random();

    //    public MathNet_RandomElimination_Solver(JointAnalysis jointAnalysis) : base(jointAnalysis)
    //    {
    //    }

    //    public MathNet_RandomElimination_Solver(List<JSNode> nodes, List<JSBar> bars, List<JSPointLoad> loads) : base(nodes, bars, loads)
    //    {
    //    }

    //    protected override void SolveEquations()
    //    {
    //        int numberToElimiate = NumberOfEquations - NumberOfUnknowns;
    //        //Random.Next()

    //        var A = Matrix<double>.Build.Sparse(SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(NumberOfEquations, NumberOfUnknowns, EquationConstants));
    //        SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(NumberOfEquations, NumberOfUnknowns, EquationConstants).
    //        var str = A.ToMatrixString("F1");

    //        //Array.Resize(ref EquationResult, NumberOfEquations);
    //        var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(EquationResult);
    //        var b_Str = b.ToVectorString("F1");


    //        SolveReducedMatrix(A, b);
    //    }

    //    private static void SolveReducedMatrix(Matrix<double> A, MathNet.Numerics.LinearAlgebra.Vector<double> b)
    //    {
    //        var inv = A.Inverse();
    //        var invStr = inv.ToMatrixString("F1");

    //        var x = A.Solve(b);
    //        var x_Str = x.ToVectorString("F1");

    //        var xQR = A.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin).Solve(b);
    //        var xQR_Str = x.ToVectorString("F1");

    //        var xQRf = A.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full).Solve(b);
    //        var xQRf_Str = x.ToVectorString("F1");
    //    }

    //    public IEnumerable<Tuple<int, int, double>> IndexedIenumerableExclusions(List<Tuple<int, int, double>> tuples, int[] excludedRows)
    //    {
    //        foreach (var tuple in tuples)
    //        {
    //            if (!excludedRows.Contains(tuple.Item1))
    //                yield return tuple;
    //        }
    //    }

    //    public int[] PickFromLinearRangeWithoutReplacement(int rangeLength, int numberToPick)
    //    {
    //        var rand = new Random();
    //        List<int> possible = Enumerable.Range(0, 48).ToList();
    //        int[] listNumbers = new int[numberToPick];
    //        for (int i = 0; i < 6; i++)
    //        {
    //            int index = rand.Next(0, possible.Count);
    //            listNumbers.Add(possible[index]);
    //            possible.RemoveAt(index);
    //        }
    //    }
    //}
}
