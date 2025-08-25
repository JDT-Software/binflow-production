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
            _southAfricaTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                "SAST",
                TimeSpan.FromHours(2),
                "South Africa Standard Time",
                "SAST"
            );
        }

        [HttpPost]
        public async Task<ActionResult<BinTipping>> PostBinTipping(CreateBinTippingDto createDto)
        {
            try
            {
                var receivedDate = DateTime.SpecifyKind(createDto.Date, DateTimeKind.Unspecified);
                var southAfricaDate = TimeZoneInfo.ConvertTimeFromUtc(
                    TimeZoneInfo.ConvertTimeToUtc(receivedDate, _southAfricaTimeZone),
                    _southAfricaTimeZone);

                var dateForStorage = southAfricaDate.Date;

                var existingShiftReport = await _context.ShiftReports
                    .FirstOrDefaultAsync(sr => sr.Date.Date == dateForStorage &&
                                               sr.LineManager == createDto.LineManager);

                if (existingShiftReport == null)
                {
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
                }

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

                return CreatedAtAction(nameof(GetBinTipping), new { id = binTipping.Id }, binTipping);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BinTipping>>> GetBinTippings()
        {
            return await _context.BinTippings.ToListAsync();
        }
    }

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
