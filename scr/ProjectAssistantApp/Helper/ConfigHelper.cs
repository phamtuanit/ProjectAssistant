namespace ProjectAssistant.App.Helper
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using log4net;
    using ProjectAssistant.Contract;

    public static class ConfigHelper
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigHelper));

        /// <summary>
        /// The default aetting file
        /// </summary>
        private const string DefaultSettingFile = "Setting.xml";

        /// <summary>
        /// Gets the setting file path.
        /// </summary>
        /// <value>
        /// The setting file path.
        /// </value>
        private static string settingFilePath => Path.Combine(Environment.CurrentDirectory, DefaultSettingFile);

        /// <summary>
        /// Gets the caching setting.
        /// </summary>
        /// <returns></returns>
        public static FilterSetting GetCachingSetting()
        {
            if (File.Exists(settingFilePath))
            {
                try
                {
                    return Deserialize<FilterSetting>(settingFilePath);
                }
                catch (Exception ex)
                {
                    Logger.Error("The error is occurred when try to load caching setting.", ex);
                }
            }

            return new FilterSetting
            {
                ProjectFilter = "*.csproj",
                NuspecFilter = "*.nuspec",
                NugetConfigFilter = "packages.config"
            };
        }

        /// <summary>
        /// Saves the setting.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public static void SaveSetting(FilterSetting setting)
        {
            try
            {
                Serialize(setting, settingFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error("The error is occurred when try to load caching setting.", ex);
            }
        }

        /// <summary>
        /// Serializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="filePath">The file path.</param>
        private static void Serialize<T>(T source, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fileWriter = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fileWriter, source);
            }
        }

        /// <summary>
        /// Deserializes the specified file path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private static T Deserialize<T>(string filePath)
        {
            using (var fileWriter = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(fileWriter);
            }
        }
    }
}
