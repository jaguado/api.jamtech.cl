if (document.location.hostname!=="localhost" && document.location.protocol!=="https:"){
    document.location = "https://jamtechapi.herokuapp.com" + document.location.pathname;
}
else
    console.log('redirect skipped for hostname: ' + document.location.hostname);