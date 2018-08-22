/**
 * INSPINIA - Responsive Admin Theme
 *
 */

/**
 * MainCtrl - controller
 */

 var baseApiUrl='//jamtechapi.herokuapp.com/v1/';

 function MainCtrl() {

    this.userName = 'Example user';
    this.helloText = 'Welcome in SeedProject';
    this.descriptionText = 'It is an application skeleton for a typical AngularJS web app. You can use it to quickly bootstrap your angular webapp projects and dev environment for these projects.';
};

function StationsCtrl($http, $scope) {
    $scope.maxDistance = 5000;
    var stationsUrl = baseApiUrl + 'CombustibleStations?order=precios.ranking_gasolina_95';
    var regionsUrl  = baseApiUrl + 'CombustibleStations/Regions';

    $scope.searchText="";
    $scope.region=null; //all is the default //TODO read from cookie or calculate by location
    $scope.combustible='Vehicular';
    $scope.regions = [];
    $scope.stations= [];
    $scope.showLocationWarning=false;

    $scope.setCombustible = function(val){
        console.log('setCombustible', val);
        $scope.combustible=val;
        $scope.searchStations();
    };

    $scope.getRegionName = function(){
        if($scope.region!=null)
            return $scope.region.nombre;
        else
            return 'Todas';
    }
    $scope.setRegion = function(val){
        console.log('setRegion', val);
        $scope.region=val;
        $scope.searchStations();
    };
    $scope.loadRegions = function() {
        return $http.get(regionsUrl).then(function(response){
            $scope.regions=response.data;
            console.log('loadRegions', $scope.regions);
            return true;
        });
    };
    $scope.loadRegions();

    $scope.searchStations = function() {
        var tempStationsUrl = stationsUrl;
        tempStationsUrl += '&type=' + $scope.combustible;
        tempStationsUrl += '&region=';
        if($scope.region!=null)
            tempStationsUrl += $scope.region.codigo;
        else
            tempStationsUrl += '0';

        if($scope.position!=null){
            tempStationsUrl += '&lat=' + $scope.position.coords.latitude +'&lng=' + $scope.position.coords.longitude;
            tempStationsUrl += '&filters=ubicacion.distancia<' + $scope.maxDistance; 
        }
        return $http.get(tempStationsUrl).then(function(response){
            $scope.stations=response.data;
            $scope.searchText = $scope.searchTextTemp;
            console.log('searchStations', $scope.stations);
            return true;
        });
    };

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function(position){
          $scope.$apply(function(){
            $scope.position = position;
            $scope.showLocationWarning=false;
            console.log('position', position);
            $scope.searchStations();
          });
        },
        function(error) {
            //load stations without location
            $scope.searchStations();
            $scope.showLocationWarning=true;
        });
    };
}

function deg2rad(deg) {
    return deg * (Math.PI/180)
}
function getDistanceBetweenTwoPointsInMeters (lat1,lon1,lat2,lon2) {
    //console.log(lat1,lon1, lat2, lon2);
    var R = 6378100; // Radius of the earth in meters
    var dLat = deg2rad(lat2-lat1);  // deg2rad below
    var dLon = deg2rad(lon2-lon1); 
    var a = 
      Math.sin(dLat/2) * Math.sin(dLat/2) +
      Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) * 
      Math.sin(dLon/2) * Math.sin(dLon/2)
      ; 
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a)); 
    var d = R * c; // Distance in meters
    return d;
};


angular
    .module('inspinia')
    .controller('MainCtrl', MainCtrl)
    .controller('StationsCtrl', StationsCtrl)
    .filter('capitalize', function() {
        return function(input) {
          return (!!input) ? input.charAt(0).toUpperCase() + input.substr(1).toLowerCase() : '';
        }
     })
    .filter('toArray', function () {
        'use strict';
        return function (obj) {
            if (!(obj instanceof Object)) {
                return obj;
            }
            return Object.keys(obj).map(function (key) {
                return Object.defineProperty(obj[key], '$key', {__proto__: null, value: key});
            });
        }
    });