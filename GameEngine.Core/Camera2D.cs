using System;
using System.IO;
using GameEngine.Common;
using SkiaSharp;

namespace GameEngine.Core
{
    /// <summary>
    /// 2D相机组件，控制游戏的视图
    /// </summary>
    public class Camera2D : Component, IUpdateable
    {
        private Vector2 position = Vector2.Zero;
        private float rotation = 0f;
        private float zoom = 1.0f;
        private Vector2 origin = Vector2.Zero;
        private SKSize viewportSize = new SKSize(800, 600);
        private SKRect bounds = new SKRect(-float.MaxValue / 2, -float.MaxValue / 2, float.MaxValue, float.MaxValue);
        private bool enableBounds = false;
        private Entity target;
        private string targetComponentName;
        private float damping = 0.1f;
        private bool enableUpdate = true;
        private int updateOrder = 0;
        private bool viewMatrixDirty = true;
        private SKMatrix viewMatrix = SKMatrix.Identity;

        /// <summary>
        /// 获取或设置相机位置
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;

                    // 如果启用了边界限制，则限制相机位置
                    if (enableBounds)
                    {
                        position.X = MathHelper.Clamp(position.X, bounds.Left, bounds.Right - (viewportSize.Width / zoom));
                        position.Y = MathHelper.Clamp(position.Y, bounds.Top, bounds.Bottom - (viewportSize.Height / zoom));
                    }

