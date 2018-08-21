/**
 * INSPINIA - Responsive Admin Theme
 *
 */

/**
 * MainCtrl - controller
 */
function MainCtrl() {

    this.userName = 'Example user';
    this.helloText = 'Welcome in SeedProject';
    this.descriptionText = 'It is an application skeleton for a typical AngularJS web app. You can use it to quickly bootstrap your angular webapp projects and dev environment for these projects.';
};

function StationsCtrl($http, $scope) {
    $scope.maxDistance = 5000;
    //TODO dynamic url depending of location 
    var stationsUrl = '//jamtechapi.herokuapp.com/v1/CombustibleStations?type=Vehicular&region=13&order=precios.gasolina_95';
    $scope.searchText="";
    $scope.stations=[];

    $scope.searchStations = function() {
        if($scope.position!=null){
            stationsUrl += '&lat=' + $scope.position.coords.latitude +'&lng=' + $scope.position.coords.longitude;
            stationsUrl += '&filters=ubicacion.distancia<' + $scope.maxDistance; 
        }
        return $http.get(stationsUrl).then(function(response){
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
            console.log('position', position);
            $scope.searchStations();
          });
        });
    };
    $scope.searchStations();   
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
    });