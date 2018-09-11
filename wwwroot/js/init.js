if (document.location.hostname!=="localhost" && 
    document.location.hostname!=="l10pc0jruck.principalusa.corp.principal.com" &&
    document.location.hostname!=="l10pc0jruck.pvchiscl.principal.com" &&
   (document.location.hostname!=="aio.jamtech.cl" || document.location.protocol!=="https:")){
    document.location = "https://aio.jamtech.cl" + document.location.pathname;
}
else
    console.log('redirect skipped for hostname: ' + document.location.hostname);