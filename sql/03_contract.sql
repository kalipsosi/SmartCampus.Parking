-- CAMPUS-109. Фаза 3 (Contract)
-- Выполняется только после выхода версии v1.10.0, которая читает и пишет
-- исключительно license_plate, и после того, как 02_migrate.sql показал 0
-- неперенесенных строк. Откатить эту фазу без резервной копии нельзя.

BEGIN;

ALTER TABLE parking_sessions
    ALTER COLUMN license_plate SET NOT NULL;

DROP INDEX IF EXISTS ix_parking_sessions_car_plate;

ALTER TABLE parking_sessions
    DROP COLUMN car_plate;

COMMIT;
