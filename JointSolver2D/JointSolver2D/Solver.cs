using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JointSolver2D
{
    /// <summary>
    /// A template for solver functions
    /// </summary>
    public abstract class SolverBase
    {
        /// <summary>
        /// Initalises the internal arrays A and B used by the solver implementation
        /// A size is [NumberOfEquations,NumberOfUnknowns], B length is NumberOfEquations
        /// (Used in step 1 of solver process).
        /// </summary>
        protected abstract void SetupMatrices();

        /// <summary>
        /// The implementation should insert a value into matrix A. (Used in step 2 of solver process).
        /// </summary>
        protected abstract void AddToMatrixA(int rowIndex, int columnIndex, double value);

        /// <summary>
        /// The implementation should insert a value into vector B. (Used in step 2 of solver process).
        /// </summary>
        protected abstract void AddToVectorB(int rowIndex, double value);

        /// <summary>
        /// The implementation should solves the matrix equation Ax=B, and returns the force vector X (Used in step 3 of solver process).
        /// </summary>
        /// <returns>Returns the matrix X result.</returns>
        protected abstract double[] SolveForMatrixX();


        /// <summary>
        /// The implementation should solve the matrix equation Ax=B, given the supplied force vector b.
        /// Used in step 4, for the displacement step (virtual work method).
        /// </summary>
        /// <returns>Returns the matrix X result.</returns>
        protected abstract double[] SolveXForGivenB(double[] vectorB);

        /// <summary>
        /// The number of equations, i.e. number of rows of matrix A, and the length of vector B.
        /// This value is populated before step 1 - SetupMatrices()
        /// </summary>
        protected int NumberOfEquations { get; private set; }

        /// <summary>
        /// The number of equations, i.e. number of columns of matrix A.
        /// This value is populated before step 1 - SetupMatrices()
        /// </summary>
        protected int NumberOfUnknowns { get; private set; }

        /// <summary> Matrix A (for debug only). </summary>
        public abstract double[,] A_Array { get; }
        
        /// <summary> Vector B (for debug only). </summary>
        public abstract double[] B_Array { get; }

        /// <summary> The last Vector X (for debug only). </summary>
        public double[] X_Array { get; private set; }



        /// <summary>
        /// The smallest absolute value of bar force/reaction which will be recorded.
        /// This is necessary because the least squares method may result in very small force values where the result should be 0
        /// </summary>
        public const double MinForceValue = 0.0001;

        protected internal int EquationNo_GlobalFx => NumberOfEquations - 3;
        protected internal int EquationNo_GlobalFy => NumberOfEquations - 2;
        protected internal int EquationNo_GlobalM => NumberOfEquations - 1;

        /// <summary>
        /// Performs the analysis: populates bars with bar forces and nodes with node reactions.
        /// </summary>
        /// <param name="jointAnalysis">The joint analysis to be updated.</param>
        public void SolveForces(JSModel jointAnalysis)
        {
            var Nodes = jointAnalysis.Nodes;
            var Bars = jointAnalysis.Bars;
            var Loads = jointAnalysis.Loads;

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Reset(i);

            for (int i = 0; i < Bars.Count; i++)
                Bars[i].Reset(i);

            foreach (var pl in Loads)
            {
                pl.Node.AppliedForces += pl.Force;
            }

            int nodeCount = Nodes.Count;
            int xConstrainedCount = 0;
            int yConstrainedCount = 0;
            int variableNumber = Bars.Count;

            //Variables number
            //0 to Nbars-1 (for each bar force)
            //Nbars to Nbars+Nconstraints-1 (for each constrained direction on each node)

            foreach (var node in Nodes)
            {
                if (node.XRestrained)
                {
                    xConstrainedCount++;
                    node.VariableNumber_Rx = variableNumber++;
                }
                if (node.YRestrained)
                {
                    yConstrainedCount++;
                    node.VariableNumber_Ry = variableNumber++;
                }
            }

            NumberOfEquations = nodeCount * 2 + 3;
            NumberOfUnknowns = variableNumber;

            if (NumberOfEquations < variableNumber) throw new Exception("Number of unknowns exceeds the number of equations");

            //Unknowns list
            //Bars 0 => n
            //Reactions 0 => n
            SetupMatrices();

            int equationNo = 0;
            foreach (var node in Nodes)
            {
                AddNodeXEquilibrium(node, equationNo++);
                AddNodeYEquilibrium(node, equationNo++);
            }
            AddGlobalXEquilibrium(Nodes, equationNo++);
            AddGlobalYEquilibrium(Nodes, equationNo++);
            AddGlobalMomentEquilibrium(Nodes, equationNo++);

            var xResult = SolveForMatrixX();

            foreach (var node in Nodes)
            {
                node.ReactionResult = new Vector2d();
                if (node.XRestrained)
                {
                    var value = xResult[node.VariableNumber_Rx];
                    if(Math.Abs(value) > MinForceValue)
                        node.ReactionResult.X = value;
                }
                if (node.YRestrained)
                {
                    var value = xResult[node.VariableNumber_Ry];
                    if (Math.Abs(value) > MinForceValue)
                        node.ReactionResult.Y = value;
                }
            }

            foreach (var bar in Bars)
            {
                var value = xResult[bar.Number];
                bar.ForceResult = Math.Abs(value) > MinForceValue ? value : 0;
            }

            X_Array = xResult;

        }

        private void AddNodeXEquilibrium(JSNode node, int equationRowNo)
        {
            if (equationRowNo != node.EquationNo_Fx) throw new Exception("Check why these numbers are not equal!!");

            foreach (var vert in node.Vertices)
            {
                var otherNode = vert.GetOtherVertex().Node;
                var bar = vert.Bar;
                var constant = (otherNode.Position.X - node.Position.X) / bar.Length;
                AddToMatrixA(equationRowNo, bar.Number, constant);
            }

            if (node.XRestrained)
                AddToMatrixA(equationRowNo, node.VariableNumber_Rx, 1);

            AddToVectorB(equationRowNo, -node.AppliedForces.X);
        }

        private void AddNodeYEquilibrium(JSNode node, int equationRowNo)
        {
            if (equationRowNo != node.EquationNo_Fy) throw new Exception("Check why these numbers are not equal!!");

            foreach (var vert in node.Vertices)
            {
                var otherNode = vert.GetOtherVertex().Node;
                var bar = vert.Bar;
                var constant = (otherNode.Position.Y - node.Position.Y) / bar.Length;
                AddToMatrixA(equationRowNo, bar.Number, constant);
            }

            if (node.YRestrained)
                AddToMatrixA(equationRowNo, node.VariableNumber_Ry, 1);

            AddToVectorB(equationRowNo, -node.AppliedForces.Y);
        }

        private void AddGlobalXEquilibrium(IList<JSNode> nodes, int equationRowNo)
        {
            if (equationRowNo != EquationNo_GlobalFx) throw new Exception("Check why these numbers are not equal!!");

            double totalAppliedXForce = 0;
            foreach (var node in nodes)
            {
                if (node.XRestrained)
                    AddToMatrixA(equationRowNo, node.VariableNumber_Rx, 1);

                totalAppliedXForce += node.AppliedForces.X;
            }
            AddToVectorB(equationRowNo, -totalAppliedXForce);
        }

        private void AddGlobalYEquilibrium(IList<JSNode> nodes, int equationRowNo)
        {
            if (equationRowNo != EquationNo_GlobalFy) throw new Exception("Check why these numbers are not equal!!");

            double totalAppliedYForce = 0;
            foreach (var node in nodes)
            {
                if (node.YRestrained)
                    AddToMatrixA(equationRowNo, node.VariableNumber_Ry, 1);

                totalAppliedYForce += node.AppliedForces.Y;
            }
            AddToVectorB(equationRowNo, -totalAppliedYForce);
        }

        private void AddGlobalMomentEquilibrium(IList<JSNode> nodes, int equationRowNo)
        {
            if (equationRowNo != EquationNo_GlobalM) throw new Exception("Check why these numbers are not equal!!");
            
            double totalAppliedMoment = 0;
            foreach (var node in nodes)
            {
                if (node.XRestrained)
                    AddToMatrixA(equationRowNo, node.VariableNumber_Rx, node.Position.Y);

                if (node.YRestrained)
                    AddToMatrixA(equationRowNo, node.VariableNumber_Ry, - node.Position.X);

                totalAppliedMoment += node.AppliedForces.X * node.Position.Y;
                totalAppliedMoment += - node.AppliedForces.Y * node.Position.X;
            }
            AddToVectorB(equationRowNo, -totalAppliedMoment);
        }


        public void SolveDeflections(JSModel model)
        {
            var LoverAE = new double[model.Bars.Count];
            foreach (var bar in model.Bars)
            {
                LoverAE[bar.Number] = bar.Length / (double.IsNaN(bar.EA) ? model.EA_default : bar.EA);
            }

            foreach (var node in model.Nodes)
            {
                var defl = new Vector2d();
                if (!node.XRestrained) defl.X = CalculateDeflection(node, GetVirtualVectorB_FromVirtualForceFx(node), LoverAE);
                if (!node.YRestrained) defl.Y = CalculateDeflection(node, GetVirtualVectorB_FromVirtualForceFy(node), LoverAE);
                node.Deflection = defl;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="virtualVectorB"></param>
        /// <param name="LoverAE">L/AE. A vector with a value for every bar.</param>
        /// <returns></returns>
        private double CalculateDeflection(JSNode node, double[] virtualVectorB, double[] LoverAE)
        {
            var virtualX = SolveXForGivenB(virtualVectorB);
            var realX = X_Array;

            double workDone = 0;
            for (int i = 0; i < LoverAE.Length; i++)
            {
                workDone += virtualX[i] * realX[i] * LoverAE[i];
            }
            //deflection = workDone / virtual load (1)
            return workDone;
        }

        private double[] GetVirtualVectorB_FromVirtualForceFx(JSNode node)
        {
            var result = new double[NumberOfEquations];
            result[node.EquationNo_Fx] = -1;
            result[EquationNo_GlobalFx] = -1;
            result[EquationNo_GlobalM] = node.Position.Y * 1;
            return result;
        }

        private double[] GetVirtualVectorB_FromVirtualForceFy(JSNode node)
        {
            var result = new double[NumberOfEquations];
            result[node.EquationNo_Fy] = -1;
            result[EquationNo_GlobalFy] = -1;
            result[EquationNo_GlobalM] = -node.Position.X * 1;
            return result;
        }

        #region Matrix Printing

        public const string PrintFormat = "F2";
        public const int PrintNumberLen = 7;

        public string GetDebugString_MatrixA()
        {
            List<string> strings = new List<string>();
            AddArrayToDebugString(strings, A_Array);
            return string.Join(Environment.NewLine, strings);
        }

        public string GetDebugString_MatrixAB()
        {
            List<string> strings = new List<string>();
            AddArrayToDebugString(strings, A_Array);
            AddArrayToDebugString(strings, B_Array, "   ");
            return string.Join(Environment.NewLine, strings);
        }

        public string GetDebugString_MatrixAXB()
        {
            List<string> strings = new List<string>();
            AddArrayToDebugString(strings, A_Array);
            AddArrayToDebugString(strings, X_Array, "   ");
            AddArrayToDebugString(strings, B_Array, "   ");
            return string.Join(Environment.NewLine, strings);
        }

        private void AddArrayToDebugString(List<string> wipStrings, double[,] array, string separator = "")
        {
            int rowLength = array.GetLength(0);
            int colLength = array.GetLength(1);

            PrepareWipStrings(wipStrings, rowLength, separator);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    var value = array[i, j];
                    wipStrings[i] += value.ToString(PrintFormat).PadLeft(PrintNumberLen);
                }
            }
        }

        private void AddArrayToDebugString(List<string> wipStrings, double[] array, string separator = "")
        {
            int rowLength = array.Length;

            PrepareWipStrings(wipStrings, rowLength, separator);

            for (int i = 0; i < rowLength; i++)
            {
                var value = array[i];
                wipStrings[i] += value.ToString(PrintFormat).PadLeft(PrintNumberLen);
            }
        }

        private static void PrepareWipStrings(List<string> wipStrings, int rowLength, string separator)
        {
            if (wipStrings.Count == 0)
            {
                for (int i = 0; i < rowLength; i++)
                {
                    wipStrings.Add(separator);
                }
            }
            else
            {
                int topStringLen = wipStrings[0].Length;

                for (int i = 0; i < rowLength; i++)
                {
                    if (i < wipStrings.Count)
                        wipStrings[i] += new string(' ', topStringLen - wipStrings[i].Length) + separator;
                    else
                        wipStrings.Add(new string(' ', topStringLen) + separator);
                }
            }
        }

        #endregion



    }

}
