using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using PuppeteerSharp;
using System.Net;
using System.Collections.Generic;

namespace Capture
{
    class Program
    {
        public static async Task<string> Capture(string url, string path)
        {
            //return string: path+filename.png
            var currentDirectory = Directory.GetCurrentDirectory();
            var downloadPath = Path.Combine(currentDirectory, "CustomChromium");
            Console.WriteLine($"Attemping to set up puppeteer to use Chromium found under directory {downloadPath} ");

            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine("Custom directory not found. Creating directory");
                Directory.CreateDirectory(downloadPath);
            }

            Console.WriteLine("Downloading Chromium...");

            var browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
            var browserFetcher = new BrowserFetcher(browserFetcherOptions);
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);

            var executablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision);

            if (string.IsNullOrEmpty(executablePath))
            {
                Console.WriteLine("Custom Chromium location is empty. Unable to start Chromium. Exiting.\n Press any key to continue");
                Console.ReadLine();
            }

            Console.WriteLine($"Attemping to start Chromium using executable path: {executablePath}");

            var options = new LaunchOptions { Headless = true, ExecutablePath = executablePath };

            using (var browser = await Puppeteer.LaunchAsync(options))
            using (var page = await browser.NewPageAsync())
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

                var waitUntil = new NavigationOptions { Timeout = 0, WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } };
                await page.GoToAsync(url, waitUntil);

                #region Screenshot Dashboard:
                var optionsScreenShot = new ScreenshotOptions { FullPage = true };
                //Đường dẫn lưu file
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("SavePath directory not found. Creating directory");
                    Directory.CreateDirectory(path);
                }
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");
                var outputfile = path + "/capture_" + date + ".png";
                await page.ScreenshotAsync(outputfile, optionsScreenShot);
                #endregion
                await page.CloseAsync();
                //return string = path+filename.png
                return outputfile;
            }


        }

        public static async Task Main(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var savePath = Path.Combine(currentDirectory, "Capture");

            var filename = await Capture("https://www.w3schools.com/cs/", savePath);
            Console.WriteLine(filename);

        }
    }

}
