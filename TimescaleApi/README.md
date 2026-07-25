# Timescale Data API

Web API приложение для обработки, валидации и агрегации временных рядов из CSV-файлов. Разработано в рамках тестового задания на позицию C# Developer Intern.

## Стек технологий
- **.NET 8**
- **Entity Framework Core** (PostgreSQL provider)
- **PostgreSQL**
- **xUnit** (Unit-тесты)
- **Swagger** (OpenAPI документация)

## Архитектура и ключевые решения
Проект построен по принципам Clean Architecture с четким разделением ответственности:
1. **Controllers**: Только прием HTTP-запросов, базовая валидация и маппинг в DTO.
2. **Services**: Чистая бизнес-логика.
    - `CsvService`: Парсинг потоком (`StreamReader`) для экономии памяти. Расчет статистики оптимизирован до **O(n)** за один проход по коллекции.
    - `DataService`: Транзакционное управление (`BeginTransactionAsync`). Использование `ExecuteDeleteAsync` для удаления старых данных без их выгрузки в память (экономия RAM и времени).
3. **Validators**: Строгая валидация каждой строки CSV с детальными сообщениями об ошибках.
4. **Data**: Настройка индексов в EF Core Fluent API. Использование `AsNoTracking()` для всех запросов на чтение для повышения производительности.

## Как запустить проект

### 1. Требования
- Установленный [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Запущенный экземпляр PostgreSQL

### 2. Настройка базы данных
Откройте `appsettings.Development.json` и укажите актуальную строку подключения:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=timescale_db;Username=postgres;Password=your_password"
}