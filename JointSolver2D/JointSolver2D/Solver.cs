using System;
using System.Collections.Generic;

namespace JointSolver2D
{
    //TODO:
    //Common method to print matrices - e.g. into an array for tabular view??

    /// <summary>
    /// A template for solver functions
    /// </summary>
    public abstract class SolverBase
    {
        /// <summary>
        /// Step 1: Initalises
        /// </summary>
        protected abstract void SetupMatrices();

        /// <summary>
        /// Step 2: Add to the 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="value"></param>
        protected abstract void AddToMatrixA(int rowIndex, int columnIndex, double value);
        protected abstract void AddToVectorB(int rowIndex, double value);
        
        /// <summary>
        /// Step 3: 
        /// </summary>
        /// <returns>Returns the matrix X result.</returns>
        protected abstract double[] SolveForMatrixX();

        protected int NumberOfEquations { get; private set; }
        protected int NumberOfUnknowns { get; private set; }

        public void Solve(JointAnalysis jointAnalysis)
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
                    node.ReactionResult.X = (float)xResult[node.VariableNumber_Rx];
                if (node.YRestrained)
                    node.ReactionResult.Y = (float)xResult[node.VariableNumber_Ry];
            }

            foreach (var bar in Bars)
            {
                bar.ForceResult = (float)xResult[bar.Number];
            }
        }

        private void AddNodeXEquilibrium(JSNode node, int equationRowNo)
        {
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

    }

}
