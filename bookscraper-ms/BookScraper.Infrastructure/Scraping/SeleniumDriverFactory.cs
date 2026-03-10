using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BookScraper.Infrastructure.Scraping;

public static class SeleniumDriverFactory
{
    private const string DockerChromePath = "/opt/chrome/chrome";
    private const string DockerChromeDriverPath = "/usr/local/bin/chromedriver";

    public static IWebDriver Create()
    {
        var options = new ChromeOptions();

        if (File.Exists(DockerChromePath))
            options.BinaryLocation = DockerChromePath;

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        if (File.Exists(DockerChromeDriverPath))
        {
            var service = ChromeDriverService.CreateDefaultService(
                Path.GetDirectoryName(DockerChromeDriverPath),
                Path.GetFileName(DockerChromeDriverPath));
            return new ChromeDriver(service, options);
        }

        return new ChromeDriver(options);
    }
}
