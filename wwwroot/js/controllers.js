var mocksBaseApiUrl = '//aio.jamtech.cl/mocks/torrents.json'
var baseApiUrl = '//aio.jamtech.cl/v1/';

function MainCtrl($scope, $rootScope, Analytics, socialLoginService) {
    $scope.user = localStorage.getItem('user') != null ? JSON.parse(localStorage.getItem('user')) : null;
    if ($scope.user != null)
        console.log('user logged in', $scope.user.name, $scope.user);
    this.helloText = 'Welcome to JAM Tech.cl';
    this.descriptionText = '';
    $scope.minimalize = function () {
        if (!$("body").hasClass("mini-navbar")) {
            $("body").toggleClass("mini-navbar");
        } else {
            $("body").removeClass("mini-navbar");
        }
    }
    $scope.minimalize();


    $scope.logoff = function(){
        socialLoginService.logout();
    };

    $rootScope.$on('event:social-sign-in-success', function (event, userDetails) {
        /*  Login ok
            userDetails = {
                            name: <user_name>, 
                            email: <user_email>, 
                            imageUrl: <image_url>, 
                            uid: <UID by social vendor>, 
                            provider: <Google/Facebook/LinkedIN>, 
                            token: < accessToken for Facebook & google, no token for linkedIN>}, 
                            idToken: < google idToken >
            };
        */
        $scope.$apply(function () {
            $scope.user = userDetails;
            localStorage.setItem('user', JSON.stringify($scope.user));
        });
        Analytics.trackEvent('aio', 'auth', $scope.user.provider);
        console.log('social-sign-in-success', $scope.user);
    });
    $rootScope.$on('event:social-sign-out-success', function (event, logoutStatus) {
        //logout ok
        $scope.user = null;
        localStorage.setItem('user', $scope.user);
        console.log('social-sign-out-success', logoutStatus);
    });
};

function TorrentsCtrl($http, $scope, $window, Analytics) {
    $scope.minimalize = function () {
        if (!$("body").hasClass("mini-navbar")) {
            $("body").toggleClass("mini-navbar");
        } else {
            $("body").removeClass("mini-navbar");
        }
    }

    $scope.useMocks = false; //mocks mode
    var searchUrl = baseApiUrl + 'Torrent?skipLinks=false&pages=4&search=';
    $scope.availableTorrentsTemplates = [{
        "name": "Table",
        "url": "views/torrents_table.html",
        "iconClass": "fas fa-table"
    }];
    $scope.gridTemplate = $scope.availableTorrentsTemplates[0];
    $scope.setTemplate = function (val) {
        Analytics.trackEvent('torrent', 'template', val);
        $scope.gridTemplate = val;
    }
    $scope.torrents = [];
    $scope.searchTorrents = function (val) {
        Analytics.trackEvent('torrent', 'search', val);
        var url = searchUrl + val;
        //using mocks for dev
        if ($scope.useMocks)
            url = mocksBaseApiUrl;

        return $http.get(url).then(function (response) {
            $scope.torrents = response.data;
            console.log('getTorrents', response.data)
            return true;
        });
    }
    $scope.download = function (val) {
        console.log('downloading', val);
        Analytics.trackEvent('torrent', 'download', val.Name);
        var url = val.Links.filter(link => link.Item2.startsWith('magnet:'));
        if (url.length > 0)
            $window.open(url[0].Item2, '_self');
    }
}

