using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_Kurs
{
    public class Matrix
    {
        private readonly double[,] _matrix = new double[4, 4];
        private Matrix()
        {}
        public double this[int i, int j]
        {
            get => _matrix[i, j];
            private set => _matrix[i, j] = value;
        }

        // умножение матриц
        public static Matrix operator *(Matrix first, Matrix second) 
        {
            var resultMatrix = new Matrix();
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        resultMatrix[i, j] += first[i, k] * second[k, j];
                    }
                }
            }
            return resultMatrix;
        }

        //матрица поворота
        public static Matrix YSpinMatrix(double rotationAngleInDegrees)
        {
            var rotationAngleInRadians = rotationAngleInDegrees * 0.0174;
            return new Matrix
            {
                [0, 0] = Math.Cos(rotationAngleInRadians),
                [0, 2] = Math.Sin(rotationAngleInRadians),
                [2, 0] = -Math.Sin(rotationAngleInRadians),
                [2, 2] = Math.Cos(rotationAngleInRadians),
                [1, 1] = 1,
                [3, 3] = 1
            };
        }

        //матрица ортогонального проекцирования
        public static Matrix MatrixProjection(double l, double r, double b, double t,
        double n, double f)
        {
            return new Matrix
            {
                [0, 0] = 2 / (r - l),
                [1, 1] = 2 / (t - b),
                [2, 2] = -2 / (f - n),
                [3, 3] = 1,
                [0, 3] = -(r + l) / (r - l),
                [1, 3] = -(t + b) / (t - b),
                [2, 3] = -(f + n) / (f - n)
            };
        }

        //матрица экранной системы координат
        public static Matrix ViewportMatrix(double x, double y, double w, double h, double depth)
        {
            return new Matrix
            {
                [0, 0] = w / 2.0,
                [1, 1] = -h / 2.0,
                [2, 2] = depth / 2.0,
                [3, 3] = 1,
                [0, 3] = x + w / 2.0,
                [1, 3] = y + h / 2.0,
                [2, 3] = depth / 2
            };
        }

        //матрица LookAt
        public static Matrix MakeLookAtMatrix(Vector rightV, Vector topV, Vector
        backV, Vector cameraPos)
        {
            return new Matrix
            {
                [0, 0] = rightV.X,
                [0, 1] = rightV.Y,
                [0, 2] = rightV.Z,
                [1, 0] = topV.X,
                [1, 1] = topV.Y,
                [1, 2] = topV.Z,
                [2, 0] = backV.X,
                [2, 1] = backV.Y,
                [2, 2] = backV.Z,
                [0, 3] = -cameraPos.X,
                [1, 3] = -cameraPos.Y,
                [2, 3] = -cameraPos.Z,
                [3, 3] = 1
            };
        }

        // матрица для пропорций объектов
        public static Matrix CreateScaleMatrix(double kx, double ky, double kz) 
        {
            return new Matrix
            {
                [0, 0] = kx,
                [1, 1] = ky,
                [2, 2] = kz,
                [3, 3] = 1
            };
        }
    }
}