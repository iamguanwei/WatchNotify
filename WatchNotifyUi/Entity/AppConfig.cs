namespace WatchNotifyUi.Entity
{
    /// <summary>
    /// 应用程序配置实体类，包含监视清单和消息发送地址等配置信息。
    /// </summary>
    public class AppConfig
    {
        #region 常量

        /// <summary>
        /// 默认配置文件名称。
        /// </summary>
        public const string DefaultConfigFileName = "config.json";

        #endregion

        #region public 属性

        /// <summary>
        /// 获取或设置消息发送地址模板，格式范例：https://x.x.x.x/?text={0}。
        /// </summary>
        public string MessageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置监视清单，每行一个发送者名称。
        /// </summary>
        public List<string> WatchList { get; set; } = new();

        #endregion
    }
}
