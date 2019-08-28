using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Schema;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Remote;
using UILocator.Enums;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UIControl : UIContainer, IUIControl
    {
        protected const string setStyleScript = @"
if(!rzCC)
{
// convert s to camel case
function rzCC(s){
  // thanks http://www.ruzee.com/blog/2006/07/\
  // retrieving-css-styles-via-javascript/
  for(var exp=/-([a-z])/; 
	  exp.test(s); 
	  s=s.replace(exp,RegExp.$1.toUpperCase()));
  return s;
}

function getStyle(e,a){
  var v=null;
  if(document.defaultView && document.defaultView.getComputedStyle){
	var cs=document.defaultView.getComputedStyle(e,null);
	if(cs && cs.getPropertyValue) v=cs.getPropertyValue(a);
  }
  if(!v && e.currentStyle) v=e.currentStyle[rzCC(a)];
  return v;
};

function setStyle(element, declaration) {
  if (declaration.charAt(declaration.length-1)==';')
	declaration = declaration.slice(0, -1);
  var pair, k, v, old='';
  var splitted = declaration.split(';');
  for (var i=0, len=splitted.length; i<len; i++) {
	 k = rzCC(splitted[i].split(':')[0]);
	 v = getStyle(element, k);
	 old = old+k+': '+v+';';
	 v = splitted[i].split(':')[1];
	 
	 eval('element.style.'+k+'=\''+v+'\'');
  }
  return old;
}
}
return setStyle(arguments[0], arguments[1]);";

        private static Dictionary<string, string> highlightStyleDict = new Dictionary<string, string>(){
            {"style.color", "red"},
            {"style.backgroundColor", "yellow"},
        };
        public const string DefaultHighlightScript = "var e =arguments[0]; e.style.color='red';e.style.backgroundColor='yellow';";
        public const string DefaultResetStyleScript = "var e=arguments[0]; e.style.color='';e.style.backgroundColor='';";

        protected const string fireEvent = @"
var event;
	if(document.createEvent){
		event = document.createEvent('HTMLEvents'); // for chrome and firefox
		event.initEvent('{0}', true, true);
		arguments[0].dispatchEvent(event); // for chrome and firefox
	}else{
		event = document.createEventObject(); // for InternetExplorer
		event.eventType = '{0}';
		arguments[0].fireEvent('on' + event.eventType, event); // for InternetExplorer
	}
";
        public static bool BypassUnchangedActions = true;
        public static bool AssureShowControlBeforeOperation = true;
        public static bool TryHighLightControlBeforeOperation = true;
        public static bool MouseOverControlBeforeOperation = false;
        public static bool DoValidationAfterOperation = true;

        public static bool WaitAfterAction = true;
        public static int WaitBeforeValidation = 20;
        public static int WaitTimeAfterAction = 200;
        //public static int DefaultElementOperationTimeout = 5 * 1000;
        public static int DefaultWaitBeforeReClicking = 1000;
        public static TimeSpan ValidElementTimeSpan = TimeSpan.FromMilliseconds(100);

        public static readonly string ControlString = Convert.ToString(Convert.ToChar(0xE009, CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);

        public static readonly string ControlA = ControlString + "a";

        public static readonly string TAB = Convert.ToString(Convert.ToChar(0xE004, CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);

        public static int DefaultHighlightTimeMillis = 100; // 0.3 second
        public static int DefaultHighlightTimes = 1;

        public static string DefaultHighlightStyle =
            "color: red; border: solid red; background-color: yellow;";

        public static int DefaultPageChangedTimeout = 60 * 1000;
        public static int DefaultWaitElementGoneTimeout = 30 * 1000;
        [ThreadStatic] protected static IUIControl currentUIControl = null;
        [ThreadStatic] protected static DateTime currentControlValidUntil = DateTime.MinValue;

        public static implicit operator UIControl(By by)
        {
            return new UIControl(by);
        }

        public static void DiscardLastInstance()
        {
            currentUIControl = null;
        }

        public IUIContainer Container { get; protected set; }
        public By by { get; protected set; }

        public int? Index { get; protected set; }


        protected RemoteWebElement element;

        public virtual RemoteWebElement Element
        {
            get
            {
                if (element != null && this == currentUIControl && DateTime.Now < currentControlValidUntil)
                {
                    return element;
                }

                currentUIControl = this;
                currentControlValidUntil = DateTime.Now + ValidElementTimeSpan;

                try
                {
                    ReadOnlyCollection<IWebElement> elements;
                    if (Container == null)
                        elements = Driver.FindElements(by);
                    else
                        elements = Container.FindElements(by);

                    if (!Index.HasValue || Index.Value == 0)
                        element = elements.FirstOrDefault() as RemoteWebElement;
                    else if (Index.Value < 0)
                        element = elements[(Index.Value + elements.Count) % elements.Count] as RemoteWebElement;
                    else if (Index.Value >= elements.Count)
                        element = null;
                    else
                        element = elements[Index.Value] as RemoteWebElement;
                }
                catch
                {
                }

                return element;
            }
        }

        #region Constructors

        public UIControl(IUIContainer container, By by, int? index = null)
            : base(container?.FramePath, container, by)
        {
            this.Container = container;
            this.by = by;
            this.Index = index;
        }

        public UIControl(IUIContainer container, By by) : this(container, by, null)
        {
        }

        public UIControl(By by) : this(null, by)
        {
        }

        public UIControl(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UIControl(IUIContainer container, String by)
            : this(container, By.CssSelector(by))
        { }

        public UIControl(String by)
            : this(By.CssSelector(by))
        { }

        #endregion

        //*/

        public virtual void Reset()
        {
            element = null;
        }

        public virtual string Text
        {
            get
            {
                Executor.Until(() => IsVisible, 2000);
                string text = Element?.Text;
                if (!string.IsNullOrEmpty(text))
                    text = Regex.Replace(text, "<[^>]*>", string.Empty);
                return text;
            }
        }

        public virtual string AllText
        {
            get
            {
                string innerHTML = ExecuteScript("return arguments[0].innerHTML;", Element).ToString();
                string allText = Regex.Replace(innerHTML, "<[^>]*>", "");
                return allText;
            }
        }

        public virtual string InnerText
        {
            get { return this.IsVisible ? ExecuteScript("return arguments[0].innerText;", Element).ToString() : null; }
        }

        public virtual string InnerHTML
        {
            get { return this.IsVisible ? ExecuteScript("return arguments[0].innerHTML;", Element).ToString() : null; }
        }

        public virtual string OuterHTML
        {
            get { return this.IsVisible ? ExecuteScript("return arguments[0].outerHTML;", Element).ToString() : null; }
        }

        public virtual bool Exists
        {
            get { return this.Element != null; }
        }

        public override bool IsVisible
        {
            get
            {
                try
                {
                    this.element = null;
                    var elem = Element;
                    return elem != null && elem.Displayed;
                }
                catch
                {
                    return false;
                }
            }
        }

        public virtual bool IsDisabled
        {
            get
            {
                if (IsVisible)
                {
                    string isDisabled = GetAttribute("disabled");
                    return !(isDisabled == null || isDisabled.Equals("false") || isDisabled.Equals("undefined"));
                }

                return true;
            }
        }

        public virtual bool Perform(params string[] args)
        {
            Click();
            return true;
        }

        public void RightClick()
        {
            Actions act = new Actions(Driver); // where driver is WebDriver type

            act.ContextClick(Element).Perform();
        }

        public void DoubleClick()
        {
            Actions action = new Actions(Driver);

            action.DoubleClick(Element).Perform();
        }

        public virtual void Click()
        {
            //this.Element.Click();
            this.PerformAction(() => this.Element.Click());
        }

        private Func<bool> getIsClicked(bool urlUnchanged, int waitTimeout = -1)
        {
            Func<bool> isClicked;
            if (urlUnchanged)
            {
                string urlBeforeClicking = WebDriverManager.Driver.Url;
                isClicked = () =>
                {
                    Thread.Sleep(1000);
                    string newUrl = null;
                    try
                    {
                        newUrl = WebDriverManager.Driver.Url;
                    }
                    catch
                    {
                    }
                    if (urlBeforeClicking != newUrl)
                    {
                        Logger.D("'{0}' ==> '{1}'", urlBeforeClicking, newUrl);
                        return true;
                    }
                    return false;
                };
            }
            else
            {
                isClicked = () =>
                {
                    Thread.Sleep(1000);
                    try
                    {
                        return currentUIControl == null || !currentUIControl.IsVisible;
                    }
                    catch
                    {
                        return true;
                    }
                };
            }

            return isClicked;

        }
        public bool DoubleClick(bool urlUnchanged, int waitTimeout = -1)
        {
            if (waitTimeout <= 0)
                waitTimeout = urlUnchanged ? DefaultPageChangedTimeout : DefaultWaitElementGoneTimeout;

            Func<bool> isClicked = getIsClicked(urlUnchanged, waitTimeout);

            return Click(isClicked, waitTimeout, new Action[]{DoubleClick});
        }

        public bool Click(bool urlUnchanged, int waitTimeout = -1)
        {
            if (waitTimeout <= 0)
                waitTimeout = urlUnchanged ? DefaultPageChangedTimeout : DefaultWaitElementGoneTimeout;

            Func<bool> isClicked = urlUnchanged ? Executor.IsUrlChanged() : Executor.IsControlGone(this);

            return Click(isClicked, waitTimeout);
        }

        public bool Click(Func<bool> isClicked, int waitTimeout = -1, params Action[] actions)
        {
            waitTimeout = waitTimeout > 0 ? waitTimeout : DefaultWaitElementGoneTimeout;

            actions = actions==null || actions.Length==0 ? new Action[]{Click, ClickByScript, ()=> Element.SendKeys(Keys.Enter) } : actions;

            try
            {
                actions[0].Invoke();
                if (isClicked==null || isClicked())
                    return true;
            }
            catch
            {
            }

            DateTime untilMoment = DateTime.Now.AddMilliseconds(waitTimeout);
            int attempts = 0;
            do
            {
                Thread.Sleep(waitTimeout / 10);
                try
                {
                    if (isClicked())
                        return true;
                    switch ((++attempts) % actions.Length)
                    {
                        case 0:
                            Logger.D("Try to click it by script");
                            actions[0].Invoke();
                            break;
                        case 1:
                            Logger.D("Try to click it another time");
                            actions[1].Invoke();
                            break;
                        case 2:
                            actions[2].Invoke();
                            break;
                    }
                    Thread.Sleep(DefaultWaitBeforeReClicking);
                }
                catch (StaleElementReferenceException)
                {
                    if (isClicked())
                        return true;
                }
                catch (NullReferenceException)
                {
                    if (isClicked())
                        return true;
                }
                catch (UnhandledAlertException)
                {
                    try
                    {
                        Driver.SwitchTo().Alert().Accept();
                        WebDriverManager.CurrentSwitchTo("");
                    }
                    catch
                    {
                    }
                    Thread.Sleep(2000);

                    continue;
                }
                catch
                {
                }
            } while (DateTime.Now < untilMoment);

            Logger.I("Clicking failed to get expected result.");
            return false;
        }

        public bool ClickToShow(UIControl another, Action action=null)
        {
            return Executor.Try(Executor.IsControlVisible(another), action??Click);
        }

        public object ExecuteScript(string script, params object[] args)
        {
            object[] args1 = new object[1 + args.Length];
            args1[0] = Element;
            Array.Copy(args, 0, args1, 1, args.Length);
            try
            {
                //Always append 'Element' as argument[0]
                return (Element.WrappedDriver as IJavaScriptExecutor).ExecuteScript(script, args1);
            }
            catch
            {
                currentUIControl = null;
                Thread.Sleep(100);
                if (IsVisible)
                {
                    try
                    {
                        return (Element.WrappedDriver as IJavaScriptExecutor).ExecuteScript(script, args1);
                    }
                    catch (Exception e)
                    {
                        Logger.W(e.Message);
                    }
                }
                return null;
            }
        }

        public void ScrollIntoViewByScript(Boolean alignTop = true)
        {
            String script = "arguments[0].scrollIntoView(" + alignTop.ToString().ToLower() + ");";
            ExecuteScript(script);
        }


        public void click(OnElement position)
        {
            Size size = Element.Size;
            int offset = Math.Min(size.Width, size.Height) / 4;
            int x = 0, y = 0;
            switch (position)
            {
                case OnElement.top:
                    x = size.Width / 2;
                    y = offset;
                    break;
                case OnElement.left:
                    x = offset;
                    y = size.Height / 2;
                    break;
                case OnElement.bottom:
                    x = size.Width / 2;
                    y = size.Height - offset;
                    break;
                case OnElement.right:
                    x = size.Width - offset;
                    y = size.Height / 2;
                    break;
            }
            clickByOffset(x, y);
        }

        public void clickByOffset(int xOffset, int yOffset)
        {
            Actions builder = new Actions(Driver);
            IAction action = builder.MoveToElement(Element, xOffset, yOffset).Click().Build();
            action.Perform();
        }


        public void ClickByScript()
        {
            ExecuteScript("arguments[0].click();", Element);
        }

        /* Notice: In Firefox, Opera, Google Chrome and Safari, the readyState property is not supported by HTML elements
            (except by the script element in Opera), only the document object supports it (in Firefox from version 3.6).*/
        public const string getReadyStateScript = @"return arguments[0].readyState  || document.readyState;";

        public ReadyState GetReadyState()
        {
            object returned = ExecuteScript(getReadyStateScript);
            string state = returned == null ? "unknown" : returned.ToString();
            ReadyState result = ReadyState.unknown;
            Enum.TryParse<ReadyState>(state, out result);
            return result;
        }

        public const string getAttributeScript = @"try{{
return arguments[0].getAttribute('{0}');
}}catch(err){{return null;}}";

        public string GetAttribute(string attributeName)
        {
            string script = getAttributeScript.Replace("{0}", attributeName);
            object result = this.ExecuteScript(script);
            return result?.ToString();
        }

        public const string getPropertyScript = @"try{{
return arguments[0].{0};
}}catch(err){{return null;}}";

        public string GetProperty(string propertyName)
        {
            string script = getPropertyScript.Replace("{0}", propertyName);
            object result = this.ExecuteScript(script);
            return result?.ToString();
        }

        public string GetCssValue(string propertyName)
        {
            return Element.GetCssValue(propertyName);
        }


        public virtual bool WaitControlReady(int timeoutMillis = -1)
        {
            timeoutMillis = timeoutMillis > 0 ? timeoutMillis : Executor.DefaultWebDriverWaitTimeout;
            bool result = Executor.Until(() => GetReadyState() == ReadyState.complete && IsVisible, timeoutMillis);

            return result;
        }

        public void FireEvent(string eventName)
        {
            string eventScript = fireEvent.Replace("{0}", eventName);
            this.element = null;
            ExecuteScript(eventScript);
        }

        public void PerformAction(Action action, Func<IWebElement, bool> validate = null, int retry = 1)
        {
            if (!this.Exists)
            {
                Logger.E(string.Format("Failed to find {0}, action cannot be fulfilled!", this));
                return;
            }

            try
            {
                if (!Element.Enabled && Executor.Until(() => Element.Enabled))
                {
                    Logger.E(new InvalidOperationException(string.Format("Element '{0}' is not enabled.", this)));
                    return;
                }

                if (BypassUnchangedActions && validate != null && validate(Element))
                {
                    Logger.V("Validation passed, action on {0} is bypassed.", this);
                    return;
                }

                if (AssureShowControlBeforeOperation)
                {
                    if (!this.Show())
                    {
                        Logger.W("{0} is still not displayed!", this);
                    }

                    if (TryHighLightControlBeforeOperation)
                    {
                        this.Highlight();
                    }
                }
                if (MouseOverControlBeforeOperation)
                {
                    this.MouseOver();
                }
                //iuiControl.Until(c => c.Displayed);

                action();
                Logger.V("Action on {0} is performed.", this);
            }
            catch (UnhandledAlertException)
            {
                try
                {
                    IAlert alert = Driver.SwitchTo().Alert();
                    string alertText = alert.Text;
                    alert.Accept();
                    Logger.D("Alert is Accepted which shows: {0}", alertText);

                    if (retry-- > 0)
                    {
                        Thread.Sleep(2000);
                        //Try again
                        action();
                    }
                }
                catch (Exception e)
                {
                    Logger.W("{0} is caught after UnhandledAlertException:\r\n{1}", e, e.Message);
                }
            }
            catch (StaleElementReferenceException)
            {
                Logger.D("StaleElement of {0} is detected in PerformAction().", this);
                //Discarding the reserved instance to search Element again
                this.element = null;
                if (this.Exists)
                    action();
            }

            if (validate != null && DoValidationAfterOperation)
            {
                if (WaitBeforeValidation > 0)
                    Thread.Sleep(WaitBeforeValidation);

                this.element = null;
                while (!validate(Element) && retry > 0)
                {
                    retry--;
                    Logger.D("Validation failed after action on {0}.", this);
                    if (retry >= 0)
                    {
                        action();
                        Logger.D("Action on {0} again after validation failed.", this);
                    }
                };

                if (retry == 0 && !validate(Element))
                    Logger.W("Validate {0} failed!", this);
            }

            if (WaitAfterAction)
                Thread.Sleep(WaitTimeAfterAction);
            //*/
        }

        public bool Show(bool alignToTop = false)
        {
            Point point;
            try
            {
                point = (Element as ILocatable).LocationOnScreenOnceScrolledIntoView;
            }
            catch (InvalidOperationException)
            {
                this.element = null;
                string script = string.Format("arguments[0].scrollIntoView();");
                //string script = string.Format("arguments[0].scrollIntoView({0});", alignToTop.ToString().ToLower());
                ExecuteScript(script, Element);
                point = Element.Coordinates.LocationInViewport;
            }
            bool isDisplayed = Element.Displayed;
            //Sleep 100ms to avoid 'The HTTP request to the remote WebDriver server ...' WebDriverException
            //Thread.Sleep(100);
            return isDisplayed;
        }

        public override string ToString()
        {
            return String.Format("{0} {1}{2}", this.GetType().Name, by,
                Index.HasValue ? string.Format("[{0}]", Index.Value) : "");
        }

        public void Highlight(string highlightScript = null, string resetScript = null, int interval = -1, int times = -1)
        {
            if (Element == null || !IsVisible)
                return;

            try
            {
                highlightScript = highlightScript ?? DefaultHighlightScript;
                resetScript = resetScript ?? DefaultResetStyleScript;

                if (this.Element.TagName == "img" || GetAttribute("type") == "checkbox")
                {
                    highlightScript = highlightScript.Replace("arguments[0]", "arguments[0].parentElement");
                    resetScript = resetScript.Replace("arguments[0]", "arguments[0].parentElement");
                }

                interval = interval > 0 ? interval : DefaultHighlightTimeMillis;
                times = times > 0 ? times : DefaultHighlightTimes;

                for (int i = times; i > 0; i--)
                {
                    ExecuteScript(highlightScript);
                    Thread.Sleep(interval);
                    ExecuteScript(resetScript);
                    Thread.Sleep(interval);
                }
            }
            catch
            { }
        }

        public void MouseOver()
        {
            Actions builder = new Actions(((RemoteWebElement)Element).WrappedDriver);
            IAction moveToElement = builder.MoveToElement(Element).Build();
            moveToElement.Perform();
        }

        public IWebElement GetScrollableParent(RemoteWebElement target = null)
        {
            string getScrollable =
                "var scrollable = arguments[0];" +
                "while(scrollable != null && scrollable.scrollHeight == scrollable.clientHeight && scrollable.scrollWidth == scrollable.clientWidth){scrollable = scrollable.parentElement;}return scrollable;";
            try
            {
                RemoteWebElement theElement = target ?? Element;
                return (IWebElement) (theElement.WrappedDriver as IJavaScriptExecutor).ExecuteScript(getScrollable,
                    theElement);
            }
            catch
            {
                return null;
            }
        }


        public bool ScrollReset(IUIControl scrollable=null)
        {
            scrollable = scrollable ?? this;
            scrollable.ExecuteScript("arguments[0].scrollTo(0, 0)");
            int origiScrollLeft = Int32.Parse(scrollable.GetProperty("scrollLeft"));
            int originScrollTop = Int32.Parse(scrollable.GetProperty("scrollTop"));
            Thread.Sleep(100);

            return originScrollTop == 0 && originScrollTop == 0;
        }

        public bool ScrollHorizontal(int? pixelToScroll = null, IUIControl scrollable = null)
        {
            scrollable = scrollable ?? this;
            int origiScrollLeft = Int32.Parse(scrollable.GetProperty("scrollLeft"));
            int scrollWidth = Int32.Parse(scrollable.GetProperty("scrollWidth"));
            int clientWidth = Int32.Parse(scrollable.GetProperty("clientWidth"));

            if (pixelToScroll == null)
            {
                if (clientWidth >= scrollWidth)
                    return false;
                pixelToScroll = clientWidth;
            }
            else if (pixelToScroll == 0) //Special case to scroll back to 0
            {
                pixelToScroll = -origiScrollLeft;
            }

            scrollable.ExecuteScript("arguments[0].scrollTo(arguments[1], 0)", pixelToScroll + origiScrollLeft);
            Executor.Until(() => origiScrollLeft != Int32.Parse(scrollable.GetProperty("scrollLeft")), 1000);
            return origiScrollLeft != Int32.Parse(scrollable.GetProperty("scrollLeft"));
        }

        public bool ScrollVertical(int? pixelToScroll = null, IUIControl scrollable = null)
        {
            scrollable = scrollable ?? this;
            int originScrollTop = Int32.Parse(scrollable.GetProperty("scrollTop"));
            int scrollHeight = Int32.Parse(scrollable.GetProperty("scrollHeight"));
            int clientHeight = Int32.Parse(scrollable.GetProperty("clientHeight"));

            if (pixelToScroll == null)
            {
                if (clientHeight >= scrollHeight)
                    return false;
                pixelToScroll = clientHeight;
            }
            else if (pixelToScroll == 0) //Special case to scroll back to 0
            {
                pixelToScroll = -originScrollTop;
            }

            ExecuteScript("arguments[0].scrollTo(0, arguments[1])", pixelToScroll + originScrollTop);
            Executor.Until(() => originScrollTop != Int32.Parse(scrollable.GetProperty("scrollTop")), 1000);
            return originScrollTop != Int32.Parse(scrollable.GetProperty("scrollTop"));
        }

        public override IWebElement FindElement(By by)
        {
            return Element.FindElement(by);
        }

        public override ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return Element.FindElements(by);
        }

        public virtual IWebElement FindElement(By by, params object[] keys)
        {
            ReadOnlyCollection<IWebElement> elements = FindElements(by);
            IWebElement firstMatched = elements.FirstOrDefault(e => e.Text.ContainsAll(keys));
            return firstMatched;
        }
    }

}
