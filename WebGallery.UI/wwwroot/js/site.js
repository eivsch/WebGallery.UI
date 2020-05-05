// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showOverlay() {
    console.log("Hit Open");
    document.getElementById("myNav1").style.width = "100%";
}
function closeOverlay() {
    console.log("Hit Open");
    document.getElementById("myNav1").style.width = "0%";
}

function overlay_switch() {
    document.getElementById("overlay-image").src = "http://192.168.1.251:5000/picture/2";
}