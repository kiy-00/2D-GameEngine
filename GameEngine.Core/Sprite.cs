using System;
using System.IO;
using GameEngine.Common;
using SkiaSharp;

namespace GameEngine.Core
{
    /// <summary>
    /// 精灵组件，用于在实体上渲染图像
    /// </summary>
    public class Sprite : Component, IRenderableComponent
    {
        private SKBitmap texture;
        private string texturePath;
        private bool visible = true;
        private RenderLayer renderLayer = RenderLayer.Default;
        private int renderOrder = 0;
        private Vector2 origin = Vector2.Zero;
        private Vector2 scale = Vector2.One;
        private float rotation = 0f;
        private SKRect? sourceRectangle;
        private SKColor tint = SKColors.White;
        private float alpha = 1.0f;
        private bool flipX = false;
        private bool flipY = false;

        /// <summary>
        /// 获取或设置精灵的纹理
        /// </summary>
        public SKBitmap Texture
        {
            get => texture;
            set => texture = value;
        }

        /// <summary>
        /// 获取或设置纹理的路径
        /// </summary>
        public string TexturePath
        {
            get => texturePath;
            set
            {
                texturePath = value;
                LoadTexture();
            }
        }

        /// <summary>
        /// 获取或设置精灵是否可见
        /// </summary>
        public bool Visible
        {
            get => visible && Enabled;
            set => visible = value;
        }

        /// <summary>
        /// 获取或设置渲染层级
        /// </summary>
        public RenderLayer RenderLayer
        {
            get => renderLayer;
            set => renderLayer = value;
        }

        /// <summary>
        /// 获取或设置同一层级内的渲染顺序
        /// </summary>
        public int RenderOrder
        {
            get => renderOrder;
            set => renderOrder = value;
        }

        /// <summary>
        /// 获取或设置旋转中心点（相对于纹理大小的比例，默认为中心点(0.5, 0.5)）
        /// </summary>
        public Vector2 Origin
        {
            get => origin;
            set => origin = value;
        }

        /// <summary>
        /// 获取或设置缩放比例
        /// </summary>
        public Vector2 Scale
        {
            get => scale;
            set => scale = value;
        }

        /// <summary>
        /// 获取或设置旋转角度（以弧度为单位）
        /// </summary>
        public float Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        /// <summary>
        /// 获取或设置源矩形（用于精灵表切片）
        /// </summary>
        public SKRect? SourceRectangle
        {
            get => sourceRectangle;
            set => sourceRectangle = value;
        }

        /// <summary>
        /// 获取或设置色调
        /// </summary>
        public SKColor Tint
        {
            get => tint;
            set => tint = value;
        }

