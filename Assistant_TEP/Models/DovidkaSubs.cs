using System;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Довідка для відділу субсидій УПСЗН
    /// </summary>
    public class DovidkaSubs
    {
        public int? AccountId { get; set; }             //ключ
        public long? AccountNumber { get; set; }        //особовий рахунок
        public long? AccountNumberNew { get; set; }     //новий особовий
        public string? PIP { get; set; }                //ПІП абонента
        public string? FullAddress { get; set; }        //адреса абонента
        public string? Pip_Pilg { get; set; }           //ПІП пільговика
        public string? Pilg_category { get; set; }      //категорія пільги
        public int? BeneficiaryQuantity { get; set; }   //к-ть пільговиків
        public int? TariffGroupId { get; set; }         //тарифна група
        public DateTime? DateFrom { get; set; }         //період, дата з
        public DateTime? DateTo { get; set; }           //період, дата по
        public decimal? Price { get; set; }             //вартість
        public string? ShortName { get; set; }          //коротка назва тарифу
        public string? TariffGroupName { get; set; }    //назва тарифної групи
        public int? MaxTariffLimit { get; set; }        //максимальна тарифна ставка
        public byte? Id { get; set; }                   //
        public int? TimeZone { get; set; }              //часова зона
        public int? IsHeating { get; set; }             //наявність централізованого опалення
        public int? Discount { get; set; }              //знижка
        public decimal? DiscountKoeff { get; set; }     //коефіцієнт знижки
        public decimal? PricePDV { get; set; }          //ПДВ
        public string? GVP { get; set; }                //наявність підігріву води
        public string? CPGV { get; set; }               //центральне гаряче водопостачання
        public int? MinValue { get; set; }              //мінімальне 
        public int? MaxValue { get; set; }              //максимальне
        public int? IncrementValue { get; set; }        //фактичне
        public string? Borg { get; set; }               //заборгованість
        public int? RegisteredQuantity { get; set; }    //подані показники
        public decimal? QuantityTo { get; set; }        //спожито
        public int? SanNormaSubsKwt { get; set; }       //санітарна норма в кВт
        public decimal? SanNormaSubsGrn { get; set; }   //санітарна норма в грн
        public decimal? QuantityToGrn { get; set; }     //спожито в грн
        public decimal? nm_pay { get; set; }            //до оплати норми
        public string? Cok { get; set; }                //організація
        public string? Vykonavets { get; set; }         //хто виписав
        public string? Nach { get; set; }               //ПІП начальника
    }
}
