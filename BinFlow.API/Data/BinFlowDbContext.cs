using Microsoft.EntityFrameworkCore;
using BinFlow.Shared.Models;

namespace BinFlow.API.Data
{
    public class BinFlowDbContext : DbContext
    {
        public BinFlowDbContext(DbContextOptions<BinFlowDbContext> options) : base(options)
        {
        }

        public DbSet<ShiftReport> ShiftReports { get; set; }
        public DbSet<BinTipping> BinTippings { get; set; }
        public DbSet<HourlyEntry> HourlyEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match Supabase (snake_case)
            modelBuilder.Entity<ShiftReport>().ToTable("shift_reports");
            modelBuilder.Entity<BinTipping>().ToTable("bin_tippings");
            modelBuilder.Entity<HourlyEntry>().ToTable("hourly_entries");

            // Configure column names to match database schema
            modelBuilder.Entity<ShiftReport>(entity =>
            {
                entity.Property(e => e.LineManager).HasColumnName("line_manager");
                entity.Property(e => e.TotalTipped).HasColumnName("total_tipped");
                entity.Property(e => e.AverageWeight).HasColumnName("average_weight");
                entity.Property(e => e.TotalDowntime).HasColumnName("total_downtime");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<BinTipping>(entity =>
            {
                entity.Property(e => e.ShiftReportId).HasColumnName("shift_report_id");
                entity.Property(e => e.BinsTipped).HasColumnName("bins_tipped");
                entity.Property(e => e.AverageBinWeight).HasColumnName("average_bin_weight");
                entity.Property(e => e.DownTime).HasColumnName("down_time");
                entity.Property(e => e.ReasonForNotAchievingTarget).HasColumnName("reason_for_not_achieving_target");
                entity.Property(e => e.IsLunchBreak).HasColumnName("is_lunch_break");
            });

            modelBuilder.Entity<HourlyEntry>(entity =>
            {
                entity.Property(e => e.LineManager).HasColumnName("line_manager");
                entity.Property(e => e.ProductionLine).HasColumnName("production_line");
                entity.Property(e => e.BinsTipped).HasColumnName("bins_tipped");
                entity.Property(e => e.AverageBinWeight).HasColumnName("average_bin_weight");
                entity.Property(e => e.DownTime).HasColumnName("down_time");
                entity.Property(e => e.ReasonsNotes).HasColumnName("reasons_notes");
                entity.Property(e => e.IsLunchBreak).HasColumnName("is_lunch_break");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // Configure relationships
            modelBuilder.Entity<BinTipping>()
                .HasOne<ShiftReport>()
                .WithMany(s => s.BinTippings)
                .HasForeignKey(b => b.ShiftReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}