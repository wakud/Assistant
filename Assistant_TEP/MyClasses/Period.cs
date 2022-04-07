using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Формування періоду в числове, стрічкове, дата формати
    /// </summary>
    public class Period
    {
        public int per_int { get; set; }    //період в числовому
        public string per_str { get; set; } //період  в символьному
        public DateTime per_date { get; set; }  //період як дата

        //період у форматі числовому (012021)
        public Period(int per_int)
        {
            this.per_int = per_int;
            this.per_str = per_int.ToString();
            int rik = Int32.Parse(per_int.ToString().Trim().Substring(0, 4));
            int mis = Int32.Parse(per_int.ToString().Trim().Substring(4, 2));
            this.per_date = new DateTime(rik, mis, 1);
        }
        //добавлення нуля в до місяців менше 10 (01, 02, 03 ... , 09)
        public string true_month(int mis)
        {
            if (mis < 10)
                return "0" + mis.ToString();
            return mis.ToString();
        }
        //період у форматі дата
        public Period(DateTime per_date)
        {
            this.per_date = per_date;
            int rik = per_date.Year;
            int mis = per_date.Month;
            this.per_str = rik.ToString() + this.true_month(mis);
            this.per_int = int.Parse(this.per_str);
        }
        //період у форматі стрічки
        public Period(string per_str)
        {
            this.per_int = Int32.Parse(per_str);
            this.per_str = per_str;
            int rik = Int32.Parse(per_int.ToString().Trim().Substring(0, 4));
            int mis = Int32.Parse(per_int.ToString().Trim().Substring(4, 2));
            this.per_date = new DateTime(rik, mis, 1);
        }
        //отримання останнього дня попереднього місяця
        public DateTime GetLastDayPrevMonth(int month, int year)
        {
            return new DateTime(year, month, 1).AddDays(-1);
        }
        //визначення періоду зараз
        public static Period per_now()
        {
            DateTime from_date = DateTime.Now;
            return new Period(from_date);
        }
    }
}
