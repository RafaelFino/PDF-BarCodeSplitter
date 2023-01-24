/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};



    /**
     * @private
     * @constructor RotatePageModel
     * @description Internal API. Constructs a model instance for IGViewer.ViewerComponents.RotatePage.
     * @param {Object} options - the options for this component
     * @see {@link IGViewer.ViewerComponents.RotatePage} for details on options
     */
    var RotatePageModel = function (options) {
        this.iconClockwise = options.iconClockwise;
        this.labelClockwise = options.labelClockwise || "";
        this.tooltipClockwise = options.tooltipClockwise;

        this.$elements = null;
        this.callbacks = [];
    };



    /**
     * @private
     * @constructor RotatePageView
     * @description Internal class used to render the view for IGViewer.ViewerComponents.RotatePage.
     * @param {RotatePageModel} model - the RotatePageModel with which the RotatePage componet was instantiated
     */
    var RotatePageView = function (viewer, model) {
        this.viewer = viewer;
        this.model = model;
    };

    RotatePageView.prototype = {
        /**
         * @private
         * @method RotatePageView#render - Renders the html for this component
         */
        render: function () {

            var htmlTemplate = '<div>' +
                                   '<button type="button" href="#" class="btn navbar-btn" data-ig-rotate-degrees="90">' +
                                       '<img class="ig-viewer-icon" src="' + this.model.iconClockwise + '" />' + this.model.labelClockwise +
                                   '</button>' +
                               '</div>';

            this.model.$elements = $("[data-ig-rotate-page]", this.viewer.model.$rootElement);
            this.model.$elements.html(htmlTemplate);
            var $buttons = $("button[data-ig-rotate-degrees]", this.model.$elements);
            for (var i = 0; i < $buttons.length; i++) {
                var $button = $($buttons[i]);
                var degs = parseInt($button[0].dataset.igRotateDegrees);
                $button.attr("title", this.model.tooltipClockwise);
            }
        }
    };



    /**
     * @public
     * @constructor IGViewer.ViewerComponents.RotatePage
     * @description Internal API. Used to create a RotatePage viewer component.
     * @param {IGViewer.Viewer} viewer - this instance of IGViewer.Viewer to which this component belongs
     * @param {Object} options - the options for this component
     * @param {string} options.iconClockwise - (required) the url to the image to be used as an icon for the button for clockwise rotation
     * @param {string} [options.labelClockwise] - (optional) the label to be displayed alongside the icon for the button for clockwise rotation
     */
    IGViewer.ViewerComponents.RotatePage = function (viewer, options) {

        this.viewer = viewer;
        this.model = new RotatePageModel(options);
        this.view = new RotatePageView(this.viewer, this.model);

        this.initialize();
    };

    IGViewer.ViewerComponents.RotatePage.prototype = {
        /**
         * @method IGViewer.ViewerComponents.RotatePage#initialize - initialize this viewer component
         */
        initialize: function () {

            var _me = this;

            // render html template for this component
            this.view.render();

            // enable this component when ViewerControl is ready
            if (this.viewer.viewerControl) {
                function initOnPageCountReady() {
                    _me.viewer.viewerControl.off("PageCountReady", initOnPageCountReady);
                    for (var i = 0; i < _me.model.$elements.length; i++) {
                        var $buttons = $("button[data-ig-rotate-degrees]", _me.model.$elements[i]);
                        for (var j = 0; j < $buttons.length; j++) {
                            IGViewer.Util.registerCallbacks
                            (
                                [{
                                    target: $buttons[j],
                                    type: "click",
                                    handler: (function (clickedElem) {
                                        return function () {
                                            _me.viewer.viewerControl.rotatePage(parseInt(clickedElem.dataset.igRotateDegrees));
                                        };
                                    })($buttons[j])
                                }],
                                _me.model.callbacks
                            );

                            _me.model.$elements.prop("disabled", false);
                        }
                    }
                }
                this.viewer.viewerControl.on("PageCountReady", initOnPageCountReady);
            }
        },

        /**
         * @method IGViewer.ViewerComponents.RotatePage#destroy - destroys this viewer component
         */
        destroy: function () {
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);
        }
    };

})();