using System.Windows;

namespace Stand7
{
    public partial class InputDialog : Window
    {
        public string ResultText { get; private set; }

        public InputDialog(string initialValue = "")
        {
            InitializeComponent();
            KeypadControl.SetInitialText(initialValue);

            // Підписуємося на події від нашого UserControl'а
            KeypadControl.EnterClicked += KeypadControl_OkClicked;
            KeypadControl.CancelClicked += KeypadControl_CancelClicked;
        }

        private void KeypadControl_OkClicked(object sender, string value)
        {
            this.ResultText = value;
            this.DialogResult = true; // Закриваємо вікно з результатом "успіх"
        }

        private void KeypadControl_CancelClicked(object sender, System.EventArgs e)
        {
            this.DialogResult = false; // Закриваємо вікно з результатом "скасовано"
        }
    }
}