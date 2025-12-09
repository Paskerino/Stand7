using CommonLogic.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
    
        public interface ITestExecutionService
        {
            // Подія, на яку підпишеться ViewModel для оновлення статусу
            event Action<SensorReading> TestStatusUpdated;

            // Подія про завершення (можна додати пізніше)
            // event Action<TestResult> TestCompleted; 

            // Метод для запуску тесту
            void StartTest(TestMode mode, Device deviceToTest);

            // (на майбутнє можна додати StopTest() тощо)
        }
    
}
