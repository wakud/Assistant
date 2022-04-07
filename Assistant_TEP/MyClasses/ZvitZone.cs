using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    /// <summary>
    /// Звіт по багатозонним лічильникам
    /// </summary>
    public class ZvitZone
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string PIP { get; set; }
        public string TarifficationBlockId { get; set; }
        public string BlockLabel { get; set; }
        public string BlockLabelName { get; set; }
        public string TariffGroupId { get; set; }
        public string TimeZonalId { get; set; }
        public string isHeating { get; set; }
        public string BasePrice { get; set; }
        public string Quantity_Nich { get; set; }
        public string Quantity_PivPick { get; set; }
        public string Quantity_Pick { get; set; }
        public string Tariff_Nich { get; set; }
        public string Tariff_PivPick { get; set; }
        public string Tariff_Pick { get; set; }
    }
}
