# Tooded_DB

Poe haldusrakendus: Windows Forms (.NET 10), Entity Framework Core 10 ja SQL Server LocalDB. Kõik vormid on tehtud koodis, ilma Designerita (välja arvatud Form2.Designer.cs). Liides on kolmes keeles: eesti, vene ja inglise.

## Käivitamine

1. Ava lahendus Visual Studios, vajuta F5.
2. Esimesel käivitamisel loob `Program.Main` andmebaasi (`Migrate()`) ja lisab näidisandmed.
3. Kui muudad mudelit: `Add-Migration Nimi`, siis `Update-Database`.

Vajalik: .NET 10 SDK, SQL Server LocalDB.

## Kasutajad

| Roll | Kasutajanimi | Parool |
|---|---|---|
| Omanik | omanik | omanik123 |
| Müüja | muuja | muuja123 |
| Klient | klient | klient123 |

Paroolid on andmebaasis BCrypt-räsina. Uue kasutaja saab registreerida sisselogimisaknas, ta saab rolli Klient.

## Õigused

| Roll | Saab teha |
|---|---|
| Omanik | kõik: ladu, kliendid ja boonused, pakkumised, müügiajalugu, tagastused |
| Müüja | kassa, uue kliendi lisamine, müügiajalugu; boonust ei muuda ja kliente ei kustuta |
| Klient | pood, veebitellimus, oma ostud ja tšekid, ostunimekiri |

## Mida rakendus oskab

- **Ladu**: toodete ja kategooriate lisamine, muutmine, kustutamine, pildid, ühik (tk või kg).
- **Kassa**: ostukorv, kaalutooted (kogus kilogrammides), kliendi otsing kaardi või nime järgi.
- **Soodused**: protsentsoodus tootel, pakkumised (N+M tasuta, komplekt hinnaga X, soodus teisele tootele), sünnipäevasoodus (−15 % ostukorvile ja −5 % kondiitritoodetele, ±3 päeva −10 %).
- **Boonus**: 5 % makstud summast, saab järgmisel ostul kasutada.
- **Maksmine**: sularahas (arvutab tagasi antava summa), kaardiga, arvega (IBAN ja viitenumber).
- **Tšekk**: PDF kaustas `Arved`, saab saata e-postiga.
- **Müügiajalugu ja tagastus**: tagastamisel läheb kaup lattu tagasi ja boonus võetakse tagasi.
- **Veebipood**: klient tellib ja maksab ise, tšekk jääb tema ostude alla.
- **Keeled**: ET / RU / EN, valik jääb meelde (`%LOCALAPPDATA%\ToodedApp\keel.txt`). Tõlgitakse ka tooted, kategooriad, tšekid ja teated.

## Struktuur

| Fail | Sisu |
|---|---|
| `Program.cs` | käivitus, migratsioonid, näidisandmed |
| `ToodedContext.cs` | DbSet-id, indeksid, seosed |
| `Toode.cs`, `Klient.cs`, `Kasutaja.cs`, `Muuk.cs`, `Pakkumine.cs`, `Kategooria.cs` | andmemudelid |
| `Sessioon.cs` | aktiivne kasutaja, rollid, BCrypt |
| `LoginForm.cs`, `RegistreeriForm.cs`, `FormMain.cs` | sisselogimine ja peamenüü |
| `Form2.cs` | ladu (omanik) |
| `Form3.cs` | kliendi pood |
| `KassaForm.cs` | kassa ja veebipood, `Maksa()` teeb ostu ühes transaktsioonis |
| `Allahindlus.cs` | soodusloogika |
| `KliendidForm.cs`, `MuugidForm.cs`, `OstudForm.cs`, `PakkumisedForm.cs`, `OstunimekiriForm.cs` | muud vormid |
| `Keel.cs` | tõlked ja keelevahetus |
| `Teema.cs` | värvid ja paigutus |
| `Epost.cs` | tšeki saatmine |

## Paketid

BCrypt.Net-Next 4.2.0, Microsoft.EntityFrameworkCore ja SqlServer 10.0.12, PdfSharpCore 1.3.67.

## E-post

Tšeki saatmiseks loo fail `%LOCALAPPDATA%\ToodedApp\epost.txt`: esimesel real Gmaili aadress, teisel rakenduse parool (App password).
