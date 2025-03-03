using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 表示游戏世界中的一个实体。实体是组件的容器，没有自己的功能。
    /// </summary>
    public class Entity : ISerializable
    {
        private static long nextId = 0;
        private readonly long id;
        private readonly Dictionary<Type, IComponent> components;
        private bool isActive;
        private string name;
        private World world;

        /// <summary>
        /// 获取实体的唯一标识符
        /// </summary>
        public long Id => id;

        /// <summary>
        /// 获取或设置实体的名称
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// 获取或设置实体是否处于活动状态
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    if (isActive)
                        OnActivated();
                    else
                        OnDeactivated();
                }
            }
        }

        /// <summary>
        /// 获取实体所属的世界
        /// </summary>
        public World World => world;

        // ...

        public Entity(string name = "Entity")
        {
            id = Interlocked.Increment(ref nextId); // Use Interlocked directly
            this.name = name;
            components = new Dictionary<Type, IComponent>();
            isActive = true;
        }

        /// <summary>
        /// 添加组件到实体
        /// </summary>
        /// <param name="component">要添加的组件</param>
        public T AddComponent<T>(T component) where T : class, IComponent
        {
            Type type = typeof(T);

            if (components.ContainsKey(type))
            {
                throw new InvalidOperationException($"Entity already has a component of type {type.Name}");
            }

            component.Owner = this;
            components[type] = component;
            component.Initialize();

            if (world != null)
            {
                world.ComponentAdded(this, component);
            }

            return component;
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>找到的组件，如果没有则返回null</returns>
        public T GetComponent<T>() where T : class, IComponent
        {
            Type type = typeof(T);

            if (components.TryGetValue(type, out IComponent component))
            {
                return component as T;
            }

            return null;
        }

        /// <summary>
        /// 移除指定类型的组件
        /// </summary>
        /// <typeparam name="T">要移除的组件类型</typeparam>
        /// <returns>如果组件被移除则返回true，否则返回false</returns>
        public bool RemoveComponent<T>() where T : class, IComponent
        {
            Type type = typeof(T);

            if (components.TryGetValue(type, out IComponent component))
            {
                components.Remove(type);
                component.Cleanup();
                component.Owner = null;

                if (world != null)
                {
                    world.ComponentRemoved(this, component);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查实体是否包含指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>如果包含该组件则返回true，否则返回false</returns>
        public bool HasComponent<T>() where T : class, IComponent
        {
            return components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 获取所有组件
        /// </summary>
        /// <returns>组件集合</returns>
        public IEnumerable<IComponent> GetAllComponents()
        {
            return components.Values;
        }

        /// <summary>
        /// 从世界中移除此实体
        /// </summary>
        public void Destroy()
        {
            if (world != null)
            {
                world.DestroyEntity(this);
            }
        }

        /// <summary>
        /// 序列化实体数据
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(name);
            writer.Write(isActive);

            // 写入组件数量
            writer.Write(components.Count);

            // 序列化每个组件
            foreach (var component in components.Values)
            {
                // 写入组件类型
                writer.Write(component.GetType().FullName);

                // 序列化组件内容
                if (component is ISerializable serializable)
                {
                    serializable.Serialize(writer);
                }
            }
        }

        /// <summary>
        /// 反序列化实体数据
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public void Deserialize(BinaryReader reader)
        {
            // ID是只读的，我们只读取它但不设置
            long readId = reader.ReadInt64();
            name = reader.ReadString();
            isActive = reader.ReadBoolean();

            // 读取组件数量
            int componentCount = reader.ReadInt32();

            // 清除现有组件
            components.Clear();

            // 组件反序列化需要由World处理，因为它需要创建组件实例
            // 在这里我们只是记录需要创建的组件类型和它们的序列化数据位置
        }

        internal void SetWorld(World world)
        {
            this.world = world;
        }

        /// <summary>
        /// 当实体被激活时调用
        /// </summary>
        protected virtual void OnActivated()
        {
            foreach (var component in components.Values)
            {
                if (component is Component c)
                {
                    c.OnEntityActivated();
                }
            }
        }

        /// <summary>
        /// 当实体被停用时调用
        /// </summary>
        protected virtual void OnDeactivated()
        {
            foreach (var component in components.Values)
            {
                if (component is Component c)
                {
                    c.OnEntityDeactivated();
                }
            }
        }

        /// <summary>
        /// 返回实体的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Entity({id}, '{name}', Components: {components.Count})";
        }
    }
}