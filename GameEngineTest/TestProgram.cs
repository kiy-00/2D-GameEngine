using System;
using System.Threading;
using GameEngine.Common;

namespace GameEngineTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GameEngine.Common 测试程序\n");

            // 测试Vector2
            TestVector2();

            // 测试Matrix2
            TestMatrix2();

            // 测试MathHelper
            TestMathHelper();

            // 测试Logger
            TestLogger();

            // 测试Timer
            TestTimer();

            Console.WriteLine("\n所有测试完成。按任意键退出。");
            Console.ReadKey();
        }

        static void TestVector2()
        {
            Console.WriteLine("=== Vector2测试 ===");

            // 创建向量
            Vector2 v1 = new Vector2(3, 4);
            Vector2 v2 = new Vector2(1, 2);

            // 显示向量信息
            Console.WriteLine($"v1 = {v1}, 长度 = {v1.Length}");
            Console.WriteLine($"v2 = {v2}, 长度 = {v2.Length}");

            // 向量运算
            Vector2 sum = v1 + v2;
            Vector2 diff = v1 - v2;
            Vector2 scaled = v1 * 2;

            Console.WriteLine($"v1 + v2 = {sum}");
            Console.WriteLine($"v1 - v2 = {diff}");
            Console.WriteLine($"v1 * 2 = {scaled}");

            // 归一化
            Vector2 normalized = v1.Normalized;
            Console.WriteLine($"v1归一化 = {normalized}, 长度 = {normalized.Length}");

            // 点积和叉积
            float dot = Vector2.Dot(v1, v2);
            float cross = Vector2.Cross(v1, v2);
            Console.WriteLine($"v1 · v2 = {dot}");
            Console.WriteLine($"v1 × v2 = {cross}");

            Console.WriteLine();
        }

        static void TestMatrix2()
        {
            Console.WriteLine("=== Matrix2测试 ===");

            // 创建矩阵
            Matrix2 identity = Matrix2.Identity;
            Matrix2 rotation = Matrix2.CreateRotation(MathHelper.PiOver4); // 旋转45度
            Matrix2 scale = Matrix2.CreateScale(2, 3);

            Console.WriteLine($"单位矩阵:\n{identity}");
            Console.WriteLine($"旋转矩阵(45度):\n{rotation}");
            Console.WriteLine($"缩放矩阵(2, 3):\n{scale}");

            // 向量变换
            Vector2 v = new Vector2(1, 0);
            Vector2 rotated = rotation.Transform(v);
            Vector2 scaled = scale.Transform(v);

            Console.WriteLine($"向量 {v} 旋转45度后 = {rotated}");
            Console.WriteLine($"向量 {v} 缩放(2,3)后 = {scaled}");

            // 矩阵运算
            Matrix2 combined = rotation * scale;
            Console.WriteLine($"旋转 * 缩放 =\n{combined}");

            // 矩阵求逆
            Matrix2 inverse = combined;
            inverse.Invert();
            Console.WriteLine($"逆矩阵 =\n{inverse}");

            Console.WriteLine();
        }

        static void TestMathHelper()
        {
            Console.WriteLine("=== MathHelper测试 ===");

            // 角度和弧度转换
            float degrees = 180;
            float radians = MathHelper.ToRadians(degrees);
            Console.WriteLine($"{degrees}度 = {radians}弧度");
            Console.WriteLine($"{radians}弧度 = {MathHelper.ToDegrees(radians)}度");

            // 插值
            float start = 10, end = 20;
            Console.WriteLine($"在{start}和{end}之间插值:");
            for (float t = 0; t <= 1; t += 0.25f)
            {
                Console.WriteLine($"  t={t}: 线性插值={MathHelper.Lerp(start, end, t)}, 平滑插值={MathHelper.SmoothStep(start, end, t)}");
            }

            // 限制值
            Console.WriteLine($"限制值5在范围[0,10]内: {MathHelper.Clamp(5, 0, 10)}");
            Console.WriteLine($"限制值-5在范围[0,10]内: {MathHelper.Clamp(-5, 0, 10)}");
            Console.WriteLine($"限制值15在范围[0,10]内: {MathHelper.Clamp(15, 0, 10)}");

            Console.WriteLine();
        }

        static void TestLogger()
        {
            Console.WriteLine("=== Logger测试 ===");

            // 配置Logger
            Logger.Level = LogLevel.Debug;
            Logger.LogToFile = false;

            // 输出不同级别的日志
            Logger.Debug("这是一条调试信息");
            Logger.Info("这是一条普通信息");
            Logger.Warning("这是一条警告信息");
            Logger.Error("这是一条错误信息");

            // 测试异常日志
            try
            {
                throw new Exception("测试异常");
            }
            catch (Exception ex)
            {
                Logger.Error("捕获到异常", ex);
            }

            Console.WriteLine();
        }

        static void TestTimer()
        {
            Console.WriteLine("=== Timer测试 ===");

            // 创建计时器
            GameEngine.Common.Timer timer = new GameEngine.Common.Timer();
            timer.Start();

            // 等待并检查时间
            Console.WriteLine("等待1秒...");
            Thread.Sleep(1000);
            timer.Update();
            Console.WriteLine($"经过时间: {timer.TotalTime:F3}秒");
            Console.WriteLine($"帧时间: {timer.DeltaTime:F3}秒");
            Console.WriteLine($"当前帧率: {timer.FramesPerSecond:F1} FPS");

            // 测试暂停
            Console.WriteLine("暂停计时器并等待1秒...");
            timer.Pause();
            Thread.Sleep(1000);
            timer.Update();
            Console.WriteLine($"暂停后经过时间: {timer.TotalTime:F3}秒 (应该与之前相同)");
            Console.WriteLine($"暂停后帧时间: {timer.DeltaTime:F3}秒 (应该为0)");

            // 恢复计时
            Console.WriteLine("恢复计时器并等待0.5秒...");
            timer.Resume();
            Thread.Sleep(500);
            timer.Update();
            Console.WriteLine($"恢复后经过时间: {timer.TotalTime:F3}秒");
            Console.WriteLine($"恢复后帧时间: {timer.DeltaTime:F3}秒");

            Console.WriteLine();
        }
    }
}