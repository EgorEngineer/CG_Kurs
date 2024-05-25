using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_Kurs
{
    public class Vector
    {
        private readonly double[] _vector = { 0, 0, 0, 1 };
        public double X
        {
            get => _vector[0];
            private set => _vector[0] = value;
        }
        public double Y
        {
            get => _vector[1];
            private set => _vector[1] = value;
        }
        public double Z
        {
            get => _vector[2];
            private set => _vector[2] = value;
        }
        private double Length { get; }
        private Vector()
        {
        }
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Length = Math.Sqrt(x * x + y * y + z * z);
        }

        private double this[int i]
        {
            get => _vector[i];
            set => _vector[i] = value;
        }
        public static Vector operator -(Vector firstVector, Vector secondVector) // разность векторов
        {
            return new Vector
            (
            firstVector.X - secondVector.X,
            firstVector.Y - secondVector.Y,
            firstVector.Z - secondVector.Z
            );
        }

        
        public static Vector operator *(double num, Vector vector)
        {
            return new Vector(vector.X * num, vector.Y * num, vector.Z * num);
        }
        
        public static Vector operator +(Vector first, Vector second)
        {
            return new Vector(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        public static Vector operator /(Vector vector, double num)
        {
            return new Vector(vector.X / num, vector.Y / num, vector.Z / num);
        }

        public static double ScalarMultiplication(Vector firstVector, Vector secondVector)
        {
            return firstVector.X * secondVector.X + firstVector.Y * secondVector.Y + firstVector.Z * secondVector.Z;
        }

        public static Vector VectorMultiplication(Vector firstVector, Vector secondVector)
        {
            return new Vector
            (
            firstVector.Y * secondVector.Z - firstVector.Z * secondVector.Y,
            firstVector.Z * secondVector.X - firstVector.X * secondVector.Z,
            firstVector.X * secondVector.Y - firstVector.Y * secondVector.X
            );
        }

        public static Vector operator *(Matrix matrix, Vector vector)
        {
            var resultVector = new Vector();
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    resultVector[i] += matrix[i, j] * vector[j];
                }
            }
            return resultVector;
        }

        // получение нормализованного вектора
        public Vector NormalizeVector()
        {
            return new Vector
            (
                X / Length,
                Y / Length,
                Z / Length
            );
        }

    }
}