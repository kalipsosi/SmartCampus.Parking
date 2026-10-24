using System.Net;
using System.Net.Http.Json;

namespace SmartCampus.Parking.SanityTests
{
    // Запускается сразу после релиза против боевого стенда.
    // Адрес стенда передается через переменную окружения SANITY_BASE_URL.
    public class ParkingSanityTests
    {
        private const string TestPlate = "SANITY000";

        private static readonly string BaseUrl =
            Environment.GetEnvironmentVariable("SANITY_BASE_URL") ?? "https://parking.smartcampus.local/";

        [Fact]
        public async Task BarrierEntry_CreatesSession_AndCleansUp()
        {
            using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            long? sessionId = null;

            try
            {
                var response = await client.PostAsJsonAsync("api/parking/sessions",
                    new { licensePlate = TestPlate, barrierId = "BARRIER-SANITY" });
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                var session = await response.Content.ReadFromJsonAsync<SessionDto>();
                Assert.NotNull(session);
                sessionId = session.Id;
                Assert.Equal(TestPlate, session.LicensePlate);

                var check = await client.GetAsync($"api/parking/sessions/{sessionId}");
                Assert.Equal(HttpStatusCode.OK, check.StatusCode);
            }
            finally
            {
                // Cleanup: тестовая сессия удаляется в любом случае, даже если проверка упала
                if (sessionId.HasValue)
                {
                    var cleanup = await client.DeleteAsync($"api/parking/sessions/{sessionId}");
                    Assert.Equal(HttpStatusCode.NoContent, cleanup.StatusCode);
                }
            }
        }

        private record SessionDto(long Id, string LicensePlate, string BarrierId);
    }
}
