using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace ChatGPT
{
    [TestFixture]
    public class RequestChatGPTClearInputTest : IDisposable
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        public void Dispose()
        {
            TearDown();
        }

        [Test]
        public void RequestChatGPTClearInput()
        {
            try
            {
                Console.WriteLine("Шаг 1: Переход на страницу входа...");
                driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/auth/login.html");
                Console.WriteLine($"Текущий URL: {driver.Url}");

                Console.WriteLine("Ожидание загрузки страницы входа...");
                wait.Until(drv =>
                {
                    try
                    {
                        return drv.FindElement(By.Id("loginform-login")).Displayed &&
                               drv.Url.ToLower().Contains("login");
                    }
                    catch
                    {
                        return false;
                    }
                });

                Console.WriteLine("Шаг 2: Заполнение поля логина...");
                var loginField = driver.FindElement(By.Id("loginform-login"));
                loginField.Clear();
                loginField.SendKeys(ValidLogin);

                Console.WriteLine("Шаг 3: Заполнение поля пароля...");
                var passwordField = driver.FindElement(By.Id("loginform-password"));
                passwordField.Clear();
                passwordField.SendKeys(ValidPassword);

                Console.WriteLine("Шаг 4: Нажатие кнопки входа...");
                var loginButton = driver.FindElement(By.CssSelector(".icon-circle-right2"));
                loginButton.Click();

                Console.WriteLine("Шаг 5: Ожидание успешного входа...");
                wait.Until(drv => !drv.Url.ToLower().Contains("login"));

                Console.WriteLine("Шаг 6: Переход на страницу ChatGPT...");
                // Прямая навигация на страницу ChatGPT
                driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/request/model.html");

                Console.WriteLine("Ожидание загрузки страницы ChatGPT...");
                wait.Until(drv =>
                {
                    try
                    {
                        return drv.FindElement(By.Id("textarea_request")).Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });

                Console.WriteLine("Шаг 7: Ввод текста в поле запроса...");
                var textArea = driver.FindElement(By.Id("textarea_request"));
                textArea.Click();
                textArea.Clear();
                textArea.SendKeys("Привет!");

                // Проверяем, что текст введен корректно
                string enteredText = textArea.GetAttribute("value");
                Assert.That(enteredText, Is.EqualTo("Привет!"), "Текст не был введен корректно");
                Console.WriteLine($"Текст введен: {enteredText}");

                Console.WriteLine("Шаг 8: Очистка поля ввода...");
                var clearButton = driver.FindElement(By.Id("clear_request"));
                clearButton.Click();

                Console.WriteLine("Шаг 9: Проверка очистки поля...");
                wait.Until(drv =>
                {
                    try
                    {
                        var currentTextArea = drv.FindElement(By.Id("textarea_request"));
                        string currentText = currentTextArea.GetAttribute("value");
                        return string.IsNullOrEmpty(currentText);
                    }
                    catch
                    {
                        return false;
                    }
                });

                // Финальная проверка
                var clearedTextArea = driver.FindElement(By.Id("textarea_request"));
                string clearedText = clearedTextArea.GetAttribute("value");

                Assert.That(string.IsNullOrEmpty(clearedText), Is.True,
                    $"Поле ввода не было очищено. Текущее содержимое: '{clearedText}'");

                Console.WriteLine("Поле ввода успешно очищено!");

                // Сделать скриншот
                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    screenshot.SaveAsFile($"поле_очищено_{timestamp}.png");
                    Console.WriteLine($"Скриншот сохранен: поле_очищено_{timestamp}.png");
                }
                catch (Exception screenshotEx)
                {
                    Console.WriteLine($"Не удалось сделать скриншот: {screenshotEx.Message}");
                }

                Console.WriteLine("Тест завершен успешно: функциональность очистки поля ввода работает корректно");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                Console.WriteLine($"Произошло исключение таймаута: {timeoutEx.Message}");
                Console.WriteLine($"Текущий URL: {driver.Url}");
                Console.WriteLine($"Заголовок страницы: {driver.Title}");

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    screenshot.SaveAsFile($"ошибка_таймаута_очистка_{timestamp}.png");
                    Console.WriteLine($"Скриншот сохранен: ошибка_таймаута_очистка_{timestamp}.png");
                }
                catch (Exception screenshotEx)
                {
                    Console.WriteLine($"Не удалось сделать скриншот: {screenshotEx.Message}");
                }

                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Тест очистки поля ввода провален: {ex.Message}");
                Console.WriteLine($"Текущий URL: {driver.Url}");
                Console.WriteLine($"Заголовок страницы: {driver.Title}");

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    screenshot.SaveAsFile($"общая_ошибка_очистка_{timestamp}.png");
                    Console.WriteLine($"Скриншот сохранен: общая_ошибка_очистка_{timestamp}.png");
                }
                catch (Exception screenshotEx)
                {
                    Console.WriteLine($"Не удалось сделать скриншот: {screenshotEx.Message}");
                }

                throw;
            }
        }
    }
}