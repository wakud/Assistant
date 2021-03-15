using System;


namespace Assistant_TEP.MyClasses
{
    public class Pilga2
    {
        public long CDPR { get; set; }
        public string IDCODE { get; set; }
        public string FIO { get; set; }
        public string PPOS { get; set; }
        public string RS { get; set; }
        public int YEARIN { get; set; }
        public int MONTHIN { get; set; }
        public int LGCODE { get; set; }
        public DateTime DATA1 { get; set; }
        public DateTime DATA2 { get; set; }
        public int LGKOL { get; set; }
        public int LGKAT { get; set; }
        public string LGNAME { get; set; }
        public int LGPRC { get; set; }
        public decimal SUMM { get; set; }
        public decimal FACT { get; set; }
        public decimal TARIF { get; set; }
        public int FLAG { get; set; }
        public int isBlock { get; set; }
        public int idNasPunkt { get; set; }
        public string NasPunkt { get; set; }
        public string VulName { get; set; }
        public string Bild { get; set; }
        public string Korp { get; set; }
        public string Apartment { get; set; }
        public decimal woz { get; set; }
        public decimal z1 { get; set; }
        public decimal z2 { get; set; }
        public decimal z3 { get; set; }
        public decimal z4 { get; set; }
        public decimal wozKwt { get; set; }
        public decimal z1Kwt { get; set; }
        public decimal z2Kwt { get; set; }
        public decimal z3Kwt { get; set; }
        public decimal z4Kwt { get; set; }
    }

    public class CategoryTotals
    {
        public int Count { get; set; }
        public decimal WoZoneCount { get; set; }
        public decimal FirstZoneCount { get; set; }
        public decimal SecondZoneCount { get; set; }
        public decimal ThirdZoneCount { get; set; }
        public decimal Lights { get; set; }
        public decimal TotalCharged { get; set; }
        public string Code { get; set; }

        public CategoryTotals()
        {
            Count = 0;
            WoZoneCount = 0.0m;
            FirstZoneCount = 0.0m;
            SecondZoneCount = 0.0m;
            ThirdZoneCount = 0.0m;
            Lights = 0.0m;
            TotalCharged = 0.0m;
            Code = "9999999999999999";
        }


    }
}
