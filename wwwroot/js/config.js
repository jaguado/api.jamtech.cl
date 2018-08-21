/**
 * INSPINIA - Responsive Admin Theme
 *
 * Inspinia theme use AngularUI Router to manage routing and views
 * Each view are defined as state.
 * Initial there are written state for all view in theme.
 *
 */
function config($stateProvider, $urlRouterProvider, $ocLazyLoadProvider) {
    $urlRouterProvider.otherwise("/combustible");

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
        .state('index.combustible2', {
            url: "/combustible2",
            templateUrl: "views/slick.html",
            data: { pageTitle: 'Combustible Search' },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([
                        {
                            files: ['css/plugins/slick/slick.css','css/plugins/slick/slick-theme.css','js/plugins/slick/slick.min.js']
                        },
                        {
                            name: 'slick',
                            files: ['js/plugins/slick/angular-slick.min.js']
                        }
                    ]);
                }
            }
        })
        .state('index.combustible', {
            url: "/combustible",
            templateUrl: "views/combustible_search.html",
            data: { pageTitle: 'Combustible Search' }
        })
        .state('combustible', {
            url: "/standalone/combustible",
            templateUrl: "views/combustible_search.html",
            data: { pageTitle: 'Combustible Search' }
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
