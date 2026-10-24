# Changelog

Все заметные изменения сервиса парковки фиксируются в этом файле.
Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/), версии нумеруются по [Semantic Versioning](https://semver.org/lang/ru/).

## [Unreleased]

### Breaking Changes

- Колонка car_plate будет удалена из таблицы parking_sessions в версии v1.10.0 (скрипт sql/03_contract.sql). Внешние отчеты и выгрузки, которые читают car_plate напрямую из БД, нужно перевести на license_plate до этого релиза.

## [1.9.0] – 2026-11-01

### Added

- Колонка license_plate в таблице parking_sessions и индекс ix_parking_sessions_license_plate (CAMPUS-109).
- Двойная запись номера автомобиля в car_plate и license_plate.
- Теги OpenTelemetry jira.issue, campus.parking_session_id и campus.barrier_id в ParkingSessionsController.
- Production Sanity-тест ParkingSanityTests с удалением тестовой сессии.

### Fixed

- Номер, присланный камерой с пробелами или в нижнем регистре, больше не создает отдельную сессию. Перед сохранением номер приводится к единому виду.

### Breaking Changes

- Нет. Изменения схемы БД только добавляют новые объекты, поэтому версия совместима с v1.8.x.
