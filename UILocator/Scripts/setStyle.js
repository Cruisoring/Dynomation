if (!rzCC) {
    // convert s to camel case
    function rzCC(s) {
        // thanks http://www.ruzee.com/blog/2006/07/\
        // retrieving-css-styles-via-javascript/
        for (var exp = /-([a-z])/;
            exp.test(s);
            s = s.replace(exp, RegExp.$1.toUpperCase()));
        return s;
    }

    function getStyle(e, a) {
        var v = null;
        if (document.defaultView && document.defaultView.getComputedStyle) {
            var cs = document.defaultView.getComputedStyle(e, null);
            if (cs && cs.getPropertyValue) v = cs.getPropertyValue(a);
        }
        if (!v && e.currentStyle) v = e.currentStyle[rzCC(a)];
        return v;
    };

    function setStyle(element, declaration) {
        if (declaration.charAt(declaration.length - 1) == ';')
            declaration = declaration.slice(0, -1);
        var pair, k, v, old = '';
        var splitted = declaration.split(';');
        for (var i = 0, len = splitted.length; i < len; i++) {
            k = rzCC(splitted[i].split(':')[0]);
            v = getStyle(element, k);
            old = old + k + ': ' + v + ';';
            v = splitted[i].split(':')[1];

            eval('element.style.' + k + '=\'' + v + '\'');
        }
        return old;
    }
}
return setStyle(arguments[0], arguments[1]);