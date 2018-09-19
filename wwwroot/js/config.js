/**
 * INSPINIA - Responsive Admin Theme
 *
 * Inspinia theme use AngularUI Router to manage routing and views
 * Each view are defined as state.
 * Initial there are written state for all view in theme.
 *
 */


/* Intercept all http calls*/
var httpInterceptor = function ($q, $location, $rootScope) {
    return {
        request: function (config) { //req
            //add access token if url is from the base api domain
            if (config.url.startsWith(baseApiUrl)) {
                if (!config.url.contains("access_token") && user != null) {
                    //console.log('httpInterceptor', 'request', 'adding access token for user', user);
                    config.url += config.url.contains("?") ? "&" : "?";
                    config.url += "access_token=" + user.token;
                    config.url += "&provider=" + user.provider;
                }
            }
            //console.log('httpInterceptor', 'request', 'baseApiUrl', baseApiUrl);
            return config;
        },

        response: function (result) { //res
            //console.log('res',result.status);
            return result;
        },

        responseError: function (rejection) { //error
            console.log('Failed with', rejection.status);
            if (rejection.status == 401 || rejection.status == 403) {
                //force logoff
                $rootScope.$broadcast("logoff");             
            }
            return $q.reject(rejection);
        }
    }
};

function config($stateProvider, $urlRouterProvider, $ocLazyLoadProvider) {
    $urlRouterProvider.otherwise("/index/dashboard");

    $ocLazyLoadProvider.config({
        // Set to true if you want to see what and when is dynamically loaded
        debug: false
    });

    $stateProvider
        .state('index', {
            abstract: true,
            url: "/index",
            templateUrl: "views/common/content.html",
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
                        name: 'cgNotify',
                        files: ['css/plugins/angular-notify/angular-notify.min.css', 'js/plugins/angular-notify/angular-notify.min.js']
                    }]);
                }
            }
        })
        .state('index.main', {
            url: "/main",
            templateUrl: "views/main.html",
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
                        files: ['css/plugins/bootstrapSocial/bootstrap-social.css']
                    }]);
                }
            }
        })
        .state('index.dashboard', {
            url: "/dashboard",
            templateUrl: "views/dashboard.html",
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
                            name: 'angles',
                            files: ['js/plugins/chartJs/angles.js', 'js/plugins/chartJs/Chart.min.js']
                        },
                        {
                            name: 'angular-peity',
                            files: ['js/plugins/peity/jquery.peity.min.js', 'js/plugins/peity/angular-peity.js']
                        },
                        {
                            serie: true,
                            name: 'angular-flot',
                            files: ['js/plugins/flot/jquery.flot.js', 'js/plugins/flot/jquery.flot.time.js', 'js/plugins/flot/jquery.flot.tooltip.min.js', 'js/plugins/flot/jquery.flot.spline.js', 'js/plugins/flot/jquery.flot.resize.js', 'js/plugins/flot/jquery.flot.pie.js', 'js/plugins/flot/curvedLines.js', 'js/plugins/flot/angular-flot.js', ]
                        }
                    ]);
                }
            }
        })
        .state('login', {
            url: "/login",
            templateUrl: "views/main.html",
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
                        files: ['css/plugins/bootstrapSocial/bootstrap-social.css']
                    }]);
                }
            }
        })
        .state('standalone', {
            abstract: true,
            url: "/standalone",
            templateUrl: "views/common/without_menu.html",
        })
        .state('index.combustible', {
            url: "/combustible",
            templateUrl: "views/combustible_search.es.html",
            data: {
                pageTitle: 'Combustible Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
            data: {
                pageTitle: 'Combustible Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
            data: {
                pageTitle: 'Products Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
            data: {
                pageTitle: 'Products Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
            data: {
                pageTitle: 'Torrents Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
            data: {
                pageTitle: 'Torrents Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
        .state('index.tools', {
            url: "/tools",
            templateUrl: "views/tools.es.html",
            data: {
                pageTitle: 'Tools'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
                            serie: true,
                            files: ['css/plugins/codemirror/codemirror.css', 'css/plugins/codemirror/ambiance.css', 'js/plugins/codemirror/codemirror.js', 'js/plugins/codemirror/mode/javascript/javascript.js', 'js/plugins/codemirror/mode/css/css.js', 'js/plugins/codemirror/mode/xml/xml.js', 'js/plugins/codemirror/mode/htmlmixed/htmlmixed.js']
                        },
                        {
                            name: 'ui.codemirror',
                            files: ['js/plugins/ui-codemirror/ui-codemirror.min.js']
                        }
                    ]);
                }
            }
        })
        .state('index.projects', {
            url: "/projects",
            templateUrl: "views/projects.html",
            data: {
                pageTitle: 'Projects view'
            }
        })
        .state('index.atms', {
            url: "/atms",
            templateUrl: "views/atms_search.es.html",
            data: {
                pageTitle: 'ATM Search'
            },
            resolve: {
                loadPlugin: function ($ocLazyLoad) {
                    return $ocLazyLoad.load([{
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
}
angular
    .module('inspinia')
    .config(config)
    .config(['AnalyticsProvider', function (AnalyticsProvider) {
        AnalyticsProvider.setAccount('UA-124610725-1');
    }])
    .config(function (socialProvider) {
        socialProvider.setGoogleKey("95717972095-f3h0t9hmvd0dhjfqctoe39qlsupbrmou.apps.googleusercontent.com");
        socialProvider.setLinkedInKey("77es90vl6bc7gi");
        socialProvider.setFbKey({
            appId: "277009742922752",
            apiVersion: "v3.1"
        });
    })
    .config(function ($httpProvider) {
        $httpProvider.interceptors.push(httpInterceptor);
    })
    .run(['Analytics', function (Analytics) {}])
    .run(function ($rootScope, $state, $locale) {
        $rootScope.$state = $state;
        $locale.NUMBER_FORMATS.GROUP_SEP = "";
        $locale.NUMBER_FORMATS.DECIMAL_SEP = ",";
    });