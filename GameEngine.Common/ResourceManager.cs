using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameEngine.Common
{
    /// <summary>
    /// 提供资源加载和管理功能
    /// </summary>
    public class ResourceManager
    {
        #region 字段

        private readonly Dictionary<string, object> _resources;
        private readonly Dictionary<Type, IResourceLoader> _loaders;
        private readonly string _basePath;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建新的资源管理器实例
        /// </summary>
        /// <param name="basePath">资源基础路径，如果为空则使用应用程序目录</param>
        public ResourceManager(string basePath = null)
        {
            _resources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _loaders = new Dictionary<Type, IResourceLoader>();
            _basePath = !string.IsNullOrEmpty(basePath) ? basePath : AppDomain.CurrentDomain.BaseDirectory;

            // 注册内置资源加载器
            RegisterDefaultLoaders();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 注册资源加载器
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="loader">资源加载器</param>
        public void RegisterLoader<T>(IResourceLoader<T> loader)
        {
            _loaders[typeof(T)] = loader;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径（相对于基础路径）</param>
        /// <returns>加载的资源</returns>
        public T Load<T>(string path)
        {
            string fullPath = GetFullPath(path);
            string key = GenerateKey<T>(path);

            // 检查资源是否已加载
            if (_resources.TryGetValue(key, out object resource))
            {
                return (T)resource;
            }

            // 尝试加载资源
            if (!_loaders.TryGetValue(typeof(T), out IResourceLoader loader))
            {
                throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");
            }

            T loadedResource = (T)loader.Load(fullPath);

            // 缓存资源
            _resources[key] = loadedResource;

            return loadedResource;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径（相对于基础路径）</param>
        /// <returns>加载的资源</returns>
        public async Task<T> LoadAsync<T>(string path)
        {
            string fullPath = GetFullPath(path);
            string key = GenerateKey<T>(path);

            // 检查资源是否已加载
            if (_resources.TryGetValue(key, out object resource))
            {
                return (T)resource;
            }

            // 尝试加载资源
            if (!_loaders.TryGetValue(typeof(T), out IResourceLoader loader))
            {
                throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");
            }

            T loadedResource = (T)await loader.LoadAsync(fullPath);

            // 缓存资源
            _resources[key] = loadedResource;

            return loadedResource;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        public void Unload<T>(string path)
        {
            string key = GenerateKey<T>(path);

            if (_resources.TryGetValue(key, out object resource))
            {
                // 如果资源实现了IDisposable接口，则调用Dispose方法
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _resources.Remove(key);
            }
        }

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        public void UnloadAll()
        {
            foreach (var resource in _resources.Values)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _resources.Clear();
        }

        /// <summary>
        /// 检查资源是否已加载
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>是否已加载</returns>
        public bool IsLoaded<T>(string path)
        {
            string key = GenerateKey<T>(path);
            return _resources.ContainsKey(key);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取资源的完整路径
        /// </summary>
        private string GetFullPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(_basePath, path);
        }

        /// <summary>
        /// 生成资源的唯一键
        /// </summary>
        private string GenerateKey<T>(string path)
        {
            return $"{typeof(T).Name}:{path}";
        }

        /// <summary>
        /// 注册默认的资源加载器
        /// </summary>
        private void RegisterDefaultLoaders()
        {
            // 注册文本文件加载器
            RegisterLoader(new TextResourceLoader());

            // 注册二进制文件加载器
            RegisterLoader(new BinaryResourceLoader());
        }

        #endregion
    }

    #region 资源加载器接口

    /// <summary>
    /// 资源加载器接口
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        object Load(string path);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        Task<object> LoadAsync(string path);
    }

    /// <summary>
    /// 泛型资源加载器接口
    /// </summary>
    public interface IResourceLoader<T> : IResourceLoader
    {
    }

    #endregion

    #region 内置资源加载器

    /// <summary>
    /// 文本资源加载器
    /// </summary>
    public class TextResourceLoader : IResourceLoader<string>
    {
        public object Load(string path)
        {
            return File.ReadAllText(path);
        }

        public async Task<object> LoadAsync(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    /// <summary>
    /// 二进制资源加载器
    /// </summary>
    public class BinaryResourceLoader : IResourceLoader<byte[]>
    {
        public object Load(string path)
        {
            return File.ReadAllBytes(path);
        }

        public async Task<object> LoadAsync(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }

    #endregion
}