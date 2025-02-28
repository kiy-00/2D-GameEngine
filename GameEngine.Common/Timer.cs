using System;
using System.Diagnostics;

namespace GameEngine.Common
{
    /// <summary>
    /// 提供高精度计时功能
    /// </summary>
    public class Timer
    {
        #region 字段

        private readonly Stopwatch _stopwatch;
        private double _totalTime;
        private double _deltaTime;
        private double _timeScale;
        private bool _isPaused;

        #endregion

        #region 属性

        /// <summary>
        /// 获取总计时时间（秒）
        /// </summary>
        public double TotalTime => _totalTime;

        /// <summary>
        /// 获取上一帧到当前帧的时间（秒）
        /// </summary>
        public double DeltaTime => _deltaTime;

        /// <summary>
        /// 获取未缩放的上一帧到当前帧的时间（秒）
        /// </summary>
        public double UnscaledDeltaTime => _deltaTime / _timeScale;

        /// <summary>
        /// 获取或设置时间缩放因子
        /// </summary>
        public double TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Max(0.0, value);
        }

        /// <summary>
        /// 获取或设置是否暂停
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        /// <summary>
        /// 获取当前帧率
        /// </summary>
        public double FramesPerSecond => _deltaTime > 0 ? 1.0 / _deltaTime : 0.0;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建新的计时器实例
        /// </summary>
        public Timer()
        {
            _stopwatch = new Stopwatch();
            _timeScale = 1.0;
            _isPaused = false;
            _totalTime = 0.0;
            _deltaTime = 0.0;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 启动计时器
        /// </summary>
        public void Start()
        {
            Reset();
            _stopwatch.Start();
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            _totalTime = 0.0;
            _deltaTime = 0.0;
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// 继续计时
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// 更新计时器（每帧调用一次）
        /// </summary>
        public void Update()
        {
            // 计算帧间隔时间
            double currentTime = _stopwatch.Elapsed.TotalSeconds;

            if (!_isPaused)
            {
                // 计算未缩放的delta time
                double unscaledDelta = currentTime - _totalTime;

                // 应用时间缩放
                _deltaTime = unscaledDelta * _timeScale;

                // 更新总时间
                _totalTime = currentTime;
            }
            else
            {
                // 暂停时不累积时间，但需要设置delta time为0
                _deltaTime = 0.0;
            }
        }

        /// <summary>
        /// 等待指定的时间（秒）
        /// </summary>
        /// <param name="seconds">等待的秒数</param>
        public void Wait(double seconds)
        {
            if (seconds <= 0)
                return;

            double endTime = _stopwatch.Elapsed.TotalSeconds + seconds;
            while (_stopwatch.Elapsed.TotalSeconds < endTime)
            {
                // 空循环等待
            }
        }

        #endregion
    }
}