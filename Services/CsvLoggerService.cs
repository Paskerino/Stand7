using CommonLogic.Logic.Core.Models;
using CommonLogic.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Stand7.Services
{
    public class CsvLoggerService : IDataLoggerService
    {
        private readonly Channel<SensorLogEntry> _channel;
        private const string Separator = ";";

        public CsvLoggerService()
        {
            // Створюємо канал (чергу)
            _channel = Channel.CreateUnbounded<SensorLogEntry>();
        }

        public void EnqueueData(SensorLogEntry entry)
        {
            _channel.Writer.TryWrite(entry);
        }

        public async Task StartProcessingAsync(string filePath, CancellationToken token)
        {
            bool isNewFile = !File.Exists(filePath);
            FileStream stream = null;
            StreamWriter writer = null;
            bool fileOpened = false;

            // --- ЕТАП 1: Безпечне відкриття файлу ---
            // Пробуємо відкрити файл протягом 2 секунд (10 спроб по 200мс)
            for (int i = 0; i < 10; i++)
            {
                // 1. Якщо під час спроб натиснули "Стоп" - виходимо м'яко
                if (token.IsCancellationRequested) return;

                try
                {
                    // Важливо: FileShare.ReadWrite дозволяє відкривати файл навіть якщо хтось його читає
                    stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true);
                    writer = new StreamWriter(stream);
                    fileOpened = true;
                    break; // Успіх! Виходимо з циклу спроб
                }
                catch (IOException)
                {
                    // Файл зайнятий. Чекаємо і пробуємо знову.
                    // Debug.WriteLine("Файл зайнятий, чекаю...");
                    await Task.Delay(200);
                }
            }

            // Якщо після 10 спроб не вдалося відкрити - виходимо (або логуємо помилку), не крашимо програму
            if (!fileOpened) return;
            using (stream)
            using (writer)
            {
                if (isNewFile)
                {
                    await writer.WriteLineAsync($"Timestamp{Separator}P1{Separator}P2{Separator}P3{Separator}P4{Separator}P5");
                }

                try
                {
                    while (await _channel.Reader.WaitToReadAsync(token))
                    {
                        // Вичитуємо все, що накопичилося в буфері
                        while (_channel.Reader.TryRead(out var entry))
                        {
                            // Форматування рядка
                            var line = string.Format("{0:yyyy-MM-dd HH:mm:ss.f}{1}{2:F2}{1}{3:F2}{1}{4:F2}{1}{5:F2}{1}{6:F2}",
                                entry.Timestamp,
                                Separator,
                                entry.P1,
                                entry.P2,
                                entry.P3,
                                entry.P4,
                                entry.P5);

                            await writer.WriteLineAsync(line);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    await WriteAllAvailableItemsAsync(writer);
                }
                catch (Exception ex)
                {
                    // Логування помилок, якщо потрібно
                    System.Diagnostics.Debug.WriteLine("Error writing to CSV: " + ex.Message);
                }
            }
        }        
    
    private async Task WriteAllAvailableItemsAsync(StreamWriter writer)
        {
            // TryRead не чекає і не блокує - він просто забирає все, що є в пам'яті прямо зараз
            while (_channel.Reader.TryRead(out var entry))
            {
                var line = string.Format("{0:yyyy-MM-dd HH:mm:ss.f}{1}{2:F2}{1}{3:F2}{1}{4:F2}{1}{5:F2}{1}{6:F2}",
                                    entry.Timestamp,
                                    Separator,
                                    entry.P1,
                                    entry.P2,
                                    entry.P3,
                                    entry.P4,
                                    entry.P5);

                await writer.WriteLineAsync(line);
            }
        }
    }
}