# Efektívne načítanie kurzov (demo)

Aplikácia:
- načíta zoznam správ zo súboru `zdrojovy_dokument.json`,
- spracuje ich paralelne podľa požiadaviek,
- zabezpečí sekvenčné spracovanie správ pre rovnakú udalosť (podľa `ProviderEventID`),
- pred každým uložením udalosti simuluje volanie externého API pauzou 0–10 sekúnd,
- uloží alebo aktualizuje dáta v MS SQL databáze s minimálnou záťažou (aktualizuje len zmenené dáta, používa efektívny zápis).

## Štruktúra repozitára
- `assignment.md` – zadanie.
- `NacitanieKurzovConsoleApp/` – .NET konzolová aplikácia.
- `docker-compose.yml` – spustí MS SQL DB aj konzolovú appku; env vars sú nastavené tu.
- `NacitanieKurzovConsoleApp/zdrojovy_dokument.json` – vzorové dáta sú súčasťou repozitára.

## Setup (Docker Compose)
Je potrebný Docker + Git.

- repo clone:
  ```bash
  git clone https://github.com/SimSef/efektivne_nacitanie_kurzov_demo.git
  ```
- cd dir:
  ```bash
  cd efektivne_nacitanie_kurzov_demo
  ```
- run docker compose:
  ```bash
  docker compose up --build
  ```

Priebeh:
- SQL Server kontajner sa spustí s parametrami z `docker-compose.yml`.
- Aplikácia počká na dostupnú DB, načíta `zdrojovy_dokument.json`, rozdelí správy podľa `ProviderEventID`, simuluje 0–10s oneskorenie a upsertuje do tabuliek udalostí a kurzov.
- Logy spracovania sú v konzole/DockerDesktop.

## Overenie dát (voliteľné)
- Ak beží compose DB, počty riadkov si možno overiť takto:
```bash
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -C -U sa -P 'YourStrong!Passw0rd' -Q "SELECT COUNT(*) AS EventsCount FROM EventsDb.dbo.Events; SELECT COUNT(*) AS OddsCount FROM EventsDb.dbo.Odds;"
```

## Poznámky a nastavenia
- Partície sa nastavujú v `PartitionedChannelProcessor.CreateChannels` (default: 30 partícií, kapacita 20).
- Connection string sa berie z `ConnectionStrings__DefaultConnection`; ak chýba, aplikácia sa ukončí.
- Schéma sa inicializuje pri štarte (`DatabaseInitializer`).
- Náhodné oneskorenie per event simuluje externé API (0–10s pred každým zápisom).
