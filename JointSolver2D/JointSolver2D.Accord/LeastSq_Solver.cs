using System;
using System.Collections.Generic;
using System.Text;
using Accord.Math;

namespace JointSolver2D.Accord
{
    /// <summary>
    /// Solves the system of equations Ax=B using Accord.Net (with Least squares method for rectangular matrix A).
    /// </summary>
    public class LeastSq_Solver : SolverBase
    {
        protected double[,] A;
        protected double[] B;

        public override double[,] A_Array => A;

        public override double[] B_Array => B;

        protected override void SetupMatrices()
        {
            A = new double[NumberOfEquations, NumberOfUnknowns];
            B = new double[NumberOfEquations];
        }

        protected override void AddToMatrixA(int rowIndex, int columnIndex, double value)
        {
            A[rowIndex, columnIndex] = value;
        }

        protected override void AddToVectorB(int rowIndex, double value)
        {
            B[rowIndex] = value;
        }


        protected override double[] SolveForMatrixX()
        {
            return Matrix.Solve(A, B, true);
        }

        protected override double[] SolveXForGivenB(double[] vectorB)
        {
            return Matrix.Solve(A, vectorB, true);
        }
    }
}
