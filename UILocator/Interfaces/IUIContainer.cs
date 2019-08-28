using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace UILocator.Interfaces
{
    public interface IUIContainer : ISearchContext
    {
        string FramePath { get; }

        RemoteWebDriver Driver { get; }

        By Locator { get; }

        bool IsVisible { get; }

        bool WaitUntilVisible(int timeoutMillis = -1);

        bool WaitUntilGone(int timeoutMillis = -1);

        string Switch();
    }
}
