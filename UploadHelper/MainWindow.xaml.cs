using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;

namespace UploadHelper
{
    public partial class MainWindow : Window
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private readonly string tempFolder;
        private bool isDraggingOut = false;
        private ObservableCollection<FileItem> fileItems;
        private bool isAscending = true;
        private Settings settings;
        private List<string> fileList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            tempFolder = Path.Combine(Path.GetTempPath(), "UploadHelper");
            Directory.CreateDirectory(tempFolder);
            fileItems = new ObservableCollection<FileItem>();
            FileListBox.ItemsSource = fileItems;

            settings = Settings.Load();
            InitializeResources();
            ApplySettings(settings);

            // 버전 정보 추가
            var appTitle = Application.Current.TryFindResource("AppTitle") as string ?? "UploadHelper";
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            Title = $"{appTitle} v{version}";
            VersionTextBlock.Text = $"v{version}";

            // KeyDown 이벤트 등록
            this.KeyDown += MainWindow_KeyDown;
        }

        private void InitializeResources()
        {
            // 언어 코드가 잘못된 경우 강제 변환
            if (settings.Language == "English" || settings.Language == "Korean")
            {
                settings.Language = settings.Language == "Korean" ? "ko-KR" : "en-US";
                settings.Save();
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            // 기본 테마 리소스 추가
            var themeResource = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Themes/{settings.Theme}Theme.xaml")
            };
            Application.Current.Resources.MergedDictionaries.Add(themeResource);

            // 기본 언어 리소스 추가
            var languageResource = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/Strings.{settings.Language}.xaml")
            };
            Application.Current.Resources.MergedDictionaries.Add(languageResource);
        }

        public void ApplySettings(Settings newSettings)
        {
            // 테마 적용
            var themeResource = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Themes/{newSettings.Theme}Theme.xaml")
            };
            Application.Current.Resources.MergedDictionaries[0] = themeResource;

            // 언어 적용
            var languageResource = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/Strings.{newSettings.Language}.xaml")
            };
            Application.Current.Resources.MergedDictionaries[1] = languageResource;

            // 현재 설정 저장
            settings = newSettings;
            settings.Save();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "파일 선택"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    AddFile(file);
                }
            }
        }

        private void AddFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            fileItems.Add(new FileItem
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = fileInfo.Length / 1024.0, // Convert to KB
                FileIcon = GetFileIcon(filePath),
                ModifiedDate = fileInfo.LastWriteTime
            });
        }

        private ImageSource? GetFileIcon(string filePath)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImg = SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
            if (shinfo.hIcon != IntPtr.Zero)
            {
                var img = Imaging.CreateBitmapSourceFromHIcon(
                    shinfo.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                DestroyIcon(shinfo.hIcon);
                return img;
            }
            return null;
        }

        private void FileListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FileListBox.SelectedItems.Count > 0)
            {
                try
                {
                    isDraggingOut = true;
                    // 임시 폴더 정리 (리스트에 없는 파일만 삭제)
                    var validPaths = fileItems.Select(f => f.FilePath).ToHashSet();
                    foreach (var file in Directory.GetFiles(tempFolder))
                    {
                        if (!validPaths.Contains(file))
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }

                    // 선택된 파일들을 임시 폴더에 복사 (이미 임시폴더에 있으면 복사하지 않음)
                    List<string> tempFiles = new List<string>();
                    foreach (var item in FileListBox.SelectedItems.Cast<FileItem>())
                    {
                        if (!File.Exists(item.FilePath))
                        {
                            MessageBox.Show("The file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        string originalFile = item.FilePath;
                        string fileName = Path.GetFileName(originalFile);
                        string tempFile = Path.Combine(tempFolder, fileName);
                        if (!File.Exists(tempFile))
                        {
                            File.Copy(originalFile, tempFile, true);
                        }
                        tempFiles.Add(tempFile);
                    }

                    // 임시 파일들을 드래그
                    if (tempFiles.Count > 0)
                    {
                        DataObject data = new DataObject(DataFormats.FileDrop, tempFiles.ToArray());
                        DragDrop.DoDragDrop(FileListBox, data, DragDropEffects.Copy);
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format((string)Application.Current.FindResource("ClipboardFileProcessError"), ex.Message);
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    isDraggingOut = false;
                }
            }
        }

        private void FileListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !isDraggingOut)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !isDraggingOut)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    // 임시 폴더의 파일은 제외
                    if (File.Exists(file) && !file.StartsWith(tempFolder))
                    {
                        AddFile(file);
                    }
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = FileListBox.SelectedItems.Cast<FileItem>().ToList();
            foreach (var item in selectedItems)
            {
                fileItems.Remove(item);
            }
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            fileItems.Clear();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedItems = isAscending 
                ? fileItems.OrderBy(x => x.FileName).ToList()
                : fileItems.OrderByDescending(x => x.FileName).ToList();

            fileItems.Clear();
            foreach (var item in sortedItems)
            {
                fileItems.Add(item);
            }
            isAscending = !isAscending;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            FileListBox.SelectAll();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue >= 0.2 && e.NewValue <= 1.0)
            {
                Opacity = e.NewValue;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // 프로그램 종료 시 임시 폴더 정리
            try
            {
                if (Directory.Exists(tempFolder))
                {
                    foreach (var file in Directory.GetFiles(tempFolder))
                    {
                        try { File.Delete(file); } catch { }
                    }
                    Directory.Delete(tempFolder);
                }
            }
            catch { }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width - e.HorizontalChange;
            double newLeft = Left + e.HorizontalChange;
            if (newWidth > MinWidth)
            {
                Width = newWidth;
                Left = newLeft;
            }
        }
        private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            if (newWidth > MinWidth)
                Width = newWidth;
        }
        private void ResizeTop_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height - e.VerticalChange;
            double newTop = Top + e.VerticalChange;
            if (newHeight > MinHeight)
            {
                Height = newHeight;
                Top = newTop;
            }
        }
        private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height + e.VerticalChange;
            if (newHeight > MinHeight)
                Height = newHeight;
        }
        private void ResizeTopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeLeft_DragDelta(sender, e);
            ResizeTop_DragDelta(sender, e);
        }
        private void ResizeTopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeRight_DragDelta(sender, e);
            ResizeTop_DragDelta(sender, e);
        }
        private void ResizeBottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeLeft_DragDelta(sender, e);
            ResizeBottom_DragDelta(sender, e);
        }
        private void ResizeBottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeRight_DragDelta(sender, e);
            ResizeBottom_DragDelta(sender, e);
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                if (MaxRestoreIcon != null)
                    MaxRestoreIcon.Text = "\xE923"; // 복원 아이콘
            }
            else
            {
                WindowState = WindowState.Normal;
                if (MaxRestoreIcon != null)
                    MaxRestoreIcon.Text = "\xE922"; // 최대화 아이콘
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (MaxRestoreIcon != null)
            {
                MaxRestoreIcon.Text = (WindowState == WindowState.Maximized) ? "\xE923" : "\xE922";
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. 파일 붙여넣기
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                if (files != null && files.Count > 0)
                {
                    foreach (string? file in files)
                    {
                        if (!string.IsNullOrEmpty(file) && File.Exists(file))
                        {
                            AddFile(file);
                        }
                    }
                }
                return;
            }

            // 2. 이미지 붙여넣기
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    try
                    {
                        Directory.CreateDirectory(tempFolder); // Ensure temp folder exists
                        string fileName = $"Clipboard_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
                        string filePath = Path.Combine(tempFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(fileStream);
                        }
                        AddFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format((string)Application.Current.FindResource("ClipboardImageSaveError"), ex.Message);
                        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            // 3. 아무것도 없을 때
            MessageBox.Show((string)Application.Current.FindResource("ClipboardNoFileOrImage"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteButton_Click(sender, e);
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            RenameSelectedFile();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F2)
            {
                RenameSelectedFile();
                e.Handled = true;
            }
        }

        private void RenameSelectedFile()
        {
            if (FileListBox.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select a single file to rename.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var item = FileListBox.SelectedItem as FileItem;
            if (item == null) return;
            string currentName = item.FileName;
            string input = ShowInputDialog("Enter new file name:", currentName);
            if (string.IsNullOrWhiteSpace(input) || input == currentName) return;
            string newFileName = input;
            string newFilePath = Path.Combine(tempFolder, newFileName);
            try
            {
                if (item.FilePath.StartsWith(tempFolder))
                {
                    File.Move(item.FilePath, newFilePath);
                }
                else
                {
                    File.Copy(item.FilePath, newFilePath, true);
                }
                item.FileName = newFileName;
                item.FilePath = newFilePath;
                FileListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 간단한 InputBox 대화상자 구현
        public static string ShowInputDialog(string text, string defaultValue)
        {
            Window inputDialog = new Window()
            {
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = text,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };
            var panel = new StackPanel { Margin = new Thickness(10) };
            var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0,10,0,10) };
            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0,0,10,0) };
            var cancelButton = new Button { Content = "Cancel", Width = 80, IsCancel = true };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(textBox);
            panel.Children.Add(buttonPanel);
            inputDialog.Content = panel;
            string result = defaultValue;
            okButton.Click += (s, e) => { result = textBox.Text; inputDialog.DialogResult = true; inputDialog.Close(); };
            cancelButton.Click += (s, e) => { inputDialog.DialogResult = false; inputDialog.Close(); };
            if (inputDialog.ShowDialog() == true)
                return result;
            return defaultValue;
        }
    }

    public class FileItem
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public double FileSize { get; set; }
        public ImageSource? FileIcon { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
} 