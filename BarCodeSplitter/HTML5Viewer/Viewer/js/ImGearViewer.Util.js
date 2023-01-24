/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.Util = IGViewer.Util || {};



    /**
     * Returns a unique identifier string
     * @param {string} prefix - (optional) an optional prefix to prepend to the unique identifier
     * @returns {string} a unique identifier string
     */
    IGViewer.Util.uniqueId = (function () {
        var summand = 45796814111,
            modulus = 70719928717,
            cur = Math.floor(Math.random() * modulus);
        return function (prefix) {
            return (typeof (prefix) === "string" ? prefix : "") + (cur = (cur + summand) % modulus);
        }
    })();

    /**
     * @typedef IGViewer.Util.CallbackEntry
     * @type {object}
     * @property {string|Element|Window|jQuery|IPCC.ViewerControl} target - the target of the callback
     * @property {string} type - the event type the callback is registered with
     * @property {function} handler - the callback method that was registered with target for type
     */

    /**
     * Registers an array of callbacks and stores the registered callbacks in cbStore
     * @param {IGViewer.Util.CallbackEntry[]} cbArray - an array of callbacks to register
     * @param {Array} cbStore - an array in which the registered callbacks should be stored
     */
    IGViewer.Util.registerCallbacks = function (cbArray, cbStore) {
        if (!((cbArray instanceof Array) && (cbStore instanceof Array)))
            throw new Error("IGViewer.Util.RegisterCallbacks: cbArray and cbStore must be arrays");
        for (var i = 0; i < cbArray.length; i++) {
            var cbEntry = cbArray[i];
            if (cbEntry.target instanceof $ || cbEntry.target instanceof IPCC.ViewerControl)
                cbEntry.target.on(cbEntry.type, cbEntry.handler);
            else if (typeof (cbEntry.target) === "string" || cbEntry.target instanceof Element || cbEntry.target instanceof Window)
                $(cbEntry.target).on(cbEntry.type, cbEntry.handler);
            else {
                throw new Error("IGViewer.Util.registerCallbacks: invalid target:" + cbEntry.target);
            }
            cbStore.push(cbEntry);
        }
    };

    /**
     * Unregisters an array of previously registered callbacks
     * @param {IGViewer.Util.CallbackEntry[]} cbStore - an array containing previously registered callbacks
     */
    IGViewer.Util.unregisterCallbacks = function (cbStore) {
        if (!(cbStore instanceof Array))
            throw new Error("IGViewer.Util.UnregisterCallbacks: cbStore must be an array");
        while (cbStore.length != 0) {
            var cbEntry = cbStore.shift();
            if (cbEntry.target instanceof $ || cbEntry.target instanceof IPCC.ViewerControl)
                cbEntry.target.off(cbEntry.type, cbEntry.handler);
            else if (typeof (cbEntry.target) === "string" || cbEntry.target instanceof Element || cbEntry.target instanceof Window)
                $(cbEntry.target).off(cbEntry.type, cbEntry.handler);
            else
                throw new Error("IGViewer.Util.registerCallbacks: invalid target:" + cbEntry.target);
        }
    };

    /**
     * Ensures that the passed URL-string ends with a forward slash
     * @param {string} urlString - a URL string
     * @returns {string} - the same URL passed in with a forward slash appended if necessary
     */
    IGViewer.Util.endWithSlash = function (urlString) {
        if (typeof (urlString) === "string" && urlString.length > 0)
            return urlString.charAt(urlString.length - 1) === "/" ? urlString : urlString + "/";
        else
            return "";
    }


    /**
     * Returns a the specified callback function "debounced" at a maximum of the specified interval. If msTimeout
     *      is not a number, or msTimout <= 0, will return fn.
     * @param {function} fn - the function to be debounced
     * @param {number} msTimeout - the millisecond timout at which it should be debounced
     * @returns {function} - the debounced callback
     */
    IGViewer.Util.debounce = function (func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };

})();