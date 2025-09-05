using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using NUnit.Framework;

namespace AutotestsAiEcosystem.Tests
{
    [TestFixture]
    public class AuthorizationLogInProdProd
    {
        private IWebDriver driver;
        public IDictionary<string, object> vars { get; private set; }
        private IJavaScriptExecutor js;
        private WebDriverWait wait;

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");
            driver = new ChromeDriver(options);
            js = (IJavaScriptExecutor)driver;
            vars = new Dictionary<string, object>();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        [TearDown]
        protected void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }

        [Test]
        public void AuthorizationLogInProd()
        {
            try
            {
                if (driver == null)
                {
                    throw new InvalidOperationException("WebDriver is not initialized");
                }

                Console.WriteLine("Step 1: Navigating to login page...");
                driver.Navigate().GoToUrl("https://ai-ecosystem.janusww.com/auth/login.html");

                Console.WriteLine("Waiting for login page to load...");
                wait.Until(d =>
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

                Console.WriteLine("Step 2: Filling login field...");
                var loginField = driver.FindElement(By.Id("loginform-login"));
                loginField.Clear();
                loginField.SendKeys("v_shutenko");

                Console.WriteLine("Step 3: Filling password field...");
                var passwordField = driver.FindElement(By.Id("loginform-password"));
                passwordField.Clear();
                passwordField.SendKeys("8nEThznM");

                Console.WriteLine("Step 4: Clicking login button...");
                var loginButton = driver.FindElement(By.CssSelector(".icon-circle-right2"));
                loginButton.Click();

                Console.WriteLine("Step 5: Waiting for successful login...");
                wait.Until(d => !d.Url.ToLower().Contains("login"));

                wait.Until(d =>
                {
                    try
                    {
                        return d.FindElement(By.CssSelector(".dropdown-user")).Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });

                Console.WriteLine("Login successful!");
                Console.WriteLine($"Current URL: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");

                Console.WriteLine("Test completed successfully: Login performed correctly");

            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                Console.WriteLine($"Timeout exception occurred: {timeoutEx.Message}");
                Console.WriteLine($"Current URL: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");

                try
                {
                    Console.WriteLine("Page source snippet:");
                    Console.WriteLine(driver.PageSource.Substring(0, 500));
                }
                catch { }

                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Current URL: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    screenshot.SaveAsFile("error_screenshot_login_prod.png");
                    Console.WriteLine("Screenshot saved as error_screenshot_login_prod.png");
                }
                catch (Exception screenshotEx)
                {
                    Console.WriteLine($"Failed to take screenshot: {screenshotEx.Message}");
                }

                throw;
            }
        }
    }
}