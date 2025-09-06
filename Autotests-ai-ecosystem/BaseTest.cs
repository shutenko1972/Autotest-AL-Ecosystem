using Autotests.Reports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;

namespace Autotests
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; }
        protected WebDriverWait Wait { get; private set; }
        protected TestReport Report { get; private set; }
        protected bool TestFailed { get; private set; }

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("=== НАСТРОЙКА НАЧАТА ===");
            TestFailed = false;

            // Инициализация драйвера
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            Driver = new ChromeDriver(options);
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

            // Инициализация отчета с именем теста
            var testName = TestContext.CurrentContext.Test.Name;
            Report = new TestReport(testName);

            Report.AddStep($"Тест '{testName}' начат");
            Console.WriteLine("=== НАСТРОЙКА ЗАВЕРШЕНА ===");
        }

        [TearDown]
        public void Teardown()
        {
            Console.WriteLine("=== ЗАВЕРШЕНИЕ НАЧАТО ===");
            try
            {
                // Сохраняем отчет только если тест провалился
                if (TestFailed)
                {
                    Report.AddStep("Тест провален - сохраняем отчет");
                    Report.SaveReport();
                }
                else
                {
                    Console.WriteLine("Тест пройден - пропускаем генерацию отчета");
                }

                Report.AddStep("Завершение теста выполнено");

                // Закрытие драйвера
                if (Driver != null)
                {
                    Driver.Quit();
                    Driver.Dispose();
                    Console.WriteLine("Драйвер закрыт");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при завершении: {ex.Message}");
            }
            Console.WriteLine("=== ЗАВЕРШЕНИЕ ЗАВЕРШЕНО ===");
        }

        protected void TakeScreenshot(string screenshotName)
        {
            try
            {
                // Делаем скриншоты только для проваленных тестов
                if (!TestFailed)
                {
                    Console.WriteLine("Тест проходит - пропускаем скриншот");
                    return;
                }

                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                var safeName = new string(screenshotName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
                var fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(Report.ReportDirectory, fileName);

                screenshot.SaveAsFile(filePath);
                Report.AddScreenshotInfo(filePath);
                Console.WriteLine($"Скриншот сохранен: {filePath}");
            }
            catch (Exception ex)
            {
                Report.AddWarning($"Не удалось сделать скриншот: {ex.Message}");
                Console.WriteLine($"Ошибка скриншота: {ex.Message}");
            }
        }

        protected void MarkTestAsFailed()
        {
            TestFailed = true;
        }
    }
}