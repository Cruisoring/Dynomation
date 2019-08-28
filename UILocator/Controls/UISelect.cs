using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UISelect : UIControl, IUISelect
    {
        public const string ANY = "ANY";
        public const string FIRST = "FIRST";
        public const string LAST = "LAST";
        public static readonly string ClickArrowScript = @"var target = arguments[0];
var event;
if(document.createEvent){
	event = document.createEvent('MouseEvents'); // for chrome and firefox
	event.initEvent('click', true, true, window, 0, 0, 0, {0}, {1}, false, false, false, false, 0, null);
	arguments[0].dispatchEvent(event); // for chrome and firefox
}else{
	event = document.createEventObject(window.event); // for InternetExplorer before version 9
	event.button =1; //Left button is down
	event.offsetX = {0};
	event.offsetY = {1};
	target.fireEvent('onmousedown', event);
}";


        public static readonly bool WaitOptionAsTextValue = true;
        public static readonly int WaitOptionAsTextValueInMills = 2000;

        public static readonly bool ExpandToSelect = true;
        public static readonly int WaitMillisAfterExpand = 1000;

        public static readonly StringComparison DefaultStringComparison = StringComparison.Ordinal;
        public static readonly bool SelectOptionCouldContainsText = false;

        public static readonly bool SleepIfOnChangeTriggered = true;
        public static readonly int SleepAfterOnChange = 1000;

        public static readonly string[] endOfOptionSpliter = new string[] { "</option>", "</OPTION>" };
        public static readonly string[] valueOfOptionSpliter = new string[] { "selected", "label", "disabled", ">" };

        public static implicit operator UISelect(By by)
        {
            return new UISelect(by);
        }

        public IWebElement SelectedOption
        {
            get
            {
                IList<IWebElement> options = this.FindElements(By.TagName("option")).ToList();
                return options.FirstOrDefault(o => o.Selected);
            }
        }

        public String[] OptionTexts
        {
            get
            {
                IList<IWebElement> options = this.FindElements(By.TagName("option")).ToList();
                return options.Select(o => HttpUtility.HtmlDecode(o.Text)).ToArray();
            }
        }

        public String[] OptionValues
        {
            get
            {
                IList<IWebElement> options = this.FindElements(By.TagName("option")).ToList();
                return options.Select(o => o.GetAttribute("value")).ToArray();
            }
        }

        public override string Text
        {
            get
            {
                if (SelectedOption != null)
                    return SelectedOption.Text;
                String value = this.GetAttribute("value");
                IWebElement selected = this.FindElement(By.CssSelector(string.Format("option[value='{0}']", value)));
                return selected == null ? null : selected.Text;
            }
        }

        public override RemoteWebElement Element
        {
            get
            {
                try
                {
                    return base.Element;
                }
                catch (Exception ex)
                {
                    Logger.E(ex);
                    return null;
                }
            }
        }

        public UISelect(IUIContainer container, By by, int? index)
            : base(container, by, index)
        { }

        public UISelect(IUIContainer container, By by)
            : base(container, by)
        {
        }

        public UISelect(By by) : base(by) { }

        public UISelect(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UISelect(IUIContainer container, String by)
            : this(container, By.CssSelector(by))
        { }

        public UISelect(String by)
            : base(By.CssSelector(by))
        { }

        public virtual void Expand()
        {
            Size size = this.Element.Size;
            int yOffset = size.Height / 2;
            int xOffset = size.Width - yOffset;
            Actions build = new Actions(Driver);
            build.MoveToElement(this.Element, xOffset, yOffset).ClickAndHold().Perform();
        }

        public virtual string Select(By optionBy, bool expandToSelect = false)
        {
            WaitControlReady();

            bool withOnChange = SleepIfOnChangeTriggered && !String.IsNullOrEmpty(this.Element.GetAttribute("onchange"));

            string selectedText = null;
            if (!expandToSelect)
            {
                UIControl optionControl = new UIControl(this, optionBy);

                try
                {
                    selectedText = optionControl.Text;
                    optionControl.Element.Click();
                    this.element = null;
                }
                catch (Exception ex)
                {
                    Logger.E(ex);
                    return "";
                }
                finally
                {
                    this.element = null;
                }
            }
            else
            {
                try
                {
                    Size size = this.Element.Size;
                    int yOffset = size.Height / 2;
                    int xOffset = size.Width - yOffset;

                    Actions build = new Actions(Driver);
                    build.MoveToElement(this.Element, xOffset, yOffset).Click().MoveToElement(this.Element, xOffset, size.Height * 3 / 2).Perform();
//                    System.Threading.Thread.Sleep(WaitMillisAfterExpand);
                    build.MoveToElement(this.Element).Release().Perform();

                    this.element = null;
                    UIControl optionControl = new UIControl(this, optionBy);
                    selectedText = optionControl.Text;
                    optionControl.Element.Click();

                    this.element = null;
                    //                    Thread.Sleep(100);
                    build = new Actions(Driver);
                    build.MoveToElement(this.Element, xOffset, yOffset).Click().Perform();
                }
                catch (Exception ex)
                {
                    Logger.E(ex);
                    return "";
                }
                finally
                {
                    this.element = null;
                }
            }

            this.WaitControlReady(1000);
            if (withOnChange)
            {
                Thread.Sleep(SleepAfterOnChange);
                Logger.V("Sleep {0}ms after select option with text of '{1}'.", SleepAfterOnChange, selectedText);
            }
            else
            {
                selectedText = this.Text;
                Logger.V("Text of selected option: {0}", selectedText);
            }
            return selectedText;
        }

        public bool selectOption(String key, Boolean expandBeforeSelect)
        {
            Boolean result = false;
            try{
                WebDriverManager.WaitPageReady();

                String innerHtml = InnerHTML;
                if (!innerHtml.ContainsIgnoreCase(key))
                {
                    return false;
                }

                String[] optionTexts = OptionTexts;
                String[] optionValues = OptionValues;

                int optionIndex = -1;
                String upperKey = key.ToUpper();
                if (optionTexts.Any(s => s.Equals(key)))
                {
                    optionIndex = Array.IndexOf(optionTexts, key);
                }
                else if (optionValues.Any(s => s.Equals(key)))
                {
                    optionIndex = Array.IndexOf(optionValues, key);
                }
                else if (optionTexts.Any(s => s.ToUpper().Equals(upperKey)))
                {
                    optionIndex = Array.FindIndex(optionTexts, s => s.ToUpper().Equals(upperKey));
                }
                else if (optionValues.Any(s => s.ToUpper().Equals(upperKey)))
                {
                    optionIndex = Array.FindIndex(optionValues, s => s.ToUpper().Equals(upperKey));
                }
                else if (optionTexts.Any(s => s.ToUpper().Contains(upperKey)))
                {
                    optionIndex = Array.FindIndex(optionTexts, s => s.ToUpper().Contains(upperKey));
                }
                else if (optionValues.Any(s => s.ToUpper().Contains(upperKey)))
                {
                    optionIndex = Array.FindIndex(optionValues, s => s.ToUpper().Contains(upperKey));
                }



            }
            catch (Exception e)
            {
                Logger.W(e.Message);
            }
            return result;
        }


        public virtual string Select(string key, bool select = true)
        {
            WaitControlReady();
            if (ExpandToSelect)
            {
                Expand();
            }

            string innerHtml = null;
            if (WaitOptionAsTextValue)
            {
                Executor.Until(() => {
                    innerHtml = this.InnerHTML;
                    if (string.IsNullOrEmpty(innerHtml)) return false;
                    innerHtml = HttpUtility.HtmlDecode(innerHtml)?.Trim();
                    return innerHtml.ContainsAll(key);
                }, WaitOptionAsTextValueInMills);
            }

            if (string.IsNullOrEmpty(innerHtml))
            {
                Logger.W("Failed to get InnerHTML of '{0}'. Select failed.", this);
                return "";
            }
            else if (!innerHtml.ContainsAll(key))
                Logger.V("InnerHTML:'{0}' doesn't contains '{1}'.", innerHtml, key);

            String[] optionTexts = OptionTexts;
            String[] optionValues = OptionValues;

            int index = -1;
            String upperKey = key.ToUpper();
            if (optionTexts.Any(s => s.Equals(key)))
            {
                index = Array.IndexOf(optionTexts, key);
            }
            else if (optionValues.Any(s => s.Equals(key)))
            {
                index = Array.IndexOf(optionValues, key);
            }
            else if (optionTexts.Any(s => s.ToUpper().Equals(upperKey)))
            {
                index = Array.FindIndex(optionTexts, s => s.ToUpper().Equals(upperKey));
            }
            else if (optionValues.Any(s => s.ToUpper().Equals(upperKey)))
            {
                index = Array.FindIndex(optionValues, s => s.ToUpper().Equals(upperKey));
            }
            else if (optionTexts.Any(s => s.ToUpper().Contains(upperKey)))
            {
                index = Array.FindIndex(optionTexts, s => s.ToUpper().Contains(upperKey));
            }
            else if (optionValues.Any(s => s.ToUpper().Contains(upperKey)))
            {
                index = Array.FindIndex(optionValues, s => s.ToUpper().Contains(upperKey));
            }

            if (index == -1)
            {
                string errorMessage = string.Format("Failed to select option '{0}' from:\r\n{1}", key, string.Join("\r\n", innerHtml));
                Logger.E(errorMessage);
                throw new NoSuchElementException(errorMessage);
            }

            index = index >= 0 ? index : optionValues.Length + index;
            string oValue = optionValues[index];
            By selectedOptionBy = null;
            if (!string.IsNullOrEmpty(oValue) && Array.IndexOf(optionValues,oValue) == Array.LastIndexOf(optionValues, oValue))
                selectedOptionBy = By.CssSelector(string.Format("option[value='{0}']", oValue));
            else
                selectedOptionBy = By.CssSelector("option".CssSelectorOfNthOfType(index));
            Logger.V("{0} => {1}", key, selectedOptionBy);

            UIControl optionToSelect = new UIControl(this, selectedOptionBy);
            optionToSelect.Click(() => optionToSelect.GetAttribute("data-selected").Equals("true"));

            if (ExpandToSelect)
            {
                Expand();
            }
            return optionToSelect.Text;
        }

        public override bool Perform(params string[] args)
        {
            string toBeSelected = null;
            bool select = true;
            if (args != null && args.Count() > 0)
            {
                toBeSelected = args[0];
                if (string.IsNullOrWhiteSpace(toBeSelected))
                    throw new ArgumentException();

                if (args.Count() > 1)
                {
                    bool boolValue = false;
                    if (bool.TryParse(args[1], out boolValue))
                    {
                        select = boolValue;
                    }
                }
            }

            string selectedText = string.Empty;
            Executor.Until(() => this.IsVisible, 10 * 1000);
            this.PerformAction(() => selectedText = this.Select(toBeSelected, select),
                c =>
                {
                    return selectedText.ToLower().Contains(toBeSelected.ToLower())
                        || this.SelectedOption.Text.ToLower().Contains(toBeSelected.ToLower());
                });

            return selectedText.ToLower().Contains(toBeSelected.ToLower());
        }

    }

}
