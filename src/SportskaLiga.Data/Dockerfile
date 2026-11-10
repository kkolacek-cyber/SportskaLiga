# Inicijalizacijski kontejner "migracije": EF Core migration bundle (dodatni Docker zahtjev).
# Build context je KORIJEN repozitorija:
#   docker build -f src/SportskaLiga.Data/Dockerfile -t sportskaliga-migracije .
#
# Migration bundle je samostalna izvršna datoteka (efbundle) koja sadrži sve migracije iz projekta Data.
# Pokrene se jednom nakon što je baza zdrava, primijeni migracije koje nedostaju i završi s kodom 0.
# Tek tada se pokreće Admin (depends_on: service_completed_successfully), koji samo seeda početne podatke.
# Prednost: shema baze ne ovisi o pokretanju web aplikacije, a runtime slike aplikacija ne trebaju dotnet-ef.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Alat dotnet-ef (verzija iz dotnet-tools.json) i restore paketa – keširani slojevi.
COPY global.json Directory.Build.props Directory.Packages.props dotnet-tools.json ./
RUN dotnet tool restore

COPY src/SportskaLiga.Domain/SportskaLiga.Domain.csproj src/SportskaLiga.Domain/
COPY src/SportskaLiga.Data/SportskaLiga.Data.csproj src/SportskaLiga.Data/
RUN dotnet restore src/SportskaLiga.Data/SportskaLiga.Data.csproj

COPY src/SportskaLiga.Domain/ src/SportskaLiga.Domain/
COPY src/SportskaLiga.Data/ src/SportskaLiga.Data/

# Isti -p/-s kao kod "dotnet ef migrations add" (Data je i projekt migracija i startup projekt).
RUN dotnet ef migrations bundle \
        --project src/SportskaLiga.Data \
        --startup-project src/SportskaLiga.Data \
        --configuration Release \
        --output /app/efbundle \
        --force

# ---- runtime ----
# Bundle je ovisan o .NET runtimeu; Data projekt referencira ASP.NET Core (Identity), pa slika aspnet.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
RUN apk add --no-cache krb5-libs
WORKDIR /app
COPY --from=build /app/efbundle ./

USER $APP_UID

# Connection string dolazi iz varijable okruženja (compose), nikad nije zapisan u slici.
ENTRYPOINT ["/bin/sh", "-c", "exec ./efbundle --connection \"$ConnectionStrings__SportskaLiga\""]
