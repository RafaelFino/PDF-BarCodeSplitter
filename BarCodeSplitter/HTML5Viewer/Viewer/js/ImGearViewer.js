/* @copyright Copyright (c) 1996-2021 Accusoft Corporation.  All rights reserved. */

/**
 * @namespace IGViewer
 * @description Contains all namespaces, classes, and methods related to IGViewer
 */
var IGViewer = IGViewer || {};

(function () {

    /**
     * @namespace IGViewer.ViewerComponents
     * @description Contains classes for viewer components (menus, buttons)
     */
    IGViewer.ViewerComponents = IGViewer.ViewerComponents || {};

    /**
     * @private
     * @constructor ViewerModel
     * @description Internal class to serve as a model for IGViewer.Viewer.
     * @param {string|Element} element - the DOM element with the Viewer is instantiated
     * @param {Object} options - the options with which hte Viewer is instantiated
     * @see {@link IGViewer.Viewer} for a description of options
     */
    var ViewerModel = function (element, options) {

        // set root element
        if (typeof (element) === "string" || element instanceof Element)
            element = $(element);
        if (!(element instanceof $) || element.length !== 1)
            throw new Error("element must either be a DOM Element or a CSS selector which uniquely identifies a DOM Element");

        /**
         * @member {string} this.viewerId - unique id for this viewer, initially set to null
         */
        this.viewerId = null;

        this.$rootElement = element;
        this.options = options || {};

        /**
         * @member {object} this.components - viewer components, all initially null
         */
        this.components = {
            prevPage: null,
            nextPage: null,
            firstPage: null,
            lastPage: null,
            goToPage: null,
            rotateDoc: null,
            rotatePage: null,
            zoomSelect: null,
            zoomIn: null,
            zoomOut: null,
            annotationBar: null,
            searchBar: null
        };

        // set default value for options.imageHandlerUrl if unspecified
        if (typeof (this.options.imageHandlerUrl) !== "string")
            this.options.imageHandlerUrl = "ImageGearService.svc";

        /**
         * @member {string} this.viewerIconsPath - Default relative path to viewer icons. This can be overidden in the options
         *          object that is passed to IGViewer.Viewer during construction.
         */
        this.viewerIconsPath = "Assets/Viewer/icons";

        /**
         * @member {Object} this.viewerIcons - Default file names of viewer icons. This can be overidden in the options object
         *          that is passed to IGViewer.Viewer during construction.
         */
        this.viewerIcons = {
            zoomOut: "zoomout.svg",
            zoomIn: "zoomin.svg",
            firstPage: "firstpage.svg",
            prevPage: "prevpage.svg",
            nextPage: "nextpage.svg",
            lastPage: "lastpage.svg",
            rotateDocCW: "rotate-doc-cw.svg",
            rotatePageCW: "rotate-page-cw.svg",
            search: "search.svg",
            searchEnter: "enter.svg",
            searchPrev: "prevpage.svg",
            searchNext: "nextpage.svg",
            searchCancel: "x.svg"
        };

        // Overwrite default options with any specified options.
        if (typeof (this.options.viewerIconsPath) === "string")
            this.viewerIconsPath = this.options.viewerIconsPath;
        if (this.options.viewerIcons instanceof Object)
            for (var x in this.options.viewerIcons)
                if (this.options.viewerIcons.hasOwnProperty(x) && this.viewerIcons.hasOwnProperty(x))
                    this.viewerIcons[x] = this.options.viewerIcons[x];

        /**
         * @member {Function[]} this.callbacks - Array to store callbacks so they can be unregistered during viewer destruction.
         */
        this.callbacks = [];

        this.resizeCallbacks = [];
    };



    /**
     * @private
     * @constructor ViewerView
     * @description Internal class used to render the view for IGViewer.Viewer.
     * @param {ViewerModel} model - the Viewer model
     */
    var ViewerView = function (model) {
        this.model = model;
    };

    ViewerView.prototype = {
        /**
         * @private
         * @method ViewerView#render - Renders the view for IGViewer
         */
        render: function () {

            // HTML template used for the viewer
            var htmlTemplate =
                '<div class="container-fluid" style="margin: 0">' +
                    '<div data-ig-main-menu-bar class="container-fluid" style="margin: 0">' +
                        '<ul class="nav justify-content-center">' +
                            '<li class="nav-item" data-ig-annotation-bar-btn></li>' +
                            '<li class="nav-item" data-ig-search-bar-btn></li>' +
                            '<li class="nav-item" data-ig-control-group="zoom">' +
                                '<div data-ig-zoom-out><a href="#">Zoom Out</a></div>' +
                                '<div data-ig-zoom-select><a href="#">Zoom: 100%</a></div>' +
                                '<div data-ig-zoom-in><a href="#">Zoom In</a></div>' +
                            '</li>' +
                            '<li class="nav-item" data-ig-control-group="zoomOptions">' +
                                '<div data-ig-zoom-select-options></div>' +
                            '</li>' +
                            '<li class="nav-item" data-ig-control-group="pagination">' +
                                '<div data-ig-first-page><a href="#">First Page</a></div>' +
                                '<div data-ig-prev-page><a href="#">Previous Page</a></div> ' +
                                '<div data-ig-goto-page><a href="#">Goto Page</a></div>' +
                                '<div data-ig-next-page><a href="#">Next Page</a></div> ' +
                                '<div data-ig-last-page><a href="#">Last Page</a></div>' +
                            '</li>' +
                            '<li class="nav-item" data-ig-control-group="rotate">' +
                                '<div data-ig-rotate-doc><a href="#">Rotate Doc</a></div>' +
                                '<div data-ig-rotate-page><a href="#">Rotate Page</a></div>' +
                            '</li>' +
                        '</ul>' +
                        '<div data-ig-annotation-bar class="collapse annotation-bar"></div>' +
                        '<div data-ig-search-bar class="collapse"></div>' +
                    '</div>'+
                    '<div class="row border-top border-dark" style="position: relative;">' +
                        '<div data-ipcc-viewer-control style="background-color: #3c4248;">' +
                        '</div>' +
                    '</div>'+
                '</div>';

            // Insert the template into the DOM of the viewer root element
            this.model.$rootElement.html(htmlTemplate);
        }
    };



    // Unique object property key that is used to store a reference to the viewer in the
    //  DOM element with which it is initialized. This is used to destroy any existing
    //  viewer created with that DOM element upon reconstruction.
    var IGViewerKey = "ImageGear.Web.IGViewer";

    /**
     * @public
     * @constructor IGViewer.Viewer
     * @description Constructs a new instance of IGViewer
     * @param {Element|string} element - a DOM Element or CSS selector specifying the DOM
     *      element in which the viewer will be constructed
     * @param {Object} options - the options with which IGViewer will be constructed
     * @param {string} options.documentID - (required) the ID of the document to display
     * @param {string} [options.imageHandlerUrl] - (optional) the path to "ImageGearService.svc" at which
     *      the ImageGear.Web back-end will be mounted. This will default default "ImageGearService.svc", expecting
     *      the file to be located in the current directory.
     * @param {string} [options.viewerIconsPath] - (optional) the path to the directory in which viewer icons are located.
     *      This will default to "Assets/Viewer/icons", relative to the current directory.
     * @param {Object} [options.viewerIcons] - (optional) an object whose properties specify the paths to images to be used as icons
     *      for viewer buttons. This icons are to be stored in the directory pointed to by options.viewerIconsPath
     * @param {string} [options.viewerIcons.zoomOut] - the file name of the icon to be used for the "Zoom Out" button.
     *      This will default to "zoomout.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.zoomIn] - the file name of the icon to be used for the "Zoom In" button.
     *      This will default to "zoomin.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.firstPage] - the file name of the icon to be used for the "First Page" button.
     *      This will default to "firstpage.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.prevPage] - the file name of the icon to be used for the "Previous Page" button.
     *      This will default to "prevpage.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.nextPage] - the file name of the icon to be used for the "Next Page" button.
     *      This will default to "nextpage.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.lastPage] - the file name of the icon to be used for the "Last Page" button.
     *      This will default to "lastpage.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.rotateDocCW] - the file name of the icon to be used for the "Rotate Document Clockwise" button.
     *      This will default to "rotate-doc-cw.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewerIcons.rotatePageCW] - the file name of the icon to be used for the "Rotate Page Clockwise" button.
     *      This will default to "rotate-page-cw.svg", to be located in or relative to options.viewerIconsPath.
     * @param {string} [options.viewMode - the mode used to view documents containing different sized pages. This will default to
     *          IPCC.ViewMode.Document. Must be one of the values in the IPCC.ViewMode enum described below:
     *      IPCC.ViewMode.Document - the viewer maintains the relative size of each page when displaying a document. For example, if page 2
     *          is smaller than page 1, it will appear smaller.
     *      IPCC.ViewMode.SinglePage - the viewer displays a single page at a time. Each page is scaled to fit within a view box, which is
     *          the initial size of hte viewer and increases in size when zooming in (and decreases in size when zooming out). After the
     *          viewer initializes, the view mode may not be changed to or from SinglePage view mode (an Error will be thrown in this case).
     *      IPCC.ViewMode.EqualFitPages - the viewer scales each page so that their width is the same, when using vertical page layout. For
     *          example, if page 2 is smaller than page 1, it will be scaled larger so that its width is equal to the width of page 1. If
     *          using horizontal page layout, the viewer scales each page so that their height is the same.
     * @param {string} [options.pageLayout] - set the placement or arrangement of the pages in the viewer. This will default to
     *          IPCC.PageLayout.Vertical. Must be one of the values in the IPCC.PageLayout enum described below:
     *      IPCC.PageLayout.Horizontal - pages are displayed as a single horizontal row and a horizontal scroll bar is displayed to bring
     *          into view pages that are not in view.
     *      IPCC.PageLayout.Vertical - pages are displayed as a single vertical column and a vertical scroll bar is dislayed to bring into
     *          view pages that are not in view.
     * @param {number} [options.pageRotation] - set a default rotation in degrees to be applied to each page. Defaults to 0. Must be a multiple
     *      of 90: {..., -270, -180, -90, 0, 90, 180, 270, ...}.
     */
    IGViewer.Viewer = function (element, options) {

        this.model = new ViewerModel(element, options);
        this.view = new ViewerView(this.model);

        this.initialize();
    };

    IGViewer.Viewer.prototype = {
        /**
         * @method IGViewer.Viewer#initialize
         * @description Internal API. Initialized a newly instantiated ImGearViewer.Viewer. This method is called as part of
         *      viewer construction. There is no need to ever call this method manually.
         */
        initialize: function () {

            this.model.viewerId = IGViewer.Util.uniqueId("IGViewer")

            // check for and destroy any existing viewer
            if (this.model.$rootElement.data(IGViewerKey) instanceof IGViewer.Viewer) {
                this.model.$rootElement.data(IGViewerKey).destroy();
            }
            this.model.$rootElement.data(IGViewerKey, this);

            // render the viewer template
            this.view.render();

            // instantiate the ViewerControl
            this.viewerControl = new IPCC.ViewerControl($("[data-ipcc-viewer-control]", this.model.$rootElement)[0], this.model.options);

            var fixedViewerIconsPath = IGViewer.Util.endWithSlash(this.model.viewerIconsPath);

            // Attach PrevPage
            this.model.components.prevPage = new IGViewer.ViewerComponents.PrevPage(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.prevPage,
                tooltip: "Previous Page",
                label: ""
            });

            // Attach NextPage
            this.model.components.nextPage = new IGViewer.ViewerComponents.NextPage(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.nextPage,
                tooltip: "Next Page",
                label: ""
            });

            // Attach FirstPage
            this.model.components.firstPage = new IGViewer.ViewerComponents.FirstPage(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.firstPage,
                tooltip: "First Page",
                label: ""
            });

            // Attach LastPage
            this.model.components.lastPage = new IGViewer.ViewerComponents.LastPage(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.lastPage,
                tooltip: "Last Page",
                label: ""
            });

            // Attach GoToPage
            this.model.components.goToPage = new IGViewer.ViewerComponents.GoToPage(this, {
                tooltip: "Go to Page",
                label: ""
            });

            // Attach RotateDoc
            this.model.components.rotateDoc = new IGViewer.ViewerComponents.RotateDoc(this, {
                iconClockwise: fixedViewerIconsPath + this.model.viewerIcons.rotateDocCW,
                tooltipClockwise: "Rotate Document Clockwise",
                labelClockwise: "",
            });

            // Attach RotatePage
            this.model.components.rotatePage = new IGViewer.ViewerComponents.RotatePage(this, {
                iconClockwise: fixedViewerIconsPath + this.model.viewerIcons.rotatePageCW,
                tooltipClockwise: "Rotate Page Clockwise",
                labelClockwise: "",
                labelCounterClockwise: ""
            });

            // Attach Zoom Select
            this.model.components.zoomSelect = new IGViewer.ViewerComponents.ZoomSelect(this, {
                useBsCollapsibleNavbar: true,
                tooltip: "Zoom Select",
                label: ""
            });

            // Attach ZoomIn
            this.model.components.zoomIn = new IGViewer.ViewerComponents.ZoomIn(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.zoomIn,
                tooltip: "Zoom In",
                label: ""
            });

            // Attach ZoomOut
            this.model.components.zoomOut = new IGViewer.ViewerComponents.ZoomOut(this, {
                icon: fixedViewerIconsPath + this.model.viewerIcons.zoomOut,
                tooltip: "Zoom Out",
                label: ""
            });

            // Attach AnnotationBar
            this.model.components.annotationBar = new IGViewer.ViewerComponents.AnnotationBar(this, {
                tooltip: "Toggle Annotation Bar"
            });

            // Attach SearchBar
            this.model.components.searchBar = new IGViewer.ViewerComponents.SearchBar(this, {
                label: "Search",
                tooltip: "Toggle Search Bar",
                iconSearch: fixedViewerIconsPath + this.model.viewerIcons.search,
                iconSearchEnter: fixedViewerIconsPath + this.model.viewerIcons.searchEnter,
                iconSearchPrev: fixedViewerIconsPath + this.model.viewerIcons.searchPrev,
                iconSearchNext: fixedViewerIconsPath + this.model.viewerIcons.searchNext,
                iconSearchCancel: fixedViewerIconsPath + this.model.viewerIcons.searchCancel
            });

            // define callbacks

            var showZoomOptions = (function () {
                $('.nav [data-ig-control-group="zoomOptions"]', this.model.$rootElement).show();
            }).bind(this);

            var hideZoomOptions = (function () {
                $('.nav [data-ig-control-group="zoomOptions"]', this.model.$rootElement).hide();
            }).bind(this);

            this.fitContent(0);

            // register callbacks
            IGViewer.Util.registerCallbacks
            (
                [{
                    target: $(".navbar-collapse", this.model.$rootElement),
                    type: "show.bs.collapse",
                    handler: showZoomOptions
                }, {
                    target: $(".navbar-collapse", this.model.$rootElement),
                    type: "hide.bs.collapse",
                    handler: hideZoomOptions
                }],
                this.model.callbacks
            );
        },

        /**
         * @method IGViewer.Viewer#destroy
         * @description destroys a previously instantiated viewer.
         */
        destroy: function () {
            // call destroy() on each applicable component
            if (this.model.components) {
                for (var compName in this.model.components) {
                    if (this.model.components.hasOwnProperty(compName) && this.model.components[compName] && (this.model.components[compName].destroy instanceof Function)) {
                        this.model.components[compName].destroy();
                    }
                }
            }

            // unregister callbacks registered during initialize()
            IGViewer.Util.unregisterCallbacks(this.model.callbacks);

            // destroy any existing ViewerControl
            if (this.viewerControl instanceof IPCC.ViewerControl) {
                this.viewerControl.destroy();
                this.viewerControl = null;
            }
        },

        /**
         * @method IGViewer.Viewer#fitContent
         * @description refits the content of the viewer upon a relevant event
         * @param {Element|string|integer} margin - This can either be a DOM Element, a CSS selector for an element, or an integer.
         *      If an Element or CSS selector is used, then the top margin will be dynamic.
         */
        fitContent: function (topMargin) {
            var resizeFunction = function () {
                var height = $("[data-ig-main-menu-bar]").height();
                // check arg type
                if (topMargin) {
                    if (typeof (topMargin) === "string" || topMargin instanceof Element) {
                        if (!($(topMargin) instanceof $) || $(topMargin).length !== 1)
                            throw new Error("Bad margin arg. Should either be an Integer, CSS selector, or Element.");
                        height += $(topMargin).height();
                    } else if (Number.isInteger(topMargin)) {
                        height += topMargin;
                    }
                }

                $("[data-ipcc-viewer-control]").css("height", "calc(100% - " + height + "px)");
            };

            var fitContentDebounced = IGViewer.Util.debounce(resizeFunction.bind(this), 200);
            
            IGViewer.Util.unregisterCallbacks(this.model.resizeCallbacks);
            IGViewer.Util.registerCallbacks
            (
                [{
                    target: this.viewerControl,
                    type: "PageCountReady",
                    handler: fitContentDebounced
                }, {
                    target: this.viewerControl,
                    type: "ViewerReady",
                    handler: fitContentDebounced
                }, {
                    target: window,
                    type: "resize",
                    handler: fitContentDebounced
                }, {
                    target: $("[data-ig-main-menu-bar]", this.model.$rootElement),
                    type: "show.bs.collapse",
                    handler: fitContentDebounced
                }, {
                    target: $("[data-ig-main-menu-bar]", this.model.$rootElement),
                    type: "hide.bs.collapse",
                    handler: fitContentDebounced
                }, {
                    target: $("[data-ig-main-menu-bar]", this.model.$rootElement),
                    type: "shown.bs.collapse",
                    handler: fitContentDebounced
                }, {
                    target: $("[data-ig-main-menu-bar]", this.model.$rootElement),
                    type: "hidden.bs.collapse",
                    handler: fitContentDebounced
                }],
                this.model.resizeCallbacks
            );
        }
    };

})();