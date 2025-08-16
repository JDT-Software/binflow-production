using BinFlow.Shared.Models;

namespace BinFlow.Client.Services
{
    public interface IProductionService
    {
        Task<List<ShiftReport>> GetShiftReportsAsync();
        Task<List<ShiftReport>> GetAllShiftReportsAsync(); // Alias for GetShiftReportsAsync
        Task<List<ShiftReport>> GetShiftReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ShiftReport?> GetShiftReportAsync(int id);
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<ShiftReport> CreateShiftReportAsync(CreateShiftReportDto createDto);
        Task UpdateShiftReportAsync(int id, ShiftReport shiftReport);
        Task DeleteShiftReportAsync(int id);
        Task AddBinTippingEntryAsync(int shiftReportId, BinTipping binTipping);
    }
}