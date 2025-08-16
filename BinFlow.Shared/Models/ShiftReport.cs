using System.ComponentModel.DataAnnotations;

namespace BinFlow.Shared.Models
{
    // Main shift report document
    public class ShiftReport
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string LineManager { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public List<BinTipping> BinTippings { get; set; } = new();
        public int TotalTipped { get; set; }
        public double AverageWeight { get; set; }
        public int TotalDowntime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Individual bin tipping entry (hourly entry)
    public class BinTipping
    {
        public int Id { get; set; }
        public int ShiftReportId { get; set; }
        public TimeSpan Time { get; set; }
        public int BinsTipped { get; set; }
        public double AverageBinWeight { get; set; }
        public int DownTime { get; set; }
        public string ReasonForNotAchievingTarget { get; set; } = string.Empty;
        public bool IsLunchBreak { get; set; }
    }

    // Hourly entry model for the new system
    public class HourlyEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string LineManager { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public int BinsTipped { get; set; }
        public double AverageBinWeight { get; set; }
        public int DownTime { get; set; }
        public string ReasonsNotes { get; set; } = string.Empty;
        public bool IsLunchBreak { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // For dashboard filtering and summary views
    public class ShiftSummary
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string LineManager { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public int TotalTipped { get; set; }
        public double AverageWeight { get; set; }
        public int TotalDowntime { get; set; }
        public double Efficiency { get; set; }
    }

    // For creating new entries
    public class CreateShiftReportDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string LineManager { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Shift { get; set; } = string.Empty;
    }

    // For dashboard charts
    public class ProductionMetrics
    {
        public DateTime Date { get; set; }
        public int TotalBinsTipped { get; set; }
        public double AverageWeight { get; set; }
        public int TotalDowntime { get; set; }
        public double EfficiencyPercentage { get; set; }
        public string LineManager { get; set; } = string.Empty;
    }

    // Enum for common downtime reasons
    public enum DowntimeReason
    {
        None,
        RotationFromJumbleFillers,
        PucVarietyExchange,
        PucExchange,
        Lunch,
        WaitingForPackingInstruction,
        CleaningForNextShift,
        MachineBreakdown,
        MaterialShortage,
        QualityIssue,
        Other
    }

    // API Response DTOs
    public class DashboardStats
    {
        public int TotalShiftsToday { get; set; }
        public int TotalBinsTippedToday { get; set; }
        public double AverageEfficiencyToday { get; set; }
        public int TotalDowntimeToday { get; set; }
        public List<ProductionMetrics> RecentMetrics { get; set; } = new();
    }
}