using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace UploadHelperWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                List<string> files = new List<string>();
                foreach (var item in FileListBox.SelectedItems)
                    files.Add(item.ToString());

                DataObject data = new DataObject(DataFormats.FileDrop, files.ToArray());
                DragDrop.DoDragDrop(FileListBox, data, DragDropEffects.Copy);
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
    }
} 