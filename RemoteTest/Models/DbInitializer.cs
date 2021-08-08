using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace RemoteTest.Models
{
    public class DbInitializer
    {
        private readonly ILogger _logger;
        private readonly DatabaseContext _databaseContext;

        public DbInitializer(ILogger<DbInitializer> logger, DatabaseContext databaseContext)
        {
            this._logger = logger;
            this._databaseContext = databaseContext;
        }

        public void InitializeWithFile(string? accountsFile)
        {
            if (_databaseContext.Accounts.Any())
            {
                _logger.LogInformation("Database already seeded.");
                return;
            }
            
            _logger.LogInformation("Initialising database database from {accountsFile}", accountsFile);
            if (accountsFile != null) {
                using var file = File.OpenText(accountsFile);
                using var csvReader = new CsvReader(file, CultureInfo.CurrentCulture);

                var records = csvReader.GetRecords<Account>();
                foreach (var account in records)
                {
                    _databaseContext.Accounts.Add(account);
                    _logger.LogInformation("Inserted Account {AccountId},{FirstName},{LastName}", 
                        account.AccountId, account.FirstName, account.LastName);
                }

                _databaseContext.SaveChanges();
            }
        }
    }
}