function ProductsCtrl($http, $scope, Analytics) {
    $scope.minimalize = function () {
        if (!$("body").hasClass("mini-navbar")) {
            $("body").toggleClass("mini-navbar");
        } else {
            $("body").removeClass("mini-navbar");
        }
    }
    var searchUrl = baseApiUrl + 'Products?pages=4&product=';
    var compareUrl = baseApiUrl + 'JumboProducts/{productId}/compare';
    $scope.availableProductTemplates = [{
            "name": "Table",
            "url": "views/products_table.html",
            "iconClass": "fas fa-table"
        },
        {
            "name": "Grid",
            "url": "views/products_grid.html",
            "iconClass": "fas fa-th"
        }
    ];
    $scope.gridTemplate = $scope.availableProductTemplates[0];
    $scope.view = 'Search';
    $scope.textToSearch = null;
    $scope.position = null;
    $scope.showLocationWarning = false;
    $scope.products = [];
    $scope.setTemplate = function (val) {
        Analytics.trackEvent('product', 'template', val);
        $scope.gridTemplate = val;
    }
    $scope.searchProduct = function (product) {
        Analytics.trackEvent('product', 'search', product);
        $scope.textToSearch = product;
        var url = searchUrl + product;

        //get stations from api.jamtech.cl
        return $http.get(url).then(function (response) {
            $scope.products = response.data;
            $scope.view = 'Search';
            return true;
        });
    };
    //$scope.searchProduct('vino');

    $scope.viewProduct = function (product) {
        Analytics.trackEvent('product', 'view', product);
        var url = compareUrl.replace('{productId}', product.product_id);
        //get stations from api.jamtech.cl
        return $http.get(url).then(function (response) {
            $scope.products = response.data;
            $scope.view = 'Compare';
            return true;
        });
    };

    //try to read geolocation from browser
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
                $scope.$apply(function () {
                    $scope.position = position;
                    $scope.showLocationWarning = false;
                    Analytics.trackEvent('product', 'geolocation', 'true');
                });
            },
            function (error) {
                Analytics.trackEvent('product', 'geolocation', 'false');
                $scope.showLocationWarning = true;
            });
    };
}

function StationsCtrl($http, $scope, Analytics) {
    $scope.minimalize = function () {
        if (!$("body").hasClass("mini-navbar")) {
            $("body").toggleClass("mini-navbar");
        } else {
            $("body").removeClass("mini-navbar");
        }
    }

    var stationsUrl = baseApiUrl + 'CombustibleStations?';
    var regionsUrl = baseApiUrl + 'CombustibleStations/Regiones';


    //defaults
    $scope.maxDistance = 10000; //in meters
    $scope.distributor = null;
    $scope.region = null; //all is the default //TODO read from cookie or calculate by location
    $scope.fuel = 'gasolina_95';
    $scope.fuelTypes = ["gasolina 93", "gasolina 95", "gasolina 97", "petroleo diesel", "kerosene", "glp vehicular"];
    $scope.distances = [1000, 5000, 10000, 15000, 20000, 50000, 100000];
    $scope.combustible = 'Vehicular';
    $scope.orderBy = 'precios.ranking_gasolina_95';
    $scope.regions = [];
    $scope.stations = [];
    $scope.showLocationWarning = false;

    /// Load parameters from local storage
    function loadFromLocalStorage() {
        var fuel = localStorage.getItem('fuel');
        if (fuel)
            $scope.fuel = fuel;
        var region = localStorage.getItem('region');
        if (region)
            $scope.region = JSON.parse(region);
        var combustible = localStorage.getItem('combustible');
        if (combustible)
            $scope.combustible = combustible;
        var distance = localStorage.getItem('distance');
        if (distance)
            $scope.maxDistance = distance;
        var distributor = localStorage.getItem('distributor');
        if (distributor)
            $scope.distributor = distributor;
    }
    loadFromLocalStorage();


    $scope.availableTemplates = [{
            "name": "Table",
            "url": "views/combustible_table.html",
            "iconClass": "fas fa-table"
        },
        {
            "name": "Grid",
            "url": "views/combustible_grid.html",
            "iconClass": "fas fa-th"
        }
    ];
    $scope.gridTemplate = $scope.availableTemplates[0];
    $scope.setTemplate = function (val) {
        Analytics.trackEvent('combustible', 'template', val);
        $scope.gridTemplate = val;
    }

    $scope.setDistributor = function (val) {
        $scope.distributor = val;
        localStorage.setItem('distributor', val != null ? val : '');
        $scope.searchStations();
    }
    $scope.getDistributors = function () {
        if ($scope.stations != null) {
            var result = $scope.stations.map(m => m.distribuidor.nombre).filter(onlyUnique);
            return result;
        }
    }
    $scope.setDistance = function (val) {
        $scope.maxDistance = val;
        localStorage.setItem('distance', val != null ? val : '');
        $scope.searchStations();
    }
    $scope.setFuel = function (val) {
        $scope.fuel = val;
        localStorage.setItem('fuel', val != null ? val : '');
        $scope.searchStations();
    }
    $scope.setCombustible = function (val) {
        if (val == 'Vehicular')
            $scope.orderBy = 'precios.ranking_gasolina_95';
        else
            $scope.orderBy = 'precios.ranking_kerosene';
        $scope.combustible = val;
        localStorage.setItem('combustible', val != null ? val : '');
        $scope.searchStations();
    };
    $scope.setRegion = function (val) {
        $scope.region = val;
        localStorage.setItem('region', val != null ? JSON.stringify(val) : '');
        $scope.searchStations();
    };
    $scope.getFuelType = function () {
        if ($scope.fuel != null)
            return $scope.fuel;
        else
            return 'Todos los tipos';
    }
    $scope.getRegionName = function () {
        if ($scope.region != null)
            return $scope.region.nombre;
        else
            return 'Todas las regiones';
    }
    $scope.getDistributorName = function () {
        if ($scope.distributor)
            return $scope.distributor;
        else
            return 'Todas las marcas';
    }
    $scope.loadRegions = function () {
        return $http.get(regionsUrl).then(function (response) {
            $scope.regions = response.data;
            return true;
        });
    };
    $scope.loadRegions();

    $scope.searchStations = function () {
        Analytics.trackEvent('combustible', 'search', $scope.combustible);
        //refresh order by
        if ($scope.fuel != null)
            $scope.orderBy = 'precios.ranking_' + $scope.fuel.replaceAll(' ', '_');

        //add filters
        var tempStationsUrl = stationsUrl;
        tempStationsUrl += '&type=' + $scope.combustible;
        tempStationsUrl += '&order=' + $scope.orderBy;
        tempStationsUrl += '&region=';
        if ($scope.region != null)
            tempStationsUrl += $scope.region.codigo;
        else
            tempStationsUrl += '0';

        if ($scope.distributor != null)
            tempStationsUrl += '&distributor=' + $scope.distributor;

        //add position
        if ($scope.position != null) {
            tempStationsUrl += '&lat=' + $scope.position.coords.latitude + '&lng=' + $scope.position.coords.longitude;
            tempStationsUrl += '&filters=ubicacion.distancia<' + $scope.maxDistance;
        }

        //get stations from api.jamtech.cl
        return $http.get(tempStationsUrl).then(function (response) {
            $scope.stations = response.data;
            $scope.searchText = $scope.searchTextTemp;
            return true;
        });
    };

    //try to read geolocation from browser
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
                $scope.$apply(function () {
                    $scope.position = position;
                    $scope.showLocationWarning = false;
                    Analytics.trackEvent('product', 'geolocation', 'true');
                    $scope.searchStations();
                });
            },
            function (error) {
                //load stations without location
                $scope.searchStations();
                $scope.showLocationWarning = true;
                Analytics.trackEvent('product', 'geolocation', 'false');
            });
    };
}





