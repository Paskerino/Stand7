using CommonLogic.Core.Models.Reports;
using CommonLogic.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7.Services
{
    public class CsvLogReaderService : ILogReaderService
    {
        private const string Separator = ";";
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss.f";

        public async Task<List<ReportRow>> ReadLogAsync(string filePath)
        {
            var result = new List<ReportRow>();

            if (!File.Exists(filePath))
                return result;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                // ConfigureAwait(false) критично важливий для бібліотек/сервісів, щоб уникнути Deadlock
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var entry = ParseLine(line);
                    if (entry != null)
                    {
                        result.Add(entry);
                        // Console.WriteLine прибираємо або робимо рідше, бо він дуже гальмує процес на великих файлах
                    }
                }
            }

            return result;
        }

        private ReportRow ParseLine(string line)
        {
            try
            {
                var parts = line.Split(new[] { Separator }, StringSplitOptions.None);

                // Перевірка на цілісність даних (Time + 5 параметрів = 6 частин)
                if (parts.Length < 6) return null;

                List<ReportCell> cells = new List<ReportCell>();
                if (DateTime.TryParseExact(parts[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    cells.Add(new ReportCell { Value = time.ToString("dd-MM-yyyy HH:mm:ss.f") });
                }
                else
                {
                    // Якщо дата бита, повертаємо null або додаємо як є
                    cells.Add(new ReportCell { Value = parts[0] });
                }

                // 2. Обробка інших колонок
                // ВАЖЛИВО: Створюємо new ReportCell для КОЖНОГО значення
                for (int i = 1; i <= 5; i++)
                {
                    cells.Add(new ReportCell { Value = parts[i] });
                }

                return new ReportRow
                {
                    Cells = cells
                };
            }
            catch (Exception)
            {
                // Логування помилок парсингу можна додати сюди.
                // Повертаємо null, щоб пропустити битий рядок.
                return null;
            }
        }
    }
}
