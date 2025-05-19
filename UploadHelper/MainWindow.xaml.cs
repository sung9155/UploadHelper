using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace UploadHelperWpf
{
    public partial class MainWindow : Window
    {
        private readonly string tempFolder;
        private bool isDraggingOut = false;

        public MainWindow()
        {
            InitializeComponent();
            tempFolder = Path.Combine(Path.GetTempPath(), "UploadHelper");
            Directory.CreateDirectory(tempFolder);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                foreach (var file in dlg.FileNames)
                {
                    if (!FileListBox.Items.Contains(file))
                        FileListBox.Items.Add(file);
                }
            }
        }

        private void FileListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FileListBox.SelectedItems.Count > 0)
            {
                try
                {
                    isDraggingOut = true;
                    // 임시 폴더 정리
                    foreach (var file in Directory.GetFiles(tempFolder))
                    {
                        try { File.Delete(file); } catch { }
                    }

                    // 선택된 파일들을 임시 폴더에 복사
                    List<string> tempFiles = new List<string>();
                    foreach (var item in FileListBox.SelectedItems)
                    {
                        string originalFile = item.ToString();
                        string fileName = Path.GetFileName(originalFile);
                        string tempFile = Path.Combine(tempFolder, fileName);
                        File.Copy(originalFile, tempFile, true);
                        tempFiles.Add(tempFile);
                    }

                    // 임시 파일들을 드래그
                    DataObject data = new DataObject(DataFormats.FileDrop, tempFiles.ToArray());
                    DragDrop.DoDragDrop(FileListBox, data, DragDropEffects.Copy);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 처리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    if (File.Exists(file) && !file.StartsWith(tempFolder) && !FileListBox.Items.Contains(file))
                    {
                        FileListBox.Items.Add(file);
                    }
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListBox.SelectedItems.Count > 0)
            {
                var selectedItems = FileListBox.SelectedItems.Cast<object>().ToList();
                foreach (var item in selectedItems)
                {
                    FileListBox.Items.Remove(item);
                }
            }
            else
            {
                MessageBox.Show("삭제할 파일을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListBox.Items.Count > 0)
            {
                var result = MessageBox.Show("모든 파일을 삭제하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    FileListBox.Items.Clear();
                }
            }
            else
            {
                MessageBox.Show("삭제할 파일이 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
} 