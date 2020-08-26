using System;
using System.Collections.Generic;
using System.Text;
using Accord.Math;

namespace JointSolver2D
{
    /// <summary>
    /// Solves the system of equations Ax=B using Accord.Net (with Least squares method for rectangular matrix A).
    /// </summary>
    public class Accord_LeastSquares_Solver : SolverBase
    {
        protected double[,] A;
        protected double[] B;

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
            var result = PrintMatrices(Matrix.Solve(A, B, true));
            return Matrix.Solve(A, B, true);
        }

        /// <summary>
        /// TEMP - for debug only. 
        /// Creates a debug string showing the matrices in the form A - B
        /// </summary>
        public string PrintMatrices()
        {
            var result = "";

            int rowLength = A.GetLength(0);
            int colLength = A.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    var value = A[i, j];
                    result += $"{(value > 0 ? " " : "")}{value.ToString("F2")}  ";
                }

                if (i >= NumberOfUnknowns)
                {
                    result += Environment.NewLine;
                    continue;
                }

                var bValue = B[i];
                result += $"    {(bValue > 0 ? " " : "")}{bValue.ToString("F2")}  ";
                result += Environment.NewLine;
            }

            return result;
        }

        /// <summary>
        /// TEMP - for debug only. 
        /// Creates a debug string showing the matrices in the form A - x - B
        /// </summary>
        public string PrintMatrices(double[] x)
        {
            var result = "";

            int rowLength = A.GetLength(0);
            int colLength = A.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    var value = A[i, j];
                    result += $"{(value > 0 ? " " : "")}{value.ToString("F2")}  ";
                }

                if (i < NumberOfUnknowns)
                {
                    var xValue = x[i];
                    result += $"    {(xValue > 0 ? " " : "")}{xValue.ToString("F2")}  ";
                }
                else
                    result += "    _____  ";

                var bValue = B[i];
                result += $"    {(bValue > 0 ? " " : "")}{bValue.ToString("F2")}  ";
                result += Environment.NewLine;
            }

            return result;
        }
    }
}
