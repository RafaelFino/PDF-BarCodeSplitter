/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/** @ignore */
var IGViewer = IGViewer || {};

(function () {

    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};



    /**
     * @private
     * @constructor GoToPageModel
     * @description Internal API. Constructs a model instance for IGViewer.ViewerComponents.GoToPage.
     * @param {Object} options
     * @see {@link IGViewer.ViewerComponents.GoToPage} for details on options
     */
    var ZoomSelectModel = function (options) {
        this.label = options.label || "";
        this.tooltip = options.tooltip;
        this.enabled = false;
        this.useBsCollapsibleNavbar = typeof(options.useBsCollapsibleNavbar) === "boolean" ? options.useBsCollapsibleNavbar : true;

        this.$element = null;
        this.$elementOptions = null;
        this.$viewPercentageElement = null;
        this.callbacks = [];
    };



    /**
     * @private
     * @constructor GoToPageView
     * @description Internal class used to render the view for IGViewer.ViewerComponents.GoToPage.
     * @param {GoToPageModel} model - the GoToPageModel with which the GoToPage componet was instantiated
     */
    var ZoomSelectView = function (viewer, model) {
        this.viewer = viewer;
        this.model = model;
    };

    ZoomSelectView.prototype = {
        /**
         * @private
         * @method ZoomSelectView#render - Renders the html for this component
         */
        render: function () {

            var htmlTemplateZoomOptions = '<ul class="dropdown-menu">' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="800"><a href="#">800%</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="400"><a href="#">400%</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="200"><a href="#">200%</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="100"><a href="#">100%</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="75"><a href="#">75% </a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-factor="50"> <a href="#">50% </a></li>' +
                                    '<li class="dropdown-item" class="divider"></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-fit="FullWidth"><a href="#">Full Width</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-fit="FullHeight"><a href="#">Full Height</a></li>' +
                                    '<li class="dropdown-item" data-ig-zoom-fit="FullPage"><a href="#">Full Page</a></li>' +
                                '</ul>';

            var htmlTemplateMain = '<div class="dropdown">' +
                                    '<button class="btn navbar-btn dropdown-toggle" type="button" data-toggle="dropdown">' +
                                        '<span data-ig-zoom-select-current></span>' +
                                        '<span class="caret"></span>' +
                                    '</button>' +
                                    htmlTemplateZoomOptions +
                                '</div>';

            this.model.$element = $("[data-ig-zoom-select]", this.viewer.model.$rootElement);

            var $element = this.model.$element;
            $element.html(htmlTemplateMain);
            $element.attr('title', this.model.tooltip);
            this.model.$viewPercentageElement = $('[data-ig-zoom-select-current]', this.viewer.model.$element);

            // If use of the Bootstrap navbar is specified through options.useBsCollapsibleNavbar...
            if (this.model.useBsCollapsibleNavbar) {
                this.model.$elementOptions = $("[data-ig-zoom-select-options]", this.viewer.model.$rootElement);

                var htmlTemplateZoomOptionsWrapper = '<div class="dropdown">' +
                                                           '<span data-toggle="dropdown" style="display:none;"></span>' +
                                                           htmlTemplateZoomOptions +
                                                      '</div>';

                var $elementOptions = this.model.$elementOptions;
                $elementOptions.html(htmlTemplateZoomOptionsWrapper);
            }
        }
    };



    /**
     * @public
     * @constructor IGViewer.ViewerComponents.ZoomSelect
     * @description Internal API. Used to create a ZoomSelect viewer component.
     * @param {IGViewer.Viewer} viewer - this instance of IGViewer.Viewer to which this component belongs
     * @param {Object} options - the options for this component
     * @param {string} [options.tooltip] - (optional) the tooltip to be displayed when the cursor hovers above the component
     * @param {boolean} [options.label] - (optional) whether to use the Bootstrap collapsible navbar
     * @param {string} [options.label] - (optional) the label to be displayed alongside the icon. Defaults to empty string
     */
    IGViewer.ViewerComponents.ZoomSelect = function (viewer, options) {

        this.viewer = viewer;
        this.model = new ZoomSelectModel(options);
        this.view = new ZoomSelectView(this.viewer, this.model);

        this.initialize();
    };

    IGViewer.ViewerComponents.ZoomSelect.prototype = {
        /**
         * @method IGViewer.ViewerComponents.ZoomSelect#initialize - initialize this viewer component
         */
        initialize: function () {

            var _me = this;

            // render html template for this component
            this.view.render();

            // define callbacks
            var updateEnabled = (function () {
                if (this.viewer.viewerControl) {
                    var pageCount = this.viewer.viewerControl.getPageCount();

                    this.model.enabled = pageCount != 0 && !(this.viewer.viewerControl.getAtMaxScale() || this.viewer.viewerControl.getAtMinScale())

                    $('input', this.model.$element).prop("disabled", !this.model.enabled)
                }
            }).bind(this);

            var updateViewPercentage = (function () {
                if (this.viewer.viewerControl) {
                    var currentScale = Math.round(this.viewer.viewerControl.getScaleFactor() * 100);
                    this.model.$viewPercentageElement.text(currentScale + '%');
                }
            }).bind(this);

            var setDefaultFactor = (function() {
                if (this.viewer.viewerControl) {
                    this.viewer.viewerControl.setScaleFactor(1);
                }
            }).bind(this);

            var setFit = (function (event) {
                if (this.viewer.viewerControl) {
                    var fit = event.currentTarget.attributes['data-ig-zoom-fit'].value;
                    this.viewer.viewerControl.fitContent(fit);
                }
            }).bind(this);

            var setFactor = (function (event) {
                if (this.viewer.viewerControl) {
                    var factor = event.currentTarget.attributes['data-ig-zoom-factor'].value / 100;
                    this.viewer.viewerControl.setScaleFactor(factor);
                }
            }).bind(this);

            var dropdownNavBarOptions = (function () {
                if (this.model.useBsCollapsibleNavbar) {
                    $('[data-toggle="dropdown"]', this.model.$elementOptions).dropdown("toggle");
                }
            }).bind(this);

            var fitContent = (function () {
                this.viewer.fitContent();
            }).bind(this);

            // function to add additional callbacks if options.useBsCollapsibleNavbar is set to true
            var initBsCollapsibleNavbarZoomSelect = (function () {
                if (this.model.useBsCollapsibleNavbar) {
                    IGViewer.Util.registerCallbacks
                    (
                        [{
                            target: $("[data-ig-zoom-factor]", this.model.$elementOptions),
                            type: "click",
                            handler: setFactor
                        }, {
                            target: $("[data-ig-zoom-fit]", this.model.$elementOptions),
                            type: "click",
                            handler: setFit
                        }, {
                            target: $(".dropdown", this.model.$element),
                            type: "show.bs.dropdown",
                            handler: dropdownNavBarOptions
                        }, {
                            target: $(".dropdown", this.model.$elementOptions),
                            type: "shown.bs.dropdown",
                            handler: fitContent
                        }, {
                            target: $(".dropdown", this.model.$elementOptions),
                            type: "hidden.bs.dropdown",
                            handler: fitContent
                        }],
                        this.model.callbacks
                    );
                }
            }).bind(this);

            // enable this component when ViewerControl is ready
            if (this.viewer.viewerControl) {
                function initOnPageCountReady() {
                    _me.viewer.viewerControl.off("PageCountReady", initOnPageCountReady);

                    // register callbacks
                    IGViewer.Util.registerCallbacks
                    (
                        [{
                            target: _me.viewer.viewerControl,
                            type: "ScaleChanged",
                            handler: updateEnabled
                        }, {
                            target: _me.viewer.viewerControl,
                            type: "ScaleChanged",
                            handler: updateViewPercentage
                        }, {
                            target: $('[data-ig-zoom-factor]', _me.model.$element),
                            type: "click",
                            handler: setFactor
                        }, {
                            target: $('[data-ig-zoom-fit]', _me.model.$element),
                            type: "click",
                            handler: setFit
                        }],
                        _me.model.callbacks
                    );
                    
                    _me.viewer.viewerControl.fitContent("FullPage");
                    setDefaultFactor();
                    initBsCollapsibleNavbarZoomSelect();
                }

                this.viewer.viewerControl.on("PageCountReady", initOnPageCountReady);
            }
        },

        /**
         * @method IGViewer.ViewerComponents.ZoomSelect#destroy - destroys this viewer components
         */
        destroy: function () {
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);
        }
    };
})();
