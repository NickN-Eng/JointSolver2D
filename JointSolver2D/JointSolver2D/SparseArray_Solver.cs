using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JointSolver2D
{
    public class SparseArray_Solver : SolverBase
    {
        protected List<Tuple<int, int, double>> MatrixAConstants;
        protected double[] VectorBValues;

        protected Matrix<double> SolutionOfA;

        public override double[,] A_Array 
        {
            get
            {
                var A = new double[NumberOfEquations, NumberOfUnknowns];
                foreach (var item in MatrixAConstants)
                {
                    A[item.Item1, item.Item2] = item.Item3;
                }
                return A;
            }
        }

        public override double[] B_Array => VectorBValues;

        protected override void SetupMatrices()
        {

            MatrixAConstants = new List<Tuple<int, int, double>>();
            VectorBValues = new double[NumberOfEquations];
        }

        protected override void AddToMatrixA(int rowIndex, int columnIndex, double value)
        {
            MatrixAConstants.Add(new Tuple<int, int, double>(rowIndex, columnIndex, value));
        }

        protected override void AddToVectorB(int rowIndex, double value)
        {
            VectorBValues[rowIndex] = value;
        }


        protected override double[] SolveForMatrixX()
        {
            int numberToElimiate = NumberOfEquations - NumberOfUnknowns;

            var A = Matrix<double>.Build.Sparse(SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(NumberOfEquations, NumberOfUnknowns, MatrixAConstants));
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfEnumerable(VectorBValues);

            //Find the least squares solution
            // x  =  (A^T A)^-1 A^T b 

            var At = A.Transpose();
            var AtA = At.Multiply(A);
            var AtA_inv = AtA.Inverse();
            SolutionOfA = AtA_inv.Multiply(At);

            var result = SolutionOfA.Multiply(b);
            return result.ToArray();
        }

        protected override double[] SolveXForGivenB(double[] vectorB)
        {
            var b = Vector<double>.Build.Dense(vectorB);
            return SolutionOfA.Multiply(b).ToArray();
        }
    }

}
