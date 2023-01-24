/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};



    /**
     * @private
     * @constructor NextPageModel
     * @description Internal API. Constructs a model instance for IGViewer.ViewerComponents.NextPage.
     * @param {Object} options - the options for this component
     * @see {@link IGViewer.ViewerComponents.NextPage} for details on options
     */
    var NextPageModel = function (options) {
        this.icon = options.icon;
        this.label = options.label || "";
        this.tooltip = options.tooltip;
        this.viewerControlReady = false;
        this.enabled = false;

        this.$elements = null;
        this.callbacks = [];
    };



    /**
     * @private
     * @constructor NextPageView
     * @description Internal class used to render the view for IGViewer.ViewerComponents.NextPage.
     * @param {NextPageModel} model - the NextPageModel with which the NextPage componet was instantiated
     */
    var NextPageView = function (viewer, model) {
        this.viewer = viewer;
        this.model = model;
    };

    NextPageView.prototype = {
        /**
         * @private
         * @method NextPageView#render - Renders the html for this component
         */
        render: function () {

            var htmlTemplate = '<button href="#" class="btn navbar-btn">' +
                                   '<img class="ig-viewer-icon" src="' + this.model.icon + '" />' +
                                   this.model.label +
                               '</button>';

            this.model.$elements = $("[data-ig-next-page]", this.viewer.model.$rootElement);
            this.model.$elements.html(htmlTemplate);
            if (typeof (this.model.tooltip) === "string") {
                $("button", this.model.$elements).attr("title", this.model.tooltip);
            }
        }
    };



    /**
     * @public
     * @constructor IGViewer.ViewerComponents.NextPage
     * @description Internal API. Used to create a NextPage viewer component.
     * @param {IGViewer.Viewer} viewer - this instance of IGViewer.Viewer to which this component belongs
     * @param {Object} options - the options for this component
     * @param {string} options.icon - (required) the url to the image to be used as an icon for this component
     * @param {string} [options.tooltip] - (optional) the tooltip to be displayed when the cursor hovers above the component
     * @param {string} [options.label] - (optional) the label to be displayed alongside the icon. Defaults to empty string
     */
    IGViewer.ViewerComponents.NextPage = function (viewer, options) {

        this.viewer = viewer;
        this.model = new NextPageModel(options);
        this.view = new NextPageView(this.viewer, this.model);

        this.initialize();
    };

    IGViewer.ViewerComponents.NextPage.prototype = {
        /**
         * @method IGViewer.ViewerComponents.NextPage#initialize - initialize this viewer component
         */
        initialize: function () {

            var _me = this;

            // render html template for this component
            this.view.render();

            // define callbacks
            var updateEnabled = (function () {
                this.model.enabled = false;
                if (this.model.viewerControlReady) {
                    var pageCount = this.viewer.viewerControl.getPageCount();
                    this.model.enabled = pageCount > 1 && this.viewer.viewerControl.getPageNumber() < pageCount;
                }
                $("button", this.model.$elements).prop("disabled", !this.model.enabled);
            }).bind(this);

            var gotoNextPage = (function () {
                if (this.model.enabled) {
                    this.viewer.viewerControl.changeToNextPage();
                    updateEnabled();
                }
            }).bind(this);

            // enable this component when ViewerControl is ready
            if (this.viewer.viewerControl) {
                function initOnPageCountReady() {
                    _me.viewer.viewerControl.off("PageCountReady", initOnPageCountReady);
                    _me.model.viewerControlReady = true;
                    updateEnabled();

                    // register callbacks
                    IGViewer.Util.registerCallbacks
                    (
                        [{
                            target: _me.viewer.viewerControl,
                            type: "PageChanged",
                            handler: updateEnabled
                        }, {
                            target: $("button", _me.model.$elements),
                            type: "click",
                            handler: gotoNextPage
                        }],
                        _me.model.callbacks
                    );
                }

                this.viewer.viewerControl.on("PageCountReady", initOnPageCountReady);
            }
            updateEnabled();
        },

        /**
         * @method IGViewer.ViewerComponents.NextPage#destroy - destroys this viewer component
         */
        destroy: function() {
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);
        }
    };

})();