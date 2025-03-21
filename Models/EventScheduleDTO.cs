namespace EventMgmt.Models
{
    public class EventScheduleDTO
    {
        public int? EventID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? EventTitle { get; set; }
        public string? Comments { get; set; }
        public string? Address { get; set; }
        public string ScheduleDate { get; set; }
    }
}
