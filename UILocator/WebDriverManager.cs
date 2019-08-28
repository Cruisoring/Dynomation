using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using UILocator.Controls;
using UILocator.Enums;

namespace UILocator
{
    public static class WebDriverManager
    {
        public const string FrameIndicator = "%";
        public static string DefaultFramePath = "";

        public static int DefaultDriverReadyTimeout = 30 * 1000;

        public static bool IEOptionsRequireWindowFocus = true;
        public static bool IEOptionsEnablePersistentHover = true;
        public static string driverName = null;
        public static string browserName = null;
        public static bool GotoMaximizedWindow = false;

        public readonly static ReadyState[] PageLoaded = new ReadyState[] { ReadyState.complete, ReadyState.loaded };

        [ThreadStatic] private static List<string> currentFrames;

        private static List<string> CurrentFrames
        {
            get
            {
                if (currentFrames == null)
                    currentFrames = new List<string>();
                return currentFrames;
            }
        }

        public static string CurrentDriverPath { get { return string.Join(FrameIndicator, CurrentFrames); } }

        [ThreadStatic] private static RemoteWebDriver _driver;
        public static RemoteWebDriver Driver
        {
            get
            {
                return _driver;
            }
            set
            {
                if (_driver != null)
                {
                    _driver.Quit();
                }

                _driver = value;
            }
        }

        [ThreadStatic]
        public static string LastDriverUrl;

        public static string CurrentDriverUrl { get { return Driver.Url; } }

