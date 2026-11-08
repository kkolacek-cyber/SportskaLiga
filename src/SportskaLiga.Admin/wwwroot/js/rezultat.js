// Obrazac rezultata: broj redaka sa strijelcima uvijek je jednak zbroju golova.
// Kad se poveća rezultat, dodaje se prazan redak (iz <template>), a kad se smanji, uklanjaju se zadnji redci.
// Nakon svake promjene redci se prenumeriraju (Unos.Golovi[0], [1]...), jer model binder traži indekse bez rupa.
(function () {
    "use strict";

    var domacin = document.getElementById("Unos_GoloviDomacin");
    var gost = document.getElementById("Unos_GoloviGost");
    var golovi = document.getElementById("golovi");
    var predlozak = document.getElementById("gol-predlozak");
    var prazno = document.getElementById("golovi-prazno");

    function broj(polje) {
        var n = parseInt(polje.value, 10);
        return isNaN(n) || n < 0 ? 0 : Math.min(n, 99);
    }

    // Zamjenjuje indeks u name/id/for atributima retka (npr. "Unos.Golovi[3].Minuta" → "Unos.Golovi[1].Minuta").
    function prenumeriraj() {
        golovi.querySelectorAll(".gol-red").forEach(function (red, i) {
            red.querySelectorAll("[name], [id], [for], [data-valmsg-for]").forEach(function (el) {
                ["name", "id", "for", "data-valmsg-for"].forEach(function (atr) {
                    var v = el.getAttribute(atr);
                    if (v) {
                        el.setAttribute(atr, v.replace(/\[\d+\]|\[__i__\]/, "[" + i + "]")
                                             .replace(/_\d+__|___i___/, "_" + i + "__")
                                             .replace(/autogol-(\d+|__i__)/, "autogol-" + i));
                    }
                });
            });
        });
    }

    function uskladi() {
        var potrebno = broj(domacin) + broj(gost);
        var redci = golovi.querySelectorAll(".gol-red");

        for (var i = redci.length; i < potrebno; i++) {
            golovi.appendChild(predlozak.content.cloneNode(true));
        }

        for (var j = redci.length - 1; j >= potrebno; j--) {
            redci[j].remove();
        }

        prenumeriraj();
        prazno.classList.toggle("d-none", potrebno > 0);
    }

    domacin.addEventListener("input", uskladi);
    gost.addEventListener("input", uskladi);

    // Pri otvaranju obrasca: ako postojeći strijelci ne odgovaraju rezultatu, broj redaka se uskladi.
    uskladi();
}());
