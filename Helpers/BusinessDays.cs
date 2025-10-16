using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public class BusinessDays
    {
        public static DateTime AddBusinessDays(DateTime start, int days)
        {
            var d = start;
            var remaining = days;
            var dir = Math.Sign(remaining);
            while (remaining != 0)
            {
                d = d.AddDays(dir);
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    remaining -= dir;
            }
            return d;
        }
    }
}
