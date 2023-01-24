/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};



    /**
     * @private
     * @constructor ZoomInModel
     * @description Internal API. Constructs a model instance for IGViewer.ViewerComponents.ZoomIn.
     * @param {Object} options - the options for this component
     * @see {@link IGViewer.ViewerComponents.ZoomIn} for details on options
     */
    var ZoomInModel = function (options) {
        this.icon = options.icon;
        this.label = options.label || "";
        this.tooltip = options.tooltip;
        this.enabled = false;

        this.$element = null;
        this.callbacks = [];
    };



    /**
     * @private
     * @constructor ZoomInView
     * @description Internal class used to render the view for IGViewer.ViewerComponents.ZoomIn.
     * @param {ZoomInModel} model - the ZoomInModel with which the ZoomIn componet was instantiated
     */
    var ZoomInView = function (viewer, model) {
        this.viewer = viewer;
        this.model = model;
    };

    ZoomInView.prototype = {
        /**
         * @private
         * @method ZoomInView#render - Renders the html for this component
         */
        render: function () {

            var htmlTemplate = '<button class="btn navbar-btn">' +
                                   '<img class="ig-viewer-icon" src="' + this.model.icon + '" />' +
                                    this.model.label +
                               '</button>';

            this.model.$element = $('[data-ig-zoom-in]', this.viewer.model.$rootElement);
            var $element = this.model.$element;
            $element.html(htmlTemplate);
            $element.attr('title', this.model.tooltip);
        }
    };



    /**
     * @public
     * @constructor IGViewer.ViewerComponents.ZoomIn
     * @description Internal API. Used to create a ZoomIn viewer component.
     * @param {IGViewer.Viewer} viewer - this instance of IGViewer.Viewer to which this component belongs
     * @param {Object} options - the options for this component
     * @param {string} options.icon - (required) the url to the image to be used as an icon for this component
     * @param {string} [options.tooltip] - (optional) the tooltip to be displayed when the cursor hovers above the component
     * @param {string} [options.label] - (optional) the label to be displayed alongside the icon. Defaults to empty string
     */
    IGViewer.ViewerComponents.ZoomIn = function (viewer, options) {

        this.viewer = viewer;
        this.model = new ZoomInModel(options);
        this.view = new ZoomInView(this.viewer, this.model);

        this.initialize();
    };

    IGViewer.ViewerComponents.ZoomIn.prototype = {
        /**
         * @method IGViewer.ViewerComponents.ZoomIn#initialize - initialize this viewer component
         */
        initialize: function () {

            var _me = this;

            // render html template for this component
            this.view.render();

            // define callbacks
            var updateEnabled = (function () {
                if (this.viewer.viewerControl) {
                    var pageCount = this.viewer.viewerControl.getPageCount();
                    var nextValue = this.viewer.viewerControl.getScaleFactor() * 1.25;
                    this.model.enabled = pageCount != 0 && !(this.viewer.viewerControl.getMaxScaleFactor() < nextValue);

                    $('button', this.model.$element).prop("disabled", !this.model.enabled)
                }
            }).bind(this);

            var zoomIn = (function () {
                if (this.viewer.viewerControl) {
                    var currentValue = this.viewer.viewerControl.getScaleFactor();
                    this.viewer.viewerControl.setScaleFactor(currentValue * 1.25);
                }
            }).bind(this);

            // enable this component when ViewerControl is ready
            if (this.viewer.viewerControl) {
                function initOnPageCountReady() {
                    _me.viewer.viewerControl.off("PageCountReady", initOnPageCountReady);
                    updateEnabled();

                    // register callbacks
                    IGViewer.Util.registerCallbacks
                    (
                        [{
                            target: _me.viewer.viewerControl,
                            type: "ScaleChanged",
                            handler: updateEnabled
                        }, {
                            target: $("button", _me.model.$element),
                            type: "click",
                            handler: zoomIn
                        }],
                        _me.model.callbacks
                    );
                }

                this.viewer.viewerControl.on("PageCountReady", initOnPageCountReady);
            }
        },

        /**
         * @method IGViewer.ViewerComponents.ZoomIn#destroy - destroys this viewer component
         */
        destroy: function() {
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);
        }
    };
})();
