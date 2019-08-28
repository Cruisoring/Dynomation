using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using UILocator.Enums;
using UILocator.Interfaces;

namespace UILocator.Browsers
{
    public class Worker {
//    public static final Dimension DefaultMonitorDimension;
//    public static final Dimension TwoThirdVerticalDimension;
//    public static final Dimension HalfHorizontalDimension;
//    public static final Dimension HalfVerticalDimension;
//    public static final Dimension OneThirdHorizontalDimension;
//    public static final Dimension QuarterMonitorDimension;
//    public static final Dimension OneSixthMonitorDimension;
//
//    public static final Map<Integer, Rectangle[]> DefaultBrowserRectangls = new HashMap<Integer, Rectangle[]>();
//
//    public final static String FrameIndicator = ">";
//    public final static String RootFramePath = "";
//    public static final String DefaultMatchingHighlightScript =
//    "var e=arguments[0]; e.style.color='teal';e.style.backgroundColor='green';";
//    public static final String DefaultContainingHighlightScript =
//    "var e=arguments[0]; e.style.color='olive';e.style.backgroundColor='orange';";
//    public static final String DefaultMismatchHighlightScript =
//    "var e=arguments[0]; e.style.color='fuchsia';e.style.backgroundColor='red';";
//    public static final boolean AlwaysWaitPageReady = false;
//    public static final boolean WaitPageReadyBeforePerforming = false;
//    public static final boolean WaitPageReadyAfterPerforming = true;
//
//    public static final Boolean defaultWithProxy;
//
//    public static final EnumSet<ReadyState> pageLoadedStates = EnumSet.of(ReadyState.complete, ReadyState.loaded);
//    public static final EnumSet<ReadyState> pageLoadingStates = EnumSet.of(ReadyState.interactive, ReadyState.loading,
//        ReadyState.unknown);
//
//    public static final int DefaultSwitchFrameRetry = 1;
//    public static final long DefaultPageReadyTimeoutMills = 30 * 1000;
//    public static final long DefaultAlertHandlingTimeoutMills = 10 * 1000;
//    public static final long DefaultIntervalBetweenActions = 300;
//
//    private final static Path proxyFile = new File("C:/working/proxies.txt").toPath();
//
//    private static Lazy<Proxy[]> proxies = new Lazy<Proxy[]>(Worker::getProxies);
//
//    static {
//        GraphicsDevice gd = GraphicsEnvironment.getLocalGraphicsEnvironment().getDefaultScreenDevice();
//        int width = gd.getDisplayMode().getWidth();
//        int height = gd.getDisplayMode().getHeight() - 300;
//        DefaultMonitorDimension = new Dimension(width, height);
//        HalfHorizontalDimension = new Dimension(width / 2, height);
//        TwoThirdVerticalDimension = new Dimension(width, height);
//        HalfVerticalDimension = new Dimension(width, height / 2);
//        OneThirdHorizontalDimension = new Dimension(width / 3, height);
//        QuarterMonitorDimension = new Dimension(width / 2, height / 2);
//        OneSixthMonitorDimension = new Dimension(width / 3, height / 2);
//
//        DefaultBrowserRectangls.put(1, new Rectangle[] {new Rectangle(new Point(0, 0), TwoThirdVerticalDimension)});
//        DefaultBrowserRectangls.put(2, new Rectangle[]
//        {
//            new Rectangle(new Point(0, 0), HalfHorizontalDimension),
//            new Rectangle(new Point(width / 2, 0), HalfHorizontalDimension)
//        });
//        DefaultBrowserRectangls.put(3, new Rectangle[]
//        {
//            new Rectangle(new Point(0, 0), OneThirdHorizontalDimension),
//            new Rectangle(new Point(width / 3, 0), OneThirdHorizontalDimension),
//            new Rectangle(new Point(2 * width / 3, 0), OneThirdHorizontalDimension)
//        });
//        DefaultBrowserRectangls.put(4, new Rectangle[]
//        {
//            new Rectangle(new Point(0, 0), QuarterMonitorDimension),
//            new Rectangle(new Point(width / 2, 0), QuarterMonitorDimension),
//            new Rectangle(new Point(0, height / 2), QuarterMonitorDimension),
//            new Rectangle(new Point(width / 2, height / 2), QuarterMonitorDimension)
//        });
//        DefaultBrowserRectangls.put(5, new Rectangle[]
//        {
//            new Rectangle(new Point(width / 3, 0), OneThirdHorizontalDimension),
//            new Rectangle(new Point(0, 0), OneSixthMonitorDimension),
//            new Rectangle(new Point(0, height / 2), OneSixthMonitorDimension),
//            new Rectangle(new Point(width * 2 / 3, 0), OneSixthMonitorDimension),
//            new Rectangle(new Point(width * 2 / 3, height / 2), OneSixthMonitorDimension)
//        });
//        DefaultBrowserRectangls.put(6, new Rectangle[]
//        {
//            new Rectangle(new Point(width / 3, 0), OneSixthMonitorDimension),
//            new Rectangle(new Point(0, 0), OneSixthMonitorDimension),
//            new Rectangle(new Point(0, height / 2), OneSixthMonitorDimension),
//            new Rectangle(new Point(width * 2 / 3, height / 2), OneSixthMonitorDimension),
//            new Rectangle(new Point(width * 2 / 3, 0), OneSixthMonitorDimension),
//            new Rectangle(new Point(width * 2 / 3, height / 2), OneSixthMonitorDimension)
//        });
//
//        Properties properties = ResourceHelper.getProperties("driver.properties");
//        for (String propName :
//        properties.stringPropertyNames()) {
//            System.setProperty(propName, properties.getProperty(propName));
//        }
//
//        String defaultWithProxySetting = System.getProperty("defaultWithProxy");
//        String defaultWithProxyProperty = System.getProperty("defaultWithProxy");
//        defaultWithProxy = defaultWithProxyProperty == null ? false : Boolean.valueOf(defaultWithProxyProperty);
//    }
//
//    private static Proxy[] getProxies()
//    {
//        try
//        {
//            List<String> candidates = Files.lines(proxyFile).collect(Collectors.toList());
//            List<Proxy> proxyConverted = candidates.stream()
//                .map(line->asProxy(line)).filter(proxy->proxy != null)
//                .collect(Collectors.toList());
//
//            return proxyConverted.toArray(new Proxy[0]);
//        }
//        catch (IOException e)
//        {
//            e.printStackTrace();
//            return new Proxy[0];
//        }
//    }
//
//    private final static Pattern IPAddressPortPattern = Pattern.compile("/(\\d{1,3}.{3,3}\\d{1,3}):(\\d{2,5})");
//
//    private static Proxy asProxy(String line)
//    {
//        if (StringUtils.isEmpty(line))
//            return null;
//
//        Proxy proxy = null;
//        try
//        {
//            proxy = new Proxy();
//            int pos = line.indexOf(':');
//            String host = line.substring(line.lastIndexOf('/') + 1);
//            if (StringUtils.containsIgnoreCase(line, "HTTP"))
//            {
//                proxy.setHttpProxy(host);
//                proxy.setSslProxy(host);
//            }
//            else if (StringUtils.containsIgnoreCase(line, "SOCKS"))
//            {
//                proxy.setSocksProxy(host);
//                proxy.setSocksVersion(4);
//            }
//
//            return proxy;
//        }
//        catch (Exception e)
//        {
//            Logger.V(e);
//            return null;
//        }
//    }
//
//    public static Proxy getNextProxy()
//    {
//        return Randomizer.getRandom(proxies.getValue());
//    }
//
//    private static Pipe<Worker> activeWorkers = new Pipe<Worker>(5);
//
//    public static List<Worker> createAll(int intervalSeconds, DriverType...types)
//    {
//        List<Worker> workers = new ArrayList<>();
//        for (DriverType type :
//        types)
//        {
//            workers.add(create(type, intervalSeconds));
//        }
//        return workers;
//    }
//
//    public static Worker create(DriverType type)
//    {
//        return create(type, 1);
//    }
//
//    public static Worker create(DriverType type, int restInSeconds)
//    {
//        return create(type, restInSeconds, null);
//    }
//
//
//    public static Worker create(DriverType type, int restInSeconds, Capabilities capabilities)
//    {
//        return create(type, restInSeconds, capabilities, defaultWithProxy ? getNextProxy() : null);
//    }
//
//    public static Worker create(DriverType type, int restInSeconds, Capabilities capabilities, Proxy proxy)
//    {
//        Worker worker = null;
//        switch (type)
//        {
//            case Chrome:
//                worker = new Chrome(proxy, capabilities);
//                break;
//            case Edge:
//                worker = new Edge(proxy, capabilities);
//                break;
//            case Firefox:
//                worker = new Firefox(proxy, capabilities);
//                break;
//            case Opera:
//                worker = new Opera(proxy, capabilities);
//                break;
//            case IE:
//                worker = new IE(proxy, capabilities);
//                break;
//            default:
//                worker = null;
//                break;
//        }
//
//        activeWorkers.push(worker);
//        arrangeAll();
//        if (restInSeconds > 0)
//            Executor.sleep(restInSeconds * 1000);
//        return worker;
//    }
//
//    public static Rectangle arrangeByDefault(Worker worker)
//    {
//        Objects.requireNonNull(worker);
//
//        int workerCount = activeWorkers.size();
//        int index = activeWorkers.indexOf(worker);
//        if (index == -1)
//            return null;
//
//        Rectangle rect = DefaultBrowserRectangls.get(workerCount)[index];
//        return arrangeWindow(worker, rect);
//    }
//
//    protected static void arrangeAll()
//    {
//        try
//        {
//            List<Rectangle> rectangles = Executor.runParallel(Worker::arrangeByDefault,
//                activeWorkers.stream().collect(Collectors.toList()), 10 * 1000);
//        }
//        catch (Exception ex)
//        {
//            Logger.W(ex);
//        }
//    }
//
//    public static Boolean tryClose(Worker worker)
//    {
//        try
//        {
//            close(worker);
//            return true;
//        }
//        catch (Exception e)
//        {
//            return false;
//        }
//    }
//
//    public static void closeAll()
//    {
//        try
//        {
//            Executor.runParallel(Worker::tryClose,
//                activeWorkers.stream().collect(Collectors.toList()), 10 * 1000);
//        }
//        catch (Exception ex)
//        {
//            Logger.W(ex);
//        }
//    }
//
//    public static void close(Worker worker)
//    {
//        try
//        {
//            activeWorkers.pop(worker);
//            arrangeAll();
//        }
//        catch (Exception ex)
//        {
//            Logger.D(ex);
//        }
//    }
//
//    public static Rectangle arrangeWindow(Worker worker, Rectangle rect)
//    {
//        WebDriver.Window window = worker.driver.manage().window();
//        Point point = window.getPosition();
//        Dimension size = window.getSize();
//        Logger.V("Before: x=%d, y=%d, width=%d, height=%d", point.x, point.y, size.width, size.height);
//        window.setPosition(new Point(rect.x, rect.y));
//        window.setSize(new Dimension(rect.width, rect.height));
//        point = window.getPosition();
//        size = window.getSize();
//        Logger.D("After: (%d, %d, %d, %d) -> x=%d, y=%d, width=%d, height=%d",
//            rect.x, rect.y, rect.width, rect.height, point.x, point.y, size.width, size.height);
//        return rect;
//    }
//
//    public static Worker getAny(DriverType...types)
//    {
//        final DriverType[] intendedTypes = types.length == 0 ? DriverType.values() : types;
//        if (!activeWorkers.isEmpty())
//        {
//            Worker firstMatched = activeWorkers.stream()
//                .filter(worker->ArrayUtils.contains(intendedTypes, worker.driverType))
//                .findFirst().orElse(null);
//            if (firstMatched != null)
//            {
//                return firstMatched;
//            }
//        }
//
//        return create(intendedTypes[0]);
//    }
//
//
//    public static Worker getChrome(boolean withProxy)
//    {
//        return getChrome(withProxy, false);
//    }
//
////    public static Worker getChrome(String... arguments) {
////        return new Chrome(arguments);
////    }
//
//    public static Worker getEdge()
//    {
//        return new Edge();
//    }
//
//    public static Worker getIE()
//    {
//        return new IE();
//    }
//
//    public static Worker getOpera()
//    {
//        return new Opera();
//    }
//
//    public static Worker getFirefox()
//    {
//        return new Firefox();
//    }
//
//    public static Worker getChrome(boolean withProxy, boolean inHeadless, String...arguments)
//    {
//        return new Chrome(withProxy, inHeadless, arguments);
//    }
//
//
//    public static String mergeFramePath(String parentFramePath, String framePath)
//    {
//        if (!StringUtils.isNotBlank(framePath))
//            return parentFramePath;
//
//        framePath = normalizeFramePath(framePath);
//        return StringUtils.isNotBlank(parentFramePath)
//            ? String.join(FrameIndicator, parentFramePath, framePath)
//            : framePath;
//    }
//
//    public static String normalizeFramePath(String original)
//    {
//        return Arrays.stream(getFrames(original))
//            .filter(StringUtils::isNotBlank)
//            .reduce((s1, s2)->s1 + FrameIndicator + s2).orElse(RootFramePath);
//    }
//
//    public static String[] getFrames(String framePath)
//    {
//        framePath = StringUtils.isBlank(framePath) ? RootFramePath : framePath;
//        return framePath.split("[>&$ ]", 0);
//    }
//
//    public RemoteWebDriver driver;
//
//    public final DriverType driverType;
//
//    protected Worker(DriverType driverType)
//    {
//        Objects.requireNonNull(driverType);
//
//        this.driverType = driverType;
//    }
//
//    public URL getUrl()
//    {
//        try
//        {
//            return new URL(driver.getCurrentUrl());
//        }
//        catch (MalformedURLException e)
//        {
//            Logger.I(e);
//            return null;
//        }
//    }
//
//    public String asBase64()
//    {
//        try
//        {
//            String base64 = ((TakesScreenshot) driver).getScreenshotAs(OutputType.BASE64);
//            return String.format("<img src='data:image/png;base64, %s'/>", base64);
//        }
//        finally
//        {
//            return "";
//        }
//    }
//
//    public static final String replaceImageAsBase64Script = "if (typeof window.replaceImage === 'undefined') {\n" +
//    "var canvas = canvas || document.createElement('canvas');\n" +
//    "var ctx = ctx || canvas.getContext('2d');\n" +
//    "window.replaceImage = function(e){\n" +
//    "if(e.tagName == 'IMG'){\n" +
//    "canvas.width = e.width;\n" +
//    "canvas.height = e.height;\n" +
//    "ctx.drawImage(e, 0, 0);\n" +
//    "e.src = canvas.toDataURL();\n" +
//    "return true;}else {return false;}\n" +
//    "}\n" +
//    "}\n" +
//    "replaceImage(arguments[0]);";
//
//    public static final String getImageBase64 = "var e = arguments[0];\n" +
//    "var canvas = document.createElement('canvas');\n" +
//    "canvas.width = e.width;\n" +
//    "canvas.height = e.height;\n" +
//    "canvas.getContext('2d').drawImage(e, 0, 0);\n" +
//    "return canvas.toDataURL();";
//
//    public boolean replaceImageAsBase64(UIObject image)
//    {
//        return (boolean) image.executeScript(replaceImageAsBase64Script);
//    }
//
//    public final Map<Class<? extends Screen>, Screen> screens = new ConcurrentHashMap<>();
//
//    public <T extends Screen> T getScreen(Class<T> screenClass)
//    {
//        if (screens.containsKey(screenClass))
//        {
//            return (T) screens.get(screenClass);
//        }
//
//        T instance = (T) Screen.screenRepository.getFirst(screenClass).orElse(null).apply(this);
//        if (instance != null)
//        {
//            screens.put(screenClass, instance);
//        }
//
//        return instance;
//    }
//
//    protected List<String> currentFrames = new ArrayList<String>();
//
//    public String getCurrentFramePath()
//    {
//        return String.join(FrameIndicator, currentFrames);
//    }
//
//    public Worker getWorker()
//    {
//        return this;
//    }
//
//    public String gotoUrl(String url)
//    {
//        driver.get(url);
//        waitPageReady(10 * 1000);
//        String currentUrl = driver.getCurrentUrl();
//        Logger.I("get to: " + currentUrl);
//        return currentUrl;
//    }
//
//    public String gotoUrl(String url, int waitMills)
//    {
//        driver.get(url);
//        waitPageReady(waitMills);
//        String currentUrl = driver.getCurrentUrl();
//        Logger.I("get to: " + currentUrl);
//        return currentUrl;
//    }
//
//    public String gotoUrl(URL url)
//    {
//        return gotoUrl(url.toString());
//    }
//
//    public void goBack()
//    {
//        driver.navigate().back();
//        waitPageReady();
//    }
//
//    public String switchTo(String framePath)
//    {
//        return switchTo(framePath, DefaultSwitchFrameRetry);
//    }
//
//    public String switchTo(String framePath, int retry)
//    {
//        framePath = framePath == null ? "" : framePath;
//
//        String oldFramePath = getCurrentFramePath();
//        if (StringUtils.equalsIgnoreCase(oldFramePath, framePath))
//        {
//            return framePath;
//        }
//
//        //Assume the page has been reloaded
//        driver.switchTo().defaultContent();
//        //lastUrl = driver.getCurrentUrl();
//        currentFrames.clear();
//
//        String[] targetFrames = getFrames(framePath);
//        int lastIdenticalIndex = -1;
//        int max = Math.min(currentFrames.size(), targetFrames.length);
//        for (int i = 0; i < max; i++)
//        {
//            if (currentFrames.get(i) == targetFrames[i])
//            {
//                lastIdenticalIndex = i;
//            }
//            else
//            {
//                break;
//            }
//        }
//
//        while (currentFrames.size() - 1 > lastIdenticalIndex)
//        {
//            driver.switchTo().parentFrame();
//            currentFrames.remove(currentFrames.size() - 1);
//        }
//
//        for (int i = lastIdenticalIndex + 1; i < targetFrames.length; i++)
//        {
//            String nextFrame = targetFrames[i];
//            try
//            {
//                if (StringUtils.isNotBlank(nextFrame))
//                {
//                    driver.switchTo().frame(nextFrame);
//                }
//
//                currentFrames.add(nextFrame);
//            }
//            catch (NoSuchFrameException ex)
//            {
//                Logger.W("Failed to switch to frame '%s'.", nextFrame);
//                if (retry > 0)
//                {
//                    driver.switchTo().defaultContent();
//                    currentFrames.clear();
//                    return switchTo(framePath, retry--);
//                }
//
//                String currentFramePath = getCurrentFramePath();
//                Logger.E("Failed to switch from '%s' to frame of '%s'.", currentFramePath, nextFrame);
//                return currentFramePath;
//            }
//        }
//
//        String currentFramePath = getCurrentFramePath();
//        if (!StringUtils.equals(oldFramePath, currentFramePath))
//            Logger.I("Switch Browser from '%s' to '%s'.", oldFramePath, currentFramePath);
//        //lastUrl = driver.getCurrentUrl();
//        return currentFramePath;
//    }
//
//    public final static String getReadyStateScript = "if(document && document.readyState)return document.readyState;"
//                                                     + "else if(contentDocument && contentDocument.readyState) return contentDocument.readyState;"
//                                                     + "else if(document && document.parentWindow) return document.parentWindow.document.readyState;"
//                                                     + "else return 'unknown';";
//
//    public String getReadyState()
//    {
//        return driver.executeScript(getReadyStateScript).toString();
//    }
//
//    protected Boolean waitForReadyState(long timeoutMillis, EnumSet<ReadyState> expectedStates)
//    {
//        Boolean isReady = Executor.testUntil(()-> {
//            ReadyState readyState = ReadyState.uninitialized;
//            readyState = ReadyState.fromString(getReadyState());
//            return readyState.isReady();
//        }, timeoutMillis);
//        return isReady;
//    }
//
//    public Boolean waitPageReady(long timeoutMills)
//    {
//        return waitForReadyState(timeoutMills, pageLoadedStates);
//    }
//
//    public Boolean waitPageReady()
//    {
//        //        Logger.V("Start waitPageReady");
//        boolean isReady = waitForReadyState(DefaultPageReadyTimeoutMills, pageLoadedStates);
//        //        Logger.V("End of waitPageReady");
//        return isReady;
//    }
//
//    public final static String isAjaxDoneScript = "return jQuery.active == 0";
//
//    public Boolean isAjaxDone()
//    {
//        Boolean result = (Boolean) driver.executeScript(isAjaxDoneScript);
//        return result;
//    }
//
//    public Boolean waitAjaxDone(long timeoutMills)
//    {
//        return Executor.testUntil(()->isAjaxDone(), timeoutMills);
//    }
//
//    public Boolean waitAjaxDone()
//    {
//        return waitAjaxDone(DefaultPageReadyTimeoutMills);
//    }
//
//    @Override
//
//    public void invalidate()
//    {
//        screens.clear();
//    }
//
//    @Override
//    public void close() throws Exception {
//        if (driver == null) {
//        return;
//    }
//
//    try {
//        Set<String> windows = driver.getWindowHandles();
//        if (windows.size() == 0)
//        {
//            return;
//        }
//
//        driver.executeScript("window.onbeforeunload = function(e){};");
//        String driverDesc = driver.toString();
//        driver.close();
//        Logger.V("Driver '%s' is closed successfully.", driverDesc);
//
//        if (activeWorkers.indexOf(this) != -1)
//        {
//            Tuple3<Boolean, Worker, String> result = activeWorkers.pop(this);
//            arrangeAll();
//            Executor.sleep(1000);
//        }
//    } catch (Exception ex) {
//        Logger.W(ex);
//    } finally {
//        driver.quit();
//    }
//    }
//
//    public Boolean closeDialog(Consumer<Alert> alertHandler, long timeoutMills)
//    {
//    if (alertHandler == null)
//    {
//        alertHandler = alert->alert.accept();
//    }
//
//    LocalDateTime timeout = LocalDateTime.now().plus(Duration.ofMillis(timeoutMills));
//        do
//
//    {
//        try
//        {
//            Alert alert = driver.switchTo().alert();
//            alertHandler.accept(alert);
//            return true;
//        }
//        catch (NoAlertPresentException noAlert)
//        {
//            try
//            {
//                TimeUnit.SECONDS.sleep(1);
//            }
//            catch (InterruptedException e)
//            {
//                Logger.V(e);
//            }
//        }
//    } while (LocalDateTime.now().isBefore(timeout));
//        return false;
//    }
//
//    public boolean acceptDialog()
//    {
//    return closeDialog(Alert::accept, DefaultAlertHandlingTimeoutMills);
//    }
//
//    public boolean dismissDialog()
//    {
//    return closeDialog(Alert::dismiss, DefaultAlertHandlingTimeoutMills);
//    }
//
//    public String getElementAttribute(WebElement element, String attributeName)
//    {
//    try
//    {
//        if (element == null)
//            throw new NullPointerException("Element is null");
//        if (StringUtils.isBlank(attributeName))
//            throw new IllegalArgumentException("Attribute Name cannot be null or empty.");
//        return element.getAttribute(attributeName);
//    }
//    catch (Exception ex)
//    {
//        Logger.W(ex.getMessage());
//        return null;
//    }
//    }
//
//    public List<WebElement> findElements(By by)
//    {
//    return driver.findElements(by);
//    }
//
//    public WebElement findElement(By by)
//    {
//    return driver.findElement(by);
//    }
//
//    public static final String isVisibleScipt =
//    "var e=arguments[0].getBoundingClientRect(); return e.width>0 && e.height>0;";
//
//    public Boolean isVisible(WebElement e)
//    {
//    JavascriptExecutor executor = driver;
//        try
//
//    {
//        if (e == null) return false;
//        if (e.isDisplayed())
//            return true;
//        String result = executor.executeScript(isVisibleScipt, e).toString();
//        return Boolean.parseBoolean(result);
//    }
//    catch (Exception ex)
//    {
//        try
//        {
//            String result = executor.executeScript(isVisibleScipt, e).toString();
//            return Boolean.parseBoolean(result);
//        }
//        catch (Exception e2)
//        {
//            Logger.V("%s with %s", ex.getClass().getSimpleName(), e.getTagName());
//            return false;
//        }
//    }
//    }
//
//    public static final String isDisabledScript =
//    "var e=arguments[0];if(false||!!document.documentMode)return e.disabled;if(e.getAttribute('disabled')!==null)return true;var clazz=e.getAttribute('class');return clazz!=null && clazz.includes('disabled');"
//    ;
//
//    public Boolean isDisabled(WebElement e)
//    {
//    try
//    {
//        if (e == null) return true;
//        JavascriptExecutor executor = driver;
//        String result = executor.executeScript(isDisabledScript, e).toString();
//        return Boolean.parseBoolean(result);
//    }
//    catch (Exception ex)
//    {
//        Logger.V("%s with %s", ex.getClass().getSimpleName(), e.getTagName());
//        return true;
//    }
//    }
//
//    public Boolean scrollTo(int xOffset, int yOffset)
//    {
//    Boolean toPosition = (Boolean) driver.executeScript(
//            "window.scrollTo(arguments[0], arguments[1]); return (arguments[0]==window.scrollX) || (arguments[1]==window.scrollY);",
//            xOffset, yOffset);
//        return toPosition;
//    }
//
//    public Boolean scrollBy(int xStep, int yStep)
//    {
//    Boolean isScrolled = (Boolean) driver.executeScript(
//            "var xPos = window.scrollX; var yPos = window.scrollY; window.scrollBy(arguments[0], arguments[1]); return (xPos!=window.scrollX) || (yPos!=window.scrollY);",
//            xStep, yStep);
//        return isScrolled;
//    }
//
//    public Boolean scrollSmoothly(int xStep, int yStep, int interval)
//    {
//    Boolean result = Executor.testUntil(()-> !scrollBy(xStep, yStep), 20 * 1000, interval, 0);
//    Logger.V("%sscrolled to %s in 20s.", result? "" : "Not ", yStep > 0 ? "bottom" : "top");
//    return result;
//    }
//
//    public Rectangle arrangeWindow(Rectangle rect)
//    {
//    return arrangeWindow(this, rect);
//    }
}

}