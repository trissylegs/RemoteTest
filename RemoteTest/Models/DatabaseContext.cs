using Microsoft.EntityFrameworkCore;

namespace RemoteTest.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>();
            modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountId);

            // As SQL is case-insensitive we snake-case the table name. Pretty sure EF can automate this but this works.
            modelBuilder.Entity<MeterReading>();
            
            // Foreign keys
            
            modelBuilder.Entity<MeterReading>()
                .HasKey(mr => new {mr.AccountId, mr.MeterReadingDateTime});
            
            modelBuilder.Entity<Account>()
                .HasMany(a => a.MeterReadings)
                .WithOne(mr => mr.Account)
                .HasForeignKey(mr => mr.AccountId);
        }

    }
}