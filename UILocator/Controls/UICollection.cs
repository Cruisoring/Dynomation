using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using UILocator.Interfaces;

namespace UILocator.Controls
{
    public class UICollection<T> : UIControl, IUICollection<T>
        where T : UIControl
    {
        public const int COLLECTION_WAIT_MILLS = 3000;
        public static readonly By ANY_BY = By.CssSelector("*");

        public const string ANY = "ANY";
        public const string FIRST = "FIRST";
        public const string LAST = "LAST";
        public static Random random = new Random();
        public readonly static By Any = By.CssSelector(".");

        static UICollection()
        {
            Type type = typeof(T);
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            foreach (var constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                    continue;
                else
                {
                    Type lastParameterType = parameters.Last().ParameterType;
                    if (lastParameterType != typeof(int) && lastParameterType != typeof(int?))
                        continue;
                }
                return;
            }
            throw new ArgumentException(string.Format("Type {0} must have constructor(s) with index like T(IContainer, By, int)", type.Name));
        }

        #region Constructors

        public UICollection(IUIContainer container, By by, int? index)
            : base(container, by, index)
        {
        }

        public UICollection(IUIContainer container, By by) : this(container, by, null)
        {
        }

        public UICollection(By by) : this(null, by)
        { }


        public UICollection(IUIContainer container, String by, int? index)
            : this(container, By.CssSelector(by), index)
        { }

        public UICollection(IUIContainer container, String by)
            : this(container, By.CssSelector(by), null)
        { }

        public UICollection(String by)
            : this(By.CssSelector(by))
        { }

        #endregion
//        public By ChildrenBy { get; protected set; }

        public override void Reset()
        {
            children = null;
        }

        protected ReadOnlyCollection<T> children;

        public virtual ReadOnlyCollection<T> Children
        {
            get
            {
                if (children != null && this == currentUIControl && DateTime.Now < currentControlValidUntil)
                {
                    return children;
                }

                currentUIControl = this;
                currentControlValidUntil = DateTime.Now + ValidElementTimeSpan;

                List<T> list = new List<T>();
                try
                { 
                    ReadOnlyCollection<IWebElement> elements = Container.FindElements(by);

                    for (int i = 0; i < elements.Count; i++)
                    {
                        T t = (T)Activator.CreateInstance(typeof(T), new object[] { Container, by, i });
                        list.Add(t);
                    }
                    children = new ReadOnlyCollection<T>(list);

                    return children;
                }
                catch (Exception e)
                {
                    Logger.W(e.Message);
                    return null;
                }
            }
        }

        public T this[int index]
        {
            get { return Math.Abs(index) >= Count ? default(T) : Children[(index + Count) % Count]; }
        }

        public virtual T this[string key]
        {
            get { return this[key, c => c.Text]; }
        }

        protected T Match(string key, Func<T, string> extractor, int retry = 3)
        {
            try
            {
                key = key.Trim().ToUpper();
                if (string.Equals(key, ANY))
                {
                    int randomIndex = random.Next(0, Count);
                    return Children[randomIndex];
                }
                else if (string.Equals(key, FIRST))
                    return Children.FirstOrDefault();
                else if (string.Equals(key, LAST))
                    return Children.LastOrDefault();

                List<string> values = new List<string>();
                foreach (T t in Children)
                {
                    values.Add(extractor(t));
                }

                List<string> matched = values.Where(v => string.Equals(v, key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matched.Count == 0)
                    matched = values.Where(v => v.ContainsIgnoreCase(key)).ToList();
                int index = -1;
                if (matched.Count == 0)
                {
                    key = key.ToUpper();
                    if (key == FIRST)
                        return Children.FirstOrDefault();
                    else if (key == LAST)
                        return Children.LastOrDefault();

                    if (int.TryParse(key, out index))
                    {
                        if (index < 0)
                            index = values.Count + index;

                        if (index < 0 || index >= values.Count)
                            throw new ArgumentOutOfRangeException("Cannot find ListItem specified by: " + key);
                        return Children[index];
                    }
                    else
                    {
                        throw new ArgumentException("Cannot find ListItem specified by: " + key);
                    }
                }
                else
                {
                    if (matched.Count == 1)
                        return Children[values.IndexOf(matched[0])];

                    //Or, select the ListItem best matched
                    List<string> best =
                        matched.Where(v => Regex.IsMatch(v.ToUpper(), string.Format(@"\b{0}\b", key.ToUpper())))
                            .ToList();

                    if (best.Count == 1)
                        return Children[values.IndexOf(best[0])];
                    //Otherwise, select the shortest
                    string shortest = matched.OrderBy(s => s.Length).First();
                    return Children[values.IndexOf(shortest)];
                }
            }
            catch(Exception e)
            {
                if (retry > 0)
                {
                    Reset();
                    return Match(key, extractor, retry - 1);
                }
                else
                {
                    Logger.W(e.Message);
                    throw e;
                }
            }
        }

        public virtual T this[string key, Func<T, string> extractor]
        {
            get { return Match(key, extractor); }
        }

        public T this[Func<IWebElement, int, bool> predicate]
        {
            get
            {
                try
                {
                    var validChildren = Children.Where(c => c.IsVisible);
                    var matched = validChildren.Where((c, i) => predicate(c.Element, i));
                    return matched.FirstOrDefault();
                }
                catch
                {
                    try
                    {
                        element = null;
                        element = Element;
                        var candidates = element.FindElements(by);
                        var matched = candidates.Where((e, i) => e.Displayed && predicate(e, i)).FirstOrDefault();
                        if (matched != null)
                            return (T)Activator.CreateInstance(typeof(T), new object[] { this, by, candidates.IndexOf(matched) });
                        return null;
                    }
                    catch (Exception exception)
                    {
                        Logger.E(exception.Message);
                        return null;
                    }
                }
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return Children?.GetEnumerator() ?? new List<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children?.GetEnumerator() ?? new List<T>().GetEnumerator();
        }

        public int Count => Children.Count;

        public override string AllText
        {
            get
            {
                ReadOnlyCollection<IWebElement> elements;
                if (Container == null)
                    elements = Driver.FindElements(by);
                else
                    elements = Container.FindElements(by);

                List<string> texts = elements.Select(e => e.Text).ToList();
                return String.Join("/t", texts);
            }
        }

        public override bool IsVisible
        {
            get { return Children.Any(c => c.IsVisible); }
        }
    }

}
