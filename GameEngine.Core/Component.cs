using System;
using System.IO;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 组件的基类，实现了IComponent接口，为所有组件提供基础功能
    /// </summary>
    public abstract class Component : IComponent, ISerializable
    {
        private readonly Guid id;
        private object owner;
        private bool enabled = true;
        private bool initialized = false;

        /// <summary>
        /// 获取组件的唯一标识符
        /// </summary>
        public Guid Id => id;

        /// <summary>
        /// 获取或设置组件所属的实体
        /// </summary>
        public object Owner
        {
            get => owner;
            set
            {
                if (owner != value)
                {
                    if (owner != null && value != null)
                    {
                        throw new InvalidOperationException("Component already belongs to an entity. Remove it first before adding to another entity.");
                    }

                    object oldOwner = owner;
                    owner = value;
                    OnOwnerChanged(oldOwner, owner);
                }
            }
        }

        /// <summary>
        /// 获取或设置组件是否启用
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    if (enabled)
                        OnEnabled();
                    else
                        OnDisabled();
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected Component()
        {
            id = Guid.NewGuid();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        public virtual void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                OnInitialize();
            }
        }

        /// <summary>
        /// 清理组件资源
        /// </summary>
        public virtual void Cleanup()
        {
            if (initialized)
            {
                initialized = false;
                OnCleanup();
            }
        }

        /// <summary>
        /// 当组件初始化时调用
        /// </summary>
        protected virtual void OnInitialize()
        {
            // 子类可以重写此方法以实现初始化逻辑
        }

        /// <summary>
        /// 当组件清理时调用
        /// </summary>
        protected virtual void OnCleanup()
        {
            // 子类可以重写此方法以实现清理逻辑
        }

        /// <summary>
        /// 当组件所属实体改变时调用
        /// </summary>
        /// <param name="oldOwner">旧实体</param>
        /// <param name="newOwner">新实体</param>
        protected virtual void OnOwnerChanged(object oldOwner, object newOwner)
        {
            // 子类可以重写此方法以处理实体变更
        }

        /// <summary>
        /// 当组件被启用时调用
        /// </summary>
        protected virtual void OnEnabled()
        {
            // 子类可以重写此方法以处理启用事件
        }

        /// <summary>
        /// 当组件被禁用时调用
        /// </summary>
        protected virtual void OnDisabled()
        {
            // 子类可以重写此方法以处理禁用事件
        }

        /// <summary>
        /// 当组件所属的实体被激活时调用
        /// </summary>
        internal virtual void OnEntityActivated()
        {
            // 子类可以重写此方法以处理激活事件
        }

        /// <summary>
        /// 当组件所属的实体被停用时调用
        /// </summary>
        internal virtual void OnEntityDeactivated()
        {
            // 子类可以重写此方法以处理停用事件
        }

        /// <summary>
        /// 序列化组件
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(id.ToString());
            writer.Write(enabled);
        }

        /// <summary>
        /// 反序列化组件
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public virtual void Deserialize(BinaryReader reader)
        {
            // ID是只读的，我们只读取它但不设置
            string idStr = reader.ReadString();
            enabled = reader.ReadBoolean();
        }

        /// <summary>
        /// 返回组件的字符串表示
        /// </summary>
        public override string ToString()
        {
            Entity entity = Owner as Entity;
            return $"{GetType().Name}(ID: {Id}, Owner: {(entity != null ? entity.Name : "None")}, Enabled: {Enabled})";
        }
    }
}