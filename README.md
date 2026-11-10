# SportskaLiga – amaterska sportska liga: raspored, rezultati i tablica

Projektni zadatak iz kolegija **Web arhitekture i dizajn** (Veleučilište u Bjelovaru, 2026./2027.), tema 14.
Autor: Karlo Kolaček.

## Problem

Rekreativna nogometna liga objavljuje raspored i rezultate u PDF datotekama, a tablica se računa ručno uz česte
pogreške. Rješenje daje:

- **javni portal** na kojem svatko vidi raspored, rezultate, tablicu i listu strijelaca (tablica se uvijek računa
  iz upisanih rezultata, nikad ručno), a momčadi se mogu prijaviti za novu sezonu;
- **administraciju** u kojoj vodstvo lige registrira momčadi i igrače, generira raspored, a delegati upisuju
  rezultate i strijelce svojih utakmica;
- **Web API** iz kojeg klubovi na svojim stranicama prikazuju tablicu, rezultate i raspored (widget).

## Struktura rješenja

```
Projekt/
├── SportskaLiga.csproj, Program.cs, Pages/   Razor Pages – javni portal (u korijenu repozitorija)
├── src/
│   ├── SportskaLiga.Domain/   entiteti, enumi, sučelja repozitorija, čista logika (tablica, Bergerov raspored)
│   ├── SportskaLiga.Data/     DbContext, migracije, repozitoriji, seed, Dockerfile kontejnera "migracije"
│   ├── SportskaLiga.Admin/    ASP.NET Core MVC – administracija (Identity, uloge Administrator i Delegat)
│   └── SportskaLiga.Api/      ASP.NET Core Web API (JWT, Swagger UI, widget)
├── postman/                   Postman kolekcija i okruženja (lokalno, Docker)
├── docker-compose.yml, .env.example
└── Dockerfile                 portal (ostali Dockerfileovi su u mapama projekata)
```

Sve tri web aplikacije dijele domenski model i bazu (PostgreSQL 17). Poslovna pravila su u **repozitorijima**
(npr. "rezultat se ne upisuje prije početka utakmice", "broj strijelaca mora odgovarati rezultatu"), pa vrijede
jednako u administraciji i API-ju. Repozitorij ne baca iznimke za očekivane greške nego vraća
`RezultatOperacije` s vrstom pogreške (`NijePronadeno`, `NeispravniPodaci`, `NeispravnoStanje`, `NemaOvlasti`), a
aplikacija je prevodi u poruku na obrascu (Admin) ili HTTP statusni kod (API).

Vrijeme početka utakmice sprema se u **UTC** (`timestamptz`), a prikazuje po hrvatskom vremenu (`Kalendar`,
Europe/Zagreb). Kultura svih aplikacija je `hr-HR`.

## Pokretanje

Potreban je samo **Docker Desktop** (s Docker Compose).

```bash
docker compose up --build
```

Prvo pokretanje traje nekoliko minuta (preuzimanje .NET SDK slika i paketa). Kada su sve usluge pokrenute:

| Aplikacija | Adresa |
|---|---|
| Javni portal | http://localhost:8081 |
| Administracija | http://localhost:8082 |
| Web API (Swagger UI) | http://localhost:8083/swagger |
| Demo widget (stranica kluba) | http://localhost:8083/widget.html |

Zaustavljanje: `Ctrl+C` ili `docker compose down`. Brisanje i podataka baze (novi seed pri sljedećem pokretanju):
`docker compose down -v`.

### Varijable okruženja

`.env` datoteka **nije potrebna**: `docker-compose.yml` ima zadane DEMO vrijednosti. Za vlastite vrijednosti kopiraj
`.env.example` u `.env` i izmijeni je (`.env` se ne commita). Connection string, lozinke testnih računa i JWT ključ
aplikacije dobivaju isključivo kroz varijable okruženja (`ConnectionStrings__SportskaLiga`, `Seed__*`, `Jwt__*`).

## Docker: usluge, volumeni i healthcheck

| Usluga | Slika | Opis |
|---|---|---|
| `db` | postgres:17-alpine | baza; volumen `pgdata`; healthcheck `pg_isready` |
| `migracije` | vlastita (EF Core migration bundle) | primijeni migracije i završi |
| `admin` | vlastita, multi-stage | seed demo podataka; healthcheck `/health` |
| `portal` | vlastita, multi-stage | javni portal; healthcheck `/health` |
| `api` | vlastita, multi-stage | Web API; healthcheck `/health` |
| `newman` | postman/newman (profil `test`) | Postman testovi iz kontejnera |

