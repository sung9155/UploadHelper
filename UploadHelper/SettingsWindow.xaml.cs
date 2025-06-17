using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;

namespace UploadHelper
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow mainWindow;
        private double originalOpacity;
        private string originalTheme;
        private string originalLanguage;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.originalOpacity = mainWindow.Opacity;
            this.originalTheme = App.Current.Properties["Theme"] as string ?? "Light";
            this.originalLanguage = App.Current.Properties["Language"] as string ?? "ko-KR";

            // 현재 설정값 로드
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // 테마 설정 로드
            var currentTheme = App.Current.Properties["Theme"] as string ?? "Light";
            foreach (ComboBoxItem item in ThemeComboBox.Items)
            {
                if ((item.Tag as string) == currentTheme)
                {
                    ThemeComboBox.SelectedItem = item;
                    break;
                }
            }

            // 언어 설정 로드
            var currentLanguage = App.Current.Properties["Language"] as string ?? "ko-KR";
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if ((item.Tag as string) == currentLanguage)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }

            // 투명도 설정 로드
            OpacitySlider.Value = App.Current.Properties["Opacity"] is double d ? d : 1.0;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var theme = selectedItem.Tag as string ?? "Light";
                App.Current.Properties["Theme"] = theme;
                ApplyTheme(theme);
                SaveSettings();
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var language = selectedItem.Tag as string ?? "ko-KR";
                App.Current.Properties["Language"] = language;
                ApplyLanguage(language);
                SaveSettings();
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow != null)
            {
                mainWindow.Opacity = e.NewValue;
                App.Current.Properties["Opacity"] = e.NewValue;
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    Theme = App.Current.Properties["Theme"] as string ?? "Light",
                    Language = App.Current.Properties["Language"] as string ?? "ko-KR",
                    Opacity = App.Current.Properties["Opacity"] is double d ? d : 1.0
                };

                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "UploadHelper",
                    "settings.json");

                string? directory = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 파일을 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DialogResult != true)
            {
                // 취소된 경우 원래 설정으로 복원
                mainWindow.Opacity = originalOpacity;
                App.Current.Properties["Theme"] = originalTheme;
                App.Current.Properties["Language"] = originalLanguage;
                ApplyTheme(originalTheme);
                ApplyLanguage(originalLanguage);
                SaveSettings();
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

            // 기존 테마 리소스 제거
            var existingThemeDict = resources.MergedDictionaries.FirstOrDefault(d => 
                d.Source?.ToString().Contains("/Themes/") == true);
            if (existingThemeDict != null)
            {
                resources.MergedDictionaries.Remove(existingThemeDict);
            }

            // 새 테마 리소스 추가
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

            // 기존 언어 리소스 제거
            var existingLangDict = resources.MergedDictionaries.FirstOrDefault(d => 
                d.Source?.ToString().Contains("/Resources/") == true);
            if (existingLangDict != null)
            {
                resources.MergedDictionaries.Remove(existingLangDict);
            }

            // 새 언어 리소스 추가
            resources.MergedDictionaries.Add(langDict);
        }
    }
} 