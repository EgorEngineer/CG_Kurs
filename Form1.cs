using System.Collections.Generic;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CG_Kurs
{
    public sealed partial class Form1 : Form
    {
        private const string File_name = "14.obj";
        private static int Angle_of_Rotation;
        private double _minX = 3200, _maxX = -3200, _minY = 3200, _maxY = -3200, _minZ = 3200, _maxZ = -3200;
        private readonly List<Vertex[]> _originalPolygons = new List<Vertex[]>();
        private List<Vertex[]> _rotatedPolygons = new List<Vertex[]>();
        private Matrix Matrix;
        private readonly List<List<double>> Z_Buffer = new List<List<double>>();

        public Form1()
        {
            InitializeComponent();
            Width = 1920;
            Height = 1080;
            BackColor = Color.CadetBlue;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            ReadFile();
            FillTransMatrix();
            KeyDown += OnKey;
            Paint += OnPaint;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            TransModel();
            DrawModel(e.Graphics);
        }

        private void InitZBuffer()
        {
            Z_Buffer.Clear();
            for (var i = 0; i < Width; i++)
            {
                var newStr = new List<double>();
                for (var j = 0; j < Height; j++)
                {
                    newStr.Add(double.MinValue);
                }
                Z_Buffer.Add(newStr);
            }
        }

        private void TransModel()
        {
            var rotationMatrix = Matrix.YSpinMatrix(Angle_of_Rotation);
            _rotatedPolygons = _originalPolygons.Select(points => points.Select(p => new Vertex(Matrix * rotationMatrix * p.Value, p.Normal)))
            .Select(p => p.ToArray()).OrderBy(a => a.Average(p => p.Value.Z)).ToList();
        }

        // поворот камеры
        // при повороте высчитывается матрица поворота, с помощью неё все остальные матрицы
        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                Angle_of_Rotation -= 20;
                Refresh();
            }
            if (e.KeyCode == Keys.Left)
            {
                Angle_of_Rotation += 20;
                Refresh();
            }
        }

        //Парсинг
        private void ReadFile()
        {
            var vectors = new List<Vector>();
            var vectorsNorm = new List<Vector>();
            foreach (var line in System.IO.File.ReadAllLines(File_name))
            {
                var words = line.Replace('.', ',').Split(' ');
                if (words[0] == "v")
                {
                    var xv = double.Parse(words[1]);
                    var yv = double.Parse(words[2]);
                    var zv = double.Parse(words[3]);
                    vectors.Add(new Vector(xv, yv, zv));
                    if (xv < _minX) _minX = xv;
                    else if (xv > _maxX) _maxX = xv;
                    if (yv < _minY) _minY = yv;
                    else if (yv > _maxY) _maxY = yv;
                    if (zv < _minZ) _minZ = zv;
                    else if (zv > _maxZ) _maxZ = zv;
                }
                else if (words[0] == "vn")
                {
                    vectorsNorm.Add(new Vector(double.Parse(words[1]), double.Parse(words[2]), double.Parse(words[3])));
                }
                else if (words[0] == "f")
                {
                    var pointIndexes = new List<int>();
                    var polygon = new List<Vertex>();

                    for (var i = 1; i < words.Length; i++)
                    {
                        pointIndexes.Add(int.Parse(words[i].Split('/')[0]) - 1);
                    }

                    foreach (var index in pointIndexes)
                    {
                        var currentNormal = vectorsNorm[int.Parse(words[1].Split('/')[2]) - 1];
                        polygon.Add(new Vertex(vectors[index], currentNormal)); 
                    }

                    // массив координат и вершин
                    _originalPolygons.Add(polygon.ToArray());
                }
            }
        }

        private void FillTransMatrix()
        {
            var viewportMatrix = Matrix.ViewportMatrix(0, -200, Width, Height, _maxZ - _minZ);
            var projectionMatrix = Matrix.MatrixProjection(_minX, _maxX, _minY, _maxY, _maxZ, _minZ);

            //направление камеры
            var cameraPosition = new Vector(0, 0.4, 1); 
            var center = new Vector(0, 0, 0);
            var z = cameraPosition - center;
            var x = Vector.VectorMultiplication(new Vector(0, 1, 0), z);
            var y = Vector.VectorMultiplication(z, x);
            var lookAtMatrix = Matrix.MakeLookAtMatrix(x, y, z, cameraPosition);

            //размер объектов на сцене
            var scaleMatrix = Matrix.CreateScaleMatrix(0.2, 0.6, 0.2);
            // матрица модели
            Matrix = viewportMatrix * projectionMatrix * lookAtMatrix * scaleMatrix; 
        }

        private void DrawModel(Graphics graphics)
        {
            InitZBuffer();
            // перебор и интерполяция всех полигонов
            foreach (var v in _rotatedPolygons)
            {
                PhongMethod(v[0], v[1], v[2], graphics);
            }
        }

        private static Color CalcColor(Vector normal, Vector point)
        {
            const float rf = 0.5f;
            const float gf = 0.9f;
            const float bf = 0.0f;
            var light =   new Vector(0.1, -0.4, 0.9);
            var Reflex = new Vector(0, 0, 0);
            var L = (point - Matrix.YSpinMatrix(Angle_of_Rotation) * light).NormalizeVector();
            if (Vector.ScalarMultiplication(normal, L) >= 0)
            {
                Reflex = (L - 2 * Vector.ScalarMultiplication(normal, L) * normal).NormalizeVector();
            }
            const float ambient = 1.0f;
            var diffuse = (float)Math.Max(Vector.ScalarMultiplication(normal.NormalizeVector(), L), 0.0f) * 0.5f;
            var specular = Math.Pow(Math.Max(0, Vector.ScalarMultiplication(Reflex, Matrix.YSpinMatrix(Angle_of_Rotation) * light)), 32) * 0.05;

            int r = (int)(230 * (ambient * rf + diffuse + specular));
            int g = (int)(100 * (ambient * gf + diffuse + specular));
            int b = (int)(150 * (ambient * bf + diffuse + specular));

            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return Color.FromArgb(r, g, b);
        }

        // реализация метода Фонга
        private void PhongMethod(Vertex v0, Vertex v1, Vertex v2, Graphics graphics)
        {
            var px1 = 0.0;
            var px2 = 0.0;
            var zx1 = 0.0;
            var zx2 = 0.0;
            if (v0.Value.Y < v2.Value.Y) (v0, v2) = (v2, v0);
            if (v0.Value.Y < v1.Value.Y) (v0, v1) = (v1, v0);
            if (v1.Value.Y < v2.Value.Y) (v1, v2) = (v2, v1);

            var nx1 = new Vector(0, 0, 0);
            var nx2 = new Vector(0, 0, 0);
            for (var y = v0.Value.Y; y >= v2.Value.Y; y--)
            {
                //Ymax
                if (y >= v1.Value.Y) 
                {
                    px1 = v0.Value.X + (v2.Value.X - v0.Value.X) * ((v0.Value.Y - y) / (v0.Value.Y - v2.Value.Y));
                    px2 = v0.Value.X + (v1.Value.X - v0.Value.X) * ((v0.Value.Y - y) / (v0.Value.Y - v1.Value.Y));
                    zx1 = v0.Value.Z + (v2.Value.Z - v0.Value.Z) * ((v0.Value.Y - y) / (v0.Value.Y - v2.Value.Y));
                    zx2 = v0.Value.Z + (v1.Value.Z - v0.Value.Z) * ((v0.Value.Y - y) / (v0.Value.Y - v1.Value.Y));
                    nx1 = (v0.Normal + (v0.Value.Y - y) / (v0.Value.Y - v2.Value.Y) * (v2.Normal - v0.Normal));
                    nx2 = (v0.Normal + (v0.Value.Y - y) / (v0.Value.Y - v1.Value.Y) * (v1.Normal - v0.Normal));
                }

                //Ymin
                if (y <= v1.Value.Y)
                {
                    px1 = v2.Value.X - (v2.Value.X - v0.Value.X) * ((v2.Value.Y - y) / (v2.Value.Y - v0.Value.Y));
                    px2 = v2.Value.X - (v2.Value.X - v1.Value.X) * ((v2.Value.Y - y) / (v2.Value.Y - v1.Value.Y));
                    zx1 = v2.Value.Z - (v2.Value.Z - v0.Value.Z) * ((v2.Value.Y - y) / (v2.Value.Y - v0.Value.Y));
                    zx2 = v2.Value.Z - (v2.Value.Z - v1.Value.Z) * ((v2.Value.Y - y) / (v2.Value.Y - v1.Value.Y));
                    nx1 = (v2.Normal - (v2.Value.Y - y) / (v2.Value.Y - v0.Value.Y) * (v2.Normal - v0.Normal));
                    nx2 = (v2.Normal - (v2.Value.Y - y) / (v2.Value.Y - v1.Value.Y) * (v2.Normal - v1.Normal));
                }

                // интерполяция внустри строки по x
                for (var x = Math.Min(px1, px2); x <= Math.Max(px1, px2); x++)
                {
                    //нормаль для x
                    var nx = nx1 + (x - px1) * (1 / (px2 - px1)) * (nx2 - nx1);
                    //z по y затем по x
                    var z = zx1 + (zx2 - zx1) * (x - px1) / (px2 - px1);

                    if (Z_Buffer[(int)x][(int)y] <= z)
                    {
                        Z_Buffer[(int)x][(int)y] = z; 
                        var col = CalcColor(nx.NormalizeVector(), new Vector(x,y,z));
                        graphics.FillRectangle(new SolidBrush(col), (int)x - 1, (int)y - 1, 2, 2);
                    }
                }
            }
        }
    }
}