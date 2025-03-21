using ClosedXML.Excel;
using EventMgmt.Exceptions;
using EventMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace EventMgmt.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class EventScheduleController : ControllerBase
    {
        private readonly AppDbcontext _context;

        public EventScheduleController(AppDbcontext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> BulkUploadEventSchedule(IFormFile formFile)
        {
            using var connection = _context.Sqlconnection();
            try
            {
                if (formFile == null || formFile.Length == 0)
                    throw new BadRequestException("No file uploaded.");

                var records = ReadFile(formFile);
                DataTable dtEventSchedule = new();
                dtEventSchedule.Columns.Add("EventID", typeof(int));
                dtEventSchedule.Columns.Add("StartDate", typeof(DateTime));
                dtEventSchedule.Columns.Add("EndDate", typeof(DateTime));
                dtEventSchedule.Columns.Add("StartTime", typeof(string));
                dtEventSchedule.Columns.Add("EndTime", typeof(string));
                dtEventSchedule.Columns.Add("EventTitle", typeof(string));
                dtEventSchedule.Columns.Add("Comments", typeof(string));
                dtEventSchedule.Columns.Add("Address", typeof(string));

                var result = await records;
                foreach (var eventDetails in result)
                {
                    dtEventSchedule.Rows.Add(eventDetails.EventID,
                                               eventDetails.StartDate, eventDetails.EndDate,
                                               eventDetails.StartTime, eventDetails.EndTime,
                                               eventDetails.EventTitle, eventDetails.Comments,
                                               eventDetails.Address);
                }
                SaveEventScheduleData(dtEventSchedule);
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }
        private async void SaveEventScheduleData(DataTable dtEventSchedule)
        {
            using var connection = _context.Sqlconnection();
            {
                using (SqlCommand command = new("sp_exBulkUploadEventSchedule", connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    SqlParameter tvpParam = new("@Data", SqlDbType.Structured)
                    {
                        Value = dtEventSchedule,
                        TypeName = "BulkUploadEvent"
                    };
                    command.Parameters.Add(tvpParam);
                    await command.ExecuteScalarAsync();
                }
            }
        }

        private static async Task<List<EventScheduleDTO>> ReadFile(IFormFile formFile)
        {
            List<EventScheduleDTO> records = new();
            try
            {
                var extension = Path.GetExtension(formFile.FileName).ToLower();
                string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy", "M-d-yyyy" };

                if (extension.Trim() == ".csv")
                {
                    using (var stream = new MemoryStream())
                    {
                        formFile.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string[] headers = sr.ReadLine().Split(',');
                            while (!sr.EndOfStream)
                            {
                                string[] rows = sr.ReadLine().Split(',');
                                DateTime? startDate = null;
                                DateTime? endDate = null;
                                string startDateCellValue = Convert.ToString(rows[1]); // Get the value as a string
                                string endDateCellValue = Convert.ToString(rows[2]);
                                if (!string.IsNullOrEmpty(startDateCellValue) && !string.IsNullOrEmpty(endDateCellValue))
                                {
                                    startDate = DateTime.ParseExact(startDateCellValue, formats, CultureInfo.InvariantCulture);
                                    endDate = DateTime.ParseExact(endDateCellValue, formats, CultureInfo.InvariantCulture);
                                }

                                EventScheduleDTO bulkImportDTO = new()
                                {
                                    EventID = int.TryParse(rows[0], out var eventId) ? (int?)eventId : null,
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    StartTime = rows[3],
                                    EndTime = rows[4],
                                    EventTitle = rows[5],
                                    Comments = rows[6],
                                    Address = rows[7]
                                };
                                records.Add(bulkImportDTO);
                            }
                        }
                    }
                }
                if (extension.Trim() == ".xlsx")
                {
                    using (var memStrem = new MemoryStream())
                    {
                        await formFile.CopyToAsync(memStrem);
                        using (var workbook = new XLWorkbook(memStrem))
                        {
                            var worksheet = workbook.Worksheets.Worksheet(1); // Get the first worksheet
                            var rows = worksheet.RowsUsed(); // Get rows with data
                            var rowsExcludingFirst = rows.Skip(1);

                            foreach (var row in rowsExcludingFirst)
                            {
                                DateTime? startDate = null;
                                DateTime? endDate = null;
                                string startDateCellValue = row.Cell(2).GetValue<string>(); // Get the value as a string
                                string endDateCellValue = row.Cell(3).GetValue<string>();
                                if (!string.IsNullOrEmpty(row.Cell(2).GetValue<string>()) && !string.IsNullOrEmpty(row.Cell(3).GetValue<string>()))
                                {
                                    startDate = DateTime.ParseExact(startDateCellValue, formats, CultureInfo.InvariantCulture);
                                    endDate = DateTime.ParseExact(endDateCellValue, formats, CultureInfo.InvariantCulture);
                                }

                                EventScheduleDTO bulkImportDTO = new()
                                {
                                    EventID = row.Cell(1).GetValue<int?>(),
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    StartTime = row.Cell(4).GetString(),
                                    EndTime = row.Cell(5).GetString(),
                                    EventTitle = row.Cell(6).GetString(),
                                    Comments = row.Cell(7).GetString(),
                                    Address = row.Cell(8).GetString()
                                };
                                records.Add(bulkImportDTO);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
            ValidateRecords(records);
            return records;
        }

        private static void ValidateRecords(List<EventScheduleDTO> records)
        {
            string errorMsg = string.Empty;
            int recordTrack = 1;
            if (records.Count > 0)
            {
                foreach (EventScheduleDTO scheduleTimeline in records)
                {
                    recordTrack++;
                    string EventTitle = string.Empty;
                    EventTitle = scheduleTimeline.EventTitle.Trim();
                    if (string.IsNullOrEmpty(EventTitle))
                        errorMsg += "Event title should not empty at line no. " + recordTrack + "\n";

                    if (scheduleTimeline.StartDate > scheduleTimeline.EndDate)
                        errorMsg += "Start date should not be greater than End date at line no. " + recordTrack + "\n";
                }
                int recordCount = records.Count;
                if (recordCount > 25)
                    errorMsg += "25 records are allowed to create event from bulk upload." + "\n";
            }
            else
                errorMsg += "No records found.";

            errorMsg = errorMsg.TrimEnd('\n');

            if (!string.IsNullOrEmpty(errorMsg))
                throw new BadRequestException(errorMsg);
        }

        [HttpPost]
        public int SaveEvent(CreateEventDTO createEvent)
        {
            using var connection = _context.Sqlconnection();
            {
                using (SqlCommand command = new("sp_AddEventSchedule", connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@StartDate", createEvent.StartDate);
                    command.Parameters.AddWithValue("@EndDate", createEvent.EndDate);
                    string StartTime = createEvent.StartDate.TimeOfDay.ToString();
                    string EndTime = createEvent.EndDate.TimeOfDay.ToString();
                    command.Parameters.AddWithValue("@StartTime", StartTime);
                    command.Parameters.AddWithValue("@EndTime", EndTime);
                    command.Parameters.AddWithValue("@EventName", createEvent.EventTitle);
                    command.Parameters.AddWithValue("@NOTES", createEvent.Comment);
                    command.Parameters.AddWithValue("@EventAddress", createEvent.EventAddress);
                    command.ExecuteNonQuery();
                }
            }
            return 0;
        }

        [HttpGet]
        public List<EventDetailsDTO> GetAllEventDetails()
        {
            using var connection = _context.Sqlconnection();
            using (var command = connection.CreateCommand())
            {
                List<EventDetailsDTO> result1 = new();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "sp_GetEvents";
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EventDetailsDTO eventDetails = new();
                            eventDetails.EventId = reader.GetFieldValue<int>("EventID");
                            eventDetails.EventName = reader.GetFieldValue<string>("EventName");
                            eventDetails.StartDate = reader.GetFieldValue<DateTime>("StartDate").ToString("MM/dd/yyyy");
                            eventDetails.EndDate = reader.GetFieldValue<DateTime>("EndDate").ToString("MM/dd/yyyy");
                            result1.Add(eventDetails);
                        }
                    }
                }
                catch (SqlException ex)
                {

                }
                return result1;
            }
        }

        [HttpDelete]
        public int DeleteEventByEventID(int id)
        {
            using var connection = _context.Sqlconnection();
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_DeleteEvent";
                    command.Parameters.Clear();
                    connection.Open();
                    command.Parameters.AddWithValue("@EventID", id);
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new(ex.Message);
                }
                return 0;
            }
        }

        [HttpGet("{id}")]
        public EventDetailsDTO GetEventDetailsByEventID(int id)
        {
            using var connection = _context.Sqlconnection();
            using (var command = connection.CreateCommand())
            {
                EventDetailsDTO createEvent = new();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "sp_GetEventDetailsByID";
                command.Parameters.AddWithValue("@EventID", id);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            createEvent.EventName = reader.GetFieldValue<string?>("EventName");
                            createEvent.Comments = reader.GetFieldValue<string?>("Comments");
                            createEvent.StartDate = reader.GetFieldValue<DateTime>("StartDate").ToString("MM/dd/yyyy");
                            createEvent.EndDate = reader.GetFieldValue<DateTime>("EndDate").ToString("MM/dd/yyyy");
                            createEvent.Address = reader.GetFieldValue<string?>("Address");
                        }
                    }
                }
                catch (SqlException ex)
                {
                }
                return createEvent;
            }
        }

        [HttpGet("{id}")]
        public List<EventScheduleDTO> GetScheduleDetailsByEventID(int id)
        {
            using var connection = _context.Sqlconnection();
            using (var command = connection.CreateCommand())
            {
                List<EventScheduleDTO> result1 = new();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "sp_GetEventScheduleDetailsByID";
                command.Parameters.AddWithValue("@EventID", id);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EventScheduleDTO eventSchedule = new();
                            eventSchedule.EventTitle = reader.GetFieldValue<string?>("EventName");
                            eventSchedule.ScheduleDate = reader.GetFieldValue<DateTime>("ScheduleDate").ToString("MM/dd/yyyy");
                            string startTime = reader.GetFieldValue<string>("StartTime");
                            eventSchedule.StartTime = startTime.Substring(0, startTime.Length - 3);
                            string endTime = reader.GetFieldValue<string>("EndTime");
                            eventSchedule.EndTime = endTime.Substring(0, endTime.Length - 3);
                            result1.Add(eventSchedule);
                        }
                    }
                }
                catch (SqlException ex)
                {

                }
                return result1;
            }
        }
    }
}
