using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UIRadioButton : UIControl, IUIRadioButton
    {
        public static bool WaitEnabledBeforeCheckingRadio = true;
        public static Action<UIRadioButton, bool>[] AllMeans = new Action<UIRadioButton, bool>[]
        {
            (radio, toCheck) =>
            {
                /*/
                IMouse mouse = (radio.Element.WrappedDriver as RemoteWebDriver).Mouse;
                ICoordinates where = radio.Element.Coordinates;
                mouse.MouseMove(where, 1, 1);
                mouse.Click(where);
                /*/
                Actions action = new Actions(radio.Element.WrappedDriver);
                IAction act =action.MoveToElement(radio.Element).Click().Build();
                act.Perform();
                //*/
            },
            (radio, toCheck) => radio.ClickByScript(),  //May not trigger onClick() event
            (radio, toCheck) => radio.ExecuteScript("arguments[0].checked=arguments[1]", toCheck.ToString().ToLower()), //May not trigger onClick() event
            (radio, toCheck) => radio.Element.Click(),  //Doesn't work for InternetExplorer???????
        };

        public static implicit operator UIRadioButton(By by)
        {
            return new UIRadioButton(by);
        }

        public bool Checked
        {
            get
            {
                //this.element = null;
                return Element.Selected;
            }
            set
            {
                PerformAction(() => Element.Click(), chk =>
                {
                    System.Threading.Thread.Sleep(50);
                    return this.Checked == value;
                });
            }
        }

        public UIRadioButton(IUIContainer container, By by, int? index)
            : base(container, by, index)
        { }

        public UIRadioButton(IUIContainer container, By by)
            : base(container, by)
        {
        }

        public UIRadioButton(By by)
            : base(by)
        {
        }

        public UIRadioButton(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UIRadioButton(IUIContainer container, String by)
            : this(container, By.CssSelector(by))
        { }

        public UIRadioButton(String by)
            : base(By.CssSelector(by))
        { }

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
                    //this.PerformAction((c => (c as IHtmlCheckBox).Checked = toCheck), (e => (e as RemoteWebElement).Selected == toCheck));
                    //First argument set Checked to true or false, second validate it is set correctly
                    this.PerformAction(() => this.Checked = toCheck);
                    return this.Checked == toCheck;
                }
            }
            return base.Perform(args);
        }
    }

}
