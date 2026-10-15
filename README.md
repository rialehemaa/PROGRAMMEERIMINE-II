# Tooded_DB

Windows Forms (.NET 10) rakendus toodete ja kategooriate haldamiseks. Andmebaas: Entity Framework Core + SQL Server LocalDB.

- `Kategooria.cs`, `Toode.cs` - andmemudelid
- `ToodedContext.cs` - DbContext (ühendus `(localdb)\MSSQLLocalDB`, andmebaas `Tooded_DB`)
- `Migrations/` - EF Core migratsioonid
- `Form2.cs` - põhivorm (lisa / uuenda / kustuta toode, pildi valik)

## Käivitamine

1. Visual Studios ava `Tooded_DB.slnx`
2. Package Manager Console: `Update-Database`
3. F5
