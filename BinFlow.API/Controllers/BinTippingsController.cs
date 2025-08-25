using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;
using BinFlow.Shared.Models;

namespace BinFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BinTippingsController : ControllerBase
    {
        private readonly BinFlowDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public BinTippingsController(BinFlowDbContext context)
        {
            _context = context;
            _southAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");
        }

        // POST: api/bintippings
        [HttpPost]
        public async Task<ActionResult<BinTipping>> PostBinTipping(CreateBinTippingDto createDto)
        {
            try
            {
                // Convert received date to South Africa timezone
                var receivedDate = DateTime.SpecifyKind(createDto.Date, DateTimeKind.Unspecified);
                var southAfricaDate = TimeZoneInfo.ConvertTimeFromUtc(
                    TimeZoneInfo.ConvertTimeToUtc(receivedDate, _southAfricaTimeZone),
                    _southAfricaTimeZone);

                Console.WriteLine($"Received Date: {createDto.Date}");
                Console.WriteLine($"South Africa Date: {southAfricaDate}");
                Console.WriteLine($"Date for storage: {southAfricaDate.Date}");

                // Use South Africa date for all operations
                var dateForStorage = southAfricaDate.Date;

                // First, find or create a shift report for this date
                var existingShiftReport = await _context.ShiftReports
                    .FirstOrDefaultAsync(sr => sr.Date.Date == dateForStorage &&
                                               sr.LineManager == createDto.LineManager);

                if (existingShiftReport == null)
                {
                    // Create a new shift report with South Africa date
                    existingShiftReport = new ShiftReport
                    {
                        Date = dateForStorage,
                        LineManager = createDto.LineManager,
                        Shift = createDto.Shift,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _southAfricaTimeZone),
                        UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _southAfricaTimeZone)
                    };
                    _context.ShiftReports.Add(existingShiftReport);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Created new shift report for date: {dateForStorage:yyyy-MM-dd}");
                }
                else
                {
                    Console.WriteLine($"Using existing shift report for date: {dateForStorage:yyyy-MM-dd}");
                }

                // Create the bin tipping entry
                var binTipping = new BinTipping
                {
                    ShiftReportId = existingShiftReport.Id,
                    Time = createDto.Time,
                    BinsTipped = createDto.BinsTipped,
                    AverageBinWeight = createDto.AverageBinWeight,
                    DownTime = createDto.DownTime,
                    ReasonForNotAchievingTarget = createDto.ReasonsNotes,
                    IsLunchBreak = createDto.IsLunchBreak
                };

                _context.BinTippings.Add(binTipping);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Successfully created bin tipping entry for {dateForStorage:yyyy-MM-dd} at {createDto.Time}");
                return CreatedAtAction(nameof(GetBinTipping), new { id = binTipping.Id }, binTipping);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bin tipping: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/bintippings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BinTipping>> GetBinTipping(int id)
        {
            var binTipping = await _context.BinTippings.FindAsync(id);

            if (binTipping == null)
            {
                return NotFound();
            }

            return binTipping;
        }

        // GET: api/bintippings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BinTipping>>> GetBinTippings()
        {
            return await _context.BinTippings.ToListAsync();
        }

        // GET: api/bintippings/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<BinTipping>>> GetBinTippingsByDate(DateTime date)
        {
            try
            {
                // Convert to South Africa timezone for consistent querying
                var southAfricaDate = TimeZoneInfo.ConvertTimeFromUtc(
                    TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), _southAfricaTimeZone),
                    _southAfricaTimeZone).Date;

                // Get shift reports for this date and then get their bin tippings
                var shiftReports = await _context.ShiftReports
                    .Where(sr => sr.Date.Date == southAfricaDate)
                    .ToListAsync();

                var binTippings = await _context.BinTippings
                    .Where(bt => shiftReports.Select(sr => sr.Id).Contains(bt.ShiftReportId))
                    .OrderBy(bt => bt.Time)
                    .ToListAsync();

                return Ok(binTippings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bin tippings by date: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // DTO for creating bin tippings
    public class CreateBinTippingDto
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string LineManager { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public int BinsTipped { get; set; }
        public double AverageBinWeight { get; set; }
        public int DownTime { get; set; }
        public string ReasonsNotes { get; set; } = string.Empty;
        public bool IsLunchBreak { get; set; }
    }
}
