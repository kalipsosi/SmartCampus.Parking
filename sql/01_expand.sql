-- CAMPUS-109. Фаза 1 (Expand)
-- Добавляется новая колонка license_plate рядом со старой car_plate.
-- Колонка допускает NULL, поэтому старая версия сервиса (v1.8.x),
-- которая про нее не знает, продолжает вставлять записи без ошибок.
-- Скрипт выполняется ДО выкатки версии v1.9.0 с двойной записью.

ALTER TABLE parking_sessions
    ADD COLUMN IF NOT EXISTS license_plate VARCHAR(15) NULL;

COMMENT ON COLUMN parking_sessions.license_plate IS
    'Госномер автомобиля. Заменяет устаревшую колонку car_plate (CAMPUS-109)';

-- Шлагбаум ищет открытые сессии по номеру, поэтому сразу нужен индекс.
-- CONCURRENTLY не блокирует запись в таблицу во время построения.
-- Эту команду нельзя выполнять внутри транзакции.
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_parking_sessions_license_plate
    ON parking_sessions (license_plate);
