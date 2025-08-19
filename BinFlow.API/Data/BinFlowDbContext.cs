using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

            // ===================================================================
            // UTC DateTime Conversion - Fixes PostgreSQL timezone issues
            // ===================================================================
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            // To database: Convert to UTC
                            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                            // From database: Specify as UTC
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                            // To database: Convert to UTC if not null
                            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
                            // From database: Specify as UTC if not null
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
                        ));
                    }
                }
            }

            // ===================================================================
            // Table Configuration
            // ===================================================================
            // Configure table names to match Supabase (snake_case)
            modelBuilder.Entity<ShiftReport>().ToTable("shift_reports");
            modelBuilder.Entity<BinTipping>().ToTable("bin_tippings");
            modelBuilder.Entity<HourlyEntry>().ToTable("hourly_entries");

            // ===================================================================
            // Column Configuration
            // ===================================================================
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

            // ===================================================================
            // Relationship Configuration
            // ===================================================================
            // Configure relationships
            modelBuilder.Entity<BinTipping>()
                .HasOne<ShiftReport>()
                .WithMany(s => s.BinTippings)
                .HasForeignKey(b => b.ShiftReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}