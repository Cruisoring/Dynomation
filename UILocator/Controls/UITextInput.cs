using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UITextInput : UIControl, IUITextInput
    {
        public const string TYPE = "type";
        public const string PASSWORD = "password";
        public static readonly string CONTROL =
            Convert.ToString(Convert.ToChar(0xE009, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        public static readonly string Tab = Convert.ToString(Convert.ToChar(0xE004, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        public static readonly string Ctrl_A = CONTROL + "a";

        public static implicit operator UITextInput(By by)
        {
            return new UITextInput(by);
        }

        public string TypeValue { get { return GetAttribute(TYPE); } }

        public bool IsPassword
        {
            get { return string.Equals(PASSWORD, TypeValue, StringComparison.OrdinalIgnoreCase); }
        }

        public UITextInput(IUIContainer container, By by, int? index)
            : base(container, by, index)
        { }

        public UITextInput(IUIContainer container, By by)
            : base(container, by)
        { }


        public UITextInput(By by)
            : base(by)
        { }

        public UITextInput(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UITextInput(IUIContainer container, String by)
            : this(container, By.CssSelector(by))
        { }

        public UITextInput(String by)
            : this(By.CssSelector(by))
        { }

        public static string SetValueScriptTemplate = "arguments[0].setAttribute('value','{0}');";

        public void EnterByScript(string text)
        {
            if (text == null)
                return;
            string script = string.Format("arguments[0].value='{0}';", text);
            this.ExecuteScript(script);

            this.FireEvent("change");
        }

        public void EnterAndTab(string text, int intervalMills=100)
        {
            this.FireEvent("focus");
            Element.SendKeys(Ctrl_A);
            foreach (var ch in text)
            {
                this.Element.SendKeys(ch.ToString());
                this.FireEvent("change");
                if (intervalMills > 0)
                {
                    System.Threading.Thread.Sleep(intervalMills);
                }
            }
            this.FireEvent("blur");
            this.Element.SendKeys(Tab);
        }

        public virtual void EnterText(string text)
        {
//            ScrollIntoViewByScript();
            PerformAction(() =>
            {
                FireEvent("click");
                Element.SendKeys(Ctrl_A);
                Element.SendKeys(text);
            });
        }

        public override string Text
        {
            get
            {
                //this.element = null;
                return Element.GetAttribute("value");
            }
        }

        public override bool Perform(params string[] args)
        {
            if (args.Count() == 1)
            {
                string text = args[0];
                if (text == null)
                    text = string.Empty;
                //TODO: handling whitespace
                Executor.Until(() => this.IsVisible, 10 * 1000);
                //this.PerformAction(() => this.EnterText(text), e => e.GetAttribute("value") == text);
                EnterText(text);
            }
            else
                Click();
            return true;
        }
    }

}
