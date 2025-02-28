using System;

namespace GameEngine.Common
{
    /// <summary>
    /// 表示二维向量的结构
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        #region 字段和属性

        /// <summary>
        /// X分量
        /// </summary>
        public float X;

        /// <summary>
        /// Y分量
        /// </summary>
        public float Y;

        /// <summary>
        /// 向量的长度（模）
        /// </summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y); }
        }

        /// <summary>
        /// 向量长度的平方
        /// </summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// 单位向量
        /// </summary>
        public Vector2 Normalized
        {
            get
            {
                Vector2 result = this;
                result.Normalize();
                return result;
            }
        }

        #endregion

        #region 常量

        /// <summary>
        /// 零向量 (0, 0)
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0);

        /// <summary>
        /// 单位向量 (1, 1)
        /// </summary>
        public static readonly Vector2 One = new Vector2(1, 1);

        /// <summary>
        /// 向上单位向量 (0, 1)
        /// </summary>
        public static readonly Vector2 Up = new Vector2(0, 1);

        /// <summary>
        /// 向下单位向量 (0, -1)
        /// </summary>
        public static readonly Vector2 Down = new Vector2(0, -1);

        /// <summary>
        /// 向左单位向量 (-1, 0)
        /// </summary>
        public static readonly Vector2 Left = new Vector2(-1, 0);

        /// <summary>
        /// 向右单位向量 (1, 0)
        /// </summary>
        public static readonly Vector2 Right = new Vector2(1, 0);

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建一个新的Vector2
        /// </summary>
        /// <param name="x">X分量</param>
        /// <param name="y">Y分量</param>
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 创建一个所有分量相同的Vector2
        /// </summary>
        /// <param name="value">X和Y分量的值</param>
        public Vector2(float value)
        {
            X = Y = value;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 将向量归一化为单位向量
        /// </summary>
        public void Normalize()
        {
            float length = Length;
            if (length > 0)
            {
                float invLength = 1.0f / length;
                X *= invLength;
                Y *= invLength;
            }
        }

        /// <summary>
        /// 计算两个向量的点积
        /// </summary>
        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// 计算两个向量的叉积
        /// </summary>
        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// 线性插值两个向量
        /// </summary>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t);
        }

        /// <summary>
        /// 返回两个向量分量的最小值
        /// </summary>
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y));
        }

        /// <summary>
        /// 返回两个向量分量的最大值
        /// </summary>
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y));
        }

        /// <summary>
        /// 反射向量
        /// </summary>
        /// <param name="vector">入射向量</param>
        /// <param name="normal">法线向量（必须为单位向量）</param>
        /// <returns>反射向量</returns>
        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            float dot = Dot(vector, normal);
            return new Vector2(
                vector.X - 2.0f * dot * normal.X,
                vector.Y - 2.0f * dot * normal.Y);
        }

        #endregion

        #region 运算符重载

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 vector, float scalar)
        {
            return new Vector2(vector.X * scalar, vector.Y * scalar);
        }

        public static Vector2 operator *(float scalar, Vector2 vector)
        {
            return new Vector2(vector.X * scalar, vector.Y * scalar);
        }

        public static Vector2 operator /(Vector2 vector, float scalar)
        {
            float invScalar = 1.0f / scalar;
            return new Vector2(vector.X * invScalar, vector.Y * invScalar);
        }

        public static Vector2 operator -(Vector2 vector)
        {
            return new Vector2(-vector.X, -vector.Y);
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        #endregion

        #region 重写

        public override bool Equals(object obj)
        {
            return (obj is Vector2) && Equals((Vector2)obj);
        }

        public bool Equals(Vector2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        #endregion
    }
}