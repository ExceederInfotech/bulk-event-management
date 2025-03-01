using ClosedXML.Excel;
using EventMgmt.Exceptions;
using EventMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace EventMgmt.Controllers
{
    [Route("api/[controller]")]
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
            using var connection = _context.sqlconnection();
            try
            {
                if (formFile == null || formFile.Length == 0)
                    throw new BadRequestException("No file uploaded.");

                var records = ReadExcelFile(formFile);
                DataTable dtEventSchedule = new();
                dtEventSchedule.Columns.Add("EventID", typeof(int));
                dtEventSchedule.Columns.Add("StartDate", typeof(DateTime));
                dtEventSchedule.Columns.Add("EndDate", typeof(DateTime));
                dtEventSchedule.Columns.Add("StartTime", typeof(string));
                dtEventSchedule.Columns.Add("EndTime", typeof(string));
                dtEventSchedule.Columns.Add("EventTitle", typeof(string));
                dtEventSchedule.Columns.Add("Note", typeof(string));
                dtEventSchedule.Columns.Add("Address", typeof(string));

                var result = await records;
                foreach (var taskDetails in result)
                {
                    dtEventSchedule.Rows.Add(taskDetails.EventID,
                                               taskDetails.StartDate, taskDetails.EndDate,
                                               taskDetails.StartTime, taskDetails.EndTime,
                                               taskDetails.EventTitle, taskDetails.Notes,
                                               taskDetails.Address);
                }
                SaveProjectScheduleData(dtEventSchedule);
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }
        private async void SaveProjectScheduleData(DataTable dtEventSchedule)
        {
            using var connection = _context.sqlconnection();
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

        private static async Task<List<BulkImportToEventScheduleDTO>> ReadExcelFile(IFormFile formFile)
        {
            List<BulkImportToEventScheduleDTO> records = new();
            try
            {
                // check file type
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

                                BulkImportToEventScheduleDTO bulkImportDTO = new()
                                {
                                    EventID = int.TryParse(rows[0], out var taskId) ? (int?)taskId : null,
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    StartTime = rows[3],
                                    EndTime = rows[4],
                                    EventTitle = rows[5],
                                    Notes = rows[6],
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

                                BulkImportToEventScheduleDTO bulkImportDTO = new()
                                {
                                    EventID = row.Cell(1).GetValue<int?>(),
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    StartTime = row.Cell(4).GetString(),
                                    EndTime = row.Cell(5).GetString(),
                                    EventTitle = row.Cell(6).GetString(),
                                    Notes = row.Cell(7).GetString(),
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

        private static void ValidateRecords(List<BulkImportToEventScheduleDTO> records)
        {
            string errorMsg = string.Empty;
            int recordTrack = 1;
            if (records.Count > 0)
            {
                foreach (BulkImportToEventScheduleDTO scheduleTimeline in records)
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
                if (recordCount > 100)
                    errorMsg += "100 records are allowed to create event from bulk upload." + "\n";
            }
            else
                errorMsg += "No records found.";

            errorMsg = errorMsg.TrimEnd('\n');

            if (!string.IsNullOrEmpty(errorMsg))
                throw new BadRequestException(errorMsg);
        }
    }
}
