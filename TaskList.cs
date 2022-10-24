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
        private ObservableCollection<Process> _process = null;
        private ObservableCollection<Process> _processInfo = null;
        //имя выбранного процесса
        private string _processName = string.Empty;
        //потоки обновления таблиц
        private Thread _instanceCaller = null;
        private Thread _instanceCallerInfo = null;
        //состояния потока запущен или остановлен
        private bool _workingProcess = true;
        private bool _workingProcessInfo = false;

        public TaskList()
        {
            InitializeComponent();

            //включаем логирование
            Log.EnableLogging(true);
            Log.Info("Приложение запущено.");
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            //создаём таблицы dataGridView
            CreateDataTable();
            CreateDataTableInfo();

            //запускаем поток обновления основной таблицы процессов
            _instanceCaller = new Thread(new ThreadStart(LoadContent));
            _instanceCaller.Start();
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
            _instanceCallerInfo = new Thread(new ThreadStart(LoadInfoContent));

            //состояние обновления таблицы информации процесса
            _workingProcessInfo = true;

            Log.Info("Запущено обновление таблицы!");

            //запускаем поток
            _instanceCallerInfo.Start();
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
                    //заполняем dataGridView1
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        //получаем все процессы
                        ObservableCollection<Process> processNew = new ObservableCollection<Process>(Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)));

                        if (_process != null)
                        {
                            //удаляем остановленные процессы из списка
                            foreach (var proces in _process.Where(p => !processNew.Any(s => s.Id == p.Id)))
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
                            foreach (var proces in processNew.Where(p => !_process.Any(s => s.Id == p.Id)))
                            {
                                dataGridView1.Rows.Add(proces.Id, proces.ProcessName);
                                Log.Info($"Запущен новый процесс {proces.ProcessName}.");
                            }
                            _process.Clear();
                            foreach (var proces in processNew.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
                                _process.Add(proces);
                        }
                        else
                        {
                            //если таблица ещё не заполнена, то заполним таблицу данными на данный момент
                            _process = new ObservableCollection<Process>();
                            foreach (var proces in processNew.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
                            {
                                dataGridView1.Rows.Add(proces.Id, proces.ProcessName);
                                _process.Add(proces);
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
                    //заполняем dataGridView2
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        if (Process.GetProcesses().Where(p => p.ProcessName == _processName && !string.IsNullOrEmpty(p.MainWindowTitle)).Count() == 0)
                        {
                            _processInfo = null;
                            dataGridView2.Rows.Clear();
                            Thread.Sleep(4000);
                            return;
                        }

                        //получаем все процессы
                        ObservableCollection<Process> processNew = new ObservableCollection<Process>(Process.GetProcesses().Where(p => p.ProcessName == _processName));

                        if (_processInfo != null)
                        {
                            //удаляем остановленные процессы из списка
                            foreach (var proces in _processInfo.Where(p => !processNew.Any(s => s.Id == p.Id)))
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
                            foreach (var proces in processNew.Where(p => !_processInfo.Any(s => s.Id == p.Id)))
                            {
                                dataGridView2.Rows.Add(proces.Id, proces.ProcessName);
                                Log.Info($"Запущен новый процесс {proces.Id} {proces.ProcessName}.");
                            }
                            _processInfo.Clear();
                            foreach (var proces in processNew)
                                _processInfo.Add(proces);
                        }
                        else
                        {
                            //если таблица ещё не заполнена, то заполним таблицу данными на данный момент
                            _processInfo = new ObservableCollection<Process>();
                            foreach (var proces in processNew)
                            {
                                dataGridView2.Rows.Add(proces.Id, proces.ProcessName);
                                _processInfo.Add(proces);
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
            Log.Info("Приложение закрыто.");
            Process.GetCurrentProcess().Kill();
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            //запоминаем название процесса
            _processName = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString();

            Log.Info($"Запрошена информация о процесса {_processName}");
            Start();
        }
    }
}
