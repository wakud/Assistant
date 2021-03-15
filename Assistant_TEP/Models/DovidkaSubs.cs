using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class DovidkaSubs
    {
        public int? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public long? AccountNumberNew { get; set; }
        public string? PIP { get; set; }
        public string? FullAddress { get; set; }
        public string? Pip_Pilg { get; set; }
        public string? Pilg_category { get; set; }
        public int? BeneficiaryQuantity { get; set; }
        public int? TariffGroupId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? Price { get; set; }
        public string? ShortName { get; set; }
        public string? TariffGroupName { get; set; }
        public int? MaxTariffLimit { get; set; }
        public byte? Id { get; set; }
        public int? TimeZone { get; set; }
        public int? IsHeating { get; set; }
        public int? Discount { get; set; }
        public decimal? DiscountKoeff { get; set; }
        public decimal? PricePDV { get; set; }
        public string? GVP { get; set; }
        public string? CPGV { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public int? IncrementValue { get; set; }
        public string? Borg { get; set; }
        public int? RegisteredQuantity { get; set; }
        public decimal? QuantityTo { get; set; }
        public int? SanNormaSubsKwt { get; set; }
        public decimal? SanNormaSubsGrn { get; set; }
        public decimal? QuantityToGrn { get; set; }
        public decimal? nm_pay { get; set; }
        public string? Cok { get; set; } 
        public string? Vykonavets { get; set; }
        public string? Nach { get; set; }
    }
}
