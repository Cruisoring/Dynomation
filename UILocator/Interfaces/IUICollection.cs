using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenQA.Selenium;

namespace UILocator.Interfaces
{
    public interface IUICollection<T> : IReadOnlyCollection<T> where T : IUIControl
    {
        ReadOnlyCollection<T> Children { get; }
        T this[int index] { get; }

        T this[string key] { get; }

        T this[string key, Func<T, string> extractor] { get; }

        T this[Func<IWebElement, int, bool> predicate] { get; }
    }
}
