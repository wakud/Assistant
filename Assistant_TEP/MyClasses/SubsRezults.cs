using System;

namespace Assistant_TEP.MyClasses
{
    /// <summary>
    /// дані з БД для заповнення файлу дбф УПСЗН субсидії
    /// </summary>
    public class SubsRezults
    {
        public decimal TARYF_6 { get; set; }
        public int NORM_F6 { get; set; }
        public decimal NM_PAY { get; set; }
        public decimal DEBT { get; set; }
    }
}
