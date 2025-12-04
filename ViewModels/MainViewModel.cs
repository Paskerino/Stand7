using CommonLogic.BusinessLogic.Interfaces;
using CommonLogic.Core.Models;
using CommonLogic.Core.Models.Reports;
using CommonLogic.Logic.Core.Models;
using CommonLogic.Logic.Services.Interfaces;
using CommonLogic.Services.Implementations;
using CommonLogic.Services.Interfaces;
using Stand7.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading; // Важливо!

namespace Stand7
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IDataManager _dataManager;
        private readonly IModbusPollingService _modbusPolling;
        private readonly List<Device> _availableDevices;
        private readonly IDialogService _dialogService;
        private readonly ITestExecutionService testExecutor;
        private readonly IDataLoggerService dataLoggerService;
        private readonly IModbusService modbusService;
        private readonly IReportService reportService;
        private readonly ILogReaderService logReaderService;

        private string _sensorValue;
        public string SensorValue
        {
            get => _sensorValue;
            set
            {
                _sensorValue = value;
                OnPropertyChanged();
            }
        }
        private string currentStatusLineKey;
        private string statusLineText;
        public string StatusLineText
        {
            get => statusLineText;
            set { statusLineText = value; OnPropertyChanged(); }
        }
        private string _bp1Value;
        private string _bp2Value;
        private string _bp3Value;
        private string _bp4Value;
        private string _bp5Value;
        private double innerTestStatus;
        public string BP1Value
        {
            get => _bp1Value;
            set { _bp1Value = value; OnPropertyChanged(); }
        }
        public string BP2Value
        {
            get => _bp2Value;
            set { _bp2Value = value; OnPropertyChanged(); }
        }
        public string BP3Value
        {
            get => _bp3Value;
            set { _bp3Value = value; OnPropertyChanged(); }
        }
        public string BP4Value
        {
            get => _bp4Value;
            set { _bp4Value = value; OnPropertyChanged(); }
        }
        public string BP5Value
        {
            get => _bp5Value;
            set { _bp5Value = value; OnPropertyChanged(); }
        }
        public double InnerTestStatus
        {
            get => innerTestStatus;
        }
        private string _currentTime;
        public string CurrentTime { get; set; }
        private string _targetValue = "123.4"; // Значення, яке ми будемо редагувати
        public string TargetValue
        {
            get => _targetValue;
            set { _targetValue = value; OnPropertyChanged(); }
        }
        private string _serialNumberText = "Введіть серійний номер";
        public string SerialNumberText { get; set; } = "000-000";
        //{
        //    get => _serialNumber;
        //    set { _serialNumber = value; OnPropertyChanged(); }
        //}
        private string _operatorCode = "Введіть код оператора";
        public string OperatorCode { get; set; } = "000-000";
        //{
        //    get => _operatorCode;
        //    set { _operatorCode = value; OnPropertyChanged(); }
        //}
        public bool isRecording;
        private CancellationTokenSource loggingCts;
        private Task writingTask;

        private string _activePropertyName;
        private Action<string> _activeInputSetter;
        public ICommand StartPollingCommand { get; }
        public ICommand StopPollingCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowInputCommand { get; }//
        public ICommand AcceptInputCommand { get; }//
        public ICommand CancelInputCommand { get; }//
        public ICommand OpenKeypadCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ChangePipesColors { get; }
        public ICommand ShowModeSelectScreen { get; }
        public ICommand TestCommand { get; }
        public ObservableCollection<PipeViewModel> Pipes { get; set; }
        private SchemeMode _currentMode;
        public SchemeMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged();
                //UpdateAllPipeColors();
            }
        }



        private readonly Brush _defaultColor = Brushes.White;

        //    Режим -> (ID труби -> Колір)
        private Dictionary<SchemeMode, Dictionary<int, Brush>> _modeConfiguration;
        public MainViewModel(
            ITestExecutionService testExecutor,
            IDataManager dataManager,
            IModbusPollingService modbusPolling,
            List<Device> devices,
            IDialogService dialogService,
            IDataLoggerService dataLoggerService,
            IModbusService modbusService,
            IReportService reportService,
            ILogReaderService logReaderService
            )
        {
            _dataManager = dataManager;
            _modbusPolling = modbusPolling;
            _availableDevices = devices;
            _dialogService = dialogService;
            this.dataLoggerService = dataLoggerService;
            this.testExecutor = testExecutor;
            this.modbusService = modbusService;
            this.reportService = reportService;
            this.logReaderService = logReaderService;

            TranslationManager.Instance.PropertyChanged += OnLanguageChanged;
            _modbusPolling.DataReceived += OnDataReceived;
            testExecutor.TestStatusUpdated += OnTestStatusUpdated;
            StartPollingCommand = new RelayCommand(StartPolling, TestButtonAvailable);
            ExitCommand = new RelayCommand(ExitApplicationAsync);
            ShowInputCommand = new RelayCommand(ShowInput);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowModeSelectScreen = new RelayCommand(o => ShowSelectModeScreen());
            TestCommand = new RelayCommand(o => test());


            // Таймер для годинника на екрані
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { CurrentTime = DateTime.Now.ToString("HH:mm:ss"); };
            timer.Start();
            BP1Value = "00.00";
            BP2Value = "00.00";
            BP3Value = "00.00";
            BP4Value = "00.00";
            BP5Value = "00.00";
            _serialNumberText = "000000-00";
            UpdateStatus("InitialTextForStatusLine");
            StartPolling(null);
            SetArchiveDirs();
            // Ініціалізація труб
            InitializePipes();
            
        }
     

        public bool TestButtonAvailable(object parameter)
        {
            if (selectedMode.HasValue && selectedMode != TestMode.None)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        TestMode? selectedMode;
        private async void ShowSelectModeScreen()
        {
            selectedMode = _dialogService.ShowModeSelectionDialog();

            if (!selectedMode.HasValue || selectedMode == TestMode.None)
            {
                if (isRecording)
                {
                    // Це гарантує, що файл закриється коректно перед виходом
                    await StopArchiveDataAsync();
                }
                // Користувач скасував вибір
                UpdateStatus("ModeSelection_Cancelled");

            }
            if (selectedMode.HasValue && selectedMode != TestMode.None)
            {
                testExecutor.StartTest(selectedMode.Value, _availableDevices[0]);
                StartArchiveData();
            }
        }
        private void OnTestStatusUpdated(string statusKey)
        {
            if (statusKey.Equals("Status_Test1_StopedNormal") ||
                statusKey.Equals("Status_Test2_StopedNormal") ||
                statusKey.Equals("Status_Test3_StopedNormal"))
            {
                modbusService.WriteRegisterAsync(_availableDevices[0], 10, 0); // Команда confirm зупинки тесту
                StopArchiveDataAsync().Wait();
                selectedMode = TestMode.None;
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateStatus(statusKey);
            });
        }
        private void UpdateStatus(string statusKey)
        {
            currentStatusLineKey = statusKey;
            StatusLineText = TranslationManager.Instance[statusKey];
        }
        private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            // Ваш TranslationManager коректно викликає PropertyChanged(null)
            if (e.PropertyName == null && !string.IsNullOrEmpty(currentStatusLineKey))
            {
                // Оновлюємо поточний рядок, використовуючи збережений ключ
                StatusLineText = TranslationManager.Instance[currentStatusLineKey];
            }
        }
        public async Task test()
        {
            var header = new ReportRow();
            header.Cells = new List<ReportCell>
            {
                new ReportCell { Value = "Time" },
                new ReportCell { Value = "BP1" },
                new ReportCell { Value = "BP2" },
                new ReportCell { Value = "BP3" },
                new ReportCell { Value = "BP4" },
                new ReportCell { Value = "BP5" }
            };
            List<ReportRow> tt = await logReaderService.ReadLogAsync("Archive/Data/03122025/Log_ID000-000_Sn000-000_time10_42_39.csv");
            tt.RemoveAt(0);
            GenericReportData reportData = new GenericReportData
            {
                Title = "Test Report",
                Headers = header,
                DataRows = tt
            };
            reportService.CreateReportFileAsync(reportData,"fff", "Archive/Reports/TestReport.xlsx").Wait();
        }
        
        private void BuildModeConfiguration()
        {


            _modeConfiguration = new Dictionary<SchemeMode, Dictionary<int, Brush>>
            {
                // Режим 1: Все біле (просто порожній словник)
                [SchemeMode.Idle] = new Dictionary<int, Brush>(),

                // Режим 2: "ModeA_Flow"
                [SchemeMode.ModeA_Flow] = new Dictionary<int, Brush>
            {
                // Твої 10 труб одного кольору
                { 1, Brushes.Green },
                { 5, Brushes.Green },
                { 12, Brushes.Green },
                // ... (ще 7 ID)
                
                // Твої 5 труб іншого кольору
                { 2, Brushes.Yellow },
                { 8, Brushes.Red },
                // ... (ще 3 ID)
                
                // Твої 7 труб третього кольору
                { 3, Brushes.Yellow },
                { 15, Brushes.Yellow },
                // ... (ще 5 ID)
            },

                // Режим 3: "Failure_Leak"
                [SchemeMode.Failure_Leak] = new Dictionary<int, Brush>
            {
                    { 1, Brushes.Red },
                    { 2, Brushes.Red },
                { 14, Brushes.Orange },
                { 30, Brushes.Orange }
            }

                // ... додай сюди всі інші режими ...
            };
        }
        private void ShowSettings(object parameter)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        private void ShowInput(object parameter)
        {
            if (parameter is string propertyName)
            {
                // Отримуємо поточне значення властивості за допомогою рефлексії
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyName);
                string initialValue = propertyInfo?.GetValue(this)?.ToString() ?? "";

                // Створюємо і показуємо наше нове вікно
                var dialog = new InputDialog("");

                // dialog.ShowDialog() - показує вікно модально (блокує основне)
                if (dialog.ShowDialog() == true)
                {
                    // Якщо користувач натиснув OK, оновлюємо властивість
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(this, dialog.ResultText);
                    }
                }
            }
        }


        private async void ExitApplicationAsync(object parameter)
        {
            if (isRecording)
            {
                // Це гарантує, що файл закриється коректно перед виходом
                await StopArchiveDataAsync();
            }
            Application.Current.Shutdown();
        }
        
        private void OnDataReceived(Dictionary<string, SensorReading> readings)
        {
            BP1Value = readings.ContainsKey("BP1") ? (readings["BP1"].Value / 100).ToString("F2") : BP1Value;
            BP2Value = readings.ContainsKey("BP2") ? (readings["BP2"].Value / 100).ToString("F2") : BP2Value;
            BP3Value = readings.ContainsKey("BP3") ? (readings["BP3"].Value / 100).ToString("F2") : BP3Value;
            BP4Value = readings.ContainsKey("BP4") ? (readings["BP4"].Value / 100).ToString("F2") : BP4Value;
            BP5Value = readings.ContainsKey("BP5") ? (readings["BP5"].Value / 100).ToString("F2") : BP5Value;
            

            if (isRecording)
            {
                var entry = new SensorLogEntry(DateTime.Now, readings["BP1"].Value / 100,
                    readings["BP2"].Value / 100,
                    readings["BP3"].Value / 100,
                    readings["BP4"].Value / 100,
                    readings["BP5"].Value / 100);
                dataLoggerService.EnqueueData(entry);
            }
        }
        private void StartArchiveData()
        {
            loggingCts = new CancellationTokenSource();
            isRecording = true;

            // Use AppDomain.CurrentDomain.BaseDirectory instead of Application.StartupPath
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Archive/Data/{DateTime.Now:ddMMyyyy}");
            DirsCreate(path);
            path = System.IO.Path.Combine(path, $"Log_ID{OperatorCode}_Sn{SerialNumberText}_time{DateTime.Now:HH_mm_ss}.csv");
            writingTask = dataLoggerService.StartProcessingAsync(path, loggingCts.Token);
        }

        public async Task StopArchiveDataAsync()
        {
            if (loggingCts != null)
            {
                loggingCts.Cancel();

                if (writingTask != null)
                {
                    try
                    {
                        await writingTask;
                    }
                    catch (OperationCanceledException)
                    {

                    }
                }

                loggingCts = null;
            }
            isRecording = false;
        }
        private void StartPolling(object parameter)
        {
            Device plc1 = _availableDevices[0];
            int interval = 50;
            _modbusPolling.StartPolling(plc1, interval);
           
        }
    }
}