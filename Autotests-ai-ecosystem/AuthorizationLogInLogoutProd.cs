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

[TestFixture]
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
        driver = new ChromeDriver(options);
        js = (IJavaScriptExecutor)driver;
        vars = new Dictionary<string, object>();
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
    }

    [TearDown]
    protected void TearDown()
    {
        driver?.Quit();
        driver?.Dispose();
    }

    [Test]
    public void AuthorizationLogInLogoutProd()
    {
        driver.Navigate().GoToUrl("https://ai-ecosystem.janusww.com/auth/login.html");

        var loginField = wait.Until(d => d.FindElement(By.Id("loginform-login")));
        loginField.Clear();
        loginField.SendKeys("v_shutenko");

        var passwordField = wait.Until(d => d.FindElement(By.Id("loginform-password")));
        passwordField.Clear();
        passwordField.SendKeys("8nEThznM");

        wait.Until(d => d.FindElement(By.CssSelector(".icon-circle-right2"))).Click();

        wait.Until(d => d.FindElement(By.CssSelector(".dropdown-user")));

        Thread.Sleep(1000);

        var userDropdown = wait.Until(d => d.FindElement(By.CssSelector(".dropdown-user .caret")));
        userDropdown.Click();

        Thread.Sleep(500);

        var logoutButton = wait.Until(d => d.FindElement(By.LinkText("Logout")));
        logoutButton.Click();

        wait.Until(d =>
        {
            var currentUrl = d.Url.ToLower();
            return currentUrl.Contains("login") ||
                   d.FindElements(By.Id("loginform-login")).Count > 0;
        });

        var loginInputs = wait.Until(d => d.FindElements(By.Id("loginform-login")));
        Assert.That(loginInputs.Count, Is.GreaterThan(0), "Login fields not found after logout");

        var loginFieldAfterLogout = loginInputs.First();
        loginFieldAfterLogout.Clear();
        loginFieldAfterLogout.SendKeys("v_shutenko");

        var passwordFieldAfterLogout = wait.Until(d => d.FindElement(By.Id("loginform-password")));
        passwordFieldAfterLogout.Clear();
        passwordFieldAfterLogout.SendKeys("8nEThznM");

        Assert.That(driver.Url.ToLower().Contains("login"), "Failed to confirm logout - not on login page");

        Console.WriteLine("Test completed successfully: Login and logout performed correctly");
    }
}