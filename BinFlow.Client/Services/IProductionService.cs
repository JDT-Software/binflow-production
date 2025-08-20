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
        Task<BinTipping> CreateBinTippingAsync(CreateBinTippingDto createDto);
        Task<List<BinTipping>> GetBinTippingsAsync();
        Task<List<BinTipping>> GetBinTippingsByDateAsync(DateTime date);
        Task<List<BinTipping>> GetBinTippingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}