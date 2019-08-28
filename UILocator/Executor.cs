using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using UILocator.Controls;
using UILocator.Interfaces;

namespace UILocator
{
    public static class Executor
    {
        public const int DefaultWaitPageLoadingTimeMillis = 60 * 1000; // 60 seconds

        public const int DefaultWaitReadyTimeMillis = 60 * 1000; // 60 seconds

        public static int DefaultWebDriverWaitTimeout = DefaultWaitReadyTimeMillis;
        public static bool InCompabilityMode = false;

        //Mininum time in mSec waited before execution
        public const int MinIntervalMills = 1000;
        //Maximum time in mSec waited before next execution
        public const int MaxIntervalMills = 5000;

        //Maxium time to be waited before timeout
        public const int DefaultTimeoutInMills = 10 * 1000;
        public static int DefaultElementOperationTimeout = 10 * 1000;

        public static bool Until(Func<bool> predicate, int timeoutMills = -1)
        {
            if (timeoutMills <= 0)
                timeoutMills = DefaultElementOperationTimeout;

            //Get the moment when the execution is considered to be timeout
            DateTime timeoutMoment = DateTime.Now + TimeSpan.FromMilliseconds(timeoutMills);

            int interval = 1000;
            Exception lastException = null;

            do
            {
                try
                {
                    //If something happen as expected, return immediately and ignore the previous exception
                    if (predicate())
                        return true;
                }
                catch (Exception ex)
                {
                    // Intentionally record only the last Exception due to the fact that usually it is due to same reason
                    lastException = ex;
                }

                //Waiting for a period before execution codes within predicate()
                System.Threading.Thread.Sleep(interval);

                //The waiting time is extended, but no more than that defined by MaxIntervalMills
                interval = Math.Min(interval * 2, 1000);

            } while (DateTime.Now < timeoutMoment);

            //Exected only when timeout before expected event happen

            return false;
        }

        public static T TryGet<T>(Func<T> valueGetter, int timeoutMills = -1, Func<T, bool> valueEvaluator = null)
        {
            if (timeoutMills <= 0)
                timeoutMills = DefaultElementOperationTimeout;

            //Get the moment when the execution is considered to be timeout
            DateTime timeoutMoment = DateTime.Now + TimeSpan.FromMilliseconds(timeoutMills);

            int interval = 1000;
            Exception lastException = null;

            T result = default(T);
            if (valueEvaluator == null)
            {
                valueEvaluator = t => t != null;
            }
            do
            {
                try
                {
                    result = valueGetter.Invoke();

                    //If something happen as expected, return immediately and ignore the previous exception
                    if (valueEvaluator.Invoke(result))
                        return result;
                }
                catch (Exception ex)
                {
                    // Intentionally record only the last Exception due to the fact that usually it is due to same reason
                    lastException = ex;
                }

                //Waiting for a period before execution codes within predicate()
                System.Threading.Thread.Sleep(interval);

                //The waiting time is extended, but no more than that defined by MaxIntervalMills
                interval = Math.Min(interval * 2, 1000);

            } while (DateTime.Now < timeoutMoment);

            //Exected only when timeout before expected event happen

            return result;
        }
        

        public static bool ActionUntil(Action action, Func<bool> expectedCondition,
            int timeoutMills = DefaultTimeoutInMills, int intervalMills = MinIntervalMills)
        {
            DateTime until = DateTime.Now.AddMilliseconds(timeoutMills);
            do
            {
                action.Invoke();

                if (expectedCondition.Invoke())
                {
                    return true;
                }

                System.Threading.Thread.Sleep(intervalMills);
            } while (DateTime.Now < until);

            return false;
        }

