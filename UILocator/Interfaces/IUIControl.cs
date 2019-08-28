using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace UILocator.Interfaces
{
    public interface IUIControl : IUIContainer
    {
        IUIContainer Container { get; }

        void Reset();
        By by { get; }
        int? Index { get; }
        RemoteWebElement Element { get; }

        string Text { get; }

        string AllText { get; }

        string InnerHTML { get; }

        string OuterHTML { get; }

        bool Exists { get; }

        bool IsDisabled { get; }

        void Click();

        void ClickByScript();

        bool Click(bool pageChanged, int waitTimeout = -1);

        bool Click(Func<bool> isClicked, int waitTimeout = -1, params Action[] actions);

        bool Perform(params string[] args);

        void PerformAction(Action action, Func<IWebElement, bool> validate = null, int retry = 1);

        object ExecuteScript(string script, params object[] args);

        bool Show(bool alignToTop = false);

        void FireEvent(string eventName);

        void Highlight(string highlightScript = null, string resetScript = null, int interval = -1, int times = -1);

        string GetAttribute(string attributeName);

        string GetProperty(string attributeName);

        string GetCssValue(string propertyName);

        bool WaitControlReady(int timeoutMillis = -1);

        IWebElement FindElement(By by, params object[] keys);
    }

}
