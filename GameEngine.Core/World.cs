using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 游戏世界类，管理所有实体和系统
    /// </summary>
    public class World : IUpdateable, ISerializable
    {
        private readonly List<Entity> entities = new List<Entity>();
        private readonly List<Entity> pendingAddEntities = new List<Entity>();
        private readonly List<Entity> pendingRemoveEntities = new List<Entity>();
        private readonly List<System> systems = new List<System>();
        private readonly string name;
        private bool isEnabled = true;
        private bool isUpdating = false;
        private readonly object updateLock = new object();
        private int updateOrder = 0;

        /// <summary>
        /// 世界名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取或设置世界是否启用
        /// </summary>
        public bool EnableUpdate => isEnabled;

        /// <summary>
        /// 获取更新优先级
        /// </summary>
        public int UpdateOrder => updateOrder;

        /// <summary>
        /// 创建一个新的游戏世界
        /// </summary>
        /// <param name="name">世界名称</param>
        /// <param name="updateOrder">更新优先级</param>
        public World(string name = "World", int updateOrder = 0)
        {
            this.name = name;
            this.updateOrder = updateOrder;
        }

        /// <summary>
        /// 启用或禁用世界
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetEnabled(bool enabled)
        {
            if (isEnabled != enabled)
            {
                isEnabled = enabled;
                if (isEnabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        /// <summary>
        /// 创建并添加一个新实体到世界
        /// </summary>
        /// <param name="name">实体名称</param>
        /// <returns>创建的实体</returns>
        public Entity CreateEntity(string name = "Entity")
        {
            Entity entity = new Entity(name);
            AddEntity(entity);
            return entity;
        }

        /// <summary>
        /// 添加现有实体到世界
        /// </summary>
        /// <param name="entity">要添加的实体</param>
        public void AddEntity(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.World != null && entity.World != this)
                throw new InvalidOperationException("Entity already belongs to another world");

            lock (updateLock)
            {
                if (isUpdating)
                {
                    pendingAddEntities.Add(entity);
                }
                else
                {
                    AddEntityImmediate(entity);
                }
            }
        }

        private void AddEntityImmediate(Entity entity)
        {
            entities.Add(entity);
            entity.SetWorld(this);

            // 检查是否有系统可以处理这个实体
            foreach (var system in systems)
            {
                system.AddEntity(entity);
            }
        }

        /// <summary>
        /// 从世界中移除实体
        /// </summary>
        /// <param name="entity">要移除的实体</param>
        public void DestroyEntity(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.World != this)
                return;

            lock (updateLock)
            {
                if (isUpdating)
                {
                    pendingRemoveEntities.Add(entity);
                }
                else
                {
                    DestroyEntityImmediate(entity);
                }
            }
        }

        private void DestroyEntityImmediate(Entity entity)
        {
            // 从所有系统中移除
            foreach (var system in systems)
            {
                system.RemoveEntity(entity);
            }

            entities.Remove(entity);
            entity.SetWorld(null);
        }

        /// <summary>
        /// 添加系统到世界
        /// </summary>
        /// <param name="system">要添加的系统</param>
        public void AddSystem(System system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            if (system.World != null)
                throw new InvalidOperationException("System already belongs to a world");

            systems.Add(system);
            system.World = this;

            // 检查现有实体是否可以被这个系统处理
            foreach (var entity in entities)
            {
                system.AddEntity(entity);
            }
        }

        /// <summary>
        /// 从世界中移除系统
        /// </summary>
        /// <param name="system">要移除的系统</param>
        /// <returns>如果系统被移除则返回true，否则返回false</returns>
        public bool RemoveSystem(System system)
        {
            if (system == null || system.World != this)
                return false;

            bool removed = systems.Remove(system);
            if (removed)
            {
                system.World = null;
            }
            return removed;
        }

        /// <summary>
        /// 获取指定类型的系统
        /// </summary>
        /// <typeparam name="T">系统类型</typeparam>
        /// <returns>找到的系统，如果没有则返回null</returns>
        public T GetSystem<T>() where T : System
        {
            return systems.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 根据ID查找实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>找到的实体，如果没有则返回null</returns>
        public Entity FindEntityById(long id)
        {
            return entities.Find(e => e.Id == id);
        }

        /// <summary>
        /// 根据名称查找实体
        /// </summary>
        /// <param name="name">实体名称</param>
        /// <returns>找到的第一个匹配实体，如果没有则返回null</returns>
        public Entity FindEntityByName(string name)
        {
            return entities.Find(e => e.Name == name);
        }

        /// <summary>
        /// 根据名称查找所有匹配的实体
        /// </summary>
        /// <param name="name">实体名称</param>
        /// <returns>匹配的实体集合</returns>
        public IEnumerable<Entity> FindEntitiesByName(string name)
        {
            return entities.Where(e => e.Name == name);
        }

        /// <summary>
        /// 更新世界及其所有系统和实体
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        public void Update(float deltaTime)
        {
            if (!isEnabled)
                return;

            lock (updateLock)
            {
                isUpdating = true;

                // 更新所有系统（按更新优先级排序）
                foreach (var system in systems.OrderBy(s => s.UpdateOrder))
                {
                    if (system.EnableUpdate)
                    {
                        system.Update(deltaTime);
                    }
                }

                isUpdating = false;

                // 处理待添加的实体
                foreach (var entity in pendingAddEntities)
                {
                    AddEntityImmediate(entity);
                }
                pendingAddEntities.Clear();

                // 处理待移除的实体
                foreach (var entity in pendingRemoveEntities)
                {
                    DestroyEntityImmediate(entity);
                }
                pendingRemoveEntities.Clear();
            }
        }

        /// <summary>
        /// 当组件添加到实体时调用
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="component">添加的组件</param>
        internal void ComponentAdded(Entity entity, IComponent component)
        {
            foreach (var system in systems)
            {
                system.ComponentAdded(entity, component);
            }
        }

        /// <summary>
        /// 当组件从实体中移除时调用
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="component">移除的组件</param>
        internal void ComponentRemoved(Entity entity, IComponent component)
        {
            foreach (var system in systems)
            {
                system.ComponentRemoved(entity, component);
            }
        }

        /// <summary>
        /// 当世界被启用时调用
        /// </summary>
        protected virtual void OnEnabled()
        {
            // 子类可以重写此方法以处理启用事件
        }

        /// <summary>
        /// 当世界被禁用时调用
        /// </summary>
        protected virtual void OnDisabled()
        {
            // 子类可以重写此方法以处理禁用事件
        }

        /// <summary>
        /// 序列化世界数据
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(isEnabled);
            writer.Write(updateOrder);

            // 写入实体数量
            writer.Write(entities.Count);

            // 序列化每个实体
            foreach (var entity in entities)
            {
                if (entity is ISerializable serializable)
                {
                    serializable.Serialize(writer);
                }
            }
        }

        /// <summary>
        /// 反序列化世界数据
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public void Deserialize(BinaryReader reader)
        {
            // 名称是只读的，不需要反序列化
            reader.ReadString(); // 跳过名称
            isEnabled = reader.ReadBoolean();
            updateOrder = reader.ReadInt32();

            // 读取实体数量
            int entityCount = reader.ReadInt32();

            // 清除现有实体
            entities.Clear();

            // 反序列化实体
            for (int i = 0; i < entityCount; i++)
            {
                Entity entity = new Entity();
                if (entity is ISerializable serializable)
                {
                    serializable.Deserialize(reader);
                    AddEntity(entity);
                }
            }
        }

        // 在 World 类中添加以下方法

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>实体集合</returns>
        public List<Entity> GetAllEntities()
        {
            // 返回实体列表的副本，以防止外部修改
            return new List<Entity>(entities);
        }
    }
}