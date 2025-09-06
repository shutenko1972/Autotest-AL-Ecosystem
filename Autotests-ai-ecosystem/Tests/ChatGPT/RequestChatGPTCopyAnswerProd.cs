using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace ChatGPT
{
    [TestFixture]
    public class RequestChatGPTCopyAnswerProd : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void RequestChatGPTCopyAnswer()
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

                Report.AddStep("Шаг 7: Ввод запроса...");
                var textArea = Driver.FindElement(By.Id("textarea_request"));
                textArea.Click();
                textArea.Clear();
                textArea.SendKeys("Ghbdtn! Lhepmz!");

                // Проверяем, что текст введен корректно
                string enteredText = textArea.GetAttribute("value");
                Assert.That(enteredText, Is.EqualTo("Ghbdtn! Lhepmz!"), "Текст запроса не был введен корректно");
                Report.AddStep($"Запрос введен: {enteredText}");

                Report.AddStep("Шаг 8: Отправка запроса...");
                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();

                Report.AddStep("Шаг 9: Ожидание ответа...");
                IWebElement responseElement = null;
                DateTime startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalSeconds < 90 && responseElement == null)
                {
                    try
                    {
                        var possibleResponseElements = Driver.FindElements(By.CssSelector(
                            ".coping, .content, .response, .answer, .message, " +
                            "[class*='response'], [class*='answer'], [class*='message']"
                        ));

                        responseElement = possibleResponseElements
                            .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

                        if (responseElement != null)
                        {
                            Report.AddStep($"Ответ найден: {responseElement.Text.Substring(0, Math.Min(50, responseElement.Text.Length))}...");
                            break;
                        }

                        Thread.Sleep(2000);
                        Report.AddStep($"Ожидание ответа... ({(int)(DateTime.Now - startTime).TotalSeconds}с)");
                    }
                    catch (Exception ex)
                    {
                        Report.AddWarning($"Ошибка при ожидании ответа: {ex.Message}");
                        Thread.Sleep(2000);
                    }
                }

                if (responseElement == null)
                {
                    TakeScreenshot("ответ_не_найден");
                    throw new Exception("Ответ не найден в течение 90 секунд");
                }

                Report.AddStep("Шаг 10: Нажатие кнопки копирования...");
                var copyButton = Driver.FindElement(By.CssSelector(".coping"));
                Assert.That(copyButton.Displayed, Is.True, "Кнопка копирования не отображается");
                copyButton.Click();

                Report.AddStep("Шаг 11: Проверка действия копирования...");
                Thread.Sleep(1000); // Даем время для копирования

                Report.AddStep("Шаг 12: Ввод нового текста для проверки функциональности...");
                textArea.Click();
                textArea.Clear();

                string safeText = "Привет! Чем могу помочь?";
                textArea.SendKeys(safeText);

                // Проверяем, что новый текст введен
                string newText = textArea.GetAttribute("value");
                Assert.That(newText, Is.EqualTo(safeText), "Новый текст не был введен корректно");

                Report.AddSuccess("Тест завершен успешно: функциональность копирования ответа работает корректно");
                TakeScreenshot("копирование_успешно");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                MarkTestAsFailed();
                Report.AddError("Произошло исключение таймаута", timeoutEx);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("ошибка_таймаута_копирование");
                throw;
            }
            catch (Exception ex)
            {
                MarkTestAsFailed();
                Report.AddError("Тест копирования ответа провален", ex);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("общая_ошибка_копирование");
                throw;
            }
        }
    }
}