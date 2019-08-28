using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UICheckBox : UIControl, IUICheckBox
    {
        public static implicit operator UICheckBox(By by)
        {
            return new UICheckBox(by);
        }

        public bool Checked
        {
            get
            {
                return Element.Selected;
            }
            set
            {
                Logger.V("About to {0} {1}.", value ? "check" : "uncheck", by);
                PerformAction(() => Element.Click(), chk =>
                {
                    System.Threading.Thread.Sleep(50);
                    return this.Checked == value;
                });
            }
        }

        public UICheckBox(IUIContainer container, By by, int? index)
            : base(container, by, index)
        { }

        public UICheckBox(IUIContainer container, By by)
            : base(container, by)
        { }

        public UICheckBox(By by) : base(by)
        {
        }

        public UICheckBox(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UICheckBox(IUIContainer container, String by="input[type='checkbox']")
            : this(container, By.CssSelector(by))
        { }

        public UICheckBox(String by)
            : this(By.CssSelector(by))
        { }

        public bool CheckByScript(bool toCheck = true)
        {
            bool originalState = Element.Selected;
            if (originalState == toCheck)
            {
                return toCheck;
            }

            string script = $"arguments[0].checked={toCheck.ToString().ToLower()}";
            ExecuteScript(script);
            return Element.Selected;
        }

        public override bool Perform(params string[] args)
        {
            if (args != null && args.Count() > 0)
            {
                string args0 = args[0];
                bool toCheck;
                if (bool.TryParse(args0, out toCheck))
                {
                    Executor.Until(() => this.Exists, 10 * 1000);
                    //First argument set Checked to true or false, second validate it is set correctly
                    this.PerformAction(() => this.Checked = toCheck);
                    return this.Checked == toCheck;
                }
            }
            return base.Perform(args);
        }
    }

}
