using Assistant_TEP.Models;
using System.Collections.Generic;

namespace Assistant_TEP.ViewModels
{
    public class Forma103
    {
        public IEnumerable<Abonents> People { get; set; }   //список абонентів для укрпошти

        public string? OrganizationName { get; set; }       // назва Цоку

        public string? OrgAdres { get; set; }               //адреса Цоку

        public string? OrgIndex { get; set; }               //індекс Цоку

        public decimal? Suma { get; set; }

        public decimal? PDV { get; set; }

        public string? SumaStr { get; set; }

        public int? Kt { get; set; }

        public string Nach { get; set; }

        public string Buh { get; set; }

        public string PostalIndex { get; set; }

        public string Postal { get; set; }

        public string? Pusto { get; set; }
    }
}
