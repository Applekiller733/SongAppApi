namespace SongAppApi.Helpers
{
    using Microsoft.EntityFrameworkCore;
    using SongAppApi.Entities;
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<File> Files { get; set; }

        private readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Configuration.GetConnectionString("SongAppApiDatabase");
            optionsBuilder.UseMySql(connectionString,
                ServerVersion.AutoDetect(connectionString));
        }
    }
}
