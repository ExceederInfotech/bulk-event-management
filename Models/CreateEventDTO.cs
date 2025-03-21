using System.ComponentModel;

namespace EventMgmt.Models
{
    public class CreateEventDTO
    {
        public int EventID { get; set; }

        [DisplayName("Start date")]
        public DateTime StartDate { get; set; }

        [DisplayName("End date")]
        public DateTime EndDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }

        [DisplayName("Event title")]
        public string? EventTitle { get; set; }
        public string? Comment { get; set; }

        [DisplayName("Address")]
        public string? EventAddress { get; set; }

        [DisplayName("Event start date")]
        public string? EventStartDate { get; set; }

        [DisplayName("Event end date")]
        public string? EventEndDate { get; set; }
    }
}
