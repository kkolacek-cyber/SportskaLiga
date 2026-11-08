// Hrvatske zadane poruke za jQuery Validation. Poruke iz DataAnnotations (data-val-*) dolaze s poslužitelja;
// ovo pokriva pravila koja jQuery izvodi sam iz HTML atributa (npr. min/max na <input type="date">).
(function ($) {
    // "2026-12-05" → "05.12.2026." (hrvatski zapis datuma u poruci).
    function izIsoDatuma(vrijednost) {
        var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(vrijednost);
        return m ? m[3] + "." + m[2] + "." + m[1] + "." : vrijednost;
    }

    // Zamjena engleskih zadanih poruka jQuery Validationa hrvatskima.
    $.extend($.validator.messages, {
        required: "Ovo polje je obavezno.",
        email: "Unesite ispravnu e-mail adresu.",
        date: "Unesite ispravan datum.",
        number: "Unesite ispravan broj.",
        digits: "Unesite samo znamenke.",
        maxlength: $.validator.format("Unesite najviše {0} znakova."),
        minlength: $.validator.format("Unesite najmanje {0} znakova."),
        min: function (granica, element) {
            return element.type === "date"
                ? "Datum mora biti " + izIsoDatuma(granica) + " ili kasniji."
                : "Unesite vrijednost veću ili jednaku " + granica + ".";
        },
        max: function (granica, element) {
            return element.type === "date"
                ? "Datum mora biti " + izIsoDatuma(granica) + " ili raniji."
                : "Unesite vrijednost manju ili jednaku " + granica + ".";
        }
    });
}(jQuery));
