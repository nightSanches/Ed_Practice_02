using Microsoft.EntityFrameworkCore;

namespace API.Classes
{
    public class DatabaseConnection : DbContext
    {

        // public DbSet<Модель> наименование {get; set;} на каждую модель
        // пример:
        // public DbSet<Document> documents { get; set; }

        public DatabaseConnection()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=127.0.0.1;port=3316;uid=root;pwd=;database=bd02", new MySqlServerVersion(new Version(8, 0, 11)));
        }
    }
}
