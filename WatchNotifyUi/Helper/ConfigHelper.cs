using System.Text.Json;
using WatchNotifyUi.Entity;

namespace WatchNotifyUi.Helper
{
    /// <summary>
    /// 配置文件管理帮助类，提供配置的加载、保存和还原功能。
    /// </summary>
    internal static class ConfigHelper
    {
        #region 常量

        /// <summary>
        /// JSON序列化选项，启用缩进和中文Unicode编码保留。
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        #endregion

        #region public 方法

        /// <summary>
        /// 获取配置文件的完整路径。
        /// </summary>
        /// <returns>配置文件的完整路径。</returns>
        public static string GetConfigFilePath()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDir, AppConfig.DefaultConfigFileName);
        }

        /// <summary>
        /// 加载配置文件，如果文件不存在则返回默认配置。
        /// </summary>
        /// <returns>加载的配置对象，如果文件不存在则返回新的默认配置。</returns>
        public static AppConfig LoadConfig()
        {
            string filePath = GetConfigFilePath();

            if (!File.Exists(filePath))
            {
                return new AppConfig();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                AppConfig? config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                return config ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        /// <summary>
        /// 保存配置到文件。
        /// </summary>
        /// <param name="config">要保存的配置对象。</param>
        /// <returns>保存成功返回true，否则返回false。</returns>
        public static bool SaveConfig(AppConfig config)
        {
            string filePath = GetConfigFilePath();

            try
            {
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
