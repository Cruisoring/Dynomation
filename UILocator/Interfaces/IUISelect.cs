using System;
using System.Collections.Generic;
using System.Text;

namespace UILocator.Interfaces
{
    public interface IUISelect : IUIControl
    {
        String[] OptionTexts { get; }
        string Select(string toBeSelected, bool select = true);
    }
}
