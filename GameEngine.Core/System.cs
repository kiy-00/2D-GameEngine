using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 系统的基类，处理特定类型组件的实体
    /// </summary>
    public abstract class System : IUpdateable
    {
        private World world;
        private bool enabled = true;
        private int updateOrder = 0;
        private readonly HashSet<Entity> entities = new HashSet<Entity>();
        private readonly List<Type> requiredComponents = new List<Type>();

        /// <summary>
        /// 获取系统所属的世界
        /// </summary>
        public World World
        {
            get => world;
            internal set => world = value;
        }

        /// <summary>
        /// 获取更新优先级
        /// </summary>
        public int UpdateOrder => updateOrder;

        /// <summary>
        /// 获取系统是否启用更新
        /// </summary>
        public bool EnableUpdate => enabled;

        /// <summary>
        /// 获取此系统处理的实体集合
        /// </summary>
        protected IReadOnlyCollection<Entity> Entities => entities;

        public static object Threading { get; internal set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="updateOrder">更新优先级</param>
        protected System(int updateOrder = 0)
        {
            this.updateOrder = updateOrder;
            InitializeRequiredComponents();
        }

        /// <summary>
        /// 设置系统是否启用
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetEnabled(bool enabled)
        {
            if (this.enabled != enabled)
            {
                this.enabled = enabled;
                if (enabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        /// <summary>
        /// 初始化系统所需的组件类型
        /// </summary>
        protected abstract void InitializeRequiredComponents();

        /// <summary>
        /// 添加系统所需的组件类型
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        protected void RequireComponent<T>() where T : IComponent
        {
            requiredComponents.Add(typeof(T));
        }

        /// <summary>
        /// 检查实体是否包含系统所需的所有组件类型
        /// </summary>
        /// <param name="entity">要检查的实体</param>
        /// <returns>如果实体满足要求则返回true，否则返回false</returns>
        public bool IsEntityCompatible(Entity entity)
        {
            foreach (Type type in requiredComponents)
            {
                if (!entity.GetAllComponents().Any(c => type.IsAssignableFrom(c.GetType())))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 添加实体到系统
        /// </summary>
        /// <param name="entity">要添加的实体</param>
        /// <returns>如果实体被添加则返回true，否则返回false</returns>
        public bool AddEntity(Entity entity)
        {
            if (IsEntityCompatible(entity))
            {
                if (entities.Add(entity))
                {
                    OnEntityAdded(entity);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从系统中移除实体
        /// </summary>
        /// <param name="entity">要移除的实体</param>
        /// <returns>如果实体被移除则返回true，否则返回false</returns>
        public bool RemoveEntity(Entity entity)
        {
            if (entities.Remove(entity))
            {
                OnEntityRemoved(entity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新系统
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        public virtual void Update(float deltaTime)
        {
            if (!enabled)
                return;

            UpdateSystem(deltaTime);
        }

        /// <summary>
        /// 实现系统的具体更新逻辑
        /// </summary>
        /// <param name="deltaTime">更新的时间间隔</param>
        protected abstract void UpdateSystem(float deltaTime);

        /// <summary>
        /// 当实体添加到系统时调用
        /// </summary>
        /// <param name="entity">被添加的实体</param>
        protected virtual void OnEntityAdded(Entity entity)
        {
            // 子类可以重写此方法以处理实体添加事件
        }

        /// <summary>
        /// 当实体从系统中移除时调用
        /// </summary>
        /// <param name="entity">被移除的实体</param>
        protected virtual void OnEntityRemoved(Entity entity)
        {
            // 子类可以重写此方法以处理实体移除事件
        }

        /// <summary>
        /// 当系统被启用时调用
        /// </summary>
        protected virtual void OnEnabled()
        {
            // 子类可以重写此方法以处理启用事件
        }

        /// <summary>
        /// 当系统被禁用时调用
        /// </summary>
        protected virtual void OnDisabled()
        {
            // 子类可以重写此方法以处理禁用事件
        }

        /// <summary>
        /// 当组件添加到实体时调用，用于检查是否应该将实体添加到系统
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="component">添加的组件</param>
        internal void ComponentAdded(Entity entity, IComponent component)
        {
            if (!entities.Contains(entity) && IsEntityCompatible(entity))
            {
                AddEntity(entity);
            }
        }

        /// <summary>
        /// 当组件从实体中移除时调用，用于检查是否应该将实体从系统中移除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="component">移除的组件</param>
        internal void ComponentRemoved(Entity entity, IComponent component)
        {
            if (entities.Contains(entity) && !IsEntityCompatible(entity))
            {
                RemoveEntity(entity);
            }
        }
    }
}