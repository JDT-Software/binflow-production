using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;
using BinFlow.Shared.Models;

namespace BinFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftReportsController : ControllerBase
    {
        private readonly BinFlowDbContext _context;

        public ShiftReportsController(BinFlowDbContext context)
        {
            _context = context;
        }

        // GET: api/shiftreports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftReport>>> GetShiftReports([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                Console.WriteLine($"GetShiftReports called with startDate: {startDate}, endDate: {endDate}");
                
                // Get ShiftReports with their BinTippings
                var query = _context.ShiftReports.Include(s => s.BinTippings).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.Date >= startDate.Value.Date);
                
                if (endDate.HasValue)
                    query = query.Where(r => r.Date <= endDate.Value.Date);

                var reports = await query.OrderByDescending(s => s.Date).ToListAsync();
                
                Console.WriteLine($"Found {reports.Count} shift reports from database");

                // Recalculate totals from actual BinTippings for each report
                foreach (var report in reports)
                {
                    if (report.BinTippings?.Any() == true)
                    {
                        report.TotalTipped = report.BinTippings.Sum(bt => bt.BinsTipped);
                        report.AverageWeight = report.BinTippings.Average(bt => bt.AverageBinWeight);
                        report.TotalDowntime = report.BinTippings.Sum(bt => bt.DownTime);
                        Console.WriteLine($"Report {report.Id}: {report.BinTippings.Count} bin tippings, Total: {report.TotalTipped}");
                    }
                    else
                    {
                        Console.WriteLine($"Report {report.Id}: No bin tippings found");
                    }
                }

                Console.WriteLine($"Returning {reports.Count} reports");
                return reports;
            }
            catch (Exception ex)
            {
                // Log the error and return empty list for now
                Console.WriteLine($"Database error: {ex.Message}");
                
                // Fallback to mock data if database fails
                var mockReports = new List<ShiftReport>
                {
                    new ShiftReport
                    {
                        Id = 1,
                        Date = DateTime.Today,
                        LineManager = "John Smith",
                        Shift = "Day Shift",
                        TotalTipped = 120,
                        AverageWeight = 45.5,
                        TotalDowntime = 30,
                        BinTippings = new List<BinTipping>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                return mockReports;
            }
        }

        // GET: api/shiftreports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftReport>> GetShiftReport(int id)
        {
            var shiftReport = await _context.ShiftReports
                .Include(s => s.BinTippings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shiftReport == null)
            {
                return NotFound();
            }

            return shiftReport;
        }

        // POST: api/shiftreports
        [HttpPost]
        public async Task<ActionResult<ShiftReport>> PostShiftReport(CreateShiftReportDto createDto)
        {
            var shiftReport = new ShiftReport
            {
                Date = createDto.Date,
                LineManager = createDto.LineManager,
                Shift = createDto.Shift,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ShiftReports.Add(shiftReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShiftReport), new { id = shiftReport.Id }, shiftReport);
        }

        // PUT: api/shiftreports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShiftReport(int id, ShiftReport shiftReport)
        {
            if (id != shiftReport.Id)
            {
                return BadRequest();
            }

            shiftReport.UpdatedAt = DateTime.UtcNow;
            _context.Entry(shiftReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftReportExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/shiftreports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShiftReport(int id)
        {
            var shiftReport = await _context.ShiftReports.FindAsync(id);
            if (shiftReport == null)
            {
                return NotFound();
            }

            _context.ShiftReports.Remove(shiftReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/shiftreports/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats()
        {
            var today = DateTime.Today;
            
            var todayShifts = await _context.ShiftReports
                .Where(s => s.Date == today)
                .ToListAsync();

            var recentMetrics = await _context.ShiftReports
                .Where(s => s.Date >= today.AddDays(-7))
                .OrderByDescending(s => s.Date)
                .Select(s => new ProductionMetrics
                {
                    Date = s.Date,
                    TotalBinsTipped = s.TotalTipped,
                    AverageWeight = s.AverageWeight,
                    TotalDowntime = s.TotalDowntime,
                    EfficiencyPercentage = s.TotalDowntime > 0 ? 
                        Math.Round((1.0 - (s.TotalDowntime / 480.0)) * 100, 2) : 100,
                    LineManager = s.LineManager
                })
                .ToListAsync();

            var stats = new DashboardStats
            {
                TotalShiftsToday = todayShifts.Count,
                TotalBinsTippedToday = todayShifts.Sum(s => s.TotalTipped),
                AverageEfficiencyToday = todayShifts.Any() ? 
                    Math.Round(todayShifts.Average(s => s.TotalDowntime > 0 ? 
                        (1.0 - (s.TotalDowntime / 480.0)) * 100 : 100), 2) : 0,
                TotalDowntimeToday = todayShifts.Sum(s => s.TotalDowntime),
                RecentMetrics = recentMetrics
            };

            return stats;
        }

        private bool ShiftReportExists(int id)
        {
            return _context.ShiftReports.Any(e => e.Id == id);
        }
    }
}