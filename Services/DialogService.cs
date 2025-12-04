using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{ 
    public class DialogService : IDialogService
    {
        public TestMode? ShowModeSelectionDialog()
        {
            var window = new ModeChooseWindow();

            // 1. Створюємо VM, передаючи їй дію для закриття вікна.
            //    VM не знає про 'window', вона просто отримує 'Action<bool>'.
            var vm = new ModeChooseModel((result) =>
            {
                // Цей код буде викликано з VM, коли натиснуто кнопку
                window.DialogResult = result;
                // window.Close() не потрібен, DialogResult сам закриє вікно
            });

            // 2. Встановлюємо DataContext
            window.DataContext = vm;

            // 3. Блокуємо виконання, поки вікно не закриється
            bool? dialogResult = window.ShowDialog();

            // 4. Повертаємо результат
            if (dialogResult == true)
            {
                // Вікно закрите успішно, читаємо результат з VM
                return vm.SelectedMode;
            }
            else
            {
                // Користувач скасував (натиснув "Скасувати" або 'X')
                return null;
            }
        }
    }
}
