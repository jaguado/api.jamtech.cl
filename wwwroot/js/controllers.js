 var baseApiUrl = '//jamtechapi.herokuapp.com/v1/';

 function MainCtrl($scope) {
     this.userName = 'Visit';
     this.helloText = 'Welcome to JAM Tech.cl';
     this.descriptionText = '';
     $scope.minimalize = function () {
         $("body").toggleClass("mini-navbar");
     }
     $scope.minimalize();
 };

 function ProductsCtrl($http, $scope) {
     var searchUrl = baseApiUrl + 'Products?pages=1&product=';
     var compareUrl = baseApiUrl + 'JumboProducts/{productId}/compare';

     $scope.view = 'Search';
     $scope.textToSearch = null;
     $scope.position = null;
     $scope.showLocationWarning = false;
     $scope.products = [];
     $scope.searchProduct = function (product) {
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
                 });
             },
             function (error) {
                 $scope.showLocationWarning = true;
             });
     };
 }

 function StationsCtrl($http, $scope) {

     var stationsUrl = baseApiUrl + 'CombustibleStations?';
     var regionsUrl = baseApiUrl + 'CombustibleStations/Regiones';


     //defaults
     $scope.maxDistance = 10000; //in meters
     $scope.distributor = null;
     $scope.region = null; //all is the default //TODO read from cookie or calculate by location
     $scope.fuel = 'gasolina_95';
     $scope.fuelTypes = ["gasolina_93", "gasolina_95", "gasolina_97", "diesel", "kerosene", "glp_vehicular"];
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
         //refresh order by
         if ($scope.fuel != null)
             $scope.orderBy = 'precios.ranking_' + $scope.fuel;

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
                     $scope.searchStations();
                 });
             },
             function (error) {
                 //load stations without location
                 $scope.searchStations();
                 $scope.showLocationWarning = true;
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

 //angular js - load controllers, filters and other stuff
 angular
     .module('inspinia')
     .controller('MainCtrl', MainCtrl)
     .controller('StationsCtrl', StationsCtrl)
     .controller('ProductsCtrl', ProductsCtrl)
     .filter('capitalize', Capitalize)
     .filter('toArray', toArray)
     .filter('getBrand', getProductBrandType);