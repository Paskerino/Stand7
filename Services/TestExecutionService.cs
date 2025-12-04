using CommonLogic.BusinessLogic.Interfaces;
using CommonLogic.Core.Models;
using CommonLogic.Services.Implementations;
using CommonLogic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Stand7.Services
{
    public class TestExecutionService : ITestExecutionService
    {
        // Залежності з CommonLogic
        private readonly IModbusService modbusService;
        private readonly IDataManager _dataManager;
        private readonly IModbusPollingService modbusPolling;
        Device deviceToTest;
        private double innerTestStatus = 0;
        private double stepNumber = 0;

        public event Action<string> TestStatusUpdated;
        private enum TestState
        {
            Idle,
            Test1,
            Test2,
            Test3,
            TestFull,
        }
        private TestState currentState = TestState.Idle;
        public TestExecutionService(IModbusService modbusService, IDataManager dataManager, IModbusPollingService mbPolling)
        {
            this.modbusService = modbusService;
            _dataManager = dataManager;
            this.modbusPolling = mbPolling;
            this.modbusPolling.DataReceived += OnDataReceived;
        }
        int deleteMe = 0;
        private void OnDataReceived(Dictionary<string, SensorReading> readings)
        {
            innerTestStatus = readings.ContainsKey("InnerTestStatus") ? readings["InnerTestStatus"].Value : innerTestStatus;
            stepNumber = readings.ContainsKey("StepNumber") ? readings["StepNumber"].Value : stepNumber;
            if (readings.TryGetValue("InnerTestStatus", out var statusReading))
            {
                CheckInnerStatus(statusReading);
            }
        }
        private void CheckInnerStatus(SensorReading statusReading)
        {

            if (statusReading.Value == 40)
            {
                deleteMe++;
                if (deleteMe > 5)
                {
                    TestStatusUpdated?.Invoke("Status_Test1_StopedNormal");
                    currentState = TestState.Idle;
                    deleteMe = 0;
                }
            }
            else if (statusReading.Value == 80)
            {
                deleteMe++;
                if (deleteMe > 5)
                {
                    TestStatusUpdated?.Invoke("Status_Test2_StopedNormal");
                    currentState = TestState.Idle;
                    deleteMe = 0;
                }
            }
            else if (statusReading.Value == 120)
            {
                deleteMe++;
                if (deleteMe > 5)
                {
                    TestStatusUpdated?.Invoke("Status_Test3_StopedNormal");
                    currentState = TestState.Idle;
                    deleteMe = 0;
                }
            }
        }

        // Приватний метод для логіки конкретного тесту
        private async Task RunFirstTestLogic(Device device)
        {
            if (currentState != TestState.Idle) return;
            try
            {
                // 1. Повідомляємо UI (через подію)
                TestStatusUpdated?.Invoke("Status_Test1_Started");
                bool writeResult = await modbusService.WriteRegisterAsync(device, 0, 110);

                if (writeResult)
                {
                    // ВСТАНОВЛЮЄМО СТАН: "Чекаємо на 111"
                    currentState = TestState.Test1;
                }
                else
                {
                    TestStatusUpdated?.Invoke("Status_Test1_WriteFailed");
                    currentState = TestState.Idle;
                }

            }
            catch (Exception ex)
            {
                // !!! Тут має бути логування помилки ex.Message
                TestStatusUpdated?.Invoke("Status_Test_Error");
            }
        }
        private async Task RunSecondTestLogic(Device device)
        {
            if (currentState != TestState.Idle) return;
            try
            {
                // 1. Повідомляємо UI (через подію)
                TestStatusUpdated?.Invoke("Status_Test2_Started");
                bool writeResult = await modbusService.WriteRegisterAsync(device, 0, 120);

                if (writeResult)
                {
                    // ВСТАНОВЛЮЄМО СТАН: "Чекаємо на 111"
                    currentState = TestState.Test2;
                }
                else
                {
                    TestStatusUpdated?.Invoke("Status_Test2_WriteFailed");
                    currentState = TestState.Idle;
                }

            }
            catch (Exception ex)
            {
                // !!! Тут має бути логування помилки ex.Message
                TestStatusUpdated?.Invoke("Status_Test_Error");
            }
        }
        private async Task RunThirdTestLogic(Device device)
        {
            if (currentState != TestState.Idle) return;
            try
            {
                // 1. Повідомляємо UI (через подію)
                TestStatusUpdated?.Invoke("Status_Test3_Started");
                bool writeResult = await modbusService.WriteRegisterAsync(device, 0, 130);

                if (writeResult)
                {
                    // ВСТАНОВЛЮЄМО СТАН: "Чекаємо на 111"
                    currentState = TestState.Test3;
                }
                else
                {
                    TestStatusUpdated?.Invoke("Status_Test3_WriteFailed");
                    currentState = TestState.Idle;
                }

            }
            catch (Exception ex)
            {
                // !!! Тут має бути логування помилки ex.Message
                TestStatusUpdated?.Invoke("Status_Test_Error");
            }
        }
        private async Task RunFullTestLogic(Device device)
        {
            if (currentState != TestState.Idle) return;
            try
            {
                // 1. Повідомляємо UI (через подію)
                TestStatusUpdated?.Invoke("Status_Test1_Started");

                bool writeResult = await modbusService.WriteRegisterAsync(device, 0, 110);
                if (writeResult)
                {
                    writeResult = await modbusService.WriteCoilAsync(device, 13, true);
                }
                else
                {
                    TestStatusUpdated?.Invoke("Status_Test1_WriteFailed");
                    currentState = TestState.Idle;
                    return;
                }


                if (writeResult)
                {
                    // ВСТАНОВЛЮЄМО СТАН: "Чекаємо на 111"
                    currentState = TestState.Test1;
                }
                else
                {
                    TestStatusUpdated?.Invoke("Status_Test1_WriteFailed");
                    currentState = TestState.Idle;
                }

            }
            catch (Exception ex)
            {
                // !!! Тут має бути логування помилки ex.Message
                TestStatusUpdated?.Invoke("Status_Test_Error");
            }
        }
        public void StartTest(TestMode mode, Device deviceToTest)
        {
            this.deviceToTest = deviceToTest;

            switch (mode)
            {
                case TestMode.FirstTest:
                    Task.Run(() => RunFirstTestLogic(deviceToTest));
                    break;
                case TestMode.SecondTest:
                    Task.Run(() => RunSecondTestLogic(deviceToTest));
                    break;
                case TestMode.ThirdTest:
                    Task.Run(() => RunThirdTestLogic(deviceToTest));
                    break;
                case TestMode.FullMode:
                    Task.Run(() => RunFullTestLogic(deviceToTest));
                    break;

                default:
                    return;
            }
        }
    }

}
