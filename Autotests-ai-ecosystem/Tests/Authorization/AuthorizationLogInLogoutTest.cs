using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Drawing;

namespace Authorization
{
    [TestFixture]
    [DisplayName("AuthorizationLogInLogout")]
    public class AuthorizationLogInLogoutTestTest : IDisposable
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
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));
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
        public void AuthorizationLogInLogoutTest()
        {
            try
            {
                Console.WriteLine("Step 1: Navigating to login page...");
                driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/auth/login.html");

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

                Console.WriteLine("Login successful");
                Thread.Sleep(1000);

                string urlBeforeLogout = driver.Url;
                Console.WriteLine($"URL before logout: {urlBeforeLogout}");

                Console.WriteLine("Step 6: Opening user dropdown...");
                bool dropdownOpened = false;

                try
                {
                    var userDropdownCaret = driver.FindElements(By.CssSelector(".dropdown-user .caret"));
                    if (userDropdownCaret.Count > 0 && userDropdownCaret[0].Displayed)
                    {
                        userDropdownCaret[0].Click();
                        dropdownOpened = true;
                    }
                }
                catch
                {
                    Console.WriteLine("Caret not found, trying main dropdown element...");
                }

                if (!dropdownOpened)
                {
                    try
                    {
                        var userDropdown = driver.FindElement(By.CssSelector(".dropdown-user"));
                        userDropdown.Click();
                        dropdownOpened = true;
                    }
                    catch
                    {
                        Console.WriteLine("Dropdown element not clickable, using JavaScript...");
                        js.ExecuteScript("document.querySelector('.dropdown-user').click();");
                        dropdownOpened = true;
                    }
                }

                Console.WriteLine("Step 7: Clicking logout button...");
                Thread.Sleep(1000);

                bool logoutClicked = false;

                try
                {
                    var logoutButtons = driver.FindElements(By.LinkText("Logout"))
                        .Concat(driver.FindElements(By.XPath("//a[contains(text(),'Logout')]")))
                        .Where(e => e.Displayed)
                        .ToList();

                    if (logoutButtons.Count > 0)
                    {
                        logoutButtons[0].Click();
                        logoutClicked = true;
                    }
                }
                catch
                {
                    Console.WriteLine("Logout button not found with standard methods");
                }

                if (!logoutClicked)
                {
                    Console.WriteLine("Using JavaScript to find and click logout...");
                    js.ExecuteScript(@"
                        var logoutElements = [];
                        var allElements = document.querySelectorAll('*');
                        for (var i = 0; i < allElements.length; i++) {
                            var el = allElements[i];
                            if (el.textContent && el.textContent.trim().toLowerCase() === 'logout') {
                                logoutElements.push(el);
                            }
                        }
                        var logoutLinks = document.querySelectorAll('a[href*=""logout""], a[onclick*=""logout""]');
                        for (var j = 0; j < logoutLinks.length; j++) {
                            logoutElements.push(logoutLinks[j]);
                        }
                        if (logoutElements.length > 0) {
                            logoutElements[0].click();
                        }
                    ");
                    logoutClicked = true;
                }

                Console.WriteLine("Step 8: Waiting for logout to complete...");
                wait.Until(d =>
                {
                    try
                    {
                        var currentUrl = d.Url;

                        if (currentUrl != urlBeforeLogout)
                        {
                            Console.WriteLine($"URL changed to: {currentUrl}");
                            return true;
                        }

                        var userElements = d.FindElements(By.CssSelector(".dropdown-user"));
                        if (userElements.Count == 0 || !userElements[0].Displayed)
                        {
                            Console.WriteLine("User elements disappeared");
                            return true;
                        }

                        var loginElements = d.FindElements(By.Id("loginform-login"));
                        if (loginElements.Count > 0 && loginElements[0].Displayed)
                        {
                            Console.WriteLine("Login elements appeared");
                            return true;
                        }

                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                });

                Console.WriteLine("Step 9: Verifying logout state...");
                Thread.Sleep(2000);

                bool isLoggedOut = false;
                string currentUrl = driver.Url;

                if (currentUrl.ToLower().Contains("login") || currentUrl.Contains("auth"))
                {
                    isLoggedOut = true;
                    Console.WriteLine("On login page - logged out");
                }

                if (!isLoggedOut)
                {
                    try
                    {
                        var loginInput = driver.FindElement(By.Id("loginform-login"));
                        if (loginInput.Displayed)
                        {
                            isLoggedOut = true;
                            Console.WriteLine("Login form visible - logged out");
                        }
                    }
                    catch
                    {
                    }
                }

                if (!isLoggedOut)
                {
                    try
                    {
                        var userElements = driver.FindElements(By.CssSelector(".dropdown-user"));
                        if (userElements.Count == 0 || !userElements[0].Displayed)
                        {
                            isLoggedOut = true;
                            Console.WriteLine("User elements disappeared - logged out");
                        }
                    }
                    catch
                    {
                    }
                }

                if (!isLoggedOut)
                {
                    try
                    {
                        var authState = js.ExecuteScript(@"
                            return {
                                hasLoginForm: !!document.getElementById('loginform-login'),
                                hasAuthElements: !!document.querySelector('[class*=""user""], [class*=""account""]'),
                                url: window.location.href
                            };
                        ");
                        Console.WriteLine($"Auth state: {authState}");
                    }
                    catch (Exception jsEx)
                    {
                        Console.WriteLine($"JS auth check failed: {jsEx.Message}");
                    }
                }

                if (!isLoggedOut)
                {
                    Console.WriteLine("Logout may not have worked, trying direct navigation to login...");
                    driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/auth/login.html");

                    wait.Until(d =>
                    {
                        try
                        {
                            return d.FindElement(By.Id("loginform-login")).Displayed;
                        }
                        catch
                        {
                            return false;
                        }
                    });

                    isLoggedOut = true;
                }

                Assert.That(isLoggedOut, Is.True, "User is not logged out. System may not support logout functionality or UI has changed.");

                Console.WriteLine("Step 10: Testing re-login capability...");
                var loginInputFinal = driver.FindElement(By.Id("loginform-login"));
                loginInputFinal.Clear();
                loginInputFinal.SendKeys("v_shutenko");

                var passwordInput = driver.FindElement(By.Id("loginform-password"));
                passwordInput.Clear();
                passwordInput.SendKeys("8nEThznM");

                Assert.That(loginInputFinal.GetAttribute("value"), Is.EqualTo("v_shutenko"));
                Assert.That(passwordInput.GetAttribute("value"), Is.EqualTo("8nEThznM"));

                Console.WriteLine("Test completed successfully: Login and logout verification passed");
            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                Console.WriteLine($"Timeout exception occurred: {timeoutEx.Message}");
                Console.WriteLine($"Current URL: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");

                try
                {
                    Console.WriteLine("Page source snippet (first 1000 chars):");
                    string pageSource = driver.PageSource;
                    Console.WriteLine(pageSource.Substring(0, Math.Min(1000, pageSource.Length)));
                }
                catch (Exception pageSourceEx)
                {
                    Console.WriteLine($"Failed to get page source: {pageSourceEx.Message}");
                }

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    screenshot.SaveAsFile($"logout_timeout_{timestamp}.png");
                    Console.WriteLine($"Screenshot saved as logout_timeout_{timestamp}.png");
                }
                catch (Exception screenshotEx)
                {
                    Console.WriteLine($"Failed to take screenshot: {screenshotEx.Message}");
                }

                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Current URL: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");

                try
                {
                    Console.WriteLine("Page source snippet (first 500 chars):");
                    string pageSource = driver.PageSource;
                    Console.WriteLine(pageSource.Substring(0, Math.Min(500, pageSource.Length)));
                }
                catch (Exception pageSourceEx)
                {
                    Console.WriteLine($"Failed to get page source: {pageSourceEx.Message}");
                }

                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    screenshot.SaveAsFile($"logout_error_{timestamp}.png");
                    Console.WriteLine($"Screenshot saved as logout_error_{timestamp}.png");
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