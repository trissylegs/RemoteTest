using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using RemoteTest.Models;

namespace RemoteTest.Controllers
{
    [ApiController]
    [Route("")]
    public class MeterReadingController : ControllerBase
    {
        private DatabaseContext _databaseContext;
        private readonly ILogger<MeterReadingController> _logger;

        public MeterReadingController(DatabaseContext databaseContext, ILogger<MeterReadingController> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        [Route("meter-reading-uploads")]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Consumes("text/csv")]
        public async Task<IActionResult> MeterReadingUploads()
        {
            Encoding encoding = Encoding.Default;
            if (MediaTypeHeaderValue.TryParse(Request.ContentType, out var contentType))
            {
                if (contentType.Encoding != null)
                    encoding = contentType.Encoding;
            }

            using var sr = new StreamReader(Request.Body, encoding);
            var headers = await sr.ReadLineAsync();
            if (headers == null)
                BadRequest("No data.");
            var split = headers.Split(',');
            if (split.Length < 3)
                BadRequest("Missing headers.");

            var accountIdIndex = Array.IndexOf(split, "AccountId");
            if (accountIdIndex < 0)
                BadRequest("Missing AccountId header");
            var meterReadingDateTimeIndex = Array.IndexOf(split, "MeterReadingDateTime");
            if (meterReadingDateTimeIndex < 0)
                BadRequest("Missing MeterReadingDateTime header");
            var meterReadValueIndex = Array.IndexOf(split, "MeterReadValue");
            if (meterReadValueIndex < 0)
                BadRequest("Missing MeterReadValue header");

            var maxIndex = Max(accountIdIndex, meterReadingDateTimeIndex, meterReadValueIndex);

            var valueRegex = new Regex(@"^\d{5}$");

            int successful = 0;
            int failed = 0;
            var cultureInfo = CultureInfo.GetCultureInfo("en-GB");

            while (true)
            {
                var line = await sr.ReadLineAsync();
                if (line == null)
                    break;

                var fields = line.Split(',');
                Action<string> badRow = (string reason) =>
                {
                    _logger.LogDebug("Bad row {reason}: {line}", reason, line);
                    failed++;
                };

                if (fields.Length <= maxIndex)
                {
                    badRow("Not enough fields");
                    continue;
                }

                if (!int.TryParse(fields[accountIdIndex], out var accountId))
                {
                    badRow("AccountId");
                    continue;
                }

                if (!DateTime.TryParse(
                    fields[meterReadingDateTimeIndex],
                    cultureInfo, DateTimeStyles.AllowWhiteSpaces,
                    out var meterReadingDateTime))
                {
                    badRow("MeterReadingDateTime");
                    continue;
                }

                if (!valueRegex.IsMatch(fields[meterReadValueIndex]))
                {
                    badRow("MeterReadValue");
                    continue;
                }

                // Guaranteed valid due to regex.
                var meterReadValue = int.Parse(fields[meterReadValueIndex]);

                try
                {
                    var inserted = await _databaseContext.Database
                        .ExecuteSqlRawAsync(
                            @"INSERT INTO MeterReadings(AccountId, MeterReadingDateTime, MeterReadValue) VALUES ({0}, {1}, {2})",
                            accountId, meterReadingDateTime, meterReadValue);
                    if (inserted < 1)
                        badRow($"SQL returned {inserted}");
                    else
                    {
                        successful++;
                        _logger.LogDebug("Inserted row {AccountId},{MeterReadingDateTime},{MeterReadValue}", accountId,
                            meterReadingDateTime, meterReadValue);
                    }
                }
                catch (DbException exception)
                {
                    // Constraint failure is expected. Others. Not so much.
                    if (!exception.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase))
                        throw;
                    _logger.LogDebug(exception, "exception on insert");
                    badRow("DbException");
                }
            }

            return Ok(new
            {
                successful, failed,
            });
        }

        private int Max(int first, params int[] values)
        {
            int result = first;
            foreach (var i in values)
            {
                if (i > result)
                    result = i;
            }

            return result;
        }
    }
}