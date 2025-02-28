using System;

namespace GameEngine.Common
{
    /// <summary>
    /// 表示2x2矩阵的结构
    /// </summary>
    public struct Matrix2 : IEquatable<Matrix2>
    {
        #region 字段

        // 矩阵按行优先存储
        // M11 M12
        // M21 M22
        public float M11, M12;
        public float M21, M22;

        #endregion

        #region 属性

        /// <summary>
        /// 获取矩阵的行列式
        /// </summary>
        public float Determinant
        {
            get { return M11 * M22 - M12 * M21; }
        }

        /// <summary>
        /// 矩阵的转置
        /// </summary>
        public Matrix2 Transposed
        {
            get
            {
                Matrix2 result = this;
                result.Transpose();
                return result;
            }
        }

        /// <summary>
        /// 矩阵的逆
        /// </summary>
        public Matrix2 Inverted
        {
            get
            {
                Matrix2 result = this;
                result.Invert();
                return result;
            }
        }

        #endregion

        #region 静态属性

        /// <summary>
        /// 返回单位矩阵
        /// </summary>
        public static Matrix2 Identity
        {
            get { return new Matrix2(1, 0, 0, 1); }
        }

        /// <summary>
        /// 返回零矩阵
        /// </summary>
        public static Matrix2 Zero
        {
            get { return new Matrix2(0, 0, 0, 0); }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 用指定值创建新的矩阵
        /// </summary>
        public Matrix2(float m11, float m12, float m21, float m22)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
        }

        /// <summary>
        /// 用标量创建对角矩阵
        /// </summary>
        public Matrix2(float scalar)
        {
            M11 = M22 = scalar;
            M12 = M21 = 0;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 创建旋转矩阵
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        public static Matrix2 CreateRotation(float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new Matrix2(
                cos, -sin,
                sin, cos);
        }

        /// <summary>
        /// 创建缩放矩阵
        /// </summary>
        public static Matrix2 CreateScale(float scale)
        {
            return new Matrix2(scale, 0, 0, scale);
        }

        /// <summary>
        /// 创建缩放矩阵
        /// </summary>
        public static Matrix2 CreateScale(float scaleX, float scaleY)
        {
            return new Matrix2(scaleX, 0, 0, scaleY);
        }

        /// <summary>
        /// 创建缩放矩阵
        /// </summary>
        public static Matrix2 CreateScale(Vector2 scale)
        {
            return new Matrix2(scale.X, 0, 0, scale.Y);
        }

        /// <summary>
        /// 将矩阵转置
        /// </summary>
        public void Transpose()
        {
            float temp = M12;
            M12 = M21;
            M21 = temp;
        }

        /// <summary>
        /// 求矩阵的逆
        /// </summary>
        public void Invert()
        {
            float det = Determinant;
            if (Math.Abs(det) < float.Epsilon)
            {
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
            }

            float invDet = 1.0f / det;

            float tempM11 = M22 * invDet;
            float tempM12 = -M12 * invDet;
            float tempM21 = -M21 * invDet;
            float tempM22 = M11 * invDet;

            M11 = tempM11;
            M12 = tempM12;
            M21 = tempM21;
            M22 = tempM22;
        }

        /// <summary>
        /// 将向量通过矩阵变换
        /// </summary>
        public Vector2 Transform(Vector2 vector)
        {
            return new Vector2(
                M11 * vector.X + M12 * vector.Y,
                M21 * vector.X + M22 * vector.Y);
        }

        #endregion

        #region 运算符重载

        public static Matrix2 operator +(Matrix2 left, Matrix2 right)
        {
            return new Matrix2(
                left.M11 + right.M11, left.M12 + right.M12,
                left.M21 + right.M21, left.M22 + right.M22);
        }

        public static Matrix2 operator -(Matrix2 left, Matrix2 right)
        {
            return new Matrix2(
                left.M11 - right.M11, left.M12 - right.M12,
                left.M21 - right.M21, left.M22 - right.M22);
        }

        public static Matrix2 operator *(Matrix2 left, Matrix2 right)
        {
            return new Matrix2(
                left.M11 * right.M11 + left.M12 * right.M21,
                left.M11 * right.M12 + left.M12 * right.M22,
                left.M21 * right.M11 + left.M22 * right.M21,
                left.M21 * right.M12 + left.M22 * right.M22);
        }

        public static Matrix2 operator *(Matrix2 matrix, float scalar)
        {
            return new Matrix2(
                matrix.M11 * scalar, matrix.M12 * scalar,
                matrix.M21 * scalar, matrix.M22 * scalar);
        }

        public static Matrix2 operator *(float scalar, Matrix2 matrix)
        {
            return matrix * scalar;
        }

        public static Vector2 operator *(Matrix2 matrix, Vector2 vector)
        {
            return matrix.Transform(vector);
        }

        public static bool operator ==(Matrix2 left, Matrix2 right)
        {
            return (left.M11 == right.M11) && (left.M12 == right.M12) &&
                   (left.M21 == right.M21) && (left.M22 == right.M22);
        }

        public static bool operator !=(Matrix2 left, Matrix2 right)
        {
            return !(left == right);
        }

        #endregion

        #region 重写

        public override bool Equals(object obj)
        {
            return (obj is Matrix2) && Equals((Matrix2)obj);
        }

        public bool Equals(Matrix2 other)
        {
            return (M11 == other.M11) && (M12 == other.M12) &&
                   (M21 == other.M21) && (M22 == other.M22);
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode() ^ M12.GetHashCode() ^
                   M21.GetHashCode() ^ M22.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{M11}, {M12}]\n[{M21}, {M22}]";
        }

        #endregion
    }
}