using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Stand7
{ 
    public class ModeChooseModel:BaseViewModel
    {
        private readonly Action<bool> _closeAction;

        // Властивість, яку сервіс прочитає ПІСЛЯ закриття вікна
        public TestMode SelectedMode { get; private set; }

        public ICommand SelectModeCommand { get; }
        public ICommand ExitCommand { get; }

        // Конструктор приймає залежність - дію для закриття
        public ModeChooseModel(Action<bool> closeAction)
        {
            _closeAction = closeAction ?? throw new ArgumentNullException(nameof(closeAction));

            // Команда приймає TestMode через CommandParameter
            SelectModeCommand = new RelayCommand(OnSelectMode);
            ExitCommand = new RelayCommand(OnExit);
        }

        private void OnSelectMode(object parameter)
        {
            if (parameter is TestMode mode)
            {
                SelectedMode = mode;
                _closeAction(true);
            }
        }

        private void OnExit(object parameter)
        {
            SelectedMode = TestMode.None;
            _closeAction(false);
        }
    }
}
