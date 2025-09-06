using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace ChatGPT
{
    [TestFixture]
    public class RequestChatGPTClearInputProd : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void RequestChatGPTClearInput()
        {
            try
            {
                Report.AddStep("Шаг 1: Переход на страницу входа...");
                Driver.Navigate().GoToUrl("https://ai-ecosystem.janusww.com/auth/login.html");
                Report.AddUrlInfo(Driver.Url);

                Report.AddStep("Ожидание загрузки страницы входа...");
                Wait.Until(d =>
                {
                    try
                    {
                        return d.FindElement(By.Id("loginform-login")).Displayed &&
                               d.Url.ToLower().Contains("login");
                    }
                    catch
                    {
                        return false;
                    }
                });

                Report.AddStep("Шаг 2: Заполнение поля логина...");
                var loginField = Driver.FindElement(By.Id("loginform-login"));
                loginField.Clear();
                loginField.SendKeys(ValidLogin);

                Report.AddStep("Шаг 3: Заполнение поля пароля...");
                var passwordField = Driver.FindElement(By.Id("loginform-password"));
                passwordField.Clear();
                passwordField.SendKeys(ValidPassword);

                Report.AddStep("Шаг 4: Нажатие кнопки входа...");
                var loginButton = Driver.FindElement(By.CssSelector(".icon-circle-right2"));
                loginButton.Click();

                Report.AddStep("Шаг 5: Ожидание успешного входа...");
                Wait.Until(d => !d.Url.ToLower().Contains("login"));

                Report.AddStep("Шаг 6: Переход на страницу ChatGPT...");
                // Прямая навигация на страницу ChatGPT
                Driver.Navigate().GoToUrl("https://ai-ecosystem.janusww.com/request/model.html");

                Report.AddStep("Ожидание загрузки страницы ChatGPT...");
                Wait.Until(d =>
                {
                    try
                    {
                        return d.FindElement(By.Id("textarea_request")).Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });

                Report.AddStep("Шаг 7: Ввод текста в поле запроса...");
                var textArea = Driver.FindElement(By.Id("textarea_request"));
                textArea.Click();
                textArea.Clear();
                textArea.SendKeys("Привет!");

                // Проверяем, что текст введен корректно
                string enteredText = textArea.GetAttribute("value") ?? string.Empty;
                Assert.That(enteredText, Is.EqualTo("Привет!"), "Текст не был введен корректно");
                Report.AddStep($"Текст введен: {enteredText}");

                Report.AddStep("Шаг 8: Очистка поля ввода...");
                var clearButton = Driver.FindElement(By.Id("clear_request"));
                clearButton.Click();

                Report.AddStep("Шаг 9: Проверка очистки поля...");
                Wait.Until(d =>
                {
                    try
                    {
                        var currentTextArea = d.FindElement(By.Id("textarea_request"));
                        string currentText = currentTextArea.GetAttribute("value") ?? string.Empty;
                        return string.IsNullOrEmpty(currentText);
                    }
                    catch
                    {
                        return false;
                    }
                });

                // Финальная проверка
                var clearedTextArea = Driver.FindElement(By.Id("textarea_request"));
                string clearedText = clearedTextArea.GetAttribute("value") ?? string.Empty;

                Assert.That(string.IsNullOrEmpty(clearedText), Is.True,
                    $"Поле ввода не было очищено. Текущее содержимое: '{clearedText}'");

                Report.AddSuccess("Поле ввода успешно очищено!");
                TakeScreenshot("поле_очищено");

                Report.AddSuccess("Тест завершен успешно: функциональность очистки поля ввода работает корректно");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                MarkTestAsFailed();
                Report.AddError("Произошло исключение таймаута", timeoutEx);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("ошибка_таймаута_очистка");
                throw;
            }
            catch (Exception ex)
            {
                MarkTestAsFailed();
                Report.AddError("Тест очистки поля ввода провален", ex);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("общая_ошибка_очистка");
                throw;
            }
        }
    }
}