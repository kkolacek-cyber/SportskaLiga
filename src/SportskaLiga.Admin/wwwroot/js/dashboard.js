// Grafovi na nadzornoj ploči (Chart.js). Podaci se dohvaćaju asinkrono (Fetch API) iz /Home/Statistika kao JSON.
(async function () {
    "use strict";

    var okvir = document.getElementById("grafovi");
    if (!okvir || typeof Chart === "undefined") {
        return;
    }

    var zelena = "#15803d";
    var zuta = "#ca8a04";

    try {
        // Adresa dolazi iz data-url atributa u pogledu (generira je Razor), pa nije upisana u skriptu.
        var odgovor = await fetch(okvir.dataset.url, {
            headers: { "X-Requested-With": "XMLHttpRequest" },
            credentials: "same-origin"
        });

        if (!odgovor.ok) {
            throw new Error("HTTP " + odgovor.status);
        }

        var podaci = await odgovor.json();

        // Vodoravni stupci: bodovi po momčadi, poredano kao tablica.
        new Chart(document.getElementById("graf-bodovi"), {
            type: "bar",
            data: {
                labels: podaci.bodovi.map(function (r) { return r.momcad; }),
                datasets: [{
                    label: "Bodovi",
                    data: podaci.bodovi.map(function (r) { return r.bodovi; }),
                    backgroundColor: zelena,
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: "y",
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });

        // Linija: ukupno golova po odigranom kolu.
        new Chart(document.getElementById("graf-golovi"), {
            type: "line",
            data: {
                labels: podaci.goloviPoKolu.map(function (k) { return k.kolo + ". kolo"; }),
                datasets: [{
                    label: "Golovi",
                    data: podaci.goloviPoKolu.map(function (k) { return k.golovi; }),
                    borderColor: zuta,
                    backgroundColor: zuta,
                    tension: 0.3
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    // Ako dohvat ne uspije, umjesto grafova prikazuje se poruka.
    } catch (pogreska) {
        okvir.querySelectorAll(".graf-okvir").forEach(function (m) {
            m.textContent = "Grafove nije moguće učitati.";
            m.classList.add("text-muted");
        });
    }
}());
