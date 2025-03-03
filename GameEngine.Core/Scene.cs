using System;
using System.Collections.Generic;
using System.IO;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 场景类，表示游戏的一个场景或关卡
    /// </summary>
    public class Scene : ISerializable
    {
        private readonly string name;
        private readonly World world;
        private bool isLoaded = false;
        private bool isActive = false;
        private readonly Dictionary<string, object> sceneData = new Dictionary<string, object>();

        /// <summary>
        /// 场景加载事件
        /// </summary>
        public event EventHandler<EventArgs> Loaded;

        /// <summary>
        /// 场景卸载事件
        /// </summary>
        public event EventHandler<EventArgs> Unloaded;

        /// <summary>
        /// 场景激活事件
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// 场景停用事件
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// 获取场景名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取场景的世界
        /// </summary>
        public World World => world;

        /// <summary>
        /// 获取场景是否已加载
        /// </summary>
        public bool IsLoaded => isLoaded;

        /// <summary>
        /// 获取场景是否处于活动状态
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">场景名称</param>
        public Scene(string name)
        {
            this.name = name;
            this.world = new World(name + " World");
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public virtual void Load()
        {
            if (isLoaded)
                return;

            OnLoad();
            isLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public virtual void Unload()
        {
            if (!isLoaded)
                return;

            // 如果场景处于活动状态，先停用它
            if (isActive)
            {
                Deactivate();
            }

            OnUnload();
            isLoaded = false;
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 激活场景
        /// </summary>
        public virtual void Activate()
        {
            if (!isLoaded)
            {
                Load();
            }

            if (isActive)
                return;

            OnActivate();
            isActive = true;
            Activated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 停用场景
        /// </summary>
        public virtual void Deactivate()
        {
            if (!isActive)
                return;

            OnDeactivate();
            isActive = false;
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 更新场景
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public virtual void Update(float deltaTime)
        {
            if (!isActive)
                return;

            // 更新世界
            world.Update(deltaTime);

            // 子类可以在OnUpdate中添加额外的更新逻辑
            OnUpdate(deltaTime);
        }

        /// <summary>
        /// 当场景加载时调用
        /// </summary>
        protected virtual void OnLoad()
        {
            // 子类可以重写此方法以添加加载逻辑
        }

        /// <summary>
        /// 当场景卸载时调用
        /// </summary>
        protected virtual void OnUnload()
        {
            // 子类可以重写此方法以添加卸载逻辑
        }

        /// <summary>
        /// 当场景激活时调用
        /// </summary>
        protected virtual void OnActivate()
        {
            // 子类可以重写此方法以添加激活逻辑
        }

        /// <summary>
        /// 当场景停用时调用
        /// </summary>
        protected virtual void OnDeactivate()
        {
            // 子类可以重写此方法以添加停用逻辑
        }

        /// <summary>
        /// 当场景更新时调用
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        protected virtual void OnUpdate(float deltaTime)
        {
            // 子类可以重写此方法以添加更新逻辑
        }

        /// <summary>
        /// 设置场景数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            sceneData[key] = value;
        }

        /// <summary>
        /// 获取场景数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值，如果不存在则返回默认值</returns>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key) || !sceneData.ContainsKey(key))
                return defaultValue;

            object value = sceneData[key];
            if (value is T typedValue)
                return typedValue;

            return defaultValue;
        }

        /// <summary>
        /// 序列化场景
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(isLoaded);
            writer.Write(isActive);

            // 序列化场景数据
            writer.Write(sceneData.Count);
            foreach (var pair in sceneData)
            {
                writer.Write(pair.Key);

                // 序列化值需要根据实际类型处理
                // 这里只处理基本类型
                if (pair.Value == null)
                {
                    writer.Write("null");
                    writer.Write(0);
                }
                else if (pair.Value is int intValue)
                {
                    writer.Write("int");
                    writer.Write(intValue);
                }
                else if (pair.Value is float floatValue)
                {
                    writer.Write("float");
                    writer.Write(floatValue);
                }
                else if (pair.Value is string strValue)
                {
                    writer.Write("string");
                    writer.Write(strValue);
                }
                else if (pair.Value is bool boolValue)
                {
                    writer.Write("bool");
                    writer.Write(boolValue);
                }
                else
                {
                    // 未知类型，跳过
                    writer.Write("unknown");
                    writer.Write(0);
                }
            }

            // 序列化世界
            if (world is ISerializable worldSerializable)
            {
                worldSerializable.Serialize(writer);
            }
        }

        /// <summary>
        /// 反序列化场景
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public void Deserialize(BinaryReader reader)
        {
            // 名称是只读的，跳过
            reader.ReadString();

            isLoaded = reader.ReadBoolean();
            isActive = reader.ReadBoolean();

            // 反序列化场景数据
            int dataCount = reader.ReadInt32();
            sceneData.Clear();

            for (int i = 0; i < dataCount; i++)
            {
                string key = reader.ReadString();
                string type = reader.ReadString();

                object value = null;

                switch (type)
                {
                    case "null":
                        reader.ReadInt32(); // 跳过null的占位值
                        break;
                    case "int":
                        value = reader.ReadInt32();
                        break;
                    case "float":
                        value = reader.ReadSingle();
                        break;
                    case "string":
                        value = reader.ReadString();
                        break;
                    case "bool":
                        value = reader.ReadBoolean();
                        break;
                    default:
                        reader.ReadInt32(); // 跳过未知类型的占位值
                        break;
                }

                if (value != null)
                {
                    sceneData[key] = value;
                }
            }

            // 反序列化世界
            if (world is ISerializable worldSerializable)
            {
                worldSerializable.Deserialize(reader);
            }
        }
    }
}