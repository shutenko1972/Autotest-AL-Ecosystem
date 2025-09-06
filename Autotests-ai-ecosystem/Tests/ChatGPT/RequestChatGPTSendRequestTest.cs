using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace ChatGPT
{
    [TestFixture]
    public class RequestChatGPTSendRequestTest : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void RequestChatGPTSendRequest()
        {
            try
            {
                Report.AddStep("Шаг 1: Переход на страницу входа...");
                Driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/auth/login.html");
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
                Driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/request/model.html");

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
                textArea.SendKeys("Привет!");

                // Проверяем, что текст введен корректно
                string enteredText = textArea.GetAttribute("value");
                Assert.That(enteredText, Is.EqualTo("Привет!"), "Текст запроса не был введен корректно");
                Report.AddStep($"Запрос введен: {enteredText}");

                Report.AddStep("Шаг 8: Отправка запроса...");
                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();

                Report.AddStep("Шаг 9: Ожидание ответа...");
                bool responseReceived = false;
                DateTime startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalSeconds < 90 && !responseReceived)
                {
                    try
                    {
                        // Ищем элементы ответа
                        var responseElements = Driver.FindElements(By.CssSelector(
                            ".content, .response, .answer, .message, " +
                            "[class*='response'], [class*='answer'], [class*='message']"
                        ));

                        var responseElement = responseElements
                            .FirstOrDefault(e => e.Displayed &&
                                !string.IsNullOrWhiteSpace(e.Text) &&
                                e.Text.Length > 20 &&
                                !e.Text.Contains("Temperature:"));

                        if (responseElement != null)
                        {
                            Report.AddStep($"Ответ получен: {responseElement.Text.Substring(0, Math.Min(50, responseElement.Text.Length))}...");
                            responseReceived = true;
                            break;
                        }

                        // Проверяем индикаторы загрузки
                        var loadingElements = Driver.FindElements(By.CssSelector(
                            ".ladda-spinner, .loading, .spinner, [class*='loading']"
                        ));

                        bool isLoading = loadingElements.Any(e => e.Displayed);

                        if (!isLoading)
                        {
                            Report.AddStep("Индикаторы загрузки исчезли, предполагаем что ответ получен");
                            responseReceived = true;
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

                if (!responseReceived)
                {
                    TakeScreenshot("ответ_не_получен");
                    throw new Exception("Ответ от ChatGPT не получен в течение 90 секунд");
                }

                Report.AddSuccess("Ответ успешно получен!");
                TakeScreenshot("ответ_получен");

                Report.AddSuccess("Тест завершен успешно: запрос отправлен и ответ получен");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                MarkTestAsFailed();
                Report.AddError("Произошло исключение таймаута", timeoutEx);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("ошибка_таймаута_запрос");
                throw;
            }
            catch (Exception ex)
            {
                MarkTestAsFailed();
                Report.AddError("Тест отправки запроса провален", ex);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("общая_ошибка_запрос");
                throw;
            }
        }
    }
}