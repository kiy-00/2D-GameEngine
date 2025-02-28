using System;

namespace GameEngine.Common
{
    /// <summary>
    /// 定义组件的基本接口
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// 获取组件的唯一标识符
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// 获取或设置组件所属的实体
        /// </summary>
        object Owner { get; set; }

        /// <summary>
        /// 获取或设置组件是否启用
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Initialize();

        /// <summary>
        /// 清理组件资源
        /// </summary>
        void Cleanup();
    }
}