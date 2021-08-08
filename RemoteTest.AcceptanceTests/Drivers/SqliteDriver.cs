using System.Linq;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace RemoteTest.AcceptanceTests.Drivers
{
    public class SqliteDriver : IDbDriver
    {
        private readonly SqliteConnection _connection;

        public SqliteDriver(SqliteConnection connection)
        {
            _connection = connection;
        }

        public void ClearTable(string table)
        {
            using var cmd = _connection.CreateCommand();
            // Use delete as TRUNCATE cant handle foreign keys. 
            
            // SAFETY: Table names are not user controlled. This is test support code.
            cmd.CommandText = $"DELETE FROM \"{table}\"";
            cmd.ExecuteNonQuery();
        }

        public long RowCount(string tableName)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
            var result = cmd.ExecuteScalar();
            return (long) result;
        }

        public void CheckContents(string tableName, Table table)
        {
            using var cmd = _connection.CreateCommand();

            var headers = table.Header.ToList();
            
            var names = string.Join(", ", table.Header.Select(n => $"\"{n}\""));
            cmd.CommandText = $"SELECT {names} from \"{tableName}\"";
            var reader = cmd.ExecuteReader();

            int rowCount = 0;
            for (; reader.Read(); rowCount++)
            {
                Assert.That(rowCount, Is.LessThan(table.RowCount), "Too many rows from query.");
                var row = table.Rows[rowCount];
                for (int i = 0; i < table.Header.Count; i++)
                {
                    var dbval = reader.GetValue(i);
                    var testval = row[i];
                    Assert.That(
                        dbval.ToString(), 
                        Is.EqualTo(testval),
                        "Column {0} does not match.", 
                        headers[i]);
                }
            }
            Assert.That(rowCount, Is.EqualTo(table.RowCount), "Less rows than expected.");
        }
    }

    public interface IDbDriver
    {
        void ClearTable(string tableName);
        long RowCount(string tableName);
        void CheckContents(string tableName, Table table);
    }
}