using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace BookScraper.Scraping;

public static class SeleniumDriverFactory
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(30);

    public static IWebDriver Create()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        IWebDriver driver;

        var remoteUrl = Environment.GetEnvironmentVariable("SELENIUM_REMOTE_URL");
        if (!string.IsNullOrEmpty(remoteUrl))
            driver = new RemoteWebDriver(new Uri(remoteUrl), options.ToCapabilities(), CommandTimeout);
        else
            driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, CommandTimeout);

        driver.Manage().Timeouts().PageLoad = PageLoadTimeout;

        return driver;
    }
}
