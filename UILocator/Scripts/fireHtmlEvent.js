var event;
if (document.createEvent) {
    event = document.createEvent('HTMLEvents'); // for chrome and firefox
    event.initEvent('{0}', true, true);
    arguments[0].dispatchEvent(event); // for chrome and firefox
} else {
    event = document.createEventObject(); // for InternetExplorer
    event.eventType = '{0}';
    arguments[0].fireEvent('on' + event.eventType, event); // for InternetExplorer
}