Redoslijed: `db` (healthy) → `migracije` (completed successfully) → `admin` (healthy, nakon seeda) → `portal`, `api`.

Svaki Dockerfile je **višefazni**: faza `build` (slika .NET SDK) radi `restore` (keširan sloj – kopiraju se najprije
samo `.csproj` datoteke) i `publish`; faza `final` (slika ASP.NET runtime, alpine) sadrži samo objavljenu aplikaciju
i radi kao korisnik bez root ovlasti. U runtime sliku se dodaju ICU (kultura hr-HR) i tzdata (Europe/Zagreb).

Volumen `dpkeys` čuva DataProtection ključeve Admina i Portala, pa prijava i antiforgery tokeni prežive ponovno
pokretanje kontejnera.

### Dodatni Docker zahtjev: inicijalizacijski kontejner za migracije

Migracije ne primjenjuje web aplikacija nego zaseban kontejner **`migracije`** (`src/SportskaLiga.Data/Dockerfile`):

1. u build fazi `dotnet ef migrations bundle` iz projekta Data stvara samostalnu izvršnu datoteku `efbundle`
   koja sadrži sve migracije;
2. kontejner se pokreće kad je baza zdrava, izvrši `efbundle --connection ...` (primijeni samo migracije koje
   nedostaju) i završi s kodom 0 – status `Exited (0)` u `docker compose ps -a` je ispravan;
3. `admin` čeka uvjet `service_completed_successfully`, a zatim samo seeda (`Baza__MigracijePriPokretanju=false`).

Ako migracija ne uspije, kontejner završi s greškom i aplikacije se uopće ne pokreću (umjesto da rade s krivom shemom).

### Razvoj izvan Dockera

```bash
docker compose up -d db                              # samo baza na localhost:5432
dotnet run --project src/SportskaLiga.Admin          # http://localhost:5182 (migrira i seeda)
dotnet run --project SportskaLiga.csproj             # http://localhost:5241
dotnet run --project src/SportskaLiga.Api            # http://localhost:5138/swagger
```

Kod `dotnet run` Admin sam primjenjuje migracije (postavka `Baza:MigracijePriPokretanju` nije zadana).

### Migracije (EF Core)

```bash
dotnet tool restore
dotnet ef migrations add NazivMigracije -p src/SportskaLiga.Data -s src/SportskaLiga.Data
dotnet ef database update -p src/SportskaLiga.Data -s src/SportskaLiga.Data
```

## Testni korisnički računi

Stvara ih seed pri prvom pokretanju (vrijednosti se mogu promijeniti u `.env`).

| Uloga | E-mail | Lozinka | Može |
|---|---|---|---|
| Administrator | admin@liga.local | Admin123! | sve: sezone, momčadi, igrači, suci, raspored, rezultati, prijave, korisnici |
| Delegat | delegat@liga.local | Delegat123! | upis rezultata i strijelaca utakmica koje su mu dodijeljene |
| Delegat | delegat2@liga.local | Delegat123! | isto, za svoje utakmice (za provjeru da delegat ne vidi tuđe) |

Seed stvara i demo ligu: 8 izmišljenih momčadi s igračima, 4 suca, odigranu sezonu 2025./2026., aktivnu sezonu
2026./2027. odigranu do današnjeg dana (uključujući utakmicu koja čeka rezultat i jednu odgođenu) te sezonu
2027./2028. otvorenu za prijave s dvije demo prijave (kodovi `HRAST27K` – na čekanju, `MACKE27X` – odbijena).

## Javni portal (Razor Pages)

- **Početna** – tablica (sažetak), zadnji rezultati i sljedeće utakmice aktivne sezone.
- **Raspored** i **Rezultati** – filtri po sezoni, kolu i momčadi (GET parametri), straničenje.
- **Tablica** – bodovi, gol-razlika, forma zadnjih 5 utakmica; računa se iz odigranih utakmica.
- **Strijelci** – lista strijelaca (autogolovi se ne broje).
- **Utakmica** i **Momčad** – detalji (strijelci po minutama, igrači, utakmice momčadi).
- **Prijava momčadi** – obrazac s validacijom → stranica potvrde s kodom prijave → provjera statusa (kod + e-mail).

## Administracija (ASP.NET Core MVC)

