if (document.location.hostname!=="localhost"){
    document.location = "https://aio.jamtech.cl" + document.location.pathname;
}
else
    console.log('redirect skipped for hostname: ' + document.location.hostname);