🚀 **Имя проекта:** UrlShortener

🛠 **Технологический стек**
- **Платформа:** .NET 10
- **Backend:** ASP.NET Core Minimal API
- **ORM:** Entity Framework Core 9 + Pomelo (MariaDB/MySQL)
- **База данных:** MariaDB / InMemory (для тестов)
- **Frontend:** Blazor WebAssembly (WASM)
- **Тесты:** xUnit

💻 **Инструкция по запуску**  

***Все команды выполняются из корневой директории проекта***  

***Бэк***
```bash
dotnet run --project UrlShortener.Api cd D:\downloads\UrlShortener --urls="https://localhost:7037" --ConnectionStrings:Default="Server=localhost;Port=3306;Database=UrlShortener;User=пользователь;Password=пароль;"
```

***Фронт***
```bash
dotnet run --project UrlShortener.Web --launch-profile https
```

***Тесты***
```bash
dotnet test UrlShortener.sln
```

***API***  
http://localhost:5227/
