# Administracija (ASP.NET Core MVC).
# Build context je KORIJEN repozitorija:
#   docker build -f src/SportskaLiga.Admin/Dockerfile -t sportskaliga-admin .

# ---- build ---- (SDK slika, ~800 MB; u konačnu sliku ne ulazi)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Najprije samo datoteke potrebne za restore: Docker kešira ovaj sloj dok se paketi i reference ne promijene,
# pa izmjena koda ne skida pakete ponovno.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/SportskaLiga.Domain/SportskaLiga.Domain.csproj src/SportskaLiga.Domain/
COPY src/SportskaLiga.Data/SportskaLiga.Data.csproj src/SportskaLiga.Data/
COPY src/SportskaLiga.Admin/SportskaLiga.Admin.csproj src/SportskaLiga.Admin/
RUN dotnet restore src/SportskaLiga.Admin/SportskaLiga.Admin.csproj

# Zatim ostatak koda i publish (Release).
COPY . .
RUN dotnet publish src/SportskaLiga.Admin/SportskaLiga.Admin.csproj -c Release -o /app/publish --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

# Alpine pokreće .NET u invariant modu: ICU podaci trebaju za kulturu hr-HR (datumi, decimalni zarez),
# tzdata za vremensku zonu Europe/Zagreb (Kalendar), a krb5-libs traži Npgsql (inače upozorenje u logu).
RUN apk add --no-cache icu-libs icu-data-full tzdata krb5-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080

WORKDIR /app
COPY --from=build /app/publish ./

# Mapa koju compose montira kao imenovani volumen (DataProtection ključevi). Prazan volumen preuzima
# vlasnika mape iz slike, pa je mora posjedovati korisnik aplikacije (ne root).
RUN mkdir -p /keys && chown $APP_UID /keys

# Aplikacija ne radi kao root (korisnik "app" iz službene slike).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "SportskaLiga.Admin.dll"]
