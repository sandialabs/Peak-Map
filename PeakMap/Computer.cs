
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PeakMap
{
    
    public static class Matrix
    {
        /// <summary>
        /// Compute the inverse of a square matrix
        /// </summary>
        /// <param name="A">Input matrix</param>
        /// <returns>inverse of A</returns>
        public static double[,] Inverse(double[,] A) 
        {
            int n = A.GetLength(0);
            if (n != A.GetLength(1))
                throw new ArgumentException("Matrix must be square");
            //create a containter
            double[,] invA = new double[n, n]; 
            //decompose the matrix
            LUDecompositionCrout(A, out double[,] L, out double[,] U);
            for (int i = 0; i < n; i++) 
            {
                //get the identity column
                double[] Ii = new double[n];
                Ii[i] = 1;
                //do the foward substitution with the identiy column
                double[] y = FowardSubstitution(L, Ii);
                //do the backward substitution with the result from the Lower
                BackwardSubstitution(U, y, invA, i);
            }
            return invA;
        }
        public static double[] Tridiagonal(double[,] A, double[] b)
        {
            int n = A.GetLength(0);
            if(n!= b.Length)
                throw new ArgumentException("The number of columns in A must be equal to the number of rows in b");
            if(n!=A.GetLength(1))
                throw new ArgumentException("Matrix must be square");
            //get the diagonal (d), above diagonal (ad), and below diagonal (bd_ elements
            double[] d = new double[n];
            double[] ad = new double[n - 1];
            double[] bd = new double[n ];
            ad[0] = A[0, 1]/A[0,0];
            d[0] = A[0, 0];
            for (int i = 1; i < n-1; i++) 
            {
                ad[i] = A[i, i + 1];
                d[i] = A[i, i];
                bd[i] = A[i, i - 1];
            }
            d[n - 1] = A[n - 1, n - 1];
            bd[n - 1] = A[n - 1, n - 2];
            b[0] /=  d[0];
            //compute abi and bi
            for (int i = 1; i < n - 1; i++) 
            {
                ad[i] /= (d[i] - bd[i] * ad[i - 1]);
                b[i] = (b[i] - bd[i] * b[i - 1]) / (d[i] - bd[i] * ad[i - 1]);
            }
            //compute the last element of b
            b[n - 1] = (b[n - 1] - bd[n - 1] * b[n - 2]) / (d[n - 1] - bd[n - 1] * ad[n - 2]);
            //perform backward subsitution
            double[] x = b;
            for (int i = n - 2; i >= 0; i--)
            {
                x[i] = b[i] - ad[i] * x[i + 1];
            }
            return x;
        }

        /// <summary>
        /// Compute the dot produts between a matrix and vector
        /// </summary>
        /// <param name="A">matrix</param>
        /// <param name="b">vector</param>
        /// <param name="transposeA">Transpose A</param>
        /// <returns></returns>
        public static double[] Dot(double[,] A, double[] b, bool transposeA = false)
        {

            int m = transposeA ? A.GetLength(0) : A.GetLength(1);
            int n = transposeA ? A.GetLength(1) : A.GetLength(0);

            if (m != b.Length && n != b.Length)
                throw new ArgumentException("The number of columns in A must be equal to the number of rows in b");

            double[] C = new double[n];
            //A is not transposed
            if (!transposeA)
            {
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < m; j++)
                    {
                        sum += A[i, j] * b[j];
                    }
                    C[i] = sum;
                }
            }
            //A is transposed
            else if (transposeA)
            {
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < m; j++)
                    {
                        sum += A[j, i] * b[j];
                    }
                    C[i] = sum;
                }
            }
            return C;
        }

        /// <summary>
        /// Compute the dot produts between a matrix and vector
        /// </summary>
        /// <param name="A">matrix</param>
        /// <param name="b">vector</param>
        /// <param name="transposeA">Transpose A</param>
        /// <returns></returns>
        public static double[] Dot(double[] a, double[,] B, bool transposeB = false)
        {

            int m = transposeB ? B.GetLength(1) : B.GetLength(0);
            int p = transposeB ? B.GetLength(0) : B.GetLength(1);

            if (m != a.Length && m != 1)
                throw new ArgumentException("The number of columns in A must be equal to the number of rows in B");

            double[] C = new double[p];
            //A is not transposed
            if (!transposeB)
            {
                for (int j = 0; j < p;j++)
                {
                    double sum = 0;
                    for (int k = 0;k < m; k++)
                    {
                        sum += a[k] * B[k,j];
                    }
                    C[j] = sum;
                }
            }
            //A is transposed
            else if (transposeB)
            {
                for (int j = 0; j < p; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < m; k++)
                    {
                        sum += a[k] * B[j, k];
                    }
                    C[j] = sum;
                }
            }
            return C;
        }
        /// <summary>
        /// Compute the dot product of two matricies with inplace transpose
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <param name="transposeA">Transpose A</param>
        /// <param name="transposeB">Transpose B</param>
        /// <returns>The resut of the dot product</returns>
        public static double[,] Dot(double[,] A, double[,] B, bool transposeA = false, bool transposeB =false) 
        {
            int m = transposeA ? A.GetLength(0) : A.GetLength(1);
            int dim = transposeB ? 1 : 0;
            if (m != B.GetLength(dim))
                throw new ArgumentException("The number of columns in A must be equal to the number of rows in B");

            int n = transposeA ? A.GetLength(1) : A.GetLength(0);
            int p = transposeB ? B.GetLength(0) : B.GetLength(1);

            double[,] C = new double[n, p];
            //neither is transposed
            if (!transposeA && !transposeB)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < m; k++)
                        {
                            sum += A[i, k] * B[k, j];
                        }
                        C[i, j] = sum;
                    }
                }
            }
            //A is transposed
            else if (transposeA && !transposeB)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < m; k++)
                        {
                            sum += A[k, i] * B[k, j];
                        }
                        C[i, j] = sum;
                    }
                }
            }
            //B is transposed
            else if (!transposeA && transposeB)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < m; k++)
                        {
                            sum += A[i, k] * B[j, k];
                        }
                        C[i, j] = sum;
                    }
                }
            }
            //Both A and B are transposed
            else
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < m; k++)
                        {
                            sum += A[k, i] * B[j, k];
                        }
                        C[i, j] = sum;
                    }
                }
            }
            return C;
        }
        /// <summary>
        /// compute the Lower (L) and Upper (U) decompostions of A
        /// </summary>
        /// <param name="A">matrix to decompose</param>
        /// <param name="L">Lower diagonal matrix</param>
        /// <param name="U">Upper diagonal matrix</param>
        public static void LUDecompositionCrout(double[,] A, out double[,] L, out double[,] U ) 
        {
            int n = A.GetLength(0);
            int m = A.GetLength(1);
            L = new double[m, n];
            U = new double[m, n];
            //compute the first colum of L and the diagonal of U
            for (int i = 0; i < n; i++) 
            {
                L[i, 0] = A[i, 0];
                U[i, i] = 1;
            }
            //Compute first row of U (except U11)
            for (int i = 1; i < n; i++)
            {
                U[0, i] = A[0, i] / L[0, 0];
            }
            //compute the rest of the rows
            for(int i = 1; i < n;i++)
            {
                //comput the lower
                for(int j = 1; j <=i; j++)
                {
                    double sum = 0;
                    for(int k = 0; k < j; k++)
                    {
                        sum += L[i, k] * U[k, j];
                    }
                    L[i, j] = A[i,j] - sum;
                }
                //compute the upper
                for (int j = i +1; j < n; j++)
                {
                    double sum = 0;
                    for (int k = 0; k <= i - 1; k++)
                    {
                        sum += L[i, k] * U[k, j];
                    }
                    U[i, j] = (A[i,j] - sum) / L[i,i];
                }
            }
        }
        /// <summary>
        /// Perfom foward Subistution on a matrix and vector
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">Solution vector</param>
        /// <returns>Result vector</returns>
        public static double[] FowardSubstitution(double[,] A, double[] b) 
        {
            
            int n = b.Length;
            if (n != A.GetLength(0))
                throw new ArgumentException("The number of Rows in A must match the number of elements in B");

            double[] y = new double[n];
            y[0] = b[0] / A[0, 0];
            for (int i = 1; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < i ; j++)
                {
                    sum -= A[i, j] *y[j];
                }
                y[i] = (b[i] + sum) / A[i, i];
            }
            return y;
        }
        /// <summary>
        /// Perform Backward Substituion on a Matrix and vector
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">solution vector</param>
        /// <returns>Result vector</returns>
        public static double[] BackwardSubstitution(double[,] A, double[] b) 
        {
            int n = b.Length;

            if (n != A.GetLength(0))
                throw new ArgumentException("The number of Rows in A must match the number of elements in B");

            double[] y = new double[n];
            y[n - 1] = b[n - 1] / A[n - 1, n - 1];

            for (int i = n - 2; i >= 0; i--)
            {
                double sum = 0;
                for (int j = n-1; j >= 0; j--) 
                {
                    sum -= A[i,j]*y[j];
                }
                y[i] = (b[i] + sum) / A[i, i];
            }
            return y;
        }
        /// <summary>
        /// Perfom backward substituion on a matric and vector and place it in an existing array in the specified column
        /// </summary>
        /// <param name="A">Coefficienct matrix</param>
        /// <param name="b">solution vector</param>
        /// <param name="C">Container for results</param>
        /// <param name="column">Destination column index</param>
        public static void BackwardSubstitution(double[,] A, double[] b, double[,] C, int column )
        {
            int n = b.Length;
            if (n != A.GetLength(0))
                throw new ArgumentException("The number of Rows in A must match the number of elements in B");
            if (C == null)
                throw new ArgumentException("The Matrix C cannot be null");
            if(n!= C.GetLength(0))
                throw new ArgumentException("The number of Rows in C must match the number of elements in B");

            C[n - 1, column] = b[n - 1] / A[n - 1, n - 1];

            for (int i = n - 2; i >= 0; i--)
            {
                double sum = 0;
                for (int j = n-1; j >= 0; j--)
                {
                    sum -= A[i, j] * C[j,column];
                }
                C[i,column] = (b[i] + sum) / A[i, i];
            }
        }


        public static double[] GetColumn(double[,] A, int index) 
        {
            return Enumerable.Range(0, A.GetLength(0)).Select(r => A[r, index]).ToArray();
        }
        public static double[] GetRow(double[,] A, int index)
        {
            return Enumerable.Range(0, A.GetLength(0)).Select(c => A[index, c]).ToArray();
        }

    }
}
