using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using TasksList.Logger;

namespace TasksList
{
    public partial class TaskList : Form
    {
        private ObservableCollection<Process> process = null;
        private ObservableCollection<Process> processInfo = null;
        //индексы выбранной стоки таблицы
        private int indexProcess = 0;
        private int indexProcessInfo = 0;
        //имя выбранного процесса
        private string processName = string.Empty;
        //потоки обновления таблиц
        private Thread InstanceCaller = null;
        private Thread InstanceCallerInfo = null;
        //состояния потока запущен или остановлен
        private bool _workingProcess = true;
        private bool _workingProcessInfo = false;

        public TaskList()
        {
            InitializeComponent();

            //включаем логирование
            Log.EnableLogging(true);
            Log.Info("Приложение запущено.");

            double[] numbers1 = { 2.0, 2.1, 2.2, 2.3, 2.4, 2.5 };
            double[] numbers2 = { 2.2 };

            IEnumerable<double> onlyInFirstSet = numbers1.Except(numbers2);
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            //создаём таблицы dataGridVie
            CreateDataTable();
            CreateDataTableInfo();

            //запускаем поток обновления основной таблици рпоцессов
            InstanceCaller = new Thread(new ThreadStart(LoadContent));
            InstanceCaller.Start();
        }

        /// <summary>
        /// Создаём таблицу dataGridView1
        /// </summary>
        private void CreateDataTable()
        {
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "Id";
            dataGridView1.Columns[0].Width = 50;

            dataGridView1.Columns[1].Name = "Name";
            dataGridView1.Columns[1].Width = 150;
        }

        /// <summary>
        /// Создаём таблицу dataGridView2
        /// </summary>
        private void CreateDataTableInfo()
        {
            dataGridView2.ColumnCount = 3;
            dataGridView2.Columns[0].Name = "Id";
            dataGridView2.Columns[0].Width = 50;

            dataGridView2.Columns[1].Name = "Name";
            dataGridView2.Columns[1].Width = 150;

            dataGridView2.Columns[2].Name = "Priority";
            dataGridView2.Columns[2].Width = 50;
        }

        /// <summary>
        /// Функция выделения строки и прокрутка скролла
        /// </summary>
        /// <param name="dataGrid">Грид</param>
        /// <param name="id">Индекс строки</param>
        private void SelectedRowDataGridView(DataGridView dataGrid, int id)
        {
            if (id < dataGrid.RowCount)
            {
                //убираем выделение строки
                dataGrid.ClearSelection();
                //выделяем строку
                dataGrid.Rows[id].Selected = true;
                //перемещаем скрол на выбранный элемант
                dataGrid.FirstDisplayedScrollingRowIndex = id;
            }
        }

        /// <summary>
        /// Запуск потока обновления таблицы информации
        /// </summary>
        private void Start()
        {
            //если поток уже запущен, то выйдем из функции
            if (_workingProcessInfo)
            {
                Log.Warn("Обновление уже запущено!");
                return;
            }

            //создаём поток на обновление таблицы
            InstanceCallerInfo = new Thread(new ThreadStart(LoadInfoContent));

            //состояние обновления таблицы информации процесса
            _workingProcessInfo = true;

            Log.Info("Запущено обновление таблицы!");

            //запускаем поток
            InstanceCallerInfo.Start();
        }

        /// <summary>
        /// Остановка потока обновления таблицы информации
        /// </summary>
        private void Stop()
        {
            //если поток уже остановлен, то выйдем из функции
            if (!_workingProcessInfo)
            {
                Log.Warn("Обновление уже остановлено!");
                return;
            }
            //состояние обновления таблицы информации процесса
            _workingProcessInfo = false;

            Log.Info("Остановлено обновление таблицы!");
        }

