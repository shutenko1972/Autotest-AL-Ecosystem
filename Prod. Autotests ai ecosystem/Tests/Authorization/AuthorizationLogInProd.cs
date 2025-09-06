using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Authorization
{
    [TestFixture]
    public class AuthorizationLogInProdProd : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void AuthorizationLogInProd()
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

                Report.AddStep("Шаг 5: Ожидание результата входа...");

                // Ждем либо успешного входа, либо появления ошибки
                bool loginResultDetected = Wait.Until(d =>
                {
                    try
                    {
                        // Проверяем успешный вход - исчезла страница логина и появились элементы dashboard
                        var currentUrl = d.Url.ToLower();
                        bool isNotLoginPage = !currentUrl.Contains("login") &&
                                             !currentUrl.Contains("auth/login");

                        bool hasDashboardElements = d.FindElements(By.CssSelector(".dropdown-user, .user-menu, .dashboard, .main-content"))
                                                   .Any(e => e.Displayed);

                        // Проверяем ошибку входа - остались на странице логина + есть сообщение об ошибке
                        bool isStillOnLoginPage = currentUrl.Contains("login") ||
                                                 currentUrl.Contains("auth/login");

                        bool hasErrorElements = d.FindElements(By.CssSelector(".error, .alert, .text-danger, .login-error, .field-error, [class*='error'], [class*='alert']"))
                                                .Any(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

                        return (isNotLoginPage && hasDashboardElements) || (isStillOnLoginPage && hasErrorElements);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!loginResultDetected)
                {
                    TakeScreenshot("таймаут_входа");
                    throw new WebDriverTimeoutException("Таймаут ожидания результата входа - не обнаружено ни успеха, ни ошибки");
                }

                // Определяем результат входа
                bool isLoginSuccessful = Wait.Until(d =>
                {
                    try
                    {
                        var currentUrl = d.Url.ToLower();
                        bool isNotLoginPage = !currentUrl.Contains("login") &&
                                             !currentUrl.Contains("auth/login");

                        return isNotLoginPage && d.FindElements(By.CssSelector(".dropdown-user, .user-menu"))
                                                 .Any(e => e.Displayed);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!isLoginSuccessful)
                {
                    // Проверяем ошибки авторизации
                    var errorElements = Driver.FindElements(By.CssSelector(".error, .alert, .text-danger, .login-error, .field-error, [class*='error']"))
                                             .Where(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text))
                                             .ToList();

                    if (errorElements.Any())
                    {
                        string errorMessage = string.Join(" | ", errorElements.Select(e => e.Text.Trim()));
                        TakeScreenshot("вход_не_удался_с_ошибкой");
                        throw new Exception($"Вход не удался. Сообщение об ошибке: {errorMessage}");
                    }
                    else
                    {
                        // Проверяем, остались ли мы на странице логина
                        bool isOnLoginPage = Driver.Url.ToLower().Contains("login") &&
                                           Driver.FindElements(By.Id("loginform-login")).Any(e => e.Displayed);

                        if (isOnLoginPage)
                        {
                            TakeScreenshot("вход_не_удался_без_сообщения_об_ошибке");
                            throw new Exception("Вход не удался - остались на странице входа, но нет сообщения об ошибке. Возможные причины: неверные учетные данные, заблокированный аккаунт или проблемы с сервером");
                        }
                        else
                        {
                            TakeScreenshot("неизвестный_результат_входа");
                            throw new Exception($"Вход не удался - неизвестный результат. Текущий URL: {Driver.Url}");
                        }
                    }
                }

                Report.AddSuccess("Вход выполнен успешно!");
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("после_успешного_входа");

                Report.AddSuccess("Тест завершен успешно: вход выполнен корректно");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                MarkTestAsFailed();
                Report.AddError("Произошло исключение таймаута во время авторизации", timeoutEx);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                Report.AddStep($"Длина исходного кода страницы: {Driver.PageSource?.Length ?? 0} символов");
                TakeScreenshot("ошибка_таймаута_авторизации");
                throw;
            }
            catch (Exception ex)
            {
                MarkTestAsFailed();
                Report.AddError("Тест авторизации провален", ex);
                Report.AddStep($"Текущий URL: {Driver.Url}");
                Report.AddStep($"Заголовок страницы: {Driver.Title}");
                TakeScreenshot("общая_ошибка_авторизации");
                throw;
            }
        }
    }
}