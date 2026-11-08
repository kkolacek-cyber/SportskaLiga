// Klijentska strana atributa [NakonDatuma] (SportskaLiga.Domain.Validation).
// Poslužitelj renderira: data-val-nakondatuma="<poruka>" data-val-nakondatuma-drugo="<ime svojstva s kojim se uspoređuje>".
// Datumi iz <input type="date"> su ISO (yyyy-MM-dd), pa se mogu uspoređivati kao tekst.
(function ($) {
    var validator = $.validator;

    // Pravilo za jQuery Validation: ovaj datum ne smije biti prije drugog; prazna polja ne smetaju.
    validator.addMethod("nakondatuma", function (value, element, drugiElement) {
        var drugi = $(drugiElement).val();
        if (!value || !drugi) {
            return true;   // prazna polja rješava [Required]
        }
        return value >= drugi;
    });

    // Poveznica između HTML atributa (data-val-nakondatuma-*) i pravila iznad.
    validator.unobtrusive.adapters.add("nakondatuma", ["drugo"], function (options) {
        // Ime polja je npr. "DatumZavrsetka" ili "Input.DatumZavrsetka"; drugo polje je u istom prefiksu.
        var ime = options.element.name;
        var prefiks = ime.substring(0, ime.lastIndexOf(".") + 1);
        var drugoIme = prefiks + options.params.drugo;

        var drugiElement = $(options.form).find(":input").filter(function () {
            return this.name === drugoIme;
        })[0];

        if (!drugiElement) {
            return;
        }

        options.rules["nakondatuma"] = drugiElement;
        options.messages["nakondatuma"] = options.message;

        // Kad se promijeni prvi datum: kalendar ne nudi raniji datum, a već upisani se ponovno provjerava.
        var $ovaj = $(options.element);
        $(drugiElement).on("change", function () {
            $ovaj.attr("min", $(drugiElement).val() || null);
            if ($ovaj.val()) {
                $ovaj.valid();
            }
        });
    });
}(jQuery));
