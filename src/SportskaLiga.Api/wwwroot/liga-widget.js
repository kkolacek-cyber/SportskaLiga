/*
 * Widget lige za web stranice klubova.
 *
 * Ugradnja na bilo koju stranicu (druga domena – zato API ima CORS politiku "Widget"):
 *   <div data-liga-widget="tablica"></div>                       tablica aktivne sezone
 *   <div data-liga-widget="rezultati" data-momcad="3"></div>     zadnji rezultati momčadi 3
 *   <div data-liga-widget="raspored" data-broj="5"></div>         sljedećih 5 utakmica
 *   <script src="http://localhost:8083/liga-widget.js"></script>
 *
 * Adresa API-ja čita se iz adrese same skripte, pa klub ne mora ništa konfigurirati.
 */
(function () {
    'use strict';

    const skripta = document.currentScript;
    const api = new URL(skripta.src).origin;

    // Stilovi su ograničeni na .liga-widget da ne utječu na ostatak stranice kluba.
    const css = `
        .liga-widget { font-family: system-ui, sans-serif; font-size: 14px; color: #1c1917; max-width: 640px; }
        .liga-widget h3 { margin: 0 0 .4rem; font-size: 15px; color: #14532d; border-bottom: 2px solid #facc15; display: inline-block; }
        .liga-widget table { width: 100%; border-collapse: collapse; }
        .liga-widget th { background: #14532d; color: #fff; font-weight: 600; padding: 4px 6px; text-align: center; }
        .liga-widget td { padding: 4px 6px; border-bottom: 1px solid #e7e5e4; text-align: center; }
        .liga-widget .lijevo { text-align: left; }
        .liga-widget .desno { text-align: right; }
        .liga-widget .rez { background: #1c1917; color: #facc15; font-weight: 700; border-radius: 4px; padding: 1px 6px; white-space: nowrap; }
        .liga-widget .sitno { color: #78716c; font-size: 12px; }
        .liga-widget .greska { color: #b91c1c; }`;
    const stil = document.createElement('style');
    stil.textContent = css;
    document.head.appendChild(stil);

    // Tekst iz API-ja se uvijek escapea (nikad se ne ubacuje kao HTML).
    const esc = (t) => String(t ?? '').replace(/[&<>"']/g, (z) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[z]));

    const datum = (iso) => new Date(iso).toLocaleString('hr-HR', {
        timeZone: 'Europe/Zagreb', day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit'
    });

    async function dohvati(putanja, parametri) {
        const url = new URL(putanja, api);
        Object.entries(parametri).forEach(([k, v]) => { if (v) url.searchParams.set(k, v); });
        const odgovor = await fetch(url);
        if (!odgovor.ok) {
            throw new Error(`HTTP ${odgovor.status}`);
        }
        return odgovor.json();
    }

    async function tablica(el) {
        const t = await dohvati('/api/tablica', { sezonaId: el.dataset.sezona });
        const redovi = t.redovi.map((r) => `
            <tr><td>${r.pozicija}.</td><td class="lijevo">${esc(r.momcad)}</td><td>${r.odigrano}</td>
            <td>${r.pobjede}</td><td>${r.nerijeseno}</td><td>${r.porazi}</td>
            <td>${r.goloviDani}:${r.goloviPrimljeni}</td><td><strong>${r.bodovi}</strong></td></tr>`).join('');
        return `<h3>Tablica ${esc(t.sezona.naziv)}</h3>
            <table><thead><tr><th>#</th><th class="lijevo">Momčad</th><th>U</th><th>P</th><th>N</th><th>I</th><th>Golovi</th><th>Bod</th></tr></thead>
            <tbody>${redovi}</tbody></table>`;
    }

    function utakmice(naslov, stranica) {
        if (stranica.stavke.length === 0) {
            return `<h3>${naslov}</h3><p class="sitno">Nema utakmica.</p>`;
        }
        const redovi = stranica.stavke.map((u) => `
            <tr><td class="sitno">${u.kolo}. kolo<br>${datum(u.pocetak)}</td>
            <td class="desno">${esc(u.domacin.naziv)}</td>
            <td><span class="rez">${esc(u.rezultat)}</span></td>
            <td class="lijevo">${esc(u.gost.naziv)}</td></tr>`).join('');
        return `<h3>${naslov}</h3><table><tbody>${redovi}</tbody></table>`;
    }

    const vrste = {
        tablica,
        rezultati: async (el) => utakmice('Zadnji rezultati', await dohvati('/api/rezultati', {
            sezonaId: el.dataset.sezona, momcadId: el.dataset.momcad, velicinaStranice: el.dataset.broj || 5
        })),
        raspored: async (el) => utakmice('Sljedeće utakmice', await dohvati('/api/raspored', {
            sezonaId: el.dataset.sezona, momcadId: el.dataset.momcad, velicinaStranice: el.dataset.broj || 5
        }))
    };

    document.querySelectorAll('[data-liga-widget]').forEach(async (el) => {
        el.classList.add('liga-widget');
        const vrsta = vrste[el.dataset.ligaWidget];
        if (!vrsta) {
            el.innerHTML = `<p class="greska">Nepoznata vrsta widgeta: ${esc(el.dataset.ligaWidget)}</p>`;
            return;
        }
        el.innerHTML = '<p class="sitno">Učitavanje…</p>';
        try {
            el.innerHTML = await vrsta(el);
        } catch (e) {
            el.innerHTML = `<p class="greska">Podaci lige trenutno nisu dostupni (${esc(e.message)}).</p>`;
        }
    });
})();