        /// <summary>
        /// 获取或设置透明度
        /// </summary>
        public float Alpha
        {
            get => alpha;
            set => alpha = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// 获取或设置是否水平翻转
        /// </summary>
        public bool FlipX
        {
            get => flipX;
            set => flipX = value;
        }

        /// <summary>
        /// 获取或设置是否垂直翻转
        /// </summary>
        public bool FlipY
        {
            get => flipY;
            set => flipY = value;
        }

        /// <summary>
        /// 获取实体的位置组件（如果有）
        /// </summary>
        private Transform2D Transform
        {
            get
            {
                if (Owner is Entity entity)
                {
                    return entity.GetComponent<Transform2D>();
                }
                return null;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Sprite() : base()
        {
        }

        /// <summary>
        /// 带纹理路径的构造函数
        /// </summary>
        /// <param name="texturePath">纹理路径</param>
        public Sprite(string texturePath) : base()
        {
            this.texturePath = texturePath;
            LoadTexture();
        }

        /// <summary>
        /// 带纹理的构造函数
        /// </summary>
        /// <param name="texture">纹理图像</param>
        public Sprite(SKBitmap texture) : base()
        {
            this.texture = texture;
        }

        /// <summary>
        /// 从纹理路径加载纹理
        /// </summary>
        private void LoadTexture()
        {
            if (string.IsNullOrEmpty(texturePath))
                return;

            try
            {
                if (File.Exists(texturePath))
                {
                    using (var stream = File.OpenRead(texturePath))
                    {
                        texture = SKBitmap.Decode(stream);
                    }
                }
                else
                {
                    Logger.Warning($"Texture file not found: {texturePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load texture: {texturePath}", ex);
            }
        }

        /// <summary>
        /// 渲染精灵
        /// </summary>
        /// <param name="graphicsObj">图形上下文</param>
        public void Render(object graphicsObj)
        {
            if (!Visible || texture == null)
                return;

            if (!(graphicsObj is SKCanvas canvas))
                return;

            Transform2D transform = Transform;
            if (transform == null)
                return;

            // 计算精灵的渲染矩形
            SKRect srcRect;
            if (sourceRectangle.HasValue)
            {
                srcRect = sourceRectangle.Value;
            }
            else
            {
                srcRect = new SKRect(0, 0, texture.Width, texture.Height);
            }

            // 计算目标矩形
            float width = srcRect.Width * scale.X;
            float height = srcRect.Height * scale.Y;

            // 计算原点（考虑翻转）
            float originX = origin.X * width;
            float originY = origin.Y * height;

            // 保存当前画布状态
            canvas.Save();

            // 应用变换
            canvas.Translate(transform.Position.X, transform.Position.Y);
            canvas.RotateRadians(rotation);

            // 处理翻转
            float scaleX = flipX ? -1 : 1;
            float scaleY = flipY ? -1 : 1;
            canvas.Scale(scaleX, scaleY);

            // 创建绘制时使用的画笔
            using (var paint = new SKPaint())
            {
                // 应用色调和透明度
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    1, 0, 0, 0, 0,
                    0, 1, 0, 0, 0,
                    0, 0, 1, 0, 0,
                    0, 0, 0, alpha, 0
                });

                // 设置滤镜质量
                paint.FilterQuality = SKFilterQuality.High;

                // 绘制精灵
                SKRect destRect = new SKRect(-originX, -originY, -originX + width, -originY + height);
                canvas.DrawBitmap(texture, srcRect, destRect, paint);
            }

            // 恢复画布状态
            canvas.Restore();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // 注册到渲染器
            if (Owner is Entity entity && entity.World != null)
            {
                Renderer renderer = entity.World.GetSystem<Renderer>();
                if (renderer != null)
                {
                    renderer.RegisterRenderable(this);
                }
            }
        }

        /// <summary>
        /// 清理组件资源
        /// </summary>
        public override void Cleanup()
        {
            // 从渲染器中注销
            if (Owner is Entity entity && entity.World != null)
            {
                Renderer renderer = entity.World.GetSystem<Renderer>();
                if (renderer != null)
                {
                    renderer.UnregisterRenderable(this);
                }
            }

            // 释放纹理资源
            texture?.Dispose();
            texture = null;

            base.Cleanup();
        }

        /// <summary>
        /// 序列化组件
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(texturePath ?? string.Empty);
            writer.Write(visible);
            writer.Write((int)renderLayer);
            writer.Write(renderOrder);
            writer.Write(origin.X);
            writer.Write(origin.Y);
            writer.Write(scale.X);
            writer.Write(scale.Y);
            writer.Write(rotation);

            bool hasSrcRect = sourceRectangle.HasValue;
            writer.Write(hasSrcRect);
            if (hasSrcRect)
            {
                writer.Write(sourceRectangle.Value.Left);
                writer.Write(sourceRectangle.Value.Top);
                writer.Write(sourceRectangle.Value.Width);
                writer.Write(sourceRectangle.Value.Height);
            }

            writer.Write(tint.Alpha);
            writer.Write(tint.Red);
            writer.Write(tint.Green);
            writer.Write(tint.Blue);
            writer.Write(alpha);
            writer.Write(flipX);
            writer.Write(flipY);
        }

        /// <summary>
        /// 反序列化组件
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);

            texturePath = reader.ReadString();
            if (!string.IsNullOrEmpty(texturePath))
            {
                LoadTexture();
            }

            visible = reader.ReadBoolean();
            renderLayer = (RenderLayer)reader.ReadInt32();
            renderOrder = reader.ReadInt32();
            origin = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            scale = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            rotation = reader.ReadSingle();

            bool hasSrcRect = reader.ReadBoolean();
            if (hasSrcRect)
            {
                float left = reader.ReadSingle();
                float top = reader.ReadSingle();
                float width = reader.ReadSingle();
                float height = reader.ReadSingle();
                sourceRectangle = new SKRect(left, top, left + width, top + height);
            }
            else
            {
                sourceRectangle = null;
            }

            byte alpha = reader.ReadByte();
            byte red = reader.ReadByte();
            byte green = reader.ReadByte();
            byte blue = reader.ReadByte();
            tint = new SKColor(red, green, blue, alpha);

            this.alpha = reader.ReadSingle();
            flipX = reader.ReadBoolean();
            flipY = reader.ReadBoolean();
        }
    }
}