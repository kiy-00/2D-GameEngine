using System;

namespace GameEngine.Common
{
    /// <summary>
    /// 提供常用的数学辅助函数
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// 表示 π 的值
        /// </summary>
        public const float Pi = (float)Math.PI;

        /// <summary>
        /// 表示 2π 的值
        /// </summary>
        public const float TwoPi = (float)(Math.PI * 2);

        /// <summary>
        /// 表示 π/2 的值
        /// </summary>
        public const float PiOver2 = (float)(Math.PI / 2);

        /// <summary>
        /// 表示 π/4 的值
        /// </summary>
        public const float PiOver4 = (float)(Math.PI / 4);

        /// <summary>
        /// 弧度到角度的转换常数
        /// </summary>
        public const float RadToDeg = 180f / (float)Math.PI;

        /// <summary>
        /// 角度到弧度的转换常数
        /// </summary>
        public const float DegToRad = (float)Math.PI / 180f;

        /// <summary>
        /// 表示接近零的极小值
        /// </summary>
        public const float Epsilon = 0.000001f;

        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        /// <param name="value">要限制的值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>限制后的值</returns>
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        /// <param name="value">要限制的值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>限制后的值</returns>
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 在两个值之间进行线性插值
        /// </summary>
        /// <param name="a">起始值</param>
        /// <param name="b">结束值</param>
        /// <param name="t">插值参数 (0.0 - 1.0)</param>
        /// <returns>插值结果</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp(t, 0f, 1f);
        }

        /// <summary>
        /// 线性插值，不限制t值范围
        /// </summary>
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// 将角度从弧度转换为角度
        /// </summary>
        /// <param name="radians">弧度制角度</param>
        /// <returns>角度制角度</returns>
        public static float ToDegrees(float radians)
        {
            return radians * RadToDeg;
        }

        /// <summary>
        /// 将角度从角度转换为弧度
        /// </summary>
        /// <param name="degrees">角度制角度</param>
        /// <returns>弧度制角度</returns>
        public static float ToRadians(float degrees)
        {
            return degrees * DegToRad;
        }

        /// <summary>
        /// 计算两个浮点数是否近似相等
        /// </summary>
        /// <param name="a">第一个值</param>
        /// <param name="b">第二个值</param>
        /// <param name="epsilon">允许的误差</param>
        /// <returns>是否近似相等</returns>
        public static bool ApproximatelyEqual(float a, float b, float epsilon = Epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        /// <summary>
        /// 将值限制在0到1之间
        /// </summary>
        public static float Saturate(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// 百分比插值，使用平滑的Hermite曲线
        /// </summary>
        public static float SmoothStep(float a, float b, float t)
        {
            t = Saturate(t);
            t = t * t * (3f - 2f * t);
            return Lerp(a, b, t);
        }

        /// <summary>
        /// 将角度归一化到 [-π, π] 区间
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            angle = (float)Math.IEEERemainder(angle, TwoPi);
            if (angle < -Pi)
                angle += TwoPi;
            else if (angle >= Pi)
                angle -= TwoPi;

            return angle;
        }

        /// <summary>
        /// 返回角度的正弦值和余弦值
        /// </summary>
        public static (float Sin, float Cos) SinCos(float angle)
        {
            return ((float)Math.Sin(angle), (float)Math.Cos(angle));
        }

        /// <summary>
        /// 计算平方
        /// </summary>
        public static float Square(float value)
        {
            return value * value;
        }

        /// <summary>
        /// 计算立方
        /// </summary>
        public static float Cube(float value)
        {
            return value * value * value;
        }

        public static float Min(double a, float b)
        {
            return (float)Math.Min(a, b);
        }
    }
}