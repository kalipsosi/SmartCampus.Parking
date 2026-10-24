-- CAMPUS-109. Фаза 2 (Migrate)
-- Перенос исторических данных из car_plate в license_plate.
-- Запускается, когда все экземпляры сервиса уже обновлены до v1.9.0
-- и новые записи пишутся в обе колонки.
-- Условие license_plate IS NULL делает скрипт безопасным для повторного запуска.

UPDATE parking_sessions
SET license_plate = car_plate
WHERE license_plate IS NULL
  AND car_plate IS NOT NULL;

-- Проверка. Результат должен быть равен 0, иначе к фазе Contract переходить нельзя.
SELECT COUNT(*) AS not_migrated
FROM parking_sessions
WHERE license_plate IS NULL;
