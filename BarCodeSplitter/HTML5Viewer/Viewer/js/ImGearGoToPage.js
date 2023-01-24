/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};

    /**
     * @private
     * @constructor GoToPageModel
     * @description Internal API. Constructs a model instance for IGViewer.ViewerComponents.GoToPage.
     * @param {Object} options - the options for this component
     * @see {@link IGViewer.ViewerComponents.GoToPage} for details on options
     */
    var GoToPageModel = function (options) {
        this.label = options.label || "";
        this.tooltip = options.tooltip;
        this.viewerControlReady = false;
        this.enabled = false;

        this.$elements = null;
        this.callbacks = [];
    };

    /**
     * @private 
     * @constructor GoToPageView
     * @description Internal class used to render the view for IGViewer.ViewerComponents.GoToPage.
     * @param {GoToPageModel} model - the GoToPageModel with which the GoToPage componet was instantiated
     */
    var GoToPageView = function (viewer, model) {
        this.viewer = viewer;
        this.model = model;
    };

    GoToPageView.prototype = {
        /**
         * @private
         * @method GoToPageViewer#render - Renders the html for this component
         */
        render: function () {

            var htmlTemplate = '<div class="data-ig-goto-page-container">' +
                                   '<input type="number" class="text-light"' +
                                   'value="' + this.viewer.viewerControl.getPageNumber() + '" >' +
                                   ' of ' +
                                   '<span>' + this.viewer.viewerControl.getPageCount() + '</span>'
            '</div>';

            var $elements = this.model.$elements = $("[data-ig-goto-page]", this.viewer.model.$rootElement);
            $elements.html(htmlTemplate);
            if (typeof (this.model.tooltip) === "string") {
                $elements.attr('title', this.model.tooltip);
            }
        }
    };



    /**
     * @public
     * @constructor IGViewer.ViewerComponents.GoToPage
     * @description Internal API. Used to create a GoToPage viewer component.
     * @param {IGViewer.Viewer} viewer - this instance of IGViewer.Viewer to which this component belongs
     * @param {Object} options - the options for this component
     * @param {string} [options.tooltip] - (optional) the tooltip to be displayed when the cursor hovers above the component
     * @param {string} [options.label] - (optional) the label to be displayed alongside the icon. Defaults to empty string
     */
    IGViewer.ViewerComponents.GoToPage = function (viewer, options) {

        this.viewer = viewer;
        this.model = new GoToPageModel(options);
        this.view = new GoToPageView(this.viewer, this.model);

        this.initialize();
    };

    IGViewer.ViewerComponents.GoToPage.prototype = {
        /**
         * @method IGViewer.ViewerComponents.GoToPage#initialize - initialize this viewer component
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
                    this.model.enabled = pageCount > 1;
                }
                $('input', this.model.$elements).prop("disabled", !this.model.enabled)
            }).bind(this);

            var updateInputValue = (function() {
                if (this.model.enabled) {
                    var inputValue = $('input', this.model.$elements).val(this.viewer.viewerControl.getPageNumber());
                }
            }).bind(this);

            var goToPage = (function (args) {
                if (this.model.enabled && args.keyCode == 13) {

                    var $input = $('input', this.model.$elements);
                    var inputValue = $input.val();
                    var inRange = inputValue > 0 && inputValue <= this.viewer.viewerControl.getPageCount();
                    if (!inRange) {
                        $input.addClass('error');
                        var pageNumber = this.viewer.viewerControl.getPageNumber();
                        setTimeout(function () {
                            $input.removeClass('error').val(pageNumber);
                        }, 1000);
                    }
                    else {
                        this.viewer.viewerControl.setPageNumber(inputValue);
                    }
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
                            target: _me.viewer.viewerControl,
                            type: "PageChanged",
                            handler:updateInputValue
                        }, {
                            target: $('input', _me.model.$elements),
                            type: "keyup",
                            handler: goToPage
                        }],
                        _me.model.callbacks
                    );

                    $('span', _me.model.$elements).html(_me.viewer.viewerControl.getPageCount())
                }

                this.viewer.viewerControl.on("PageCountReady", initOnPageCountReady);
            }
            updateEnabled();
        },

        /**
         * @method IGViewer.ViewerComponents.GoToPage#destroy - destroys this viewer component
         */
        destroy: function () {
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);
        }
    };
})();