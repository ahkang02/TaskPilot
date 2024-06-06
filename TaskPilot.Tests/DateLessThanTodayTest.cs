using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Utility.CustomValidator;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class DateLessThanTodayTest
    {
        [TestCase("12/06/2024", ExpectedResult = true)]
        [TestCase("05/06/2024", ExpectedResult = false)]
        [TestCase("06/06/2024", ExpectedResult = true)]
        public bool DateValidator_InputExpectedDateRange_DateValidity(DateTime date)
        {
            DateLessThanToday dateLessThanToday = new DateLessThanToday();
            return dateLessThanToday.IsValid(date.Date);
        }
    }
}
