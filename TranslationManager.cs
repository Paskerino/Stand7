using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Stand7
{
    public class TranslationManager : INotifyPropertyChanged
    {
        public static TranslationManager Instance { get; } = new TranslationManager();

        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public List<CultureInfo> AvailableLanguages { get; private set; } = new List<CultureInfo>();

        private TranslationManager()
        {
            LoadAvailableLanguages();

            string lang = Properties.Settings.Default.Language;
            if (string.IsNullOrEmpty(lang))
            {
                lang = "uk-UA"; // Мова за замовчуванням
            }
            SetLanguage(new CultureInfo(lang));
        }

        public string this[string key] => _translations.TryGetValue(key, out var value) ? value : $"_{key}_";

        public CultureInfo CurrentLanguage
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (!Thread.CurrentThread.CurrentUICulture.Equals(value))
                {
                    SetLanguage(value);
                    // Повідомляємо UI, що всі прив'язки треба оновити
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
                }
            }
        }

        private void SetLanguage(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentUICulture = culture;
            LoadLanguageFile(culture);
        }

        private void LoadAvailableLanguages()
        {
            try
            {
                string languagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
                if (!Directory.Exists(languagesDir)) return;

                var files = Directory.GetFiles(languagesDir, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string langCode = Path.GetFileNameWithoutExtension(file);
                        AvailableLanguages.Add(new CultureInfo(langCode));
                    }
                    catch (CultureNotFoundException) { /* Ігноруємо */ }
                }
                AvailableLanguages = AvailableLanguages.OrderBy(l => l.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження мов: {ex.Message}");
            }
        }

        private void LoadLanguageFile(CultureInfo culture)
        {
            try
            {
                string langCode = culture.Name;
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", $"{langCode}.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                else
                {
                    _translations.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження файлу мови: {ex.Message}");
                _translations.Clear();
            }
        }
    }
}