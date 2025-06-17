using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace UploadHelper
{
    public class Settings
    {
        private const string SettingsFileName = "settings.json";
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UploadHelper",
            SettingsFileName);

        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "ko-KR";

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    return settings ?? new Settings();
                }
            }
            catch (Exception)
            {
                // 설정 파일을 읽는 데 실패하면 기본 설정을 반환합니다.
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception)
            {
                // 설정 파일을 저장하는 데 실패하면 무시합니다.
            }
        }

        public static string GetLanguageDisplayName(string languageCode)
        {
            return languageCode switch
            {
                "ko-KR" => "한국어",
                "en-US" => "English",
                "ja-JP" => "日本語",
                "zh-CN" => "中文",
                _ => languageCode
            };
        }
    }
} 