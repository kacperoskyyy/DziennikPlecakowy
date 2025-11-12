var map = null;
var polyline = null;
var startMarker = null;
var endMarker = null;

window.loadMapAndRoute = (geoPointListJson) => {

    console.log("[DEBUG] Funkcja JS 'loadMapAndRoute' zosta³a wywo³ana.");

    try {
        var geoPointList;
        if (typeof geoPointListJson === 'string') {
            geoPointList = JSON.parse(geoPointListJson);
        } else {
            geoPointList = geoPointListJson;
        }

        if (map === null) {
            map = L.map('map').setView([51.505, -0.09], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }).addTo(map);
        }

        if (polyline) map.removeLayer(polyline);
        if (startMarker) map.removeLayer(startMarker);
        if (endMarker) map.removeLayer(endMarker);

        if (!geoPointList || geoPointList.length === 0) {
            console.log("[DEBUG] Brak punktów (geoPointList jest pusty).");
            return;
        }

        var latLngs = geoPointList.map(p => [p.latitude, p.longitude]);

        polyline = L.polyline(latLngs, { color: 'blue' }).addTo(map);
        map.fitBounds(polyline.getBounds());

        startMarker = L.marker(latLngs[0]).addTo(map).bindPopup('Start');
        endMarker = L.marker(latLngs[latLngs.length - 1]).addTo(map).bindPopup('Koniec');

        console.log("[DEBUG] Mapa narysowana pomyœlnie!");

    } catch (e) {
        console.error("[DEBUG] KRYTYCZNY B£¥D w mapInterop.js:", e.message, e.stack);
    }
};

// --- POTWIERDZENIE GOTOWOŒCI ---
console.log("[DEBUG] mapInterop.js za³adowany. Wysy³anie sygna³u 'jsready'...");
window.location.href = 'jsready://';