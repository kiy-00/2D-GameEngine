using System.IO;

namespace GameEngine.Common
{
    /// <summary>
    /// 定义可序列化对象的接口
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// 序列化对象到流
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        void Serialize(BinaryWriter writer);

        /// <summary>
        /// 从流中反序列化对象
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        void Deserialize(BinaryReader reader);
    }
}