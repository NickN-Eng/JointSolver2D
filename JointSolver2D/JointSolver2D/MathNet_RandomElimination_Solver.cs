using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JointSolver2D
{
    /// <summary>
    /// Solves the system of equations Ax=B by first reducing matrix A to a square matrix (even if there may be more equations than unknowns).
    /// This is currently done by random selection (and doesnt quite work).
    /// TODO: Debug, Eliminating redundant equations (e.g. equations with zero on both sides 0 = 0)
    /// WIP and important!!! Getting a square matrix which can be inverted is important for finding node displacements for the virtual work method.
    /// </summary>
    public class MathNet_RandomElimination_Solver : SolverBase
    {
        protected List<Tuple<int, double>>[] MatrixAConstants_ListedByRow;
        protected double[] VectorBValues;

        public int numberOfTries;

        protected override void SetupMatrices()
        {
            MatrixAConstants_ListedByRow = Enumerable.Repeat(new List<Tuple<int, double>>(), NumberOfEquations).ToArray();
            //EquationConstants = new List<(int, double)>[NumberOfEquations];
            VectorBValues = new double[NumberOfEquations];
        }

        protected override void AddToMatrixA(int rowIndex, int columnIndex, double value)
        {
            MatrixAConstants_ListedByRow[rowIndex].Add(new Tuple<int, double>(columnIndex, value));
        }

        protected override void AddToVectorB(int rowIndex, double value)
        {
            VectorBValues[rowIndex] = value;
        }


        protected override double[] SolveForMatrixX()
        {
            int numberToElimiate = NumberOfEquations - NumberOfUnknowns;

            //TODO: Identify redundant equations (e.g. zero constants, etc...)
            var exclusions = PickFromLinearRangeWithoutReplacement(NumberOfEquations, numberToElimiate);
            var A = Matrix<double>.Build.Sparse(SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(NumberOfUnknowns, NumberOfUnknowns, GetMatrixAValues_WithRowExclusions(exclusions)));

            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfEnumerable(GetVectorBValues_WithRowExclusions(exclusions));
            var x = A.Solve(b);

            //Debug string
            var str = A.ToMatrixString("F1");
            var x_Str = x.ToVectorString("F1");
            var b_Str = b.ToVectorString("F1");


            //var inv = A.Inverse();
            //var invStr = inv.ToMatrixString("F1");
            //var xQR = A.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin).Solve(b);
            //var xQR_Str = x.ToVectorString("F1");
            //var xQRf = A.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full).Solve(b);
            //var xQRf_Str = x.ToVectorString("F1");

            return x.ToArray();
        }

        public IEnumerable<Tuple<int, int, double>> GetMatrixAValues_WithRowExclusions(int[] sortedExcludedRows)
        {
            int iExcludedRow = 0;
            int excludedRowNumber = sortedExcludedRows != null && sortedExcludedRows.Length > 0 ? sortedExcludedRows[0] : -1;

            int iRowNumber = 0;
            for (int i = 0; i < MatrixAConstants_ListedByRow.Length; i++)
            {
                if (i == excludedRowNumber)
                {
                    iExcludedRow++;
                    excludedRowNumber = iExcludedRow < sortedExcludedRows.Length ? sortedExcludedRows[iExcludedRow] : -1;
                    continue;
                }

                foreach (var tuple in MatrixAConstants_ListedByRow[i])
                {
                    yield return new Tuple<int, int, double>(iRowNumber, tuple.Item1, tuple.Item2);
                }

                iRowNumber++;
            }
        }

        public IEnumerable<double> GetVectorBValues_WithRowExclusions(int[] sortedExcludedRows)
        {
            int iExcludedRow = 0;
            int excludedRowNumber = sortedExcludedRows != null && sortedExcludedRows.Length > 0 ? sortedExcludedRows[0] : -1;

            for (int i = 0; i < VectorBValues.Length; i++)
            {
                if (i == excludedRowNumber)
                {
                    iExcludedRow++;
                    excludedRowNumber = iExcludedRow < sortedExcludedRows.Length ? sortedExcludedRows[iExcludedRow] : -1;
                    continue;
                }

                yield return VectorBValues[i];
            }
        }

        public int[] PickFromLinearRangeWithoutReplacement(int rangeLength, int numberToPick)
        {
            var rand = new Random();
            List<int> possible = Enumerable.Range(0, rangeLength).ToList();
            int[] listNumbers = new int[numberToPick];
            for (int i = 0; i < numberToPick; i++)
            {
                int index = rand.Next(0, possible.Count);
                listNumbers[i] = possible[index];
                possible.RemoveAt(index);
            }
            return listNumbers.OrderBy(i => i).ToArray();
        }
    }

}
