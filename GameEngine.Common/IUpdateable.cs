namespace GameEngine.Common
{
    /// <summary>
    /// 定义可更新对象的接口
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// 获取或设置更新优先级
        /// </summary>
        /// <remarks>
        /// 较小的值表示较高的优先级。默认值为0。
        /// </remarks>
        int UpdateOrder { get; }

        /// <summary>
        /// 获取对象是否启用更新
        /// </summary>
        bool EnableUpdate { get; }

        /// <summary>
        /// 更新对象状态
        /// </summary>
        /// <param name="deltaTime">自上一帧以来的时间（秒）</param>
        void Update(float deltaTime);
    }
}