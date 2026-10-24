using Npgsql;
using SmartCampus.Parking.Models;

namespace SmartCampus.Parking.Services
{
    public class ParkingSessionRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public ParkingSessionRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        // Фаза Expand: номер пишется одновременно в старую колонку car_plate
        // и в новую license_plate, чтобы старая версия сервиса продолжала работать.
        public async Task<ParkingSession> CreateAsync(string licensePlate, string barrierId)
        {
            const string sql = @"
                INSERT INTO parking_sessions (car_plate, license_plate, barrier_id, entered_at)
                VALUES (@carPlate, @licensePlate, @barrierId, now())
                RETURNING id, entered_at";

            await using var command = _dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("carPlate", licensePlate);
            command.Parameters.AddWithValue("licensePlate", licensePlate);
            command.Parameters.AddWithValue("barrierId", barrierId);

            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();

            return new ParkingSession
            {
                Id = reader.GetInt64(0),
                LicensePlate = licensePlate,
                BarrierId = barrierId,
                EnteredAt = reader.GetDateTime(1)
            };
        }

        // Пока не выполнена фаза Migrate, у старых записей license_plate пустая,
        // поэтому значение берется из car_plate.
        public async Task<ParkingSession?> GetAsync(long id)
        {
            const string sql = @"
                SELECT id, COALESCE(license_plate, car_plate), barrier_id, entered_at
                FROM parking_sessions
                WHERE id = @id";

            await using var command = _dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new ParkingSession
            {
                Id = reader.GetInt64(0),
                LicensePlate = reader.GetString(1),
                BarrierId = reader.GetString(2),
                EnteredAt = reader.GetDateTime(3)
            };
        }

        public async Task<bool> DeleteAsync(long id)
        {
            await using var command = _dataSource.CreateCommand("DELETE FROM parking_sessions WHERE id = @id");
            command.Parameters.AddWithValue("id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
