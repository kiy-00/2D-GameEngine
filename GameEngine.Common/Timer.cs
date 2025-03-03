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
        private double _lastFrameTime; // 上一帧的时间戳
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
            _lastFrameTime = 0.0;
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
            Debug.WriteLine("Timer started, stopwatch running: " + _stopwatch.IsRunning);
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
            _lastFrameTime = 0.0;
            Debug.WriteLine("Timer reset");
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
            if (_isPaused)
            {
                _isPaused = false;
                // 更新上一帧时间以避免大的时间跳跃
                _lastFrameTime = _stopwatch.Elapsed.TotalSeconds;
                Debug.WriteLine($"Timer resumed, _lastFrameTime = {_lastFrameTime}");
            }
        }

        /// <summary>
        /// 更新计时器（每帧调用一次）
        /// </summary>
        public void Update()
        {
            // 确保Stopwatch正在运行
            if (!_stopwatch.IsRunning)
            {
                Debug.WriteLine("Warning: Stopwatch was not running, starting it now");
                _stopwatch.Start();
            }

            // 获取当前时间（秒）
            double currentTime = _stopwatch.Elapsed.TotalSeconds;

            // 检查并输出时间
            Debug.WriteLine($"Stopwatch time: {currentTime} seconds");

            if (!_isPaused)
            {
                // 首次更新时，设置上一帧时间为当前时间，避免大的deltaTime
                if (_lastFrameTime == 0.0)
                {
                    _lastFrameTime = currentTime;
                    Debug.WriteLine($"First update, setting _lastFrameTime = {_lastFrameTime}");
                    _deltaTime = 0.016; // 约60fps的默认值
                }
                else
                {
                    // 计算delta time（确保它是正值）
                    double rawDelta = currentTime - _lastFrameTime;

                    // 检查delta time是否有效
                    if (rawDelta <= 0 || double.IsNaN(rawDelta))
                    {
                        Debug.WriteLine($"Invalid delta time: {rawDelta}, using default");
                        rawDelta = 0.016; // 使用默认值
                    }
                    else if (rawDelta > 0.1) // 防止过大的时间跳跃
                    {
                        Debug.WriteLine($"Delta time too large: {rawDelta}, clamping");
                        rawDelta = 0.1;
                    }

                    // 应用时间缩放
                    _deltaTime = rawDelta * _timeScale;

                    // 更新总时间和上一帧时间
                    _totalTime += _deltaTime;
                    _lastFrameTime = currentTime;
                }

                Debug.WriteLine($"Timer.Update: deltaTime = {_deltaTime}秒, totalTime = {_totalTime}秒");
            }
            else
            {
                // 暂停时不累积时间，但需要设置delta time为一个很小的值playerSprite.Scale
                _deltaTime = 0.000001;
                Debug.WriteLine("Timer is paused, deltaTime set to minimum value");
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