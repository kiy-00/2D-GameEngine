using System;
using System.Collections.Generic;
using System.Threading;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 游戏循环委托，用于游戏更新和渲染
    /// </summary>
    /// <param name="deltaTime">更新的时间间隔</param>
    public delegate void GameLoopCallback(float deltaTime);

    /// <summary>
    /// 游戏循环状态枚举
    /// </summary>
    public enum GameLoopState
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        Uninitialized,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 暂停
        /// </summary>
        Paused,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped
    }

    /// <summary>
    /// 游戏主循环，控制游戏的更新和渲染
    /// </summary>
    public class GameLoop
    {
        private GameLoopState state = GameLoopState.Uninitialized;
        private bool isFixedTimeStep = true;
        private float targetElapsedTime = 1.0f / 60.0f; // 目标帧率为60FPS
        private float maxElapsedTime = 0.5f;
        private float accumulatedFixedTime = 0.0f;

        private readonly List<IUpdateable> updateables = new List<IUpdateable>();
        private readonly List<IUpdateable> fixedUpdateables = new List<IUpdateable>();

        private GameLoopCallback updateCallback;
        private GameLoopCallback fixedUpdateCallback;
        private GameLoopCallback renderCallback;

        private Thread gameThread;
        private readonly object syncRoot = new object();
        private readonly ManualResetEvent pauseEvent = new ManualResetEvent(true);
        private bool exitRequested = false;

        /// <summary>
        /// 获取游戏循环的当前状态
        /// </summary>
        public GameLoopState State => state;

        /// <summary>
        /// 获取或设置是否使用固定时间步长
        /// </summary>
        public bool IsFixedTimeStep
        {
            get => isFixedTimeStep;
            set => isFixedTimeStep = value;
        }

        /// <summary>
        /// 获取或设置目标帧时间间隔（秒）
        /// </summary>
        public float TargetElapsedTime
        {
            get => targetElapsedTime;
            set => targetElapsedTime = Math.Max(0.001f, value);
        }

        /// <summary>
        /// 获取或设置最大允许的帧时间（秒）
        /// </summary>
        public float MaxElapsedTime
        {
            get => maxElapsedTime;
            set => maxElapsedTime = Math.Max(targetElapsedTime, value);
        }

        /// <summary>
        /// 获取或设置更新回调
        /// </summary>
        public GameLoopCallback UpdateCallback
        {
            get => updateCallback;
            set => updateCallback = value;
        }

        /// <summary>
        /// 获取或设置固定更新回调
        /// </summary>
        public GameLoopCallback FixedUpdateCallback
        {
            get => fixedUpdateCallback;
            set => fixedUpdateCallback = value;
        }

        /// <summary>
        /// 获取或设置渲染回调
        /// </summary>
        public GameLoopCallback RenderCallback
        {
            get => renderCallback;
            set => renderCallback = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public GameLoop()
        {
        }

        /// <summary>
        /// 初始化游戏循环
        /// </summary>
        public void Initialize()
        {
            if (state != GameLoopState.Uninitialized)
                return;

            Time.Initialize();
            state = GameLoopState.Stopped;
        }

        /// <summary>
        /// 启动游戏循环
        /// </summary>
        public void Start()
        {
            if (state == GameLoopState.Uninitialized)
                Initialize();

            if (state == GameLoopState.Running)
                return;

            lock (syncRoot)
            {
                exitRequested = false;
                pauseEvent.Set();

                if (gameThread == null || !gameThread.IsAlive)
                {
                    gameThread = new Thread(Run)
                    {
                        Name = "GameLoop Thread",
                        IsBackground = true
                    };
                    gameThread.Start();
                }

                state = GameLoopState.Running;
            }
        }

        /// <summary>
        /// 暂停游戏循环
        /// </summary>
        public void Pause()
        {
            if (state != GameLoopState.Running)
                return;

            lock (syncRoot)
            {
                pauseEvent.Reset();
                state = GameLoopState.Paused;
                Time.Pause();
            }
        }

        /// <summary>
        /// 恢复游戏循环
        /// </summary>
        public void Resume()
        {
            if (state != GameLoopState.Paused)
                return;

            lock (syncRoot)
            {
                pauseEvent.Set();
                state = GameLoopState.Running;
                Time.Resume();
            }
        }

        /// <summary>
        /// 停止游戏循环
        /// </summary>
        public void Stop()
        {
            if (state == GameLoopState.Stopped || state == GameLoopState.Uninitialized)
                return;

            lock (syncRoot)
            {
                exitRequested = true;
                pauseEvent.Set();

                // 等待游戏线程结束
                if (gameThread != null && gameThread.IsAlive && Thread.CurrentThread.ManagedThreadId != gameThread.ManagedThreadId)
                {
                    gameThread.Join(1000);
                    if (gameThread.IsAlive)
                    {
                        Logger.Warning("GameLoop thread did not terminate gracefully.");
                    }
                }

                state = GameLoopState.Stopped;
            }
        }

        /// <summary>
        /// 添加可更新对象
        /// </summary>
        /// <param name="updateable">可更新对象</param>
        /// <param name="fixedUpdate">是否添加到固定更新列表</param>
        public void AddUpdateable(IUpdateable updateable, bool fixedUpdate = false)
        {
            if (updateable == null)
                return;

            lock (syncRoot)
            {
                if (fixedUpdate)
                {
                    if (!fixedUpdateables.Contains(updateable))
                    {
                        fixedUpdateables.Add(updateable);
                        fixedUpdateables.Sort((a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder));
                    }
                }
                else
                {
                    if (!updateables.Contains(updateable))
                    {
                        updateables.Add(updateable);
                        updateables.Sort((a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder));
                    }
                }
            }
        }

        /// <summary>
        /// 移除可更新对象
        /// </summary>
        /// <param name="updateable">可更新对象</param>
        /// <param name="fixedUpdate">是否从固定更新列表移除</param>
        public void RemoveUpdateable(IUpdateable updateable, bool fixedUpdate = false)
        {
            if (updateable == null)
                return;

            lock (syncRoot)
            {
                if (fixedUpdate)
                {
                    fixedUpdateables.Remove(updateable);
                }
                else
                {
                    updateables.Remove(updateable);
                }
            }
        }

        /// <summary>
        /// 执行一帧更新
        /// </summary>
        public void DoFrame()
        {
            Time.Update();
            float deltaTime = Time.DeltaTime;

            // 确保deltaTime不会过大
            if (deltaTime > maxElapsedTime)
                deltaTime = maxElapsedTime;

            if (isFixedTimeStep)
            {
                // 固定时间步长逻辑
                accumulatedFixedTime += deltaTime;

                while (accumulatedFixedTime >= Time.FixedDeltaTime)
                {
                    DoFixedUpdate(Time.FixedDeltaTime);
                    accumulatedFixedTime -= Time.FixedDeltaTime;
                }
            }

            // 可变时间步长更新
            DoUpdate(deltaTime);

            // 渲染
            DoRender(deltaTime);
        }

        /// <summary>
        /// 游戏循环主函数
        /// </summary>
        private void Run()
        {
            try
            {
                Time.Reset();

                while (!exitRequested)
                {
                    // 等待恢复信号
                    pauseEvent.WaitOne();

                    if (exitRequested)
                        break;

                    DoFrame();

                    // 如果使用固定时间步长，控制帧率
                    if (isFixedTimeStep)
                    {
                        double elapsed = Time.UnscaledDeltaTime;
                        double sleepTime = targetElapsedTime - elapsed;

                        if (sleepTime > 0)
                        {
                            int sleepMillis = (int)(sleepTime * 1000);
                            Thread.Sleep(sleepMillis);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in GameLoop thread", ex);
            }
            finally
            {
                state = GameLoopState.Stopped;
            }
        }

        /// <summary>
        /// 执行固定更新
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        private void DoFixedUpdate(float deltaTime)
        {
            lock (syncRoot)
            {
                // 调用固定更新回调
                fixedUpdateCallback?.Invoke(deltaTime);

                // 更新所有启用的固定更新对象
                foreach (var updateable in fixedUpdateables)
                {
                    if (updateable.EnableUpdate)
                    {
                        updateable.Update(deltaTime);
                    }
                }
            }
        }

        /// <summary>
        /// 执行可变更新
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        private void DoUpdate(float deltaTime)
        {
            lock (syncRoot)
            {
                // 调用更新回调
                updateCallback?.Invoke(deltaTime);

                // 更新所有启用的更新对象
                foreach (var updateable in updateables)
                {
                    if (updateable.EnableUpdate)
                    {
                        updateable.Update(deltaTime);
                    }
                }
            }
        }

        /// <summary>
        /// 执行渲染
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        private void DoRender(float deltaTime)
        {
            // 渲染不需要锁定，因为我们只是调用回调，而不修改内部状态
            renderCallback?.Invoke(deltaTime);
        }
    }
}