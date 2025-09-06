using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Autotests.Reports
{
    public class TestReport
    {
        private readonly string _testName;
        private readonly List<string> _steps;
        private readonly StringBuilder _reportContent;
        private bool _testPassed;
        private string _reportDirectory;

        public TestReport(string testName)
        {
            _testName = testName ?? "НеизвестныйТест";
            _steps = new List<string>();
            _reportContent = new StringBuilder();
            _testPassed = true;
            _reportDirectory = GetReportsDirectory();

            EnsureReportDirectoryExists();
            Console.WriteLine($"Директория отчетов: {_reportDirectory}");
        }

        private string GetReportsDirectory()
        {
            try
            {
                // Получаем путь к папке проекта
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var projectDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.FullName;

                // Если не нашли папку проекта, используем директорию исполнения
                if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
                {
                    projectDirectory = baseDirectory;
                }

                var reportsPath = Path.Combine(projectDirectory, "TestReports");
                Console.WriteLine($"Путь отчетов: {reportsPath}");
                return reportsPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения директории отчетов: {ex.Message}");
                return Path.Combine(Directory.GetCurrentDirectory(), "TestReports");
            }
        }

        private void EnsureReportDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_reportDirectory))
                {
                    Directory.CreateDirectory(_reportDirectory);
                    Console.WriteLine($"Создана директория отчетов: {_reportDirectory}");
                }
                else
                {
                    Console.WriteLine($"Директория отчетов уже существует: {_reportDirectory}");
                }

                // Проверяем, что можем писать в директорию
                var testFile = Path.Combine(_reportDirectory, "test_write.access");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                Console.WriteLine($"Директория доступна для записи: {_reportDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось создать директорию отчетов: {ex.Message}");
                // Используем временную директорию как запасной вариант
                _reportDirectory = Path.Combine(Path.GetTempPath(), "TestReports");
                Directory.CreateDirectory(_reportDirectory);
                Console.WriteLine($"Используем временную директорию: {_reportDirectory}");
            }
        }

        public void AddStep(string stepDescription, string status = "ИНФО")
        {
            if (stepDescription == null)
                stepDescription = "[ОПИСАНИЕ ШАГА NULL]";

            if (status == null)
                status = "ИНФО";

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var step = $"[{timestamp}] [{status}] {stepDescription}";
            _steps.Add(step);
            _reportContent.AppendLine(step);

            Console.WriteLine(step);
        }

        public void AddSuccess(string message)
        {
            if (message == null)
                message = "Успешный шаг без сообщения";

            AddStep($"УСПЕХ: {message}", "УСПЕХ");
        }

        public void AddWarning(string message)
        {
            if (message == null)
                message = "Предупреждение без сообщения";

            AddStep($"ПРЕДУПРЕЖДЕНИЕ: {message}", "ПРЕДУПРЕЖДЕНИЕ");
        }

        public void AddError(string message, Exception? exception = null)
        {
            _testPassed = false;

            if (message == null)
                message = "Ошибка без сообщения";

            var errorMessage = $"ОШИБКА: {message}";

            if (exception != null)
            {
                errorMessage += $"\nИсключение: {exception.Message}\nТрассировка стека: {exception.StackTrace}";
            }

            AddStep(errorMessage, "ОШИБКА");
        }

        public void AddUrlInfo(string url)
        {
            if (url == null)
                url = "[NULL URL]";

            AddStep($"URL: {url}", "ИНФО_URL");
        }

        public void AddScreenshotInfo(string screenshotPath)
        {
            if (screenshotPath == null)
                screenshotPath = "[NULL ПУТЬ СКРИНШОТА]";

            AddStep($"Скриншот сохранен: {screenshotPath}", "ИНФО");
        }

        public void SaveReport()
        {
            try
            {
                // Сохраняем отчет только если тест провалился
                if (_testPassed)
                {
                    Console.WriteLine("Тест пройден - пропускаем сохранение отчета");
                    return;
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var status = _testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН";

                var safeTestName = new string(_testName
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                    .ToArray());

                var fileName = $"{safeTestName}_{status}_{timestamp}.txt";
                var filePath = Path.Combine(_reportDirectory, fileName);

                var reportHeader = new StringBuilder();
                reportHeader.AppendLine("=".PadRight(80, '='));
                reportHeader.AppendLine($"ОТЧЕТ О ТЕСТЕ: {_testName}");
                reportHeader.AppendLine($"СТАТУС: {status}");
                reportHeader.AppendLine($"ВРЕМЯ: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                reportHeader.AppendLine($"ПУТЬ К ОТЧЕТУ: {filePath}");
                reportHeader.AppendLine("=".PadRight(80, '='));
                reportHeader.AppendLine();

                var reportFooter = new StringBuilder();
                reportFooter.AppendLine();
                reportFooter.AppendLine("-".PadRight(80, '-'));
                reportFooter.AppendLine($"ВСЕГО ШАГОВ: {_steps.Count}");
                reportFooter.AppendLine($"РЕЗУЛЬТАТ: {(_testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН")}");
                reportFooter.AppendLine($"ОТЧЕТ СОХРАНЕН: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                reportFooter.AppendLine($"ДИРЕКТОРИЯ: {_reportDirectory}");
                reportFooter.AppendLine("-".PadRight(80, '-'));

                var fullReport = reportHeader + _reportContent.ToString() + reportFooter;

                File.WriteAllText(filePath, fullReport);
                Console.WriteLine($"=== ОТЧЕТ СОХРАНЕН В: {filePath} ===");

                // Проверяем, что файл действительно создался
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    Console.WriteLine($"=== РАЗМЕР ФАЙЛА: {fileInfo.Length} байт ===");
                    Console.WriteLine($"=== СОДЕРЖИМОЕ ДИРЕКТОРИИ: {string.Join(", ", Directory.GetFiles(_reportDirectory).Select(Path.GetFileName))} ===");
                }
                else
                {
                    Console.WriteLine($"!!! ФАЙЛ НЕ СОЗДАН: {filePath} !!!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! НЕ УДАЛОСЬ СОХРАНИТЬ ОТЧЕТ: {ex.Message}");
                Console.WriteLine($"!!! ТРАССИРОВКА СТЕКА: {ex.StackTrace}");
            }
        }

        public bool TestPassed => _testPassed;
        public string ReportDirectory => _reportDirectory;
    }
}