        public static bool Try(Func<bool> evaluateFunc, Action action=null, int times=3)
        {
            while (times-- > 0)
            {
                try
                {
                    action?.Invoke();
                    if (evaluateFunc.Invoke())
                    {
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }

        public static Action<IWebElement> ClickOnParentWhenNotVisible = parent => {
            Point point = (parent as ILocatable).LocationOnScreenOnceScrolledIntoView;
            parent.Click();
        };

        public static string CssSelectorOfNthOfType(this string tagName, int indexFromZero)
        {
            if (!InCompabilityMode)
            {
                return $"{tagName}:nth-of-type({indexFromZero + 1})";
            }

            if (indexFromZero == 0)
            {
                return tagName;
            }

            StringBuilder sb = new StringBuilder(tagName).Insert(0, $"{tagName}+", indexFromZero);
            return sb.ToString();
        }

        public static bool IsWithin(this IWebElement target, IWebElement container)
        {
            int targetLeft = target.Location.X;
            int targetTop = target.Location.Y;
            int containerLeft = container.Location.X;
            int containerTop = container.Location.Y;

            if (targetLeft < containerLeft || targetTop < containerTop)
            {
                return false;
            }
            if (targetLeft + target.Size.Width > containerLeft + container.Size.Width ||
                targetTop + target.Size.Height > containerTop + container.Size.Height)
            {
                return false;
            }
            return true;
        }

        public static bool IsYOverlap(this IWebElement target, IWebElement container)
        {
            int targetTop = target.Location.Y;
            int containerTop = container.Location.Y;
            int targetBottom = targetTop + target.Size.Height;
            int containerBottom = containerTop + container.Size.Height;

            return (targetTop >= containerTop) && (targetBottom <= containerBottom);
        }

        public static By AsContainer(this By containerBy, By childBy)
        {
            string childCss = childBy.ToString();
            if (!childCss.Contains("Css"))
            {
                throw new ArgumentException($"{childCss} is not a CSS Selector.");
            }

            childCss = childCss.Substring(childCss.IndexOf(':') + 1).TrimStart();
            return containerBy.AsContainer(childCss);
        }

        public static By AsContainer(this By containerBy, string childCss)
        {
            string containerCss = containerBy.ToString();
            if (!containerCss.Contains("Css"))
                throw new ArgumentException(containerCss + " is not a CSS Selector.");

            containerCss = containerCss.Substring(containerCss.IndexOf(':') + 1).TrimStart();
            string combinedCss = string.Format("{0} {1}", containerCss, childCss);
            return By.CssSelector(combinedCss);
        }

        public static string outerHTML(this IWebElement element)
        {
            try
            {
                string result = element.GetAttribute("outerHTML");
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static void ClickByScript(this RemoteWebElement element)
        {
            WebDriverManager.Driver.ExecuteScript("arguments[0].click();", element);
        }

        public static void PerformClick(this IUIControl iuiControl)
        {
            Logger.V("Click on {0}", iuiControl);
            iuiControl.PerformAction(() => iuiControl.Click());
        }

        public static void PerformSendEnter(this IUIControl iuiControl)
        {
            Logger.V("Send {ENTER} on {0}", iuiControl);
            iuiControl.PerformAction(() => iuiControl.Element.SendKeys(Keys.Enter));
        }

        public static bool KillProcessesByName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return false;

            var process = Process.GetProcessesByName(processName);

            if (process.Length > 0)
            {
                Logger.V("There are {0} process(es) of {1}", process.Length, processName);
                foreach (var p in process)
                {
                    try
                    {
                        long memoryUsed = p.PrivateMemorySize64;
                        int inMegaBytes = (int)memoryUsed / (1024 * 1024);
                        Logger.V("Memory used by {0}: {1}M.", processName, inMegaBytes);
                        p.Kill();
                        return true;
                    }
                    catch
                    { }
                }
            }

            process = Process.GetProcessesByName(processName);
            return process.Length == 0;
        }

        public static readonly char[] AccentChars =
            "àèìòùÀÈÌÒÙäëïöüÄËÏÖÜâêîôûÂÊÎÔÛáéíóúÁÉÍÓÚðÐýÝãñõÃÑÕšŠžŽçÇåÅøØ-".ToCharArray();
        public static readonly char[] NormalChars =
            "aeiouAEIOUaeiouAEIOUaeiouAEIOUaeiouAEIOUdDyYanoANOsSzZcCaAoO ".ToCharArray();

        public static string ReplaceAccent(this string str)
        {
            int index = 0;
            do
            {
                index = str.IndexOfAny(AccentChars, index);
                if (index == -1)
                    return str;
                char accent = str[index];
                str = str.Replace(accent.ToString(), NormalChars[Array.IndexOf(AccentChars, accent)].ToString());
            } while (index != -1);
            return str;
        }
        public static bool ContainsIgnoreCase(this string content, object key)
        {
            string normalString = content.ReplaceAccent();
            if (key is DateTime)
            {
                DateTime date = (DateTime)key;
                return content.Contains(date.ToString("d-MMM-yyyy")) || content.Contains(date.ToShortDateString())
                    || content.Contains(date.ToShortDateString());
            }
            else if (key is String)
            {
                return content.IndexOf(key.ToString().ReplaceAccent().Trim(), StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            return content.IndexOf(key.ToString().TrimStart('0'), StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private static readonly string neglectableSelector = @"[\W]";

        public static bool SimilarTo(this string expected, string actual)
        {
            string s1 = Regex.Replace(expected, neglectableSelector, string.Empty);
            string s2 = Regex.Replace(actual, neglectableSelector, string.Empty);
            return s1.Length >= s2.Length ? s1.ContainsIgnoreCase(s2) : s2.ContainsIgnoreCase(s1);
        }

        private static string lastContent;
        public static int IndexOfMissing(this string content, params object[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                object k = keys[i];
                if (k is decimal)
                {
                    decimal amount = (decimal)k;
                    if (!content.Contains(amount.ToString("$#,##0.00")) && !content.Contains(amount.ToString("#,##0.00")) && !content.Contains(amount.ToString()))
                        return i;
                }
                else if (k is DateTime)
                {
                    DateTime date = (DateTime)k;
                    if (!content.Contains(date.ToString("d/MM/yyyy")) && !content.Contains(date.ToString("d-MMM-yyyy")))
                        return i;
                }
                else
                {
                    if (content.IndexOf(k.ToString().TrimStart('0'), StringComparison.InvariantCultureIgnoreCase) < 0)
                        return i;
                }
            }
            return -1;
        }

        public static bool ContainsAll(this string content, params object[] keys)
        {
            foreach (object k in keys)
            {
                if (k is decimal)
                {
                    decimal amount = (decimal)k;
                    if (!content.Contains(amount.ToString("$#,##0.00")) && !content.Contains(amount.ToString("#,##0.00")) && !content.Contains(amount.ToString()))
                    {
                        if (lastContent != content)
                        {
                            lastContent = content;
                        }
                        return false;
                    }
                }
                else if (k is DateTime)
                {
                    DateTime date = (DateTime)k;
                    if (!content.Contains(date.ToString("d/MM/yyyy"))
                        && !content.Contains(date.ToString("d-MMM-yyyy")))
                    {
                        if (lastContent != content)
                        {
                            lastContent = content;
                        }
                        return false;
                    }
                }
                else
                {
                    if (content.IndexOf(k.ToString().TrimStart('0'), StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        if (lastContent != content)
                        {
                            lastContent = content;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool ContainsAny(this string content, params object[] keys)
        {
            foreach (object k in keys)
            {
                if (k is decimal)
                {
                    decimal amount = (decimal)k;
                    if (content.Contains(amount.ToString("$#,##0.00")) || content.Contains(amount.ToString("#,##0.00")) || content.Contains(amount.ToString()))
                    {
                        return true;
                    }
                }
                else if (k is DateTime)
                {
                    DateTime date = (DateTime)k;
                    if (content.Contains(date.ToString("d/MM/yyyy")) || content.Contains(date.ToString("d-MMM-yyyy")))
                    {
                        return true;
                    }
                }
                else
                {
                    if (content.IndexOf(k.ToString().TrimStart('0'), StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

//        public static byte[] ToByteArray(this MediaTypeNames.Image image, ImageFormat format = null)
//        {
//            if (image == null) return null;
//            format = format ?? ImageFormat.Bmp;
//            using (MemoryStream ms = new MemoryStream())
//            {
//                image.Save(ms, format);
//                return ms.ToArray();
//            }
//        }
//
//        public const PixelFormat DefaultPixelFormat = PixelFormat.Format24bppRgb;
//        public static byte[] AsBytes(this Bitmap bmp, Rectangle rect, PixelFormat format = DefaultPixelFormat)
//        {
//            if (bmp == null) return null;
//            //TODO: checking rect?
//
//            byte[] bytes = null;
//            BitmapData bmpData = null;
//            try
//            {
//                bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, format);
//
//                // Get the address of the first line.
//                IntPtr ptr = bmpData.Scan0;
//
//                // Declare an array to hold the bytes of the bitmap. 
//                bytes = new byte[bmpData.Stride * bmpData.Height];
//
//                // Copy the RGB values into the array.
//                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, bytes.Length);
//            }
//            finally
//            {
//                if (bmpData != null)
//                    bmp.UnlockBits(bmpData);
//            }
//
//            return bytes;
//        }
//        public static byte[] AsBytes(this Bitmap bmp, PixelFormat format = DefaultPixelFormat)
//        {
//            if (bmp == null) return null;
//
//            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
//            return AsBytes(bmp, rect, format);
//        }
//
//        public static Bitmap FromBytes(this byte[] bytes, int width, int height, PixelFormat format = DefaultPixelFormat)
//        {
//            int bitsPerPixel = ((int)format & 0xff00) >> 8;
//            int bytesPerPixel = (bitsPerPixel + 7) / 8;
//            int stride = 4 * ((width * bytesPerPixel + 3) / 4);
//
//            Bitmap bmp = new Bitmap(width, height, format);
//            BitmapData data = null;
//            try
//            {
//                data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, format);
//                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
//            }
//            finally
//            {
//                if (data != null) bmp.UnlockBits(data);
//            }
//
//            return bmp;
//        }
//
//        public static void Update(this Dictionary<string, string> dict, string key, string value)
//        {
//            if (string.IsNullOrEmpty(key)) throw new ArgumentException();
//            if (dict.ContainsKey(key))
//                dict[key] = value;
//            else
//            {
//                dict.Add(key, value);
//            }
//        }

        public static bool HighlightAndMarked(this IUIControl iuiControl, string[] lines, List<string> validated)
        {
            string displayed = iuiControl.Text;
            if (displayed.ContainsAny(lines))
            {
                validated.AddRange(lines.Where(l => !validated.Contains(l) && (displayed.ContainsIgnoreCase(l) || SimilarTo(l, displayed))));
                iuiControl.Highlight(UIContainer.DefaultMatchingHighlightScript);
                return true;
            }
            else
            {
                Logger.E("Failed to match '{0}' with given details", displayed);
                iuiControl.Highlight(UIContainer.DefaultMismatchHighlightScript);
                return false;
            }
        }

        /// <summary>
        /// Returns a Secure string from the source string
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static SecureString AsSecureString(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            else
            {
                SecureString result = new SecureString();
                foreach (char c in source.ToCharArray())
                    result.AppendChar(c);
                return result;
            }
        }

        public static Func<bool> IsUrlChanged()
        {
            string currentUrl = WebDriverManager.CurrentDriverUrl;
            return () => WebDriverManager.CurrentDriverUrl != currentUrl;
        }

        public static Func<bool> IsControlVisible(IUIControl expectedControl)
        {
            return () => expectedControl.IsVisible;
        }

        public static Func<bool> IsControlGone(IUIControl expectedControl)
        {
            return () => !expectedControl.IsVisible;
        }
    }

}
