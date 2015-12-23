using System;
using System.Collections.Generic;
using System.Linq;

namespace Markovcd.Classes
{
    public class Matrix
    {
        public Matrix(double[,] array)
        {
            Array = new double[array.GetLength(0), array.GetLength(1)];
            System.Array.Copy(array, 0, Array, 0, array.Length);
        }

        public Matrix(double[] array)
        {
            Array = new double[array.Length, 1];

            for (var i = 0; i < array.Length; i++)
                this[i] = array[i];
        }

        public Matrix(int height, int width = 1)
        {
            Array = new double[height, width];
        }

        public double[,] Array { get; }

        public int Height => Array.GetLength(0);
        public int Width => Array.GetLength(1);
        public bool IsSquare => Height == Width;

        public double this[int i, int j = 0]
        {
            get { return Array[i, j]; }
            set { Array[i, j] = value; }
        }

        public double[] GetColumn(int column = 0)
        {
            var result = new double[Height];

            for (var i = 0; i < Height; i++)
                result[i] = this[i, column];

            return result;
        }

        public double[] GetRow(int row = 0)
        {
            var result = new double[Width];

            for (var i = 0; i < Width; i++)
                result[i] = this[row, i];

            return result;
        }

        public Matrix Invert() => new Matrix(MatrixHelper.Invert(Array));
        public bool CanMultiply(Matrix other) => Width == other.Height;
        public bool IsSameSize(Matrix other) => Height == other.Height && Width == other.Width;
        
        public static Matrix operator *(Matrix first, Matrix second) => first.Multiply(second);
        public static Matrix operator +(Matrix first, Matrix second) => first.Add(second);
        public static Matrix operator -(Matrix first, Matrix second) => first.Subtract(second);

        public Matrix Multiply(Matrix other)
        {
            if (!CanMultiply(other)) throw new InvalidOperationException();

            var result = new Matrix(Height, other.Width);

            for (var i = 0; i < result.Height; i++)
                for (var j = 0; j < result.Width; j++)
                    for (var k = 0; k < Width; k++)
                        result[i, j] += this[i, k] * other[k, j];

            return result;
        }

        public Matrix Add(Matrix other)
        {
            if (!IsSameSize(other)) throw new InvalidOperationException();

            var result = new Matrix(Height, Width);

            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    result[i, j] = this[i, j] + other[i, j];

            return result;
        }

        public Matrix Subtract(Matrix other)
        {
            if (!IsSameSize(other)) throw new InvalidOperationException();

            var result = new Matrix(Height, Width);

            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    result[i, j] = this[i, j] - other[i, j];

            return result;
        }
    }
}
