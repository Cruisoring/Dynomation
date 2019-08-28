using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UIContainer : IUIContainer
    {
        #region static fields/properties
        public const string DefaultMatchingHighlightScript = "var e=arguments[0]; e.style.color='teal';e.style.backgroundColor='green';";
        public const string DefaultContainingHighlightScript = "var e=arguments[0]; e.style.color='olive';e.style.backgroundColor='orange';";
        public const string DefaultMismatchHighlightScript = "var e=arguments[0]; e.style.color='fuchsia';e.style.backgroundColor='red';";

        public static readonly bool AlwaysWaitPageReady = false;
        public static readonly bool WaitPageReadyBeforePerforming = false;
        public static readonly bool WaitPageReadyAfterPerforming = true;

        public static readonly int DefaultIntervalBetweenActions = 1000;

        public static readonly int DefaultRootValidPeriod = 5000;

        public static readonly string DefaultRootCssSelector = "body, form, div, span, table";
        public static By DefaultByOfRoot { get { return By.CssSelector(DefaultRootCssSelector); } }

        protected static readonly Type htmlControlType = typeof(IUIControl);

        public static readonly Type IControlType = typeof(IUIControl);

        [ThreadStatic]
        public static Dictionary<string, Dictionary<string, IUIControl>> ControlsByContainerName;

        [ThreadStatic]
        protected static Dictionary<Type, IUIContainer> AllContainers;

        #endregion

        public static T Instance<T>() where T : UIContainer
        {
            return GetScreen<T>();
        }

        [ThreadStatic]
        private static IUIContainer lastScreen;

        public static T GetScreen<T>(bool urlChangeExpected = true) where T : IUIContainer
        {
            Type t = typeof(T);
            IUIContainer screen;
            if (AllContainers == null)
                AllContainers = new Dictionary<Type, IUIContainer>();
            if (!AllContainers.ContainsKey((t)))
            {
                if (!t.IsClass)
                {
                    throw new ArgumentException(
                        string.Format(
                            "{0} is not class to be constructed, call SetScreen<T>(screen) first to setup the instance.",
                            t));
                }
                else if (null == t.GetConstructor(Type.EmptyTypes))
                {
                    throw new ArgumentException(
                        string.Format(
                            "{0} has no parameterless constructor, call SetScreen<T>(screen) first to setup the instance.",
                            t));
                }

                screen = Activator.CreateInstance<T>();
                AllContainers.Add(t, screen);
            }

            screen = AllContainers[t];
            if (urlChangeExpected && screen != lastScreen)
            {
                lastScreen = screen;
                WebDriverManager.WaitPageReady(1000);
            }
            return (T)screen;
        }


        public static T SetScreen<T>(T screen) where T : IUIContainer
        {
            Type t = typeof(T);
            if (AllContainers.ContainsKey(t))
            {

                AllContainers[t] = screen;
            }
            else
                AllContainers.Add(t, screen);

            return (T)AllContainers[t];
        }

        public RemoteWebDriver Driver
        {
            get
            {
                return WebDriverManager.Driver;
            }
        }

        private IWebElement theRoot = null;
        private DateTime validRootUntil = DateTime.MinValue;
        protected IWebElement Root
        {
            get
            {
                if (theRoot == null || DateTime.Now > validRootUntil)
                {
                    try
                    {
                        theRoot = Driver.FindElement(ByOfRoot);
                    }
                    catch (Exception ex)
                    {
                        if (ex is NoSuchElementException || ex is ElementNotVisibleException || ex is StaleElementReferenceException)
                        {
                            Driver.SwitchTo().Window(Driver.CurrentWindowHandle);
                            WebDriverManager.CurrentSwitchTo("");
                            theRoot = Driver.FindElement(ByOfRoot);
                        }
                        else
                        {
                            Logger.D($"Unexpected {ex}");
                        }
                    }
                    validRootUntil = DateTime.Now.AddMilliseconds(DefaultRootValidPeriod);
                }
                return theRoot;
            }
        }

        protected readonly By ByOfRoot;

        public By Locator => ByOfRoot;

        public Predicate<string> IsUrlMatched { get; protected set; }

        public Predicate<string> IsTitleMatched { get; protected set; }

        public string FramePath { get; protected set; }

        public string ThisFrameName { get; protected set; }

        public UIContainer(string framePath, IUIContainer parent = null, By rootBy = null)
        {
            ByOfRoot = rootBy ?? DefaultByOfRoot;
            string path = (parent == null) ? "" : parent.FramePath;

            if (!string.IsNullOrEmpty(framePath))
            {
                path = WebDriverManager.FrameIndicator + framePath;
            }

            string[] targetFrames = path.Split(new char[] { ' ', '>', '$', '%' }, StringSplitOptions.RemoveEmptyEntries);

            FramePath = string.Join(WebDriverManager.FrameIndicator, targetFrames);
            ThisFrameName = targetFrames.Length > 0 ? targetFrames.Last() : "";
        }

        public UIContainer(By rootBy) : this(null, null, rootBy)
        {}

        public UIContainer(String byString) : this(By.CssSelector(byString))
        { }

        public UIContainer()
            : this(null, null)
        { }

        public virtual bool IsVisible
        {
            get
            {
                WebDriverManager.WaitPageReady();
                try
                {
                    if (IsUrlMatched != null && !IsUrlMatched(Driver.Url))
                    {
                        return false;
                    }

                    if (IsTitleMatched != null && !IsTitleMatched(Driver.Title))
                    {
                        return false;
                    }

                    return true;
                }
                catch (UnhandledAlertException)
                {
                    Logger.V("UnhandledAlertException happened: alert blocks accessing the target UIContainer.");
                }

                return false;
            }
        }

        public virtual bool WaitUntilVisible(int timeoutMillis = -1)
        {
            WebDriverManager.WaitPageReady(timeoutMillis);
            timeoutMillis = timeoutMillis > 0 ? timeoutMillis : Executor.DefaultWebDriverWaitTimeout;
            bool result = Executor.Until(() => IsVisible, timeoutMillis);

            if (!result)
                Logger.I("The '{0}' is still not visible after {1}s.", this.GetType().Name, timeoutMillis);

            return result;
        }

        public virtual bool WaitUntilGone(int timeoutMillis = -1)
        {
            WebDriverManager.WaitPageReady(timeoutMillis);
            timeoutMillis = timeoutMillis > 0 ? timeoutMillis : Executor.DefaultWebDriverWaitTimeout;
            bool result = Executor.Until(() => !IsVisible, timeoutMillis);

            if (!result)
                Logger.W("The '{0}' is still not gone after {1}ms.", this.GetType().Name, timeoutMillis);

            return result;
        }

        public virtual string Switch()
        {
            return WebDriverManager.CurrentSwitchTo(this.FramePath);
        }

        public virtual IWebElement FindElement(By by)
        {
            IReadOnlyCollection<IWebElement> allElements = null;
            try
            {
                allElements = FindElements(by);
                return (allElements == null || allElements.Count() == 0) ?
                    null : allElements.FirstOrDefault(x => x.Displayed);
            }
            catch
            {
                theRoot = null;
                return null;
            }
        }

        public virtual ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            ReadOnlyCollection<IWebElement> result = null;
            if (Switch().Equals(this.FramePath))
            {
                try
                {
                    result = Root.FindElements(by);
                }
                catch (Exception ex)
                {
                    //Try again when the Root is not valid
                    if (ex is StaleElementReferenceException || ex is ElementNotVisibleException)
                    {
                        theRoot = null;
                        result = Root.FindElements(by);
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            return result;
        }


        //Get the IUIControl matching the given controlName
        public virtual IUIControl ControlFromName(string controlName)
        {
            Type containerType = this.GetType();
            string containerName = containerType.Name;
            Dictionary<string, IUIControl> controlsByName;
            if (ControlsByContainerName == null)
                ControlsByContainerName = new Dictionary<string, Dictionary<string, IUIControl>>();
            if (!ControlsByContainerName.ContainsKey(containerName))
            {
                controlsByName = new Dictionary<string, IUIControl>();
                var fields = containerType.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => IControlType.IsAssignableFrom(f.FieldType)).ToList();
                fields.ForEach(f => controlsByName.Add(f.Name, f.GetValue(this) as IUIControl));
                ControlsByContainerName.Add(containerName, controlsByName);
            }
            else
                controlsByName = ControlsByContainerName[containerName];

            if (controlsByName.ContainsKey(controlName))
                return controlsByName[controlName];

            //Logger.W("UIControl named as '{0}' is not found in current Screen defintion.", controlName);
            return null;
        }

        public static bool CloseDialog(int times = 3, Action<IAlert> handleAlert = null)
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
                    WebDriverManager.CurrentSwitchTo("");
                    Thread.Sleep(2000);
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    Thread.Sleep(2000);
                    continue;
                }
            } while (times-- > 0);
            return false;
        }

        protected void enterDetails(Dictionary<string, string> details)
        {
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties = properties.Where(p => htmlControlType.IsAssignableFrom(p.GetMethod.ReturnType)).ToArray();
            PropertyInfo[] declaringProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => htmlControlType.IsAssignableFrom(p.GetMethod.ReturnType)).ToArray();
            string[] declaringPropertyNames = declaringProperties.Select(p => p.Name).ToArray();
            IEnumerable<PropertyInfo> baseOnlyProperties = properties.Except(declaringProperties).Where(p => !declaringPropertyNames.Contains(p.Name));

            List<PropertyInfo> orderedProperties = new List<PropertyInfo>(baseOnlyProperties);
            orderedProperties.AddRange(declaringProperties);
            string[] accessorNames = orderedProperties.Select(p => p.Name).ToArray();

            for (int i = 0; i < accessorNames.Count(); i++)
            {
                string name = accessorNames[i];
                string onclick = null;

                if (details.ContainsKey(name))
                {
                    if (WaitPageReadyBeforePerforming)
                        WebDriverManager.WaitPageReady();

                    IUIControl iuiControl = orderedProperties[i].GetMethod.Invoke(this, null) as IUIControl;
                    if (iuiControl == null || !iuiControl.IsVisible)
                    {
                        continue;
                    }
                    iuiControl.Perform(details[name]);
                    if (iuiControl.Element.TryGetAttribute("onclick", out onclick) && !string.IsNullOrEmpty(onclick))
                        Thread.Sleep(1000);
                    else if (iuiControl.Element.TryGetAttribute("onchange", out onclick) && !string.IsNullOrEmpty(onclick))
                        Thread.Sleep(1000);

                    if (WaitPageReadyAfterPerforming)
                        WebDriverManager.WaitPageReady();
                }
            }
        }

        public virtual bool PerformAll(string[][] details, bool bypassIfNotPresented = false)
        {
            WaitUntilVisible();
            int length = details[0].Length;
            if (details[1].Length != length)
                throw new ArgumentException("string[2][] details should contain two array of the same size!");
            for (int i = 0; i < length; i++)
            {
                string controlName = details[0][i];
                string value = details[1][i];
                IUIControl iuiControl = ControlFromName(controlName);
                if (iuiControl == null || !iuiControl.WaitUntilVisible(bypassIfNotPresented ? 3000 : 10 * 1000))
                {
                    if (bypassIfNotPresented)
                        continue;
                    return false;
                }

                try
                {
                    if (UIControl.AssureShowControlBeforeOperation && !iuiControl.Show())
                    {
                        Logger.W("{0} is still not displayed!", iuiControl);
                    }

                    iuiControl.Perform(value);
                    Logger.D("{0} => {1}", controlName, value);
                    Thread.Sleep(300);
                }
                catch (StaleElementReferenceException)
                {
                    Logger.W("StaleElement of {0}.", iuiControl);
                    UIControl.DiscardLastInstance();
                    iuiControl.Perform(value);
                }
                if (WaitPageReadyAfterPerforming)
                    WebDriverManager.WaitPageReady();
            }
            return true;
        }

        public virtual bool HighlightAll(string[] keys)
        {
            foreach (var key in keys)
            {
                IUIControl iuiControl = ControlFromName(key);
                if (iuiControl == null || !iuiControl.WaitUntilVisible(2000))
                {
                    Logger.W("UIControl '{0}' is not visible.", iuiControl);
                    return false;
                }
                iuiControl.Highlight();
                Logger.D("{0}='{1}'", key, iuiControl.Text);
            }
            return true;
        }

        public virtual bool ValidateAll(string[][] kvps, bool ignoreCase = true)
        {
            WebDriverManager.WaitPageReady();
            int length = kvps[0].Length;
            bool result = true;
            for (int i = 0; i < length; i++)
            {
                string controlName = kvps[0][i];
                string value = kvps[1][i];
                IUIControl iuiControl = ControlFromName(controlName);
                if (iuiControl == null || !iuiControl.IsVisible)
                    continue;

                string text = iuiControl.Text;
                if (string.Equals(text, value) || (ignoreCase && string.Equals(text, value, StringComparison.OrdinalIgnoreCase)))
                    iuiControl.Highlight(DefaultMatchingHighlightScript);
                else if (text.ContainsAll(value))
                {
                    iuiControl.Highlight(DefaultContainingHighlightScript);
                }
                else
                {
                    iuiControl.Highlight(DefaultMismatchHighlightScript);
                    result = false;
                }
            }
            return result;
        }

    }


}
