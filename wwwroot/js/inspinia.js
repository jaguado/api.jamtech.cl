$(document).ready(function () {
    // Full height of sidebar
    function fix_height() {
        var heightWithoutNavbar = $("body > #wrapper").height() - 61;
        $(".sidebar-panel").css("min-height", heightWithoutNavbar + "px");

        var navbarHeight = $('nav.navbar-default').height();
        var wrapperHeight = $('#page-wrapper').height();

        if(navbarHeight > wrapperHeight){
            $('#page-wrapper').css("min-height", navbarHeight + "px");
        }

        if(navbarHeight < wrapperHeight){
            $('#page-wrapper').css("min-height", $(window).height()  + "px");
        }

        if ($('body').hasClass('fixed-nav')) {
            if (navbarHeight > wrapperHeight) {
                $('#page-wrapper').css("min-height", navbarHeight  + "px");
            } else {
                $('#page-wrapper').css("min-height", $(window).height() - 60 + "px");
            }
        }

    }

    $(window).bind("load resize scroll", function() {
        if(!$("body").hasClass('body-small')) {
                fix_height();
        }
    });

    // Move right sidebar top after scroll
    $(window).scroll(function(){
        if ($(window).scrollTop() > 0 && !$('body').hasClass('fixed-nav') ) {
            $('#right-sidebar').addClass('sidebar-top');
        } else {
            $('#right-sidebar').removeClass('sidebar-top');
        }
    });

    setTimeout(function(){
        fix_height();
    });

});

// Minimalize menu when screen is less than 768px
$(window).bind("load resize", function () {
    if ($(document).width() < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }
});

String.prototype.replaceAll = function (searchStr, replaceStr) {
    var str = this;
    // escape regexp special characters in search string
    searchStr = searchStr.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
    return str.replace(new RegExp(searchStr, 'gi'), replaceStr);
};

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}

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

function capitalize(input) {
    return (!!input) ? input.charAt(0).toUpperCase() + input.substr(1).toLowerCase() : '';
}

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