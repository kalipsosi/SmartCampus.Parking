using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Parking.Models;
using SmartCampus.Parking.Services;

namespace SmartCampus.Parking.Controllers
{
    [ApiController]
    [Route("api/parking/sessions")]
    public class ParkingSessionsController : ControllerBase
    {
        private const string JiraIssue = "CAMPUS-109";
        private readonly ParkingSessionRepository _repository;

        public ParkingSessionsController(ParkingSessionRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<ActionResult<ParkingSession>> Enter(EntryRequest request)
        {
            var activity = Activity.Current;
            activity?.SetTag("jira.issue", JiraIssue);
            activity?.SetTag("campus.barrier_id", request.BarrierId);

            string plate = NormalizePlate(request.LicensePlate);
            if (plate.Length == 0)
            {
                return BadRequest("Не указан номер автомобиля");
            }

            var session = await _repository.CreateAsync(plate, request.BarrierId);
            activity?.SetTag("campus.parking_session_id", session.Id);

            return CreatedAtAction(nameof(Get), new { id = session.Id }, session);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ParkingSession>> Get(long id)
        {
            var activity = Activity.Current;
            activity?.SetTag("jira.issue", JiraIssue);
            activity?.SetTag("campus.parking_session_id", id);

            var session = await _repository.GetAsync(id);
            return session is null ? NotFound() : Ok(session);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var activity = Activity.Current;
            activity?.SetTag("jira.issue", JiraIssue);
            activity?.SetTag("campus.parking_session_id", id);

            return await _repository.DeleteAsync(id) ? NoContent() : NotFound();
        }

        // Камера шлагбаума может прислать номер в разном регистре и с пробелами
        private static string NormalizePlate(string plate)
        {
            return plate.Replace(" ", string.Empty).Trim().ToUpperInvariant();
        }
    }
}
