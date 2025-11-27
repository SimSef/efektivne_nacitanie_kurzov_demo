# ZADANIE – Efektívne načítanie kurzov

## Vstupné dáta

Na vstupe sú dáta (celý zoznam správ, udalostí a ich odds je v prílohe – `zdrojovy_dokument.zip`, treba manuálne rozbaliť a pracovať so súborom `zdrojovy_dokument.json`) v nasledujúcom formáte:

```json
[
  {
    "MessageID": "d5787924-d080-40d4-bfad-f0f2e09e6abe",
    "GeneratedDate": "2022-10-10T12:44:44.9304711+02:00",
    "Event": {
      "ProviderEventID": 1499420517,
      "EventName": "Team B vs. Team H",
      "EventDate": "2022-10-18T00:30:00",
      "OddsList": [
        {
          "ProviderOddsID": 764331995,
          "OddsName": "Home",
          "OddsRate": 1.981,
          "Status": "suspended"
        },
        {
          "ProviderOddsID": 1635670100,
          "OddsName": "Draw",
          "OddsRate": 2.56,
          "Status": "active"
        },
        {
          "ProviderOddsID": 820885954,
          "OddsName": "Away",
          "OddsRate": 1.454,
          "Status": "active"
        }
        ...
      ]
    }
  }
]
```

## Cieľ

Cieľom je implementovať (napr. konzolovú) aplikáciu (.NET Framework alebo .NET Core), ktorá dokáže uložiť športové udalosti (ďalej len „udalosti“) a ich odds (ďalej len „oddy“) do MS SQL databázy čo najrýchlejšie, pri dodržaní:

- poradia spracovania per udalosť (udalosti medzi sebou sa môžu predbiehať, ale všetky správy patriace k jednej udalosti musia byť spracované v presnom poradí, v akom sú v zdrojovom dokumente),
- čo najnižšej záťaže na databázu.

## Špecifikácia spracovania

### 1. Načítanie dát

- Všetky udalosti je možné naraz načítať zo súboru a rozparsovať.
- Nesmie prebiehať žiadne dodatočné radenie – iterácia má ísť v poradí pôvodného súboru.
- Po načítaní by mala nasledovať iterácia per udalosť (simulácia prijatia každej udalosti samostatne) bez akéhokoľvek radenia daného zoznamu.

### 2. Unikátne identifikátory

- Každá športová udalosť bude v databáze len raz.  
  → unikátny kľúč: `ProviderEventID`
- Každý odds bude v databáze len raz.  
  → unikátny kľúč: `ProviderOddsID`
- V databáze majú mať všetky entity vlastné auto-increment ID (primárne kľúče).

### 3. Aktualizácie

- V zdrojovom dokumente sa niektoré udalosti opakujú viackrát – opakujúce sa udalosti treba **updatovať**, nie znovu vkladať.
- U udalosti sa môže meniť:
  - `EventDate`
- U odds sa môže meniť:
  - `Status`
  - `OddsRate`

### 4. Externé API (simulácia)

Pred uložením každej udalosti do databázy musí byť vykonané volanie externého API, ktoré sa simuluje:

- zastavením vlákna na náhodné trvanie **0 až 10 sekúnd** (sleep).

### 5. Paralelizmus a poradie

- Udalosti ako celky sa môžu spracovávať **paralelne**.
- Správy patriace k tej istej udalosti musia byť spracovávané **sekvenčne** v poradí, v akom sú v súbore.
- Na poradí spracovania rôznych udalostí medzi sebou nezáleží (môžu sa predbiehať / bežať paralelne).
- Cieľom nie je len uložiť dáta, ale urobiť to čo najrýchlejšie (využiť paralelizmus) a s čo najnižšou záťažou na databázu – vhodne zvoliť spôsob ukladania dát (dôraz na výkon).
- Nie je nutné riešiť situáciu vypnutia aplikácie počas spracovávania.

## Zhrnutie očakávaného výsledku

Aplikácia by mala:

- načítať zoznam správ zo súboru `zdrojovy_dokument.json`,
- spracovať ich paralelne podľa požiadaviek,
- zabezpečiť sekvenčné spracovanie správ pre rovnakú udalosť (podľa `ProviderEventID`),
- pred každým uložením udalosti simulovať volanie externého API pauzou 0–10 sekúnd,
- uložiť alebo aktualizovať dáta v MS SQL databáze s minimálnou záťažou (aktualizovať len zmenené dáta, využiť efektívny spôsob zápisu).

