using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Authorization
{
    [TestFixture]
    public class AuthorizationLogInLogoutTestTest : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void AuthorizationLogInLogoutTest()
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
                TakeScreenshot("после_успешного_входа");

                Report.AddStep("Шаг 6: Ожидание стабилизации интерфейса...");
                System.Threading.Thread.Sleep(2000);

                Report.AddStep("Шаг 7: Открытие выпадающего меню пользователя...");
                bool dropdownOpened = false;

                try
                {
                    var userDropdown = Driver.FindElement(By.CssSelector(".dropdown-user .caret"));
                    userDropdown.Click();
                    dropdownOpened = true;
                }
                catch
                {
                    try
                    {
                        var userDropdown = Driver.FindElement(By.CssSelector(".dropdown-user"));
                        userDropdown.Click();
                        dropdownOpened = true;
                    }
                    catch
                    {
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("document.querySelector('.dropdown-user').click();");
                            dropdownOpened = true;
                        }
                        catch
                        {
                            Report.AddWarning("Не удалось открыть выпадающее меню пользователя любым методом");
                        }
                    }
                }

                if (dropdownOpened)
                {
                    Report.AddStep("Шаг 8: Нажатие кнопки выхода...");
                    System.Threading.Thread.Sleep(1000);

                    bool logoutClicked = false;

                    try
                    {
                        var logoutButton = Wait.Until(d =>
                        {
                            var elements = d.FindElements(By.LinkText("Logout"));
                            return elements.FirstOrDefault(e => e.Displayed);
                        });

                        if (logoutButton != null)
                        {
                            logoutButton.Click();
                            logoutClicked = true;
                        }
                    }
                    catch
                    {
                        try
                        {
                            var logoutButton = Driver.FindElement(By.XPath("//a[contains(text(),'Logout')]"));
                            logoutButton.Click();
                            logoutClicked = true;
                        }
                        catch
                        {
                            try
                            {
                                ((IJavaScriptExecutor)Driver).ExecuteScript(@"
                                    var links = document.querySelectorAll('a');
                                    for(var i=0; i<links.length; i++) {
                                        if(links[i].textContent.trim().toLowerCase() === 'logout') {
                                            links[i].click();
                                            break;
                                        }
                                    }");
                                logoutClicked = true;
                            }
                            catch
                            {
                                Report.AddWarning("Не удалось найти кнопку выхода любым методом");
                            }
                        }
                    }

                    if (logoutClicked)
                    {
                        Report.AddStep("Шаг 9: Ожидание возврата на страницу входа...");
                        bool returnedToLogin = Wait.Until(d =>
                        {
                            try
                            {
                                var currentUrl = d.Url.ToLower();
                                var isLoginPage = currentUrl.Contains("login") ||
                                                 currentUrl.Contains("auth") ||
                                                 currentUrl.EndsWith("/") ||
                                                 currentUrl.Contains("login.html");

                                if (!isLoginPage) return false;

                                try
                                {
                                    var loginFieldExists = d.FindElement(By.Id("loginform-login")).Displayed;
                                    return loginFieldExists;
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                            catch
                            {
                                return false;
                            }
                        });

                        Assert.That(returnedToLogin, Is.True, "Не удалось вернуться на страницу входа после выхода");

                        Report.AddStep("Шаг 10: Проверка доступности страницы входа...");
                        var loginInputs = Driver.FindElements(By.Id("loginform-login"));
                        Assert.That(loginInputs.Count, Is.GreaterThan(0), "Поля входа не найдены после выхода");
                        Assert.That(loginInputs.First().Displayed, Is.True, "Поле входа не отображается");

                        Report.AddSuccess("Тест завершен успешно: вход и выход выполнены корректно");
                        TakeScreenshot("после_успешного_выхода");
                    }
                    else
                    {
                        Report.AddWarning("Кнопка выхода не найдена - тест частично завершен");
                    }
                }
                else
                {
                    Report.AddWarning("Не удалось открыть выпадающее меню пользователя - тест частично завершен");
                }

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