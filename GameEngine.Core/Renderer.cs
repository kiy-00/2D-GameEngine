using System;
using System.Collections.Generic;
using GameEngine.Common;
using SkiaSharp;

namespace GameEngine.Core
{
    /// <summary>
    /// 渲染层级枚举，定义渲染的顺序
    /// </summary>
    public enum RenderLayer
    {
        Background = 0,
        Default = 1000,
        Foreground = 2000,
        UI = 3000
    }

    /// <summary>
    /// 基础渲染器类，负责管理和执行游戏中的渲染操作
    /// </summary>
    public class Renderer : GameEngine.Core.System
    {
        private readonly List<IRenderableComponent> renderables = new List<IRenderableComponent>();
        private readonly Dictionary<RenderLayer, List<IRenderableComponent>> layeredRenderables = new Dictionary<RenderLayer, List<IRenderableComponent>>();
        private Camera2D camera;
        private SKColor clearColor = SKColors.Black;
        private object renderTarget;
        private readonly object syncRoot = new object();

        /// <summary>
        /// 获取或设置默认相机
        /// </summary>
        public Camera2D Camera
        {
            get => camera;
            set => camera = value;
        }

        /// <summary>
        /// 获取或设置清屏颜色
        /// </summary>
        public SKColor ClearColor
        {
            get => clearColor;
            set => clearColor = value;
        }

        /// <summary>
        /// 获取或设置渲染目标
        /// </summary>
        public object RenderTarget
        {
            get => renderTarget;
            set => renderTarget = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Renderer() : base(int.MaxValue) // 渲染应该是最后执行的，使用最高优先级
        {
            // 初始化每个层级的渲染列表
            foreach (RenderLayer layer in Enum.GetValues(typeof(RenderLayer)))
            {
                layeredRenderables[layer] = new List<IRenderableComponent>();
            }

            // 创建默认相机
            camera = new Camera2D();
        }

        /// <summary>
        /// 初始化需要的组件类型
        /// </summary>
        protected override void InitializeRequiredComponents()
        {
            // 渲染器不需要特定的组件类型
            // 如果需要特定组件，可以在这里使用RequireComponent<T>方法添加
        }

        /// <summary>
        /// 注册可渲染组件
        /// </summary>
        /// <param name="renderable">可渲染组件</param>
        public void RegisterRenderable(IRenderableComponent renderable)
        {
            lock (syncRoot)
            {
                if (!renderables.Contains(renderable))
                {
                    renderables.Add(renderable);

                    // 添加到对应的层级
                    RenderLayer layer = renderable.RenderLayer;
                    layeredRenderables[layer].Add(renderable);

                    // 对层级内的渲染组件进行排序
                    layeredRenderables[layer].Sort((a, b) => a.RenderOrder.CompareTo(b.RenderOrder));
                }
            }
        }

        /// <summary>
        /// 取消注册可渲染组件
        /// </summary>
        /// <param name="renderable">可渲染组件</param>
        public void UnregisterRenderable(IRenderableComponent renderable)
        {
            lock (syncRoot)
            {
                if (renderables.Remove(renderable))
                {
                    // 从对应的层级中移除
                    layeredRenderables[renderable.RenderLayer].Remove(renderable);
                }
            }
        }

        /// <summary>
        /// 实现系统的更新逻辑
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        protected override void UpdateSystem(float deltaTime)
        {
            if (camera == null)
                return;

            // 更新相机
            if (camera is IUpdateable updateable && updateable.EnableUpdate)
            {
                updateable.Update(deltaTime);
            }

            // 执行渲染
            Render();
        }

        /// <summary>
        /// 执行渲染操作
        /// </summary>
        public void Render()
        {
            if (renderTarget == null)
                return;

            // 使用SKCanvas对象进行渲染
            if (renderTarget is SKCanvas canvas)
            {
                // 清屏
                canvas.Clear(clearColor);

                // 获取视图和投影矩阵信息
                SKMatrix viewMatrix = camera.GetViewMatrix();

                // 对每个层级的渲染组件进行渲染
                foreach (RenderLayer layer in Enum.GetValues(typeof(RenderLayer)))
                {
                    // 根据层级决定是否应用相机变换
                    bool applyCamera = layer != RenderLayer.UI;

                    if (applyCamera)
                    {
                        // 保存当前变换
                        canvas.Save();

                        // 应用相机变换
                        canvas.SetMatrix(viewMatrix);

                        // 渲染这个层级的所有组件
                        foreach (var renderable in layeredRenderables[layer])
                        {
                            if (renderable.Visible)
                            {
                                renderable.Render(canvas);
                            }
                        }

                        // 恢复原来的变换
                        canvas.Restore();
                    }
                    else
                    {
                        // 不应用相机变换直接渲染（用于UI等）
                        foreach (var renderable in layeredRenderables[layer])
                        {
                            if (renderable.Visible)
                            {
                                renderable.Render(canvas);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 定义可渲染组件的接口
    /// </summary>
    public interface IRenderableComponent
    {
        /// <summary>
        /// 获取组件是否可见
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// 获取渲染层级
        /// </summary>
        RenderLayer RenderLayer { get; }

        /// <summary>
        /// 获取渲染顺序
        /// </summary>
        int RenderOrder { get; }

        /// <summary>
        /// 执行渲染
        /// </summary>
        /// <param name="graphics">图形上下文</param>
        void Render(object graphics);
    }
}