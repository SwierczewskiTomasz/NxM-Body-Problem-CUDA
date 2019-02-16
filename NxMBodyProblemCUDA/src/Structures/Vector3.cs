using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;

namespace NxMBodyProblemCUDA.Structures
{
    public struct Vector3
    {
        public double x;
        public double z;
        public double y;

        public Vector3(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public static Vector3 operator - (Vector3 vector)
        {
            return new Vector3(-vector.x, -vector.y, -vector.z);
        }

        public static Vector3 operator + (Vector3 vector1, Vector3 vector2)
        {
            double x = vector1.x + vector2.x;
            double y = vector1.y + vector2.y;
            double z = vector1.z + vector2.z;
            return new Vector3(x, y, z);
        }

        public static Vector3 operator - (Vector3 vector1, Vector3 vector2)
        {
            return vector1 + (-vector2);
        }

        public static Vector3 operator * (Vector3 vector, double a)
        {
            return new Vector3(vector.x * a, vector.y * a, vector.z * a);
        }

        public static Vector3 operator / (Vector3 vector, double a)
        {
            if(a!=0)
                return vector * (1 / a);
            return vector;
        }

        public Vector3 LookAt(Vector3 vector)
        {
            return vector - this;
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public Vector3 Versor()
        {
            return this / Length();
        }

        public float4 ToFloat4()
        {
            return new float4((float)x, (float)y, (float)z, 0);
        }

        public float4 ToFloat4(float mass)
        {
            return new float4((float)x, (float)y, (float)z, mass);
        }

        public float4 ToFloat4(double mass)
        {
            return new float4((float)x, (float)y, (float)z, (float)mass);
        }
    }
}
