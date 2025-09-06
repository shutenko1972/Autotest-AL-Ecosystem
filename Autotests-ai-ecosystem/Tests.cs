using Autotests;
using NUnit.Framework;
using OpenQA.Selenium;
using System;

namespace ABAutotestsExample
{
    [TestFixture]
    public class SimpleTest : BaseTest
    {
        [Test]
        public void TestReportGeneration()
        {
            try
            {
                Report.AddStep("Начинаем простой тест...");
                Report.AddStep($"Директория отчетов: {Report.ReportDirectory}");

                // Простая проверка
                Driver.Navigate().GoToUrl("https://www.google.com");
                Report.AddUrlInfo(Driver.Url);

                var title = Driver.Title;
                Report.AddStep($"Заголовок страницы: {title}");

                Assert.That(title, Contains.Substring("Google"), "Должна быть страница Google");

                Report.AddSuccess("Простой тест пройден");
            }
            catch (Exception ex)
            {
                MarkTestAsFailed(); // Помечаем тест как проваленный
                Report.AddError("Тест провален", ex);
                TakeScreenshot("Произошла ошибка");
                throw;
            }
        }
    }
}