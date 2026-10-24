# Отчет по практической работе №5
## Проектирование безопасных релизных процессов, паттернов миграции данных и эксплуатационной документации ИС «Умный кампус»

---

### Титульная информация

| Параметр | Значение |
|----------|----------|
| **ФИО студента** | Тимофеева Полина Антоновна |
| **Группа** | И14-23-9 |
| **Номер варианта** | 9 |
| **Модуль ИС** | Парковка (Шлагбаум) |
| **Задача Jira** | CAMPUS-109 |
| **Суть миграции БД** | Переименование поля CarPlate > LicensePlate |
| **Дата выполнения** | 24.10.2026 |

---

## Ссылки на артефакты

### Блок 1. Единый цифровой след (Digital Traceability)
- [Контроллер с тегами OpenTelemetry](src/SmartCampus.Parking/Controllers/ParkingSessionsController.cs)
- [Подключение OpenTelemetry в Program.cs](src/SmartCampus.Parking/Program.cs)
- Коммит `[CAMPUS-109] Add tracing tags and business logic`

### Блок 2. Регламент релиза и Change Management
- [Change Request CR-2026-109](docs/change-request-v1.9.0.md)
- [Правила защиты ветки main](docs/branch-protection.md)
- [Проверка сборки в CI](../.github/workflows/ci.yml)

### Блок 3. Безопасная миграция базы данных (Expand and Contract)
- [Фаза 1. Expand](sql/01_expand.sql)
- [Фаза 2. Migrate](sql/02_migrate.sql)
- [Фаза 3. Contract](sql/03_contract.sql)
- [Репозиторий с двойной записью car_plate и license_plate](src/SmartCampus.Parking/Services/ParkingSessionRepository.cs)

### Блок 4. Sanity-тестирование и Docs-as-Code
- [Production Sanity-тест](tests/SmartCampus.Parking.SanityTests/ParkingSanityTests.cs)
- [ADR-0009](docs/adr/0009-rename-carplate-to-licenseplate.md)
- [Changelog](CHANGELOG.md)

---

## Описание заполненного Change Request

**Номер CR** – CR-2026-109  
**Версия** – v1.9.0  
**Сервис** – parking-service (модуль «Парковка (Шлагбаум)»)

### Бизнес-цель
Номер автомобиля хранится в колонке `car_plate` таблицы `parking_sessions`. Камеры распознавания и другие сервисы кампуса называют это поле `LicensePlate`, поэтому в коде постоянно приходится переводить одно название в другое. Релиз добавляет колонку `license_plate`, двойную запись и теги трассировки. Цель – единое название поля во всех сервисах без остановки шлагбаума.

### Окно обслуживания
Выбрано время **воскресенье 01.11.2026, 03:00–05:00 МСК**. Гостевой въезд на территорию кампуса ночью закрыт, а по журналу шлагбаума в это время проезжает 3–4 служебных автомобиля в час. В воскресенье нет занятий, и до утреннего потока остается запас времени на откат. Работы занимают около 1,5 часа.

### План проведения работ
1. **Подготовка** (03:00) – резервная копия таблицы `parking_sessions`.
2. **Expand** (03:10) – выполнение `01_expand.sql`, добавление колонки `license_plate` и индекса.
3. **Выкатка** (03:20) – `kubectl set image` на образ 1.9.0 с двойной записью, контроль `kubectl rollout status`.
4. **Sanity-тест** (03:35) – создание и удаление тестовой сессии въезда.
5. **Migrate** (03:45) – выполнение `02_migrate.sql`, проверка `not_migrated = 0`.
6. **Проверка трасс** (04:00) – поиск по тегу `jira.issue = CAMPUS-109`.

Фаза **Contract** (`03_contract.sql`) выполняется отдельной заявкой вместе с версией v1.10.0, когда сервис уже не обращается к `car_plate`.

### План отката
Откат выполняется, если Sanity-тест не прошел, ошибок 5xx больше 2 % дольше 5 минут или шлагбаум не открывается по распознанному номеру.
```bash
kubectl rollout undo deployment/parking-service -n smart-campus
kubectl rollout status deployment/parking-service -n smart-campus
```
Колонку `license_plate` удалять не нужно, предыдущая версия ее не замечает. При повреждении данных таблица восстанавливается из резервной копии.

### Branch Protection Rules
Для ветки `main` требуется Pull Request, **1 одобрение** и обязательное согласование владельца кода для папки `sql/`. Слияние возможно только после успешной проверки **build** в CI. Force push и удаление ветки запрещены, правила действуют и для администраторов.
