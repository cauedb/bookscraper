using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace BookScraper.Infrastructure.Scraping;

public static class SeleniumDriverFactory
{
    public static IWebDriver Create()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        var remoteUrl = Environment.GetEnvironmentVariable("SELENIUM_REMOTE_URL");
        if (!string.IsNullOrEmpty(remoteUrl))
            return new RemoteWebDriver(new Uri(remoteUrl), options);

        return new ChromeDriver(options);
    }
}
