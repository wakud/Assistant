using Microsoft.EntityFrameworkCore;

namespace Assistant_TEP.Models
{
    public class MainContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Coks { get; set; }

        public DbSet<Report> Reports { get; set; }
        public DbSet<DbType> DbTypes { get; set; }
        public DbSet<TypeReport> TypeReports { get; set; }
        public DbSet<ReportParam> ReportParams { get; set; }
        public DbSet<ReportParamType> ReportParamTypes { get; set; }

        public MainContext(DbContextOptions<MainContext> options)
            : base(options)
        {
            //Database.EnsureCreated();     //створення БД без міграції
        }
    }
}
