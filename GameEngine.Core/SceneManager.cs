using System;
using System.Collections.Generic;
using System.IO;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 场景管理器，管理游戏中的所有场景
    /// </summary>
    public class SceneManager : IUpdateable
    {
        private static SceneManager instance;
        private readonly Dictionary<string, Scene> scenes = new Dictionary<string, Scene>();
        private Scene activeScene;
        private Scene pendingScene;
        private bool isTransitioning = false;
        private bool isFading = false;
        private float transitionProgress = 0;
        private float transitionDuration = 1.0f;
        private bool enableUpdate = true;
        private int updateOrder = 0;

        /// <summary>
        /// 场景转换开始事件
        /// </summary>
        public event EventHandler<SceneTransitionEventArgs> TransitionStarted;

        /// <summary>
        /// 场景转换完成事件
        /// </summary>
        public event EventHandler<SceneTransitionEventArgs> TransitionCompleted;

        /// <summary>
        /// 场景加载事件
        /// </summary>
        public event EventHandler<SceneEventArgs> SceneLoaded;

        /// <summary>
        /// 场景卸载事件
        /// </summary>
        public event EventHandler<SceneEventArgs> SceneUnloaded;

        /// <summary>
        /// 获取场景管理器的单例实例
        /// </summary>
        public static SceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 获取当前活动场景
        /// </summary>
        public Scene ActiveScene => activeScene;

        /// <summary>
        /// 获取是否正在进行场景转换
        /// </summary>
        public bool IsTransitioning => isTransitioning;

        /// <summary>
        /// 获取场景转换进度（0-1）
        /// </summary>
        public float TransitionProgress => transitionProgress;

        /// <summary>
        /// 获取或设置场景转换持续时间（秒）
        /// </summary>
        public float TransitionDuration
        {
            get => transitionDuration;
            set => transitionDuration = Math.Max(0.1f, value);
        }

        /// <summary>
        /// 获取或设置是否使用淡入淡出效果
        /// </summary>
        public bool IsFading
        {
            get => isFading;
            set => isFading = value;
        }

        /// <summary>
        /// 获取更新顺序
        /// </summary>
        public int UpdateOrder => updateOrder;

        /// <summary>
        /// 获取是否启用更新
        /// </summary>
        public bool EnableUpdate => enableUpdate;

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private SceneManager()
        {
        }

        /// <summary>
        /// 设置更新顺序
        /// </summary>
        /// <param name="order">更新顺序</param>
        public void SetUpdateOrder(int order)
        {
            updateOrder = order;
        }

        /// <summary>
        /// 设置是否启用更新
        /// </summary>
        /// <param name="enable">是否启用</param>
        public void SetEnableUpdate(bool enable)
        {
            enableUpdate = enable;
        }

        /// <summary>
        /// 注册场景
        /// </summary>
        /// <param name="scene">要注册的场景</param>
        public void RegisterScene(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (scenes.ContainsKey(scene.Name))
                throw new ArgumentException($"Scene '{scene.Name}' is already registered.");

            scenes[scene.Name] = scene;
        }

        /// <summary>
        /// 注销场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否成功注销</returns>
        public bool UnregisterScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            if (!scenes.ContainsKey(sceneName))
                return false;

            Scene scene = scenes[sceneName];

            // 不能注销当前活动场景或正在转换的场景
            if (scene == activeScene || (isTransitioning && scene == pendingScene))
                return false;

            // 如果场景已加载，先卸载它
            if (scene.IsLoaded)
            {
                scene.Unload();
            }

            scenes.Remove(sceneName);
            return true;
        }

        /// <summary>
        /// 获取场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>场景实例，如果不存在则返回null</returns>
        public Scene GetScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !scenes.ContainsKey(sceneName))
                return null;

            return scenes[sceneName];
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否成功加载</returns>
        public bool LoadScene(string sceneName)
        {
            Scene scene = GetScene(sceneName);
            if (scene == null)
                return false;

            if (!scene.IsLoaded)
            {
                scene.Load();
                SceneLoaded?.Invoke(this, new SceneEventArgs(scene));
            }

            return true;
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否成功卸载</returns>
        public bool UnloadScene(string sceneName)
        {
            Scene scene = GetScene(sceneName);
            if (scene == null)
                return false;

            // 不能卸载当前活动场景或正在转换的场景
            if (scene == activeScene || (isTransitioning && scene == pendingScene))
                return false;

            if (scene.IsLoaded)
            {
                scene.Unload();
                SceneUnloaded?.Invoke(this, new SceneEventArgs(scene));
            }

            return true;
        }

        /// <summary>
        /// 切换活动场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="immediate">是否立即切换（不使用转场效果）</param>
        /// <returns>是否成功开始切换</returns>
        public bool ChangeScene(string sceneName, bool immediate = false)
        {
            Scene scene = GetScene(sceneName);
            if (scene == null)
                return false;

            // 不能切换到当前正在转换的场景
            if (isTransitioning)
                return false;

            // 如果已经是当前场景且已激活，则无需切换
            if (scene == activeScene && scene.IsActive)
                return false;

            // 加载场景
            if (!scene.IsLoaded)
            {
                scene.Load();
                SceneLoaded?.Invoke(this, new SceneEventArgs(scene));
            }

            if (immediate)
            {
                // 立即切换
                PerformSceneChange(scene);
                return true;
            }

            // 开始转场
            pendingScene = scene;
            isTransitioning = true;
            transitionProgress = 0;

            TransitionStarted?.Invoke(this, new SceneTransitionEventArgs(activeScene, pendingScene));

            return true;
        }

        /// <summary>
        /// 执行场景切换
        /// </summary>
        /// <param name="newScene">新场景</param>
        private void PerformSceneChange(Scene newScene)
        {
            Scene oldScene = activeScene;

            // 停用当前场景
            if (activeScene != null)
            {
                activeScene.Deactivate();
            }

            // 激活新场景
            activeScene = newScene;
            activeScene.Activate();

            // 重置转场状态
            pendingScene = null;
            isTransitioning = false;
            transitionProgress = 0;

            // 触发转场完成事件
            TransitionCompleted?.Invoke(this, new SceneTransitionEventArgs(oldScene, activeScene));
        }

        /// <summary>
        /// 更新场景管理器
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public void Update(float deltaTime)
        {
            if (!enableUpdate)
                return;

            // 更新当前活动场景
            if (activeScene != null && activeScene.IsActive)
            {
                activeScene.Update(deltaTime);
            }

            // 处理场景转换
            if (isTransitioning && pendingScene != null)
            {
                // 更新转场进度
                transitionProgress += deltaTime / transitionDuration;

                // 检查转场是否完成
                if (transitionProgress >= 1.0f)
                {
                    PerformSceneChange(pendingScene);
                }
            }
        }

        /// <summary>
        /// 序列化场景管理器状态
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        public void Serialize(BinaryWriter writer)
        {
            // 写入当前活动场景名称
            writer.Write(activeScene != null ? activeScene.Name : string.Empty);

            // 写入场景数量
            writer.Write(scenes.Count);

            // 序列化每个场景
            foreach (var scene in scenes.Values)
            {
                if (scene is ISerializable serializable)
                {
                    serializable.Serialize(writer);
                }
            }
        }

        /// <summary>
        /// 反序列化场景管理器状态
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        public void Deserialize(BinaryReader reader)
        {
            // 读取活动场景名称
            string activeSceneName = reader.ReadString();

            // 读取场景数量
            int sceneCount = reader.ReadInt32();

            // 临时存储反序列化的场景
            Dictionary<string, Scene> deserializedScenes = new Dictionary<string, Scene>();

            // 反序列化每个场景
            for (int i = 0; i < sceneCount; i++)
            {
                // 注意：这里假设Scene的序列化方法会写入场景名称作为第一个字段
                string sceneName = reader.ReadString();

                Scene scene = new Scene(sceneName);
                if (scene is ISerializable serializable)
                {
                    // 注意：因为我们已经读取了场景名称，所以需要设置读取器的位置
                    // 在实际应用中，可能需要更复杂的机制来处理这种情况
                    serializable.Deserialize(reader);
                }

                deserializedScenes[sceneName] = scene;
            }

            // 替换现有场景集合
            scenes.Clear();
            foreach (var pair in deserializedScenes)
            {
                scenes[pair.Key] = pair.Value;
            }

            // 恢复活动场景
            if (!string.IsNullOrEmpty(activeSceneName) && scenes.ContainsKey(activeSceneName))
            {
                activeScene = scenes[activeSceneName];

                // 确保活动场景已加载并激活
                if (!activeScene.IsLoaded)
                {
                    activeScene.Load();
                }

                if (!activeScene.IsActive)
                {
                    activeScene.Activate();
                }
            }
            else
            {
                activeScene = null;
            }

            // 重置转场状态
            pendingScene = null;
            isTransitioning = false;
            transitionProgress = 0;
        }

        /// <summary>
        /// 获取当前活动场景
        /// </summary>
        /// <returns>当前活动场景，如果没有活动场景则返回null</returns>
        public Scene GetActiveScene()
        {
            return activeScene;
        }
    }

    /// <summary>
    /// 场景事件参数
    /// </summary>
    public class SceneEventArgs : EventArgs
    {
        /// <summary>
        /// 获取相关场景
        /// </summary>
        public Scene Scene { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="scene">相关场景</param>
        public SceneEventArgs(Scene scene)
        {
            Scene = scene;
        }
    }

    /// <summary>
    /// 场景转换事件参数
    /// </summary>
    public class SceneTransitionEventArgs : EventArgs
    {
        /// <summary>
        /// 获取旧场景
        /// </summary>
        public Scene OldScene { get; }

        /// <summary>
        /// 获取新场景
        /// </summary>
        public Scene NewScene { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldScene">旧场景</param>
        /// <param name="newScene">新场景</param>
        public SceneTransitionEventArgs(Scene oldScene, Scene newScene)
        {
            OldScene = oldScene;
            NewScene = newScene;
        }
    }
}