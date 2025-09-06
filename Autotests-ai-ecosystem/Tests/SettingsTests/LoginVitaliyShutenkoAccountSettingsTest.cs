using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace SettingsTests
{
    [TestFixture]
    public class LoginVitaliyShutenkoAccountSettingsTest : Autotests.BaseTest
    {
        private const string ValidLogin = "v_shutenko";
        private const string ValidPassword = "8nEThznM";

        [Test]
        public void LoginVitaliyShutenkoAccountSettings()
        {
            try
            {
                // 1. Логин в систему
                Driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/auth/login.html");

                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
                wait.Until(d => d.FindElement(By.Id("loginform-login")).Displayed);

                Driver.FindElement(By.Id("loginform-login")).SendKeys(ValidLogin);
                Driver.FindElement(By.Id("loginform-password")).SendKeys(ValidPassword);
                Driver.FindElement(By.CssSelector(".icon-circle-right2")).Click();

                // Ожидание успешного логина
                wait.Until(d => d.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user")).Any(e => e.Displayed));
                System.Threading.Thread.Sleep(2000);

                // 2. Нажимаем на меню пользователя "Vitaliy Shutenko"
                string urlBeforeClick = Driver.Url;
                Console.WriteLine($"URL до открытия меню: {urlBeforeClick}");

                // Ищем меню пользователя с текстом "Vitaliy Shutenko" или аналогичный элемент
                var userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, [data-toggle='dropdown'], .user-name"))
                    .FirstOrDefault(e => e.Displayed && e.Text.Contains("Vitaliy"));

                if (userMenu == null)
                {
                    // Альтернативный поиск по любому элементу, который может быть меню пользователя
                    userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user"))
                        .FirstOrDefault(e => e.Displayed);
                }

                Assert.That(userMenu, Is.Not.Null, "Меню пользователя 'Vitaliy Shutenko' не найдено");
                userMenu.Click();
                Console.WriteLine("Меню пользователя 'Vitaliy Shutenko' открыто");

                // 3. Ищем кнопку Account Settings в открытом меню
                System.Threading.Thread.Sleep(1000); // Даем время меню раскрыться

                // Ищем кнопку Account Settings
                var accountSettingsButton = Driver.FindElements(By.LinkText("Account settings"))
                    .FirstOrDefault(e => e.Displayed);

                // Если не найдено по точному тексту, ищем по частичному совпадению
                if (accountSettingsButton == null)
                {
                    accountSettingsButton = Driver.FindElements(By.PartialLinkText("Account"))
                        .FirstOrDefault(e => e.Displayed && e.Text.ToLower().Contains("account"));
                }

                Assert.That(accountSettingsButton, Is.Not.Null, "Кнопка 'Account Settings' не найдена в меню");
                Assert.That(accountSettingsButton.Enabled, Is.True, "Кнопка 'Account Settings' не активна");

                // Запоминаем URL перед кликом
                string currentUrl = Driver.Url;
                accountSettingsButton.Click();
                Console.WriteLine("Кнопка 'Account Settings' нажата");

                // 4. Проверка что кнопка сработала - произошел переход
                wait.Until(d => d.Url != currentUrl);
                string urlAfterClick = Driver.Url;
                Console.WriteLine($"URL после нажатия: {urlAfterClick}");

                // Проверяем что переход произошел
                Assert.That(urlAfterClick, Is.Not.EqualTo(currentUrl),
                    "URL не изменился после нажатия кнопки Account Settings");

                Console.WriteLine("ТЕСТ ПРОЙДЕН: Кнопка Account Settings работает корректно!");
                Console.WriteLine($"Перенаправление: {currentUrl} -> {urlAfterClick}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ТЕСТ НЕ ПРОЙДЕН: {ex.Message}");
                Console.WriteLine($"Текущий URL: {Driver.Url}");

                // Дополнительная диагностика
                Console.WriteLine("Попытка найти элементы меню:");
                try
                {
                    var possibleElements = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, [data-toggle='dropdown']"));
                    Console.WriteLine($"Найдено возможных элементов меню: {possibleElements.Count}");
                    foreach (var element in possibleElements)
                    {
                        Console.WriteLine($"Элемент: {element.TagName}, Displayed: {element.Displayed}, Text: {element.Text}");
                    }
                }
                catch (Exception diagEx)
                {
                    Console.WriteLine($"Ошибка диагностики: {diagEx.Message}");
                }

                throw;
            }
        }
    }
}