using System;
using System.Collections.Generic;
using System.Text;

namespace UILocator.Interfaces
{
    public interface IUIRadioButton : IUIControl
    {
        bool Checked { get; set; }
    }
}
