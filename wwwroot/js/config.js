/**
 * INSPINIA - Responsive Admin Theme
 *
 * Inspinia theme use AngularUI Router to manage routing and views
 * Each view are defined as state.
 * Initial there are written state for all view in theme.
 *
 */
function config($stateProvider, $urlRouterProvider, $ocLazyLoadProvider) {
    $urlRouterProvider.otherwise("/index/combustible");

    $ocLazyLoadProvider.config({
        // Set to true if you want to see what and when is dynamically loaded
        debug: false
    });

    $stateProvider
        .state('index', {
            abstract: true,
            url: "/index",
            templateUrl: "views/common/content.html",
        })
        .state('standalone', {
            abstract: true,
            url: "/standalone",
            templateUrl: "views/common/without_menu.html",
        })
        .state('index.combustible', {
            url: "/combustible",
            templateUrl: "views/combustible_search.es.html",
            data: { pageTitle: 'Combustible Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('standalone.combustible', {
            url: "/combustible",
            templateUrl: "views/combustible_search.es.html",
            data: { pageTitle: 'Combustible Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('index.products', {
            url: "/products",
            templateUrl: "views/products_search.es.html",
            data: { pageTitle: 'Products Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('standalone.products', {
            url: "/products",
            templateUrl: "views/products_search.es.html",
            data: { pageTitle: 'Products Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('index.torrents', {
            url: "/torrents",
            templateUrl: "views/torrents_search.es.html",
            data: { pageTitle: 'Torrents Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('standalone.torrents', {
            url: "/torrents",
            templateUrl: "views/torrents_search.es.html",
            data: { pageTitle: 'Torrents Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['js/plugins/footable/footable.all.min.js', 'css/plugins/footable/footable.core.css']
                        },
                        {
                            name: 'ui.footable',
                            files: ['js/plugins/footable/angular-footable.js']
                        }
                    ]);
                }
            }
        })
        .state('index.projects', {
            url: "/projects",
            templateUrl: "views/projects.html",
            data: { pageTitle: 'Projects view' }
        })
}
angular
    .module('inspinia')
    .config(config)
    .config(['AnalyticsProvider', function (AnalyticsProvider) {
        AnalyticsProvider.setAccount('UA-124610725-1');
     }])
     .run(['Analytics', function(Analytics) { }])
    .run(function($rootScope, $state) {
        $rootScope.$state = $state;
    });
