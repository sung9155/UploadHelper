using System;
using System.Windows;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace UploadHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UploadHelper",
            "settings.json");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 설정 파일 로드
            LoadSettings();

            // 설정 적용 (리소스 적용)
            string theme = Properties["Theme"] as string ?? "Light";
            string language = Properties["Language"] as string ?? "ko-KR";
            ApplyTheme(theme);
            ApplyLanguage(language);

            // 메인 윈도우 실행
            var mainWindow = new MainWindow();
            mainWindow.Opacity = Properties["Opacity"] is double d ? d : 1.0;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 설정 저장
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        Properties["Theme"] = settings.Theme ?? "Light";
                        Properties["Language"] = settings.Language ?? "ko-KR";
                        Properties["Opacity"] = settings.Opacity;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 파일을 로드하는 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 기본값 설정
            if (!Properties.Contains("Theme"))
                Properties["Theme"] = "Light";
            if (!Properties.Contains("Language"))
                Properties["Language"] = "ko-KR";
            if (!Properties.Contains("Opacity"))
                Properties["Opacity"] = 1.0;
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    Theme = Properties["Theme"] as string ?? "Light",
                    Language = Properties["Language"] as string ?? "ko-KR",
                    Opacity = Properties["Opacity"] is double d ? d : 1.0
                };

                string? directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 파일을 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyTheme(string theme)
        {
            var resources = Application.Current.Resources;
            var themeDict = new ResourceDictionary();
            if (theme == "Dark")
            {
                themeDict.Source = new Uri("/Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else
            {
                themeDict.Source = new Uri("/Themes/LightTheme.xaml", UriKind.Relative);
            }
            var existingThemeDict = resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source?.ToString().Contains("/Themes/") == true);
            if (existingThemeDict != null)
            {
                resources.MergedDictionaries.Remove(existingThemeDict);
            }
            resources.MergedDictionaries.Add(themeDict);
        }

        private void ApplyLanguage(string language)
        {
            var resources = Application.Current.Resources;
            var langDict = new ResourceDictionary();
            switch (language)
            {
                case "en-US":
                    langDict.Source = new Uri("/Resources/Strings.en-US.xaml", UriKind.Relative);
                    break;
                case "ja-JP":
                    langDict.Source = new Uri("/Resources/Strings.ja-JP.xaml", UriKind.Relative);
                    break;
                case "zh-CN":
                    langDict.Source = new Uri("/Resources/Strings.zh-CN.xaml", UriKind.Relative);
                    break;
                default:
                    langDict.Source = new Uri("/Resources/Strings.ko-KR.xaml", UriKind.Relative);
                    break;
            }
            var existingLangDict = resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source?.ToString().Contains("/Resources/") == true);
            if (existingLangDict != null)
            {
                resources.MergedDictionaries.Remove(existingLangDict);
            }
            resources.MergedDictionaries.Add(langDict);
        }
    }

    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "ko-KR";
        public double Opacity { get; set; } = 1.0;
    }
}
