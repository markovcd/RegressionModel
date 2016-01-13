using System;

namespace Markovcd.Classes
{
    public abstract class MatrixBase
    {
        /*http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/07/Algorithms-In-C-LUP-Decomposition.aspx
        * Perform LUP decomposition on a matrix A.
        * Return L and U as a single matrix(double[][]) and P as an array of ints.
        * We implement the code to compute LU "in place" in the matrix A.
        * In order to make some of the calculations more straight forward and to 
        * match Cormen's et al. pseudocode the matrix A should have its first row and first columns
        * to be all 0.
        * */
        public static Tuple<double[,], int[]> LupDecomposition(double[,] m)
        {
            var n = m.GetLength(0);
            var a = new double[n, n];
            System.Array.Copy(m, 0, a, 0, m.Length);

            /*
            * pi represents the permutation matrix.  We implement it as an array
            * whose value indicates which column the 1 would appear.  We use it to avoid 
            * dividing by zero or small numbers.
            * */
            var pi = new int[n];

            //Initialize the permutation matrix, will be the identity matrix
            for (var j = 0; j < n; j++)
                pi[j] = j;

            var kp = 0;

            for (var k = 0; k < n; k++)
            {
                /*
                * In finding the permutation matrix p that avoids dividing by zero
                * we take a slightly different approach.  For numerical stability
                * We find the element with the largest 
                * absolute value of those in the current first column (column k).  If all elements in
                * the current first column are zero then the matrix is singluar and throw an
                * error.
                * */
                var p = 0d;

                for (var i = k; i < n; i++)
                {
                    if (!(Math.Abs(a[i, k]) > p)) continue;

                    p = Math.Abs(a[i, k]);
                    kp = i;
                }

                if (p == 0) throw new InvalidOperationException("singular matrix");

                /*
                * These lines update the pivot array (which represents the pivot matrix)
                * by exchanging pi[k] and pi[kp].
                * */
                var pik = pi[k];
                var pikp = pi[kp];
                pi[k] = pikp;
                pi[kp] = pik;

                /*
                * Exchange rows k and kpi as determined by the pivot
                * */
                for (var i = 0; i < n; i++)
                {
                    var aki = a[k, i];
                    var akpi = a[kp, i];
                    a[k, i] = akpi;
                    a[kp, i] = aki;
                }

                /*
                * Compute the Schur complement
                * */
                for (var i = k + 1; i < n; i++)
                {
                    a[i, k] = a[i, k] / a[k, k];

                    for (var j = k + 1; j < n; j++)
                        a[i, j] = a[i, j] - (a[i, k] * a[k, j]);
                }
            }

            return Tuple.Create(a, pi);
        }

        /*http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/14/Algorithms-In-C-Solving-A-System-Of-Linear-Equations.aspx
        * Given L,U,P and b solve for x.
        * Input the L and U matrices as a single matrix LU.
        * Return the solution as a double[].
        * LU will be a n+1xm+1 matrix where the first row and columns are zero.
        * This is for ease of computation and consistency with Cormen et al.
        * pseudocode.
        * The pi array represents the permutation matrix.
        * */
        public static double[] LupSolve(double[,] lu, int[] pi, double[] b)
        {
            var n = lu.GetLength(0);
            var x = new double[n];
            var y = new double[n];

            /*
            * Solve for y using formward substitution
            * */
            for (var i = 0; i < n; i++)
            {
                var suml = 0d;

                for (var j = 0; j < i; j++)
                {
                    /*
                    * Since we've taken L and U as a singular matrix as an input
                    * the value for L at index i and j will be 1 when i equals j, not LU[i][j], since
                    * the diagonal values are all 1 for L.
                    * */
                    var lij = i == j ? 1 : lu[i, j];
                    suml = suml + (lij * y[j]);
                }

                y[i] = b[pi[i]] - suml;
            }

            //Solve for x by using back substitution
            for (var i = n - 1; i >= 0; i--)
            {
                var sumu = 0d;

                for (var j = i + 1; j < n; j++)
                    sumu = sumu + lu[i, j] * x[j];

                x[i] = (y[i] - sumu) / lu[i, i];
            }

            return x;
        }

        /* http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/21/Algorithms-In-C-Finding-The-Inverse-Of-A-Matrix.aspx
        * Given an nXn matrix A, solve n linear equations to find the inverse of A.
        * */
        public static double[,] Invert(double[,] a)
        {
            var n = a.GetLength(0);

            // x will hold the inverse matrix to be returned
            var x = new double[n, n];

            // Get the LU matrix and P matrix (as an array)
            var results = LupDecomposition(a);

            var lu = results.Item1;
            var p = results.Item2;

            // Solve AX = e for each column ei of the identity matrix using LUP decomposition
            for (var i = 0; i < n; i++)
            {
                // e will represent each column in the identity matrix
                var e = new double[n];
                e[i] = 1;

                /*
                * solve will contain the vector solution for the LUP decomposition as we solve
                * for each vector of x. We will combine the solutions into the double[,] array x.
                * */
                var solve = LupSolve(lu, p, e);

                for (var j = 0; j < solve.Length; j++)
                    x[j, i] = solve[j];
            }

            return x;
        }
    }
}
