using System.Collections.Generic;

namespace CG_Kurs
{
    public class Vertex
    {
        public Vector Value;
        public Vector Normal;

        public Vertex(Vector value, Vector normal)
        {
            Value = value;
            Normal = normal;
        }
    }
}