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
        .state('index.combustible', {
            url: "/combustible",
            templateUrl: "views/combustible_search.es.html",
            data: { pageTitle: 'Combustible Search' }
        })
        .state('combustible', {
            url: "/standalone/combustible",
            templateUrl: "views/combustible_search.es.html",
            data: { pageTitle: 'Combustible Search' }
        })
        .state('index.products', {
            url: "/products",
            templateUrl: "views/products_search.es.html",
            data: { pageTitle: 'Products Search' }
        })
        .state('products', {
            url: "/standalone/products",
            templateUrl: "views/products_search.es.html",
            data: { pageTitle: 'Products Search' }
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
    .run(function($rootScope, $state) {
        $rootScope.$state = $state;
    });
