using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class Period
    {
        public int per_int { get; set; }
        public string per_str { get; set; }
        public DateTime per_date { get; set; }


        public Period(int per_int)
        {
            this.per_int = per_int;
            this.per_str = per_int.ToString();
            int rik = Int32.Parse(per_int.ToString().Trim().Substring(0, 4));
            int mis = Int32.Parse(per_int.ToString().Trim().Substring(4, 2));
            this.per_date = new DateTime(rik, mis, 1);

        }

        public string true_month(int mis)
        {
            if (mis < 10)
                return "0" + mis.ToString();
            return mis.ToString();
        }

        public Period(DateTime per_date)
        {
            this.per_date = per_date;
            int rik = per_date.Year;
            int mis = per_date.Month;
            this.per_str = rik.ToString() + this.true_month(mis);
            this.per_int = int.Parse(this.per_str);
        }

        public Period(string per_str)
        {
            this.per_int = Int32.Parse(per_str);
            this.per_str = per_str;
            int rik = Int32.Parse(per_int.ToString().Trim().Substring(0, 4));
            int mis = Int32.Parse(per_int.ToString().Trim().Substring(4, 2));
            this.per_date = new DateTime(rik, mis, 1);
        }

        public DateTime GetLastDayPrevMonth(int month, int year)
        {
            return new DateTime(year, month, 1).AddDays(-1);
        }

        public static Period per_now()
        {
            DateTime from_date = DateTime.Now;
            return new Period(from_date);
        }
    }
}
