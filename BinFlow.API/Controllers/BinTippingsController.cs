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

        public BinTippingsController(BinFlowDbContext context)
        {
            _context = context;
        }

        // POST: api/bintippings
        [HttpPost]
        public async Task<ActionResult<BinTipping>> PostBinTipping(CreateBinTippingDto createDto)
        {
            try
            {
                // First, find or create a shift report for this date
                var existingShiftReport = await _context.ShiftReports
                    .FirstOrDefaultAsync(sr => sr.Date.Date == createDto.Date.Date && sr.LineManager == createDto.LineManager);

                if (existingShiftReport == null)
                {
                    // Create a new shift report
                    existingShiftReport = new ShiftReport
                    {
                        Date = createDto.Date.Date,
                        LineManager = createDto.LineManager,
                        Shift = createDto.Shift,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ShiftReports.Add(existingShiftReport);
                    await _context.SaveChangesAsync();
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

                return CreatedAtAction(nameof(GetBinTipping), new { id = binTipping.Id }, binTipping);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bin tipping: {ex.Message}");
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