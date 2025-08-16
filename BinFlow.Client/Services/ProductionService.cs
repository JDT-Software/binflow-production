using System.Net.Http.Json;
using BinFlow.Shared.Models;
using System.Timers;

namespace BinFlow.Client.Services
{
    public class ProductionService : IProductionService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly System.Timers.Timer _pollTimer;
        public event Action? OnDataUpdated;

        public ProductionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _pollTimer = new System.Timers.Timer();
            _pollTimer.Elapsed += OnTimerElapsed;
            StartSmartPolling();
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                // Refresh data automatically
                OnDataUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during auto-refresh: {ex.Message}");
            }
        }

        private void StartSmartPolling()
        {
            var now = DateTime.Now.TimeOfDay;
            var isWorkHours = now >= TimeSpan.FromHours(8) && now <= TimeSpan.FromHours(18);
            
            if (isWorkHours)
            {
                // Poll every 3 minutes during work hours
                _pollTimer.Interval = TimeSpan.FromMinutes(3).TotalMilliseconds;
                _pollTimer.Start();
                Console.WriteLine("Started polling - work hours mode (3 minutes)");
            }
            else
            {
                // Poll every hour outside work hours to check if work hours started
                _pollTimer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
                _pollTimer.Start();
                Console.WriteLine("Started polling - off hours mode (1 hour)");
            }
        }

        public async Task<List<ShiftReport>> GetShiftReportsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ShiftReport>>("api/ShiftReports");
                return response ?? new List<ShiftReport>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching shift reports: {ex.Message}");
                return new List<ShiftReport>();
            }
        }

        public async Task<List<ShiftReport>> GetAllShiftReportsAsync()
        {
            return await GetShiftReportsAsync();
        }

        public async Task<List<ShiftReport>> GetShiftReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var allReports = await GetShiftReportsAsync();
                return allReports.Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching shift reports by date range: {ex.Message}");
                return new List<ShiftReport>();
            }
        }

        public async Task<ShiftReport?> GetShiftReportAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ShiftReport>($"api/ShiftReports/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching shift report {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DashboardStats>("api/ShiftReports/dashboard");
                return response ?? new DashboardStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard stats: {ex.Message}");
                return new DashboardStats();
            }
        }

        public async Task<ShiftReport> CreateShiftReportAsync(CreateShiftReportDto createDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ShiftReports", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ShiftReport>() ?? new ShiftReport();
        }

        public async Task UpdateShiftReportAsync(int id, ShiftReport shiftReport)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/ShiftReports/{id}", shiftReport);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteShiftReportAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ShiftReports/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task AddBinTippingEntryAsync(int shiftReportId, BinTipping binTipping)
        {
            try
            {
                var shiftReport = await GetShiftReportAsync(shiftReportId);
                if (shiftReport != null)
                {
                    binTipping.ShiftReportId = shiftReportId;
                    shiftReport.BinTippings.Add(binTipping);
                    await UpdateShiftReportAsync(shiftReportId, shiftReport);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding bin tipping entry: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _pollTimer?.Dispose();
        }
    }
}