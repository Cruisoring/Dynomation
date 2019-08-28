if (document && document.readyState)
    return document.readyState;
else if (contentDocument && contentDocument.readyState)
    return contentDocument.readyState;
else if (document && document.parentWindow) return document.parentWindow.document.readyState;
    else return 'unknown';