using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Stand7
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            PopulateLanguages();
            // Встановлюємо поточну дату та час у контроли
            DatePicker.SelectedDate = DateTime.Now;
            TimeTextBox.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        private void PopulateLanguages()
        {
            LanguageComboBox.Items.Clear();
            foreach (var lang in TranslationManager.Instance.AvailableLanguages)
            {
                LanguageComboBox.Items.Add(new ComboBoxItem
                {
                    Content = lang.NativeName,
                    Tag = lang.Name
                });
            }
            string currentLang = TranslationManager.Instance.CurrentLanguage.Name;
            LanguageComboBox.SelectedItem = LanguageComboBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == currentLang);
        }
        private void SaveAndRestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string lang = selectedItem.Tag.ToString();
                    TranslationManager.Instance.CurrentLanguage = new CultureInfo(lang);

                    Properties.Settings.Default.Language = lang;
                    Properties.Settings.Default.Save();
                }
                // Логіка зміни часу
                DateTime selectedDate = DatePicker.SelectedDate ?? DateTime.Today;
                TimeSpan selectedTime = TimeSpan.Parse(TimeTextBox.Text);
                DateTime newDateTime = selectedDate + selectedTime;

                // Встановлюємо системний час
               // SystemService.SetSystemTime(newDateTime);


                // Перезапуск програми для застосування всіх налаштувань
               // Process.Start(Application.ResourceAssembly.Location);
               // Application.Current.Shutdown();
               Close();
            }
            catch (Exception ex)
            {
                throw new Exception("SaveSettings error: " + ex.Message);
            }
        }
        private void TimeTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Створюємо діалог, передаючи йому поточне значення з TextBox
            var dialog = new InputDialog(TimeTextBox.Text);

            // Показуємо діалог і чекаємо на результат
            if (dialog.ShowDialog() == true)
            {
                // Якщо користувач натиснув "OK", оновлюємо текст у TextBox
                TimeTextBox.Text = dialog.ResultText;
            }
        }
    }

    // Допоміжний клас для роботи з системними функціями
    public static class SystemService
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);

        public static void SetSystemTime(DateTime newDateTime)
        {
            DateTime universalTime = newDateTime.ToUniversalTime();
            SYSTEMTIME st = new SYSTEMTIME
            {
                wYear = (short)universalTime.Year,
                wMonth = (short)universalTime.Month,
                wDay = (short)universalTime.Day,
                wHour = (short)universalTime.Hour,
                wMinute = (short)universalTime.Minute,
                wSecond = (short)universalTime.Second,
                wMilliseconds = (short)universalTime.Millisecond
            };

            if (!SetSystemTime(ref st))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}