using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UITable : UIControl, IUITable
    {
        public static By RowSelector = By.CssSelector("tbody>tr");

        public static By CellSelector = By.CssSelector("tbody>tr>td");
        public static By HeaderCellSelector = By.CssSelector("thead>tr>th");

        public static implicit operator UITable(By by)
        {
            return new UITable(by);
        }

        public class RowComparer : IEqualityComparer<IWebElement>
        {
            public bool Equals(IWebElement e1, IWebElement e2)
            {
                return e1.Text == e2.Text;
            }

            public int GetHashCode(IWebElement e1)
            {
                return e1.Text.GetHashCode();
            }
        }

        public static IEqualityComparer<IWebElement> textComparer = new RowComparer();

        public List<string> Headers
        {
            get
            {
                ReadOnlyCollection<IWebElement> headerCells = Element.FindElements(HeaderCellSelector);
                return headerCells.Select(h => h.Text).ToList();
            }
        }


        private DateTime timeBeforeChange = DateTime.MinValue;
        private string textBeforeChanged;

        public UITable(IUIContainer container, By by, int? index)
            : base(container, by, index)
        { }

        public UITable(IUIContainer container, By by)
            : base(container, by)
        { }

        public UITable(By by) : base(by) { }

        public UITable(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UITable(IUIContainer container, String by)
            : this(container, By.CssSelector(by))
        { }

        public UITable(String by)
            : base(By.CssSelector(by))
        { }

        public IUIControl this[int column, int row]
        {
            get
            {
                if (row < 0 || column < 0)
                    return null;

                string rowSelector = "tbody>" + "tr".CssSelectorOfNthOfType(row);
                string cellSelector = "td".CssSelectorOfNthOfType(column);
                By fullBy = By.CssSelector(rowSelector + ">" + cellSelector);
                return new UIControl(this, fullBy);
            }
        }

        public IUIControl this[string columnName, int row]
        {
            get
            {
                if (row < 0 || string.IsNullOrEmpty(columnName))
                    return null;

                int column = Headers.IndexOf(columnName);
                if (column == -1)
                    return null;

                return this[column, row];
            }
        }

        public string[] AllColumnText(string columnName, bool includeFirstRow = false)
        {
            int cellIndex = Headers.IndexOf(columnName);
            if (cellIndex == -1)
                return null;

            List<IWebElement> rows = Element.FindElements(RowSelector).ToList();
            //*/ Temporary solution when IE7.0 dosen't support 
            List<IWebElement> directRows = new List<IWebElement>();

            foreach (var r in rows)
            {
                if (directRows.Count == 0 || !r.IsYOverlap(directRows.Last()))
                    directRows.Add(r);
            }

            if (!includeFirstRow)
                directRows.RemoveAt(0);

            By cellByNth = By.CssSelector(string.Format("{0}", "td".CssSelectorOfNthOfType(cellIndex)));

            List<string> cellTextList = new List<string>();
            foreach (IWebElement row in directRows)
            {
                IWebElement cell = row.FindElement(cellByNth);
                cellTextList.Add(cell.Text);
            }

            return cellTextList.ToArray();
        }


        public IUIControl RowOf(params object[] keys)
        {
            string byString = this.by.ToString();
            //this.WaitControlReady();
            IWebElement row;
            int index;
            if (byString.Contains("CssSelector"))
            {
                index = byString.IndexOf(':');
                string[] subStrings = byString.Substring(index + 1).Split(',');
                IEnumerable<string> directRows = subStrings.Select(s => string.Format("{0}>tbody>tr", s));
                By directRowBy = By.CssSelector(string.Join(", ", directRows));
                List<IWebElement> rows = this.Container.FindElements(directRowBy).ToList();
                row = rows.FirstOrDefault(r => r.Text.ContainsAll(keys));
                if (row == null)
                {
                    Logger.D("Failed to find row containing '{0}'.", String.Join(", ", keys));
                    return null;
                }
                index = rows.IndexOf(row);
                By rowBy = By.CssSelector(string.Join(", ", subStrings.Select(s => string.Format("{0}>tbody>{1}", s, "tr".CssSelectorOfNthOfType(index)))));
                return new UIControl(this.Container, rowBy);
            }
            else
            {
                List<IWebElement> rows = Element.FindElements(RowSelector).ToList();
                //*/ Temporary solution when IE7.0 dosen't support 
                List<IWebElement> directRows = new List<IWebElement>();

                foreach (var r in rows)
                {
                    if (directRows.Count == 0 || !r.IsYOverlap(directRows.Last()))
                        directRows.Add(r);
                }
                //Log.V("Non overlapped rows: " + string.Join(" | ", directRows.Select(r => r.Text)));

                row = directRows.FirstOrDefault(r => r.Text.ContainsAll(keys));
                index = directRows.IndexOf(row);
                if (row == null)
                    return null;

                By rowByNth = By.CssSelector("tr".CssSelectorOfNthOfType(index));

                return new UIControl(this, rowByNth);
            }
        }

        public string CellTextOfRow(string columnName, params object[] keys)
        {
            IUIControl row = RowOf(keys);
            if (row == null)
                return null;

            int cellIndex = Headers.IndexOf(columnName);
            if (cellIndex == -1)
                return null;
            By cellByNth = By.CssSelector(string.Format("{0}", "td".CssSelectorOfNthOfType(cellIndex)));
            IUIControl cell = new UIControl(row, cellByNth);
            return cell.Text;
        }

        public IUIControl CellOf(params object[] keys)
        {
            IUIControl row = RowOf(keys);

            if (row == null)
                return null;

            string rowBy = row.by.ToString();
            int index;
            string[] subStrings = rowBy.Substring(rowBy.IndexOf(':') + 1).Split(',');
            IEnumerable<string> cellBy = subStrings.Select(s => string.Format("{0}>td", s));
            By directRowBy = By.CssSelector(string.Join(", ", cellBy));
            List<IWebElement> cells = row.Container.FindElements(directRowBy).ToList();
            index = cells.FindIndex(c => c.Text.ContainsAll(keys[0]));
            string cellLocator = String.Join(",", subStrings.Select(s => string.Format("{0}>{1}", s, "td".CssSelectorOfNthOfType(index))));
            return new UIControl(row.Container, By.CssSelector(cellLocator));
        }

        public IUIControl DistinctCellOf(params object[] keys)
        {
            List<IWebElement> rows = Element.FindElements(RowSelector).Distinct(textComparer).ToList();
            if (rows == null || rows.Count == 0)
                return null;
            IWebElement row = rows.FirstOrDefault(r => r.Text.ContainsAll(keys));
            if (row == null)
                return null;
            int index = rows.IndexOf(row);
            List<IWebElement> cells = row.FindElements(CellSelector).ToList();

            IWebElement cellMatchFirstKey = keys.Count() > 0 ? cells.Find(cell => cell.Text.ContainsAll(keys[0])) : cells[0];
            if (cellMatchFirstKey == null)
                return null;

            int cellIndex = cells.IndexOf(cellMatchFirstKey);
            if (cellIndex == -1)
                return null;

            By cellByNth = By.CssSelector(string.Format("{0} {1}", "tr".CssSelectorOfNthOfType(index), "td".CssSelectorOfNthOfType(cellIndex)));

            return new UIControl(this, cellByNth);
        }

        public IUIControl RowByColumn(string columnName, object columnValue)
        {
            int columnIndex = Headers.IndexOf(columnName);
            if (columnIndex == -1)
                return null;

            List<IWebElement> rows = Element.FindElements(RowSelector).ToList();
            string cellSelector = string.Format("td".CssSelectorOfNthOfType(columnIndex));
            //string cellSelector = string.Format("td:nth-of-type({0})", columnIndex+1);
            List<IWebElement> cellsOfColumn = rows.Select(row => row.FindElement(By.CssSelector(cellSelector))).ToList();
            int index = cellsOfColumn.FindIndex(c => c.Text == columnValue.ToString());
            if (index == -1)
            {
                index = cellsOfColumn.FindIndex(c => c.Text.ContainsAll(columnValue));
                if (index == -1)
                    index = cellsOfColumn.FindIndex(c => c.Text.ContainsIgnoreCase(columnValue));
            }

            return index == -1 ? null : new UIControl(this, By.CssSelector("tr".CssSelectorOfNthOfType(index)));
        }

        public IUIControl CellByColumn(string columnName, object columnValue)
        {
            int columnIndex = Headers.IndexOf(columnName);
            if (columnIndex == -1)
                return null;

            List<IWebElement> rows = Element.FindElements(RowSelector).ToList();
            if (rows.Count == 0)
                return null;
            By cellSelector = By.CssSelector(string.Format("td".CssSelectorOfNthOfType(columnIndex)));
            List<IWebElement> cellsOfColumn = rows.Select(row => row.FindElement(cellSelector)).ToList();
            int index = cellsOfColumn.FindIndex(c => c.Text == columnValue.ToString());
            if (index == -1)
            {
                index = cellsOfColumn.FindIndex(c => c.Text.ContainsAll(columnValue));
                if (index == -1)
                    index = cellsOfColumn.FindIndex(c => c.Text.ContainsIgnoreCase(columnValue));
            }

            if (index == -1)
                return null;

            By cellByNth = By.CssSelector(string.Format("{0} {1}", "tr".CssSelectorOfNthOfType(index), "td".CssSelectorOfNthOfType(columnIndex)));

            return new UIControl(this, cellSelector);
        }

        public bool IsChanged(DateTime timestamp)
        {
            if (!timeBeforeChange.Equals(timestamp))
            {
                timeBeforeChange = timestamp;
                textBeforeChanged = Element.Text;
                return false;
            }

            string currentText = Element.Text;
            return currentText != textBeforeChanged;
        }

        public bool Contains(string keyword)
        {
            string currentText = Element.Text;
            return currentText.Contains(keyword.Trim());
        }
    }

}
