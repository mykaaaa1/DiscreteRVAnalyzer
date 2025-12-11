using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Менеджер конфигурации с резервной копией
    /// </summary>
    public static class ConfigurationManager
    {
        private const string ConfigFileName = "app_config.json";
        private const string ConfigBackupFileName = "app_config.backup.json";

        public record AppConfig
        {
            [JsonPropertyName("distributionIndex")]
            public int DistributionIndex { get; init; } = 0;

            [JsonPropertyName("parameterN")]
            public string ParameterN { get; init; } = "10";

            [JsonPropertyName("parameterP")]
            public string ParameterP { get; init; } = "0.5";

            [JsonPropertyName("parameterLambda")]
            public string ParameterLambda { get; init; } = "3";

            [JsonPropertyName("parameterK")]
            public string ParameterK { get; init; } = "5";

            [JsonPropertyName("theme")]
            public string Theme { get; init; } = "Light";

            [JsonPropertyName("lastModified")]
            public DateTime LastModified { get; init; } = DateTime.Now;
        }

        public static AppConfig LoadConfig()
        {
            try
            {
                // Пытаемся загрузить основной конфиг
                if (File.Exists(ConfigFileName))
                {
                    string json = File.ReadAllText(ConfigFileName);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    return config ?? GetDefaultConfig();
                }

                // Если основной повреждён, пытаемся загрузить резервный
                if (File.Exists(ConfigBackupFileName))
                {
                    ErrorHandler.LogError(null, "Основной конфиг повреждён, загружаем резервный");
                    string json = File.ReadAllText(ConfigBackupFileName);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    return config ?? GetDefaultConfig();
                }

                return GetDefaultConfig();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Ошибка загрузки конфигурации");
                return GetDefaultConfig();
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                // Создаём резервную копию
                if (File.Exists(ConfigFileName))
                {
                    File.Copy(ConfigFileName, ConfigBackupFileName, overwrite: true);
                }

                // Сохраняем новый конфиг
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFileName, json);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Ошибка сохранения конфигурации");
                ErrorHandler.ShowUserWarning("Не удалось сохранить конфигурацию. Данные будут потеряны при перезагрузке.");
            }
        }

        private static AppConfig GetDefaultConfig()
        {
            return new AppConfig();
        }

        public static void DeleteCorruptedConfig()
        {
            try
            {
                if (File.Exists(ConfigFileName)) File.Delete(ConfigFileName);
                if (File.Exists(ConfigBackupFileName)) File.Delete(ConfigBackupFileName);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Ошибка удаления повреждённого конфига");
            }
        }
    }
}
