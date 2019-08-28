using System;
using System.Collections.Generic;
using System.Text;

namespace UILocator.Interfaces
{
    public interface IUITable : IUIControl
    {
        IUIControl RowOf(params object[] keys);
        IUIControl CellOf(params object[] keys);

        //IHtmlControl DistinctRowOf(params object[] keys);
        IUIControl DistinctCellOf(params object[] keys);

        IUIControl RowByColumn(string columnName, object columnValue);
        IUIControl CellByColumn(string columnName, object columnValue);

        string[] AllColumnText(string columnName, bool includeFirstRow = false);

        string CellTextOfRow(string columnName, params object[] keys);

        bool IsChanged(DateTime timestamp);

        bool Contains(string keyword);
    }
}
