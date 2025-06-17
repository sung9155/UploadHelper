using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace UploadHelper
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private readonly Settings settings;
        private readonly MainWindow mainWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string SelectedLanguage
        {
            get => settings.Language;
            set
            {
                if (settings.Language != value)
                {
                    settings.Language = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            }
        }

        public SettingsWindow(MainWindow mainWindow, Settings currentSettings)
        {
            InitializeComponent();
            settings = currentSettings;
            this.mainWindow = mainWindow;
            DataContext = this;

            // 테마 콤보박스 초기화
            ThemeComboBox.Items.Clear();
            var lightItem = new ComboBoxItem { Content = "라이트 테마", Tag = "Light" };
            var darkItem = new ComboBoxItem { Content = "다크 테마", Tag = "Dark" };
            ThemeComboBox.Items.Add(lightItem);
            ThemeComboBox.Items.Add(darkItem);
            ThemeComboBox.SelectedValue = settings.Theme;

            // 언어 콤보박스 아이템 동적 추가
            LanguageComboBox.Items.Clear();
            var koItem = new ComboBoxItem { Content = "한국어 (Korean)", Tag = "ko-KR" };
            var enItem = new ComboBoxItem { Content = "영어 (English)", Tag = "en-US" };
            var jaItem = new ComboBoxItem { Content = "일본어 (日本語)", Tag = "ja-JP" };
            var zhItem = new ComboBoxItem { Content = "중국어 (中文)", Tag = "zh-CN" };
            LanguageComboBox.Items.Add(koItem);
            LanguageComboBox.Items.Add(enItem);
            LanguageComboBox.Items.Add(jaItem);
            LanguageComboBox.Items.Add(zhItem);
            LanguageComboBox.SelectedValue = settings.Language;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 테마 설정 저장
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedTheme && selectedTheme.Tag is string themeValue)
            {
                settings.Theme = themeValue;
            }

            // 언어 설정 저장
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string langCode)
            {
                settings.Language = langCode;
            }

            // 설정 저장
            settings.Save();

            // 메인 창에 설정 적용
            mainWindow.ApplySettings(settings);

            // 창 닫기
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 