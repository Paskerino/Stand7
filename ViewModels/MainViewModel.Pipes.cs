using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
    public partial class MainViewModel
    {

        private void InitializePipes()
        {
            Pipes = new ObservableCollection<PipeViewModel>();
            // ... створюєш всі 30 труб з їх Id і координатами ...
            Pipes.Add(new PipeViewModel { Id = 1, X1 = 651, Y1 = 1995, X2 = 651, Y2 = 1683 });
            Pipes.Add(new PipeViewModel { Id = 2, X1 = 551, Y1 = 1991, X2 = 1672, Y2 = 1991 });

            // 3. Один раз будуємо нашу "книгу"
            BuildModeConfiguration();

            // 4. Встановлюємо початковий стан
            CurrentMode = SchemeMode.Failure_Leak;

            // UpdateAllPipeColors();
        }
        private void UpdateAllPipeColors()
        {
            // 1. Дістаємо конфігурацію для ПОТОЧНОГО режиму
            if (!_modeConfiguration.TryGetValue(CurrentMode, out var colorMap))
            {
                // (можна поставити лог помилки, 
                //  якщо режим не знайдено)
                return;
            }

            // 2. Проходимо по ВСІМ 30 трубам
            foreach (var pipe in Pipes)
            {
                // 3. Перевіряємо: чи є для цієї труби 
                //    запис у конфігурації?
                if (colorMap.TryGetValue(pipe.Id, out var specialColor))
                {
                    // Так? Застосовуємо особливий колір
                    pipe.Color = specialColor;
                }
                else
                {
                    // Ні? Ставимо колір за замовчуванням
                    pipe.Color = _defaultColor;
                }
            }
        }
    }
}
