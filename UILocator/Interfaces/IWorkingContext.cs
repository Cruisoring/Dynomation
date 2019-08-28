using OpenQA.Selenium;
using UILocator.Browsers;

namespace UILocator.Interfaces
{
    public interface IWorkingContext : ISearchContext
    {
        Worker getWorker();
        void invalidate();
    }
}
