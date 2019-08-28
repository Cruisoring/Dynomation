using System;
using System.Collections.Generic;
using System.Text;

namespace UILocator.Interfaces
{
    public interface IUITextInput : IUIControl
    {
        void EnterText(string text);
    }
}
