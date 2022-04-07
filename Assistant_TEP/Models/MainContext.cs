using Microsoft.EntityFrameworkCore;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// БД і її таблиці (головний контекст)
    /// </summary>
    public class MainContext : DbContext
    {
        public DbSet<User> Users { get; set; }                          //таблиця користувачів
        public DbSet<Organization> Coks { get; set; }                   //таблиця організацій
        public DbSet<Report> Reports { get; set; }                      //таблиця звітів
        public DbSet<DbType> DbTypes { get; set; }                      //таблиця типів БД
        public DbSet<TypeReport> TypeReports { get; set; }              //таблиця типів звітів
        public DbSet<ReportParam> ReportParams { get; set; }            //таблиця параметрів
        public DbSet<ReportParamType> ReportParamTypes { get; set; }    //таблиця типів параметрів
        public DbSet<Abonents> abonents { get; set; }                   //таблиця абонентів
        public DbSet<TarifUkrPost> TarifUkrPosts { get; set; }          //таблиця тарифів укрпошти
        /// <summary>
        /// створення БД без міграції
        /// </summary>
        /// <param name="options"></param>
        public MainContext(DbContextOptions<MainContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