- Prijava (Identity, kolačić), zaključavanje nakon 5 neuspjelih pokušaja, globalni `AuthorizeFilter` i antiforgery.
- **Nadzorna ploča** – brojke, grafovi (Chart.js, podaci dohvaćeni Fetch API-jem), prijave na čekanju.
- **Sezone, momčadi, igrači, suci** – CRUD s ViewModelima, validacijom i straničenjem.
- **Utakmice** – popis koji se osvježava Fetch API-jem (filtri bez ponovnog učitavanja stranice), **generiranje
  rasporeda** Bergerovim algoritmom (svatko sa svakim, jednostruki ili dvostruki krug), unos rezultata i strijelaca.
- **Prijave momčadi** – odobravanje (momčad se stvara i upisuje u sezonu) ili odbijanje s odgovorom.
- **Korisnici** – upravljanje računima i ulogama (samo Administrator).

## Web API

Dokumentacija: **Swagger UI** na `/swagger` (OpenAPI dokument `/openapi/v1.json`). Zaštićene operacije traže JWT u
zaglavlju `Authorization: Bearer {token}`; token se dobiva s `POST /api/auth/login`. Greške su
`application/problem+json`.

| Metoda i ruta | Pristup | Opis |
|---|---|---|
| `POST /api/auth/login` | javno | prijava, vraća JWT |
| `GET /api/sezone` | javno | sve sezone |
| `GET /api/tablica?sezonaId=` | javno | tablica (bez `sezonaId` aktivna sezona) |
| `GET /api/strijelci?sezonaId=&najvise=` | javno | lista strijelaca |
| `GET /api/raspored?sezonaId=&kolo=&momcadId=` | javno | utakmice bez rezultata, straničeno |
| `GET /api/rezultati?sezonaId=&kolo=&momcadId=` | javno | odigrane utakmice, najnovije prve |
| `GET /api/utakmice/{id}` | javno | utakmica sa strijelcima |
| `GET /api/utakmice/cekaju-rezultat` | Delegat, Administrator | utakmice koje čekaju rezultat (delegat samo svoje) |
| `PUT /api/utakmice/{id}/rezultat` | Delegat, Administrator | upis rezultata i strijelaca |
| `DELETE /api/utakmice/{id}/rezultat` | Administrator | poništavanje rezultata |
| `GET /api/momcadi`, `GET /api/momcadi/{id}` | javno | momčadi, detalji s igračima |
| `POST/PUT/DELETE /api/momcadi` | Administrator | dodavanje, izmjena, brisanje momčadi |

Statusni kodovi: 200, 201 (+ `Location`), 204, 400 (validacija), 401 (nema/nevaljan token), 403 (kriva uloga ili
tuđa utakmica), 404, 409 (npr. utakmica još nije počela, naziv momčadi zauzet, momčad ima igrače).

**Widget:** klub na svoju stranicu doda

```html
<div data-liga-widget="tablica"></div>
<div data-liga-widget="rezultati" data-momcad="3" data-broj="5"></div>
<script src="http://localhost:8083/liga-widget.js"></script>
```

API ima CORS politiku koja s bilo koje domene dopušta samo `GET`, pa widget radi na tuđim stranicama, a izmjene
iz tuđeg preglednika nisu moguće.

### Postman testovi

Kolekcija `postman/SportskaLiga.postman_collection.json` ima 43 zahtjeva s testovima za sve endpointe (uspješne i
neuspješne slučajeve). Zahtjevi spremaju token i Id-eve u varijable, pa se kolekcija pokreće redom; na kraju vrati
podatke kakvi su bili (poništi upisani rezultat, obriše testnu momčad).

- Postman: uvezi kolekciju i okruženje `SportskaLiga.docker` (ili `SportskaLiga.lokalno` za `dotnet run`), zatim
  *Run collection*.
- Iz Dockera (bez instalacije Postmana): `docker compose --profile test run --rm newman`

## Domenski model

| Entitet | Bitno |
|---|---|
| `Sezona` | naziv, datumi, bodovi za pobjedu/neriješeno, aktivna (samo jedna) |
| `Momcad` | jedinstveni naziv, grad, igralište; u sezone se upisuje preko `SezonaMomcad` (M:N) |
| `Igrac` | ime, prezime, datum rođenja, broj dresa (jedinstven u momčadi), pozicija |
| `Sudac` | ime, prezime, kontakt |
| `Utakmica` | sezona, kolo, početak (UTC), domaćin, gost, golovi, status (Zakazana/Odigrana/Odgođena), sudac, delegat |
| `Gol` | utakmica, strijelac, minuta (1–120), autogol; momčad kojoj se gol pribraja |
| `PrijavaMomcadi` | prijava s portala: kod, podaci momčadi i kontakta, status, odgovor lige |
