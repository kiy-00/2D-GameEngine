using System;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 时间管理类，提供游戏时间相关的功能
    /// </summary>
    public static class Time
    {
        private static GameEngine.Common.Timer gameTimer = new GameEngine.Common.Timer();
        private static float timeScale = 1.0f;
        private static float fixedDeltaTime = 1.0f / 60.0f; // 60 FPS
        private static float maximumDeltaTime = 0.1f; // 10 FPS
        private static bool isFirstFrame = true;

        /// <summary>
        /// 获取游戏启动至今经过的总时间（秒）
        /// </summary>
        public static double TotalTime => gameTimer.TotalTime;

        /// <summary>
        /// 获取上一帧到当前帧的实际时间间隔（秒）
        /// </summary>
        public static double UnscaledDeltaTime => gameTimer.DeltaTime;

        /// <summary>
        /// 获取上一帧到当前帧的缩放后时间间隔（秒）
        /// </summary>
        public static float DeltaTime => MathHelper.Min(gameTimer.DeltaTime * timeScale, maximumDeltaTime);

        /// <summary>
        /// 获取或设置固定更新的时间间隔（秒）
        /// </summary>
        public static float FixedDeltaTime
        {
            get => fixedDeltaTime;
            set => fixedDeltaTime = Math.Max(0.0001f, value);
        }

        /// <summary>
        /// 获取或设置时间缩放因子（默认为1.0）
        /// </summary>
        public static float TimeScale
        {
            get => timeScale;
            set => timeScale = Math.Max(0.0f, value);
        }

        /// <summary>
        /// 获取或设置最大允许的帧时间（秒）
        /// </summary>
        public static float MaximumDeltaTime
        {
            get => maximumDeltaTime;
            set => maximumDeltaTime = Math.Max(fixedDeltaTime, value);
        }

        /// <summary>
        /// 获取当前帧率
        /// </summary>
        public static double FramesPerSecond => gameTimer.FramesPerSecond;

        /// <summary>
        /// 初始化时间系统
        /// </summary>
        public static void Initialize()
        {
            gameTimer.Start();
            isFirstFrame = true;
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public static void Update()
        {
            if (isFirstFrame)
            {
                // 第一帧的DeltaTime设为0，避免初始状态下大的时间间隔
                gameTimer.Reset();
                isFirstFrame = false;
            }

            gameTimer.Update();
        }

        /// <summary>
        /// 暂停时间
        /// </summary>
        public static void Pause()
        {
            gameTimer.Pause();
        }

        /// <summary>
        /// 恢复时间
        /// </summary>
        public static void Resume()
        {
            gameTimer.Resume();
        }

        /// <summary>
        /// 重置时间
        /// </summary>
        public static void Reset()
        {
            gameTimer.Reset();
        }
    }
}