namespace EventManagementApi.Entity
{
    public class EventRegistration
    {
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; } = "Unregistered";
        public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
    }
}