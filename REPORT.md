# Практическая работа №5

**Проектирование безопасных релизных процессов, паттернов миграции данных и эксплуатационной документации ИС «Умный кампус»**

| | |
|---|---|
| Студент | Тимофеева Полина Антоновна |
| Группа | И14-23-9 |
| Дисциплина | МДК 03.01 |
| Вариант | 9. Парковка (Шлагбаум) |
| Задача | CAMPUS-109. Переименование поля CarPlate > LicensePlate |

## Состав работы

Контроллер с тегами OpenTelemetry – [ParkingSessionsController.cs](src/SmartCampus.Parking/Controllers/ParkingSessionsController.cs)

Двойная запись в старую и новую колонку – [ParkingSessionRepository.cs](src/SmartCampus.Parking/Services/ParkingSessionRepository.cs)

SQL-скрипты миграции – [01_expand.sql](sql/01_expand.sql), [02_migrate.sql](sql/02_migrate.sql), [03_contract.sql](sql/03_contract.sql)

Production Sanity-тест – [ParkingSanityTests.cs](tests/SmartCampus.Parking.SanityTests/ParkingSanityTests.cs)

Change Request – [change-request-v1.9.0.md](docs/change-request-v1.9.0.md)

Правила защиты ветки main – [branch-protection.md](docs/branch-protection.md)

ADR – [0009-rename-carplate-to-licenseplate.md](docs/adr/0009-rename-carplate-to-licenseplate.md)

Changelog – [CHANGELOG.md](CHANGELOG.md)

Изменения зафиксированы коммитом с сообщением `[CAMPUS-109] Add tracing tags and business logic`.

## Цифровой след

В каждом методе контроллера ParkingSessionsController текущая операция берется из Activity.Current. В нее записываются тег jira.issue со значением CAMPUS-109 и доменный идентификатор campus.parking_session_id. Для въезда дополнительно указывается campus.barrier_id. Сам номер автомобиля в теги не попадает, так как это персональные данные. Трассировка подключена в Program.cs через OpenTelemetry с инструментированием ASP.NET Core.

## Change Request

Заявка CR-2026-109 оформлена на релиз v1.9.0 сервиса parking-service. Релиз добавляет колонку license_plate, двойную запись номера и теги трассировки. Цель релиза – единое название поля во всех сервисах кампуса без остановки шлагбаума.

Окно обслуживания назначено на воскресенье 01.11.2026 с 03:00 до 05:00 МСК. В это время гостевой въезд закрыт, а по журналу шлагбаума проезжает всего 3–4 служебных автомобиля в час. Утренний пик в воскресенье начинается поздно, поэтому на откат остается достаточно времени.

План работ начинается с резервной копии таблицы parking_sessions. Затем выполняется скрипт 01_expand.sql, выкатывается образ 1.9.0 и запускается Sanity-тест. Только после этого переносятся исторические данные скриптом 02_migrate.sql. Фаза Contract в этот релиз не входит и пойдет отдельной заявкой вместе с v1.10.0.

Откат выполняется командой kubectl rollout undo deployment/parking-service -n smart-campus. Поводом для него служат проваленный Sanity-тест, доля ошибок 5xx выше 2 % дольше пяти минут или отказ шлагбаума открыться по распознанному номеру. Новую колонку при откате удалять не нужно, предыдущая версия ее не замечает.

## Миграция Expand and Contract

На фазе Expand добавляется колонка license_plate с разрешенным NULL и индекс, построенный без блокировки таблицы. Версия v1.9.0 пишет номер сразу в обе колонки. На фазе Migrate старые записи получают значение из car_plate, а контрольный запрос показывает, сколько строк осталось. Фаза Contract делает license_plate обязательной и удаляет car_plate, но только после выхода версии, которая со старой колонкой уже не работает.

## Sanity-тест

Тест ParkingSanityTests после релиза создает на боевом стенде тестовую сессию въезда с номером SANITY000 и проверяет, что ее можно прочитать. В блоке finally сессия удаляется в любом случае, даже если одна из проверок не прошла. Адрес стенда задается переменной окружения SANITY_BASE_URL.