// End of controllers
//math operations used to calculate distnace between two points
function deg2rad(deg) {
    return deg * (Math.PI / 180)
}

function getDistanceBetweenTwoPointsInMeters(lat1, lon1, lat2, lon2) {
    //console.log(lat1,lon1, lat2, lon2);
    var R = 6378100; // Radius of the earth in meters
    var dLat = deg2rad(lat2 - lat1); // deg2rad below
    var dLon = deg2rad(lon2 - lon1);
    var a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c; // Distance in meters
    return d;
};

function Capitalize() {
    return function (input) {
        return (!!input) ? input.charAt(0).toUpperCase() + input.substr(1).toLowerCase() : '';
    }
};


function toArray() {
    'use strict';
    return function (obj) {
        if (!(obj instanceof Object)) {
            return obj;
        }
        return Object.keys(obj).map(function (key) {
            return Object.defineProperty(obj[key], '$key', {
                __proto__: null,
                value: key
            });
        });
    }
};

function getProductBrandType() {
    return function (product) {
        return product.product_type + ' ' + product.brand;
    }
}

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}

function GetPrice() {
    return function (input) {

    }
}

function GetPrice() {
    return function (input) {
        return 'no price yet';
    }
};

String.prototype.replaceAll = function (searchStr, replaceStr) {
    var str = this;

    // escape regexp special characters in search string
    searchStr = searchStr.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');

    return str.replace(new RegExp(searchStr, 'gi'), replaceStr);
};

//angular js - load controllers, filters and other stuff
angular
    .module('inspinia')
    .controller('MainCtrl', MainCtrl)
    .controller('StationsCtrl', StationsCtrl)
    .controller('ProductsCtrl', ProductsCtrl)
    .controller('TorrentsCtrl', TorrentsCtrl)
    .filter('capitalize', Capitalize)
    .filter('toArray', toArray)
    .filter('getBrand', getProductBrandType)
    .filter('getPrice', GetPrice);