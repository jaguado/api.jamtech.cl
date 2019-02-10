if (document.location.hostname !== "localhost" &&
    document.location.hostname !== "l10pc0jruck.principalusa.corp.principal.com" &&
    document.location.hostname !== "l10pc0jruck.pvchiscl.principal.com" &&
    (document.location.hostname !== "aio.jamtech.cl" || document.location.protocol !== "https:")) {
    //document.location = "https://aio.jamtech.cl" + document.location.pathname;
    console.log('redirect ommited');
} else
    console.log('redirect skipped for hostname: ' + document.location.hostname);


// Initialize Firebase
var config = {
    apiKey: "AIzaSyCAP7HkyqUvzk9OwH8bwjLKO6oadZI02Sc",
    authDomain: "jam-tech-aio.firebaseapp.com",
    databaseURL: "https://jam-tech-aio.firebaseio.com",
    projectId: "jam-tech-aio",
    storageBucket: "jam-tech-aio.appspot.com",
    messagingSenderId: "181464837188"
};
firebase.initializeApp(config);


// Pace options
paceOptions = {
    ajax: true, // disabled
    document: false, // disabled
    eventLag: true // disabled
  };