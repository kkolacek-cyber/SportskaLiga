// Popis utakmica (asinkroni prikaz – ishod I3): tablica se učitava Fetch API-jem kao HTML fragment s /Utakmice/Lista.
// Promjena filtra ili stranice osvježava samo tablicu, bez ponovnog učitavanja cijele stranice.
// Kad se promijeni sezona, izbornik kola puni se JSON-om s /Utakmice/Kola.
(function () {
    "use strict";

    var obrazac = document.getElementById("utakmice-filter");
    var lista = document.getElementById("utakmice-lista");
    var poruke = document.getElementById("utakmice-poruke");
    var sezona = document.getElementById("sezonaId");
    var kolo = document.getElementById("kolo");
    var stranica = parseInt(new URLSearchParams(location.search).get("stranica"), 10) || 1;

    // Trenutni filtri kao query string; checkboxi se šalju uvijek (true/false), da delegat može isključiti "samo moje".
    function parametri() {
        var p = new URLSearchParams();
        if (sezona.value) p.set("sezonaId", sezona.value);
        if (kolo.value) p.set("kolo", kolo.value);
        var status = document.getElementById("status").value;
        if (status) p.set("status", status);
        p.set("samoMoje", document.getElementById("samoMoje").checked);
        p.set("samoCekajuRezultat", document.getElementById("samoCekajuRezultat").checked);
        if (stranica > 1) p.set("stranica", stranica);
        return p;
    }

    function prikaziGresku(tekst) {
        var okvir = document.createElement("div");
        okvir.className = "alert alert-danger";
        okvir.setAttribute("role", "alert");
        okvir.textContent = tekst;   // textContent: poruka se nikad ne tumači kao HTML
        poruke.replaceChildren(okvir);
    }

    // Dohvaća HTML fragment tablice i njime zamjenjuje popis; adresa preglednika se ažurira (F5 i "natrag" rade).
    async function ucitaj() {
        lista.classList.add("opacity-50");
        poruke.replaceChildren();

        var upit = parametri().toString();
        history.replaceState(null, "", location.pathname + "?" + upit);

        try {
            var odgovor = await fetch(lista.dataset.url + "?" + upit, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                credentials: "same-origin"
            });

            if (odgovor.redirected) {
                // Sjednica je istekla: fetch je slijedio preusmjeravanje na prijavu.
                location.href = odgovor.url;
                return;
            }

            if (!odgovor.ok) {
                throw new Error("HTTP " + odgovor.status);
            }

            lista.innerHTML = await odgovor.text();
        } catch (pogreska) {
            prikaziGresku("Popis utakmica nije moguće učitati. Pokušajte ponovno.");
        } finally {
            lista.classList.remove("opacity-50");
        }
    }

    // Nova sezona ima druga kola: izbornik se puni JSON-om (npr. [1,2,3,...]) s poslužitelja.
    async function ucitajKola() {
        kolo.replaceChildren(new Option("Sva kola", ""));
        if (!sezona.value) {
            return;
        }

        try {
            var odgovor = await fetch(sezona.dataset.kolaUrl + "?sezonaId=" + encodeURIComponent(sezona.value), {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                credentials: "same-origin"
            });
            var kola = await odgovor.json();
            kola.forEach(function (k) {
                kolo.appendChild(new Option(k + ". kolo", k));
            });
        } catch (pogreska) {
            // Bez kola se i dalje mogu prikazati sve utakmice sezone.
        }
    }

    // --- događaji ---
    obrazac.addEventListener("change", async function (e) {
        stranica = 1;
        if (e.target === sezona) {
            await ucitajKola();
        }
        ucitaj();
    });

    // Gumb "Traži" (i Enter) ne šalje obrazac, nego osvježava samo tablicu.
    obrazac.addEventListener("submit", function (e) {
        e.preventDefault();
        stranica = 1;
        ucitaj();
    });

    // Klik na stranicu u _Pager partialu: delegiranje događaja jer se sadržaj popisa zamjenjuje.
    lista.addEventListener("click", function (e) {
        var veza = e.target.closest("a[data-stranica]");
        if (veza) {
            e.preventDefault();
            if (!veza.closest(".disabled")) {
                stranica = parseInt(veza.dataset.stranica, 10);
                ucitaj();
            }
        }
    });

    // Prvo učitavanje popisa pri otvaranju stranice.
    ucitaj();
}());
