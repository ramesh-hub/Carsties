rmdir /S /Q "Data/Migrations"

rem dotnet ef migrations add Users -c ApplicationDbContext -o Data/Migrations
dotnet ef migrations add "InitialCreate" -o Data/Migrations
