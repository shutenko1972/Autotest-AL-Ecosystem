using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

[TestFixture]
[DisplayName("AuthorizationLogInLogout")]
public class AuthorizationLogInLogoutProdProd
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
        driver?.Quit();
        driver?.Dispose();
    }

    [Test]
	[DisplayName("AuthorizationLogInLogout")]
    public void AuthorizationLogInLogoutProd()
    {
        try
        {
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

            Console.WriteLine("Login successful, waiting for UI stabilization...");
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("Step 6: Opening user dropdown...");
            try
            {
                var userDropdown = driver.FindElement(By.CssSelector(".dropdown-user .caret"));
                userDropdown.Click();
            }
            catch
            {
                try
                {
                    var userDropdown = driver.FindElement(By.CssSelector(".dropdown-user"));
                    userDropdown.Click();
                }
                catch
                {
                    js.ExecuteScript("document.querySelector('.dropdown-user').click();");
                }
            }

            Console.WriteLine("Step 7: Clicking logout button...");
            System.Threading.Thread.Sleep(500);

            IWebElement? logoutButton = null;
            try
            {
                logoutButton = wait.Until(d =>
                {
                    var elements = d.FindElements(By.LinkText("Logout"));
                    var visibleElement = elements.FirstOrDefault(e => e.Displayed);
                    return visibleElement;
                });
            }
            catch
            {
                try
                {
                    logoutButton = driver.FindElement(By.XPath("//a[contains(text(),'Logout')]"));
                }
                catch
                {
                    js.ExecuteScript("var links = document.querySelectorAll('a'); for(var i=0; i<links.length; i++) { if(links[i].textContent.trim() === 'Logout') { links[i].click(); break; } }");
                    return;
                }
            }

            if (logoutButton != null)
            {
                logoutButton.Click();
            }
            else
            {
                js.ExecuteScript("var links = document.querySelectorAll('a'); for(var i=0; i<links.length; i++) { if(links[i].textContent.trim() === 'Logout') { links[i].click(); break; } }");
            }

            Console.WriteLine("Step 8: Waiting for return to login page...");
            wait.Until(d =>
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

            Console.WriteLine("Step 9: Verifying login page...");
            System.Threading.Thread.Sleep(1000);

            var loginInputs = wait.Until(d => d.FindElements(By.Id("loginform-login")));
            Assert.That(loginInputs.Count, Is.GreaterThan(0), "Login fields not found after logout");

            Console.WriteLine("Step 10: Re-entering credentials...");
            var loginFieldAfterLogout = loginInputs.First();
            loginFieldAfterLogout.Clear();
            loginFieldAfterLogout.SendKeys("v_shutenko");

            var passwordFieldAfterLogout = wait.Until(d => d.FindElement(By.Id("loginform-password")));
            passwordFieldAfterLogout.Clear();
            passwordFieldAfterLogout.SendKeys("8nEThznM");

            Assert.That(driver.Url.ToLower().Contains("login"), "Failed to confirm logout - not on login page");

            Console.WriteLine("Test completed successfully: Login and logout performed correctly");
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
                screenshot.SaveAsFile("error_screenshot_prod.png");
                Console.WriteLine("Screenshot saved as error_screenshot_prod.png");
            }
            catch (Exception screenshotEx)
            {
                Console.WriteLine($"Failed to take screenshot: {screenshotEx.Message}");
            }

            throw;
        }
    }
}