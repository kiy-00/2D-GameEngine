using System;
using System.IO;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 二维变换组件，存储实体的位置、旋转和缩放
    /// </summary>
    public class Transform2D : Component
    {
        private Vector2 position;
        private Vector2 scale = Vector2.One;
        private float rotation;
        private Transform2D parent;
        private bool dirty = true;
        private Matrix2 localTransform = Matrix2.Identity;
        private Matrix2 worldTransform = Matrix2.Identity;

        /// <summary>
        /// 获取或设置局部位置
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// 获取或设置局部缩放
        /// </summary>
        public Vector2 Scale
        {
            get => scale;
            set
            {
                if (scale != value)
                {
                    scale = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// 获取或设置局部旋转（以弧度为单位）
        /// </summary>
        public float Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// 获取或设置父变换
        /// </summary>
        public Transform2D Parent
        {
            get => parent;
            set
            {
                if (parent != value)
                {
                    parent = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// 获取世界位置
        /// </summary>
        public Vector2 WorldPosition
        {
            get
            {
                UpdateTransformIfNeeded();
                return new Vector2(worldTransform.M21, worldTransform.M22); // 修改为正确的矩阵元素
            }
        }

        /// <summary>
        /// 获取世界缩放
        /// </summary>
        public Vector2 WorldScale
        {
            get
            {
                UpdateTransformIfNeeded();
                return new Vector2(
                    (float)Math.Sqrt(worldTransform.M11 * worldTransform.M11 + worldTransform.M12 * worldTransform.M12),
                    (float)Math.Sqrt(worldTransform.M21 * worldTransform.M21 + worldTransform.M22 * worldTransform.M22)
                );
            }
        }

        /// <summary>
        /// 获取世界旋转（以弧度为单位）
        /// </summary>
        public float WorldRotation
        {
            get
            {
                UpdateTransformIfNeeded();
                return (float)Math.Atan2(worldTransform.M12, worldTransform.M11);
            }
        }

        /// <summary>
        /// 获取局部变换矩阵
        /// </summary>
        public Matrix2 LocalTransform
        {
            get
            {
                UpdateTransformIfNeeded();
                return localTransform;
            }
        }

        /// <summary>
        /// 获取世界变换矩阵
        /// </summary>
        public Matrix2 WorldTransform
        {
            get
            {
                UpdateTransformIfNeeded();
                return worldTransform;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Transform2D() : base()
        {
        }

        /// <summary>
        /// 带初始位置的构造函数
        /// </summary>
        /// <param name="position">初始位置</param>
        public Transform2D(Vector2 position) : base()
        {
            this.position = position;
        }

        /// <summary>
        /// 带初始位置和旋转的构造函数
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <param name="rotation">初始旋转（以弧度为单位）</param>
        public Transform2D(Vector2 position, float rotation) : base()
        {
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        /// 带初始位置、旋转和缩放的构造函数
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <param name="rotation">初始旋转（以弧度为单位）</param>
        /// <param name="scale">初始缩放</param>
        public Transform2D(Vector2 position, float rotation, Vector2 scale) : base()
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        /// <summary>
        /// 将局部坐标转换为世界坐标
        /// </summary>
        /// <param name="localPoint">局部坐标点</param>
        /// <returns>世界坐标点</returns>
        public Vector2 LocalToWorld(Vector2 localPoint)
        {
            UpdateTransformIfNeeded();
            return worldTransform.Transform(localPoint);
        }

        /// <summary>
        /// 将世界坐标转换为局部坐标
        /// </summary>
        /// <param name="worldPoint">世界坐标点</param>
        /// <returns>局部坐标点</returns>
        public Vector2 WorldToLocal(Vector2 worldPoint)
        {
            UpdateTransformIfNeeded();
            Matrix2 inverseWorldTransform = worldTransform;
            inverseWorldTransform.Invert();
            return inverseWorldTransform.Transform(worldPoint);
        }

        /// <summary>
        /// 平移变换
        /// </summary>
        /// <param name="translation">平移向量</param>
        public void Translate(Vector2 translation)
        {
            position += translation;
            SetDirty();
        }

        /// <summary>
        /// 旋转变换
        /// </summary>
        /// <param name="angle">旋转角度（以弧度为单位）</param>
        public void Rotate(float angle)
        {
            rotation += angle;
            SetDirty();
        }

        

        /// <summary>
        /// 将变换标记为需要更新
        /// </summary>
        private void SetDirty()
        {
            dirty = true;

            // 如果这个变换是其他变换的父变换，那么也需要将子变换标记为dirty
            if (Owner is Entity entity)
            {
                foreach (var component in entity.GetAllComponents())
                {
                    if (component is Transform2D childTransform && childTransform.Parent == this)
                    {
                        childTransform.SetDirty();
                    }
                }
            }
        }

        /// <summary>
        /// 更新变换矩阵（如果需要）
        /// </summary>
        private void UpdateTransformIfNeeded()
        {
            if (!dirty)
                return;

            // 更新局部变换矩阵
            Matrix2 translationMatrix = Matrix2.CreateTranslation(position);
            Matrix2 rotationMatrix = Matrix2.CreateRotation(rotation);
            Matrix2 scaleMatrix = Matrix2.CreateScale(scale);

            // 组合变换，顺序是：缩放 -> 旋转 -> 平移
            localTransform = scaleMatrix * rotationMatrix * translationMatrix;

            // 计算世界变换矩阵
            if (parent != null)
            {
                // 确保父变换是最新的
                parent.UpdateTransformIfNeeded();
                worldTransform = localTransform * parent.WorldTransform;
            }
            else
            {
                worldTransform = localTransform;
            }

            dirty = false;
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
            writer.Write(scale.X);
            writer.Write(scale.Y);
            writer.Write(rotation);

            // 序列化父变换的引用（如果有）
            bool hasParent = parent != null;
            writer.Write(hasParent);
            if (hasParent && parent.Owner is Entity parentEntity)
            {
                writer.Write(parentEntity.Id);
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
            scale = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            rotation = reader.ReadSingle();

            // 父变换的引用需要在所有实体加载完成后设置
            bool hasParent = reader.ReadBoolean();
            if (hasParent)
            {
                long parentEntityId = reader.ReadInt64();
                // 此处不直接设置parent，需要在实体加载完成后通过parentEntityId查找并设置
            }

            SetDirty();
        }
    }
}