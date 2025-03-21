using System.ComponentModel;

namespace EventMgmt.Models
{
    public class EventDetailsDTO
    {
        public int EventId { get; set; }
        [DisplayName("Event name")]
        public string EventName { get; set; }
        public string Comments { get; set; }
        [DisplayName("Start date")]
        public string StartDate { get; set; }
        [DisplayName("End date")]
        public string EndDate { get; set; }
        public string Address { get; set; }
        public List<EventScheduleDTO> EventSchedule { get; set; }
    }
}
