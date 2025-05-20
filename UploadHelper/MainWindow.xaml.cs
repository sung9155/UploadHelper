using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UploadHelperWpf
{
    public partial class MainWindow : Window
    {
        private readonly string tempFolder;
        private ObservableCollection<FileItem> fileItems;
        private bool isAscending = true;

        public MainWindow()
        {
            InitializeComponent();
            tempFolder = Path.Combine(Path.GetTempPath(), "UploadHelper");
            Directory.CreateDirectory(tempFolder);
            fileItems = new ObservableCollection<FileItem>();
            FileListBox.ItemsSource = fileItems;
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
                FileSize = fileInfo.Length / 1024.0 // Convert to KB
            });
        }

        private void FileListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FileListBox.SelectedItems.Count > 0)
            {
                var items = FileListBox.SelectedItems.Cast<FileItem>().Select(x => x.FilePath).ToArray();
                DragDrop.DoDragDrop(FileListBox, items, DragDropEffects.Copy);
            }
        }

        private void FileListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    AddFile(file);
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

    public class FileItem
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public double FileSize { get; set; } // Changed to double for decimal KB values
    }
} 