        public static bool Authenticate(string username, string password)
        {
            try
            {
                IAlert alert = Driver.SwitchTo().Alert();
                alert.SetAuthenticationCredentials(username, password);
                return WaitPageReady(30*1000);
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static string UnifiedSwitchPath(string framePath)
        {
            string[] targetFrames = framePath.Split(new char[] { ' ', '>', '$', '%' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(FrameIndicator, targetFrames);
        }

        public static string CurrentSwitchTo(string framePath, int retry = 1)
        {
            string[] targetFrames = framePath.Split(new char[] { ' ', '>', '$', '%' }, StringSplitOptions.RemoveEmptyEntries);

            if (Driver.Url == LastDriverUrl && !string.IsNullOrEmpty(Driver.PageSource))
            {
                if (targetFrames.Count() == CurrentFrames.Count() && targetFrames.SequenceEqual(CurrentFrames))
                    return CurrentDriverPath;
            }
            else
            {
                //Means the page has been reloaded.
                Driver.SwitchTo().Window(Driver.CurrentWindowHandle);
                LastDriverUrl = Driver.Url;
                CurrentFrames.Clear();
            }

            int lastSameIndex = Math.Min(CurrentFrames.Count, targetFrames.Count()) - 1;

            string oldPath = CurrentDriverPath;

            for (int i = lastSameIndex; i >= 0; i--)
            {
                if (CurrentFrames[i] != targetFrames[i])
                {
                    lastSameIndex = i - 1;
                }
            }

            if (lastSameIndex == -1)
            {
                //Switch to the root of the loaded page
                if (CurrentFrames.Count != 0)
                {
                    Driver.SwitchTo().Window(Driver.CurrentWindowHandle);
                    //Driver.SwitchTo().DefaultContent();
                    CurrentFrames.Clear();
                }
            }
            else
            {
                //Otherwise, switch to the first parent frame that potentially contains target frame
                while (CurrentFrames.Count - 1 > lastSameIndex)
                {
                    Driver.SwitchTo().ParentFrame();
                    CurrentFrames.RemoveAt(CurrentFrames.Count - 1);
                }
            }

            for (int i = lastSameIndex + 1; i < targetFrames.Count(); i++)
            {
                string nextFrame = targetFrames[i];
                try
                {
                    Driver.SwitchTo().Frame(nextFrame);
                    CurrentFrames.Add(nextFrame);
                }
                catch (NoSuchFrameException ex)
                {
                    Logger.V("Failed to swith to frame '{0}'.", nextFrame);
                    if (retry > 0)
                    {
                        Driver.SwitchTo().Window(Driver.CurrentWindowHandle);
                        CurrentFrames.Clear();
                        return CurrentSwitchTo(framePath, retry - 1);
                    }
                    Logger.E(ex);

                    Driver.SwitchTo().Window(Driver.CurrentWindowHandle);
                    CurrentFrames.Clear();
                }
            }

            if (oldPath != CurrentDriverPath)
                Logger.V("Path switch from '{0}' to '{1}'.", oldPath, CurrentDriverPath);
            LastDriverUrl = Driver.Url;
            return CurrentDriverPath;
        }

        public static ReadOnlyCollection<IWebElement> BestMatchedElements(By by, string framePath = null, By byOfRoot = null)
        {
            if (!string.IsNullOrEmpty(framePath))
                CurrentSwitchTo(framePath);

            ReadOnlyCollection<IWebElement> result = null;
            IWebElement root = Driver.FindElement(byOfRoot ?? UIContainer.DefaultByOfRoot);
            result = by.FindElements(root);
            if (result.Count() != 0)
                return result;

            IEnumerable<IWebElement> frames = By.CssSelector("frame, iframe").FindElements(Driver);
            foreach (var frame in frames)
            {
                string frameNameOrId = null;
                if (frame.TryGetAttribute("name", out frameNameOrId) || frame.TryGetAttribute("id", out frameNameOrId))
                {
                    result = BestMatchedElements(by, frameNameOrId, byOfRoot);
                    if (result.Count() != 0)
                        return result;
                }
            }
            return result;
        }

        public static void GoToUrl(string url, int? timeout = null)
        {
            Driver.Navigate().GoToUrl(url);
            WebDriverManager.WaitPageReady(timeout ?? DefaultDriverReadyTimeout);
            if (GotoMaximizedWindow)
                Driver.Manage().Window.Maximize();
        }

        public static bool WaitPageLoading(int maxWaitTimeInMillis = -1)
        {
            if (maxWaitTimeInMillis < 0)
                maxWaitTimeInMillis = DefaultDriverReadyTimeout;
            return WaitReadyState(maxWaitTimeInMillis, ReadyState.loading, ReadyState.interactive, ReadyState.unknown);
        }

        public static bool WaitPageReady(int maxWaitTimeInMillis = -1)
        {
            if (maxWaitTimeInMillis < 0)
                maxWaitTimeInMillis = DefaultDriverReadyTimeout;
            return WaitReadyState(maxWaitTimeInMillis, PageLoaded);
        }

        public static bool WaitReadyState(int maxWaitTimeInMillis, params ReadyState[] expected)
        {
            expected = expected ?? PageLoaded;

            var driver = Driver;
            try
            {
                return Executor.Until(() =>
                {
                    ReadyState state = driver.GetReadyState();
                    return expected.Contains(state);
                }, maxWaitTimeInMillis);
            }
            catch (UnhandledAlertException)
            {
                string alertText;
                WebDriverManager.AcceptDialog(out alertText);
                Logger.W("UnhandledAlert accepted: {0}", alertText);
                return WaitPageReady(20 * 1000);
            }
            finally
            {
                if (Driver.Url != LastDriverUrl)
                {
                    Logger.V("Driver Url changed from '{0}' to '{1}'.", LastDriverUrl, Driver.Url);
                    WebDriverManager.CurrentSwitchTo("");
                    LastDriverUrl = Driver.Url;
                }
            }
        }

        public static void Close(bool closeAll = false)
        {
            var driver = Driver;
            try
            {
                if (driver != null)
                {
                    var windows = driver.WindowHandles;
                    Logger.V("There are {0} windows opened by the driver.", windows.Count);

                    if (windows.Count == 0)
                        return;

                    if (closeAll)
                    {
                        foreach (string window in windows)
                        {
                            driver.SwitchTo().Window(window);
                            driver.ExecuteScript("window.onbeforeunload = function(e){};");
                            driver.Close();
                        }
                    }
                    else
                    {
                        driver.ExecuteScript("window.onbeforeunload = function(e){};");
                        driver.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.E(ex);
                Quit();
            }
        }

        public static void Quit()
        {
            var driver = Driver;
            try
            {
                if (driver != null)
                {
                    //Log.V("After Close(), there are still {0} windows opened by the driver.", driver.WindowHandles.Count());
                    driver.Quit();
                    //Log.V("After Quit(), there are {0} windows opened by the driver.", driver.WindowHandles.Count());

                    driver.Dispose();
                    driver = null;
                }
            }
            catch (Exception ex)
            {
                Logger.E(ex);
                driver = null;
            }
            finally
            {
                Executor.KillProcessesByName(driverName);
            }
        }

        public static bool DismissDialog(int times = 10, Action<IAlert> handleAlert = null)
        {
            if (handleAlert == null)
            {
                handleAlert = (alert) => alert.Accept();
            }

            do
            {
                try
                {
                    IAlert alert = WebDriverManager.Driver.SwitchTo().Alert();
                    handleAlert(alert);
                    Thread.Sleep(2000);
                    return true;
                }
                catch// (NoAlertPresentException noAlert)
                {
                    Thread.Sleep(3000);
                    continue;
                }
            } while (times-- > 0);
            return false;
        }

        public static bool AcceptDialog(out string text, int times = 10)
        {
            do
            {
                try
                {
                    IAlert alert = WebDriverManager.Driver.SwitchTo().Alert();
                    text = alert.Text;
                    Logger.V("Alert: {0}", text);
                    alert.Accept();
                    WebDriverManager.Driver.SwitchTo().DefaultContent();
                    return true;
                }
                catch// (NoAlertPresentException noAlert)
                {
                    Thread.Sleep(2000);
                    continue;
                }
            } while (times-- > 0);
            text = null;
            return false;
        }

        public static ReadyState CurrentReadyState()
        {
            return Driver.GetReadyState();
        }

        public const string getReadyStateScript = @"if(document && document.readyState)return document.readyState;
else if(contentDocument && contentDocument.readyState) return contentDocument.readyState;
else if(document && document.parentWindow) return document.parentWindow.document.readyState;
else return 'unknown';";

        public static ReadyState GetReadyState(this RemoteWebDriver driver)
        {
            try
            {
                string stateStr = driver.ExecuteScript(getReadyStateScript).ToString();
                ReadyState result = ReadyState.unknown;
                Enum.TryParse<ReadyState>(stateStr, out result);
                return result;
            }
            catch (UnhandledAlertException)
            {
                return ReadyState.loaded;
            }
            catch (Exception ex)
            {
                Logger.E(ex);
                string temp = driver.ExecuteScript("return frames;").ToString();
                if (temp != "undefined")
                {
                    temp = driver.ExecuteScript("return frames[0].outerHTML;").ToString();
                    Logger.D(temp);
                }
                else
                {
                    temp = driver.ExecuteScript("return frames;").ToString();
                    Logger.D(temp);
                }
                return ReadyState.unknown;
            }
        }

        public static bool TryGetAttribute(this IWebElement elem, string attributeName, out string attributeValue)
        {
            try
            {
                if (elem == null)
                    throw new ArgumentNullException("Element cannot be null!");
                if (string.IsNullOrWhiteSpace(attributeName))
                    throw new ArgumentException("Attribute name cannot be null or empty!");

                attributeValue = elem.GetAttribute(attributeName.Trim());
                return true;
            }
            catch (Exception ex)
            {
                attributeValue = null;
                if (ex is StaleElementReferenceException || ex is ArgumentException)
                {
                    Logger.W(ex.Message);
                    return false;
                }
                return true;
            }
        }

        public static void SpawnCredential(string domain, string username, string password)
        {
            var info = new System.Diagnostics.ProcessStartInfo(@"C:\Windows\System32\cmdkey.exe")
            {
                Arguments = string.Format("/add:{0} /user:{1} /pass:{2}", domain, username, password),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = System.Diagnostics.Process.Start(info);
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds))
            {
                process.Kill();
                throw new Exception("Failed to save login in Windows Credential Store.");
            }
        }

    }


}
