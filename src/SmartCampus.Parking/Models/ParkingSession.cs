namespace SmartCampus.Parking.Models
{
    public class ParkingSession
    {
        public long Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string BarrierId { get; set; } = string.Empty;
        public DateTime EnteredAt { get; set; }
    }
}