                    viewMatrixDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置相机旋转（以弧度为单位）
        /// </summary>
        public float Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    viewMatrixDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置相机缩放级别
        /// </summary>
        public float Zoom
        {
            get => zoom;
            set
            {
                float newZoom = MathHelper.Clamp(value, 0.01f, 100.0f);
                if (zoom != newZoom)
                {
                    zoom = newZoom;
                    viewMatrixDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置视口大小
        /// </summary>
        public SKSize ViewportSize
        {
            get => viewportSize;
            set
            {
                if (viewportSize != value)
                {
                    viewportSize = value;
                    viewMatrixDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置相机原点（相对于视口的比例，默认为中心点(0.5, 0.5)）
        /// </summary>
        public Vector2 Origin
        {
            get => origin;
            set
            {
                if (origin != value)
                {
                    origin = value;
                    viewMatrixDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置相机边界矩形
        /// </summary>
        public SKRect Bounds
        {
            get => bounds;
            set
            {
                bounds = value;
                if (enableBounds)
                {
                    Position = position; // 重新设置位置以应用边界限制
                }
            }
        }

        /// <summary>
        /// 获取或设置是否启用边界限制
        /// </summary>
        public bool EnableBounds
        {
            get => enableBounds;
            set
            {
                if (enableBounds != value)
                {
                    enableBounds = value;
                    if (enableBounds)
                    {
                        Position = position; // 重新设置位置以应用边界限制
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置跟随目标
        /// </summary>
        public Entity Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null)
                {
                    // 尝试获取Transform2D组件
                    Transform2D transform = target.GetComponent<Transform2D>();
                    if (transform == null)
                    {
                        Logger.Warning("Camera target entity does not have a Transform2D component");
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置目标组件名称，用于在目标实体有多个Transform2D组件时指定
        /// </summary>
        public string TargetComponentName
        {
            get => targetComponentName;
            set => targetComponentName = value;
        }

        /// <summary>
        /// 获取或设置相机跟随的阻尼系数（0表示立即跟随，值越大跟随越平滑）
        /// </summary>
        public float Damping
        {
            get => damping;
            set => damping = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// 获取更新优先级
        /// </summary>
        public int UpdateOrder => updateOrder;

        /// <summary>
        /// 获取是否启用更新
        /// </summary>
        public bool EnableUpdate => enableUpdate;

        public float LerpFactor { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Camera2D() : base()
        {
            origin = new Vector2(0.5f, 0.5f); // 默认使用视口中心作为原点
        }

        /// <summary>
        /// 构造函数（指定位置）
        /// </summary>
        /// <param name="position">初始位置</param>
        public Camera2D(Vector2 position) : this()
        {
            this.position = position;
        }

        /// <summary>
        /// 构造函数（指定位置和缩放）
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <param name="zoom">初始缩放</param>
        public Camera2D(Vector2 position, float zoom) : this(position)
        {
            this.zoom = MathHelper.Clamp(zoom, 0.01f, 100.0f);
        }

        /// <summary>
        /// 设置相机的更新优先级
        /// </summary>
        /// <param name="order">优先级值</param>
        public void SetUpdateOrder(int order)
        {
            updateOrder = order;
        }

        /// <summary>
        /// 设置相机是否启用更新
        /// </summary>
        /// <param name="enable">是否启用</param>
        public void SetEnableUpdate(bool enable)
        {
            enableUpdate = enable;
        }

        /// <summary>
        /// 获取相机的视图矩阵
        /// </summary>
        /// <returns>视图矩阵</returns>
        public SKMatrix GetViewMatrix()
        {
            if (viewMatrixDirty)
            {
                UpdateViewMatrix();
            }
            return viewMatrix;
        }

        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        private void UpdateViewMatrix()
        {
            // 创建新的变换矩阵
            SKMatrix matrix = SKMatrix.Identity;

            // 计算原点偏移
            float originX = viewportSize.Width * origin.X;
            float originY = viewportSize.Height * origin.Y;

            // 先计算平移到原点的矩阵
            SKMatrix translateToOrigin = SKMatrix.CreateTranslation(originX, originY);

            // 缩放矩阵
            SKMatrix scaleMatrix = SKMatrix.CreateScale(zoom, zoom);

            // 旋转矩阵 (注意SkiaSharp中旋转是顺时针的，而System.Drawing是逆时针，可能需要调整)
            SKMatrix rotateMatrix = SKMatrix.CreateRotationDegrees(MathHelper.ToDegrees(rotation));

            // 平移矩阵(负值因为我们想要相反方向移动场景)
            SKMatrix translateMatrix = SKMatrix.CreateTranslation(-position.X, -position.Y);

            // 组合矩阵：先平移到原点，然后缩放，旋转，最后平移
            // 注意SkiaSharp中矩阵乘法的顺序与System.Drawing相反
            matrix = SKMatrix.Concat(translateToOrigin, scaleMatrix);
            matrix = SKMatrix.Concat(matrix, rotateMatrix);
            matrix = SKMatrix.Concat(matrix, translateMatrix);

            viewMatrix = matrix;
            viewMatrixDirty = false;
        }

        /// <summary>
        /// 将屏幕坐标转换为世界坐标
        /// </summary>
        /// <param name="screenPosition">屏幕坐标</param>
        /// <returns>世界坐标</returns>
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            SKMatrix inverseMatrix;
            if (!viewMatrix.TryInvert(out inverseMatrix))
            {
                // 如果矩阵不可逆，返回原始坐标
                return screenPosition;
            }

            SKPoint point = new SKPoint(screenPosition.X, screenPosition.Y);
            SKPoint transformedPoint = inverseMatrix.MapPoint(point);

            return new Vector2(transformedPoint.X, transformedPoint.Y);
        }

        /// <summary>
        /// 将世界坐标转换为屏幕坐标
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <returns>屏幕坐标</returns>
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            SKMatrix matrix = GetViewMatrix();
            SKPoint point = new SKPoint(worldPosition.X, worldPosition.Y);
            SKPoint transformedPoint = matrix.MapPoint(point);

            return new Vector2(transformedPoint.X, transformedPoint.Y);
        }

        /// <summary>
        /// 更新相机
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public void Update(float deltaTime)
        {
            if (!enableUpdate || target == null)
                return;

            // 获取目标实体的变换组件
            Transform2D targetTransform = target.GetComponent<Transform2D>();
            if (targetTransform == null)
                return;

            // 计算目标位置
            Vector2 targetPosition = targetTransform.Position;

            // 如果启用了阻尼，则平滑相机移动
            if (damping > 0f)
            {
                // 计算新位置
                Vector2 newPosition = Vector2.Lerp(position, targetPosition, 1.0f - MathHelper.Clamp(damping, 0.0f, 0.99f));
                Position = newPosition; // 使用属性以应用边界限制
            }
            else
            {
                // 直接跟随目标
                Position = targetPosition;
            }
        }

        /// <summary>
        /// 序列化组件
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(rotation);
            writer.Write(zoom);
            writer.Write(origin.X);
            writer.Write(origin.Y);
            writer.Write(viewportSize.Width);
            writer.Write(viewportSize.Height);
            writer.Write(bounds.Left);
            writer.Write(bounds.Top);
            writer.Write(bounds.Width);
            writer.Write(bounds.Height);
            writer.Write(enableBounds);
            writer.Write(damping);
            writer.Write(enableUpdate);
            writer.Write(updateOrder);
            writer.Write(targetComponentName ?? string.Empty);

            // 序列化目标实体引用
            bool hasTarget = target != null;
            writer.Write(hasTarget);
            if (hasTarget)
            {
                writer.Write(target.Id);
            }
        }

        /// <summary>
        /// 反序列化组件
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);

            position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            rotation = reader.ReadSingle();
            zoom = reader.ReadSingle();
            origin = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            viewportSize = new SKSize(reader.ReadSingle(), reader.ReadSingle());

            float left = reader.ReadSingle();
            float top = reader.ReadSingle();
            float width = reader.ReadSingle();
            float height = reader.ReadSingle();
            bounds = new SKRect(left, top, left + width, top + height);

            enableBounds = reader.ReadBoolean();
            damping = reader.ReadSingle();
            enableUpdate = reader.ReadBoolean();
            updateOrder = reader.ReadInt32();
            targetComponentName = reader.ReadString();

            // 目标实体需要在所有实体加载完成后设置
            bool hasTarget = reader.ReadBoolean();
            if (hasTarget)
            {
                long targetEntityId = reader.ReadInt64();
                // 此处不直接设置target，需要在实体加载完成后通过targetEntityId查找并设置
            }

            viewMatrixDirty = true;
        }
    }
}