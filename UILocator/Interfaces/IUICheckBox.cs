using System;
using System.Collections.Generic;
using System.Text;

namespace UILocator.Interfaces
{
    public interface IUICheckBox : IUIControl
    {
        bool Checked { get; set; }
    }
}
