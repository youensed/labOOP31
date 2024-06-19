using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OOP_31
{
    public partial class MainWindow : Window
    {
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName);
            ProcessList.ItemsSource = processes;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void ProcessList_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (ProcessList.SelectedItem == null)
            {
                ProcessList.ContextMenu.IsEnabled = false;
            }
            else
            {
                ProcessList.ContextMenu.IsEnabled = true;
            }
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is Process selectedProcess)
            {
                MessageBox.Show($"Ім'я: {selectedProcess.ProcessName}\nІдентифікатор процесу: {selectedProcess.Id}\nПам'ять: {selectedProcess.WorkingSet64}", "Деталі процесу", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is Process selectedProcess)
            {
                try
                {
                    selectedProcess.Kill();
                    MessageBox.Show("Процес успішно завершено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProcesses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завершити процес: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName);
            string filePath = "process_list.txt";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var process in processes)
                {
                    writer.WriteLine($"{process.ProcessName} - Ідентифікатор процесу: {process.Id}");
                }
            }

            MessageBox.Show($"Список процесів експортовано в {filePath}", "Експорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowThreadsAndModules_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is Process selectedProcess)
            {
                try
                {
                    var threads = selectedProcess.Threads;
                    var modules = selectedProcess.Modules;
                    StringBuilder info = new StringBuilder();

                    info.AppendLine("Потоки:");
                    foreach (ProcessThread thread in threads)
                    {
                        info.AppendLine($"Ідентифікатор процесу: {thread.Id}, Стан: {thread.ThreadState}");
                    }

                    info.AppendLine("\nМодулі:");
                    foreach (ProcessModule module in modules)
                    {
                        info.AppendLine($"Ім'я: {module.ModuleName}, Шлях: {module.FileName}");
                    }

                    MessageBox.Show(info.ToString(), "Потоки та модулі");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося отримати потоки або модулі: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                Sort(sortBy, direction);

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ProcessList.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}