        /// <summary>
        /// Обновление общей таблицы процессов
        /// </summary>
        private void LoadContent()
        {
            //обновляем таблицу пока поток запущен
            while (_workingProcess)
            {
                try
                {
                    //получаем все процессы
                    ObservableCollection<Process> processNew = new ObservableCollection<Process>(Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)));
                    //заполняем dataGridView1
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        if(process != null)
                        {
                            //удаляем остановленные процессы из списка
                            foreach (var proces in process.Where(p => !processNew.Any(s => s.Id == p.Id)))
                            {
                                foreach (DataGridViewRow row in dataGridView1.Rows)
                                {
                                    string sl = row.Cells[0].Value.ToString();
                                    if (row.Cells[0].Value.ToString() == $"{proces.Id}")
                                    {
                                        dataGridView1.Rows.RemoveAt(row.Index);
                                        Log.Info($"Процесс {proces.ProcessName} остановлен.");
                                    }
                                }
                            }
                            //добавляем новые запущенные процессы в список
                            foreach (var proces in processNew.Where(p => !process.Any(s => s.Id == p.Id)))
                            {
                                dataGridView1.Rows.Add(proces.Id, proces.ProcessName);
                                Log.Info($"Запущен новый процесс {proces.ProcessName}.");
                            }
                            process.Clear();
                            foreach (var proces in processNew.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
                                process.Add(proces);
                        }
                        else
                        {
                            //если таблица ещё не заполнена, то заполним таблицу данными на данный момент
                            process = new ObservableCollection<Process>();
                            foreach (var proces in processNew.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
                            {
                                dataGridView1.Rows.Add(proces.Id, proces.ProcessName);
                                process.Add(proces);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Обновление таблицы информации процессов
        /// </summary>
        private void LoadInfoContent()
        {
            //обновляем таблицу пока поток запущен
            while (_workingProcessInfo)
            {
                try
                {
                    //получаем все процессы
                    ObservableCollection<Process> processNew = new ObservableCollection<Process>(Process.GetProcesses().Where(p => p.ProcessName == processName));
                    //заполняем dataGridView2
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        if (processInfo != null)
                        {
                            //удаляем остановленные процессы из списка
                            foreach (var proces in processInfo.Where(p => !processNew.Any(s => s.Id == p.Id)))
                            {
                                foreach (DataGridViewRow row in dataGridView2.Rows)
                                {
                                    string sl = row.Cells[0].Value.ToString();
                                    if (row.Cells[0].Value.ToString() == $"{proces.Id}")
                                    {
                                        dataGridView2.Rows.RemoveAt(row.Index);
                                        Log.Info($"Процесс {proces.Id} {proces.ProcessName} остановлен.");
                                    }
                                }
                            }
                            //добавляем новые запущенные процессы в список
                            foreach (var proces in processNew.Where(p => !processInfo.Any(s => s.Id == p.Id)))
                            {
                                dataGridView2.Rows.Add(proces.Id, proces.ProcessName);
                                Log.Info($"Запущен новый процесс {proces.Id} {proces.ProcessName}.");
                            }
                            processInfo.Clear();
                            foreach (var proces in processNew)
                                processInfo.Add(proces);
                        }
                        else
                        {
                            //если таблица ещё не заполнена, то заполним таблицу данными на данный момент
                            processInfo = new ObservableCollection<Process>();
                            foreach (var proces in processNew)
                            {
                                dataGridView2.Rows.Add(proces.Id, proces.ProcessName);
                                processInfo.Add(proces);
                            }
                        }
                    });
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                }

                Thread.Sleep(4000);
            }
        }

        /// <summary>
        /// Событие выбора процесса на общей таблице
        /// </summary>
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            //запоминаем выделенный индекс строки
            try
            {
                indexProcess = dataGridView1.CurrentRow.Index;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Событие выбора процесса на таблице информации
        /// </summary>
        private void dataGridView2_Click(object sender, EventArgs e)
        {
            //запоминаем выделенный индекс строки
            try
            {
                indexProcessInfo = dataGridView2.CurrentRow.Index;
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Запуск обновления таблицы информации
        /// </summary>
        private void bStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        /// <summary>
        /// Остановка обновления таблицы информации
        /// </summary>
        private void bStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        private void TaskList_FormClosed(object sender, FormClosedEventArgs e)
        {
            //останавливаем работу процессов
            _workingProcess = false;
            Stop();

            Log.Info("Приложение закрыто.");
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            //запоминаем название процесса
            processName = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString();

            Log.Info($"Запрошена информация о процесса {processName}");
            Start();
        }
    }
}
