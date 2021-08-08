using System;

namespace RemoteTest.Models
{
    public class MeterReading
    {
        public int AccountId { get; set; }
        public DateTime MeterReadingDateTime { get; set; }
        public int MeterReadValue { get; set; }
        
        public Account? Account { get; set; }
    }
}