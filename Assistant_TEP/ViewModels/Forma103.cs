using Assistant_TEP.Models;
using System.Collections.Generic;

namespace Assistant_TEP.ViewModels
{
    /// <summary>
    /// форма 103 для укрпошти на екран
    /// </summary>
    public class Forma103
    {
        public IEnumerable<Abonents> People { get; set; }   //список абонентів для укрпошти

        public string? OrganizationName { get; set; }       // назва організації

        public string? OrgAdres { get; set; }               //адреса організації

        public string? OrgIndex { get; set; }               //індекс організації

        public decimal? Suma { get; set; }                  //сума до оплати за послуги

        public decimal? PDV { get; set; }                   //ПДВ

        public string? SumaStr { get; set; }                //сума прописом

        public int? Kt { get; set; }                        //к-ть записів

        public string Nach { get; set; }                    //ПІП начальника

        public string Buh { get; set; }                     //ПІП бухгалтера

        public string PostalIndex { get; set; }             //поштовий індекс

        public string Postal { get; set; }                  //поштова адреса

        public string? Pusto { get; set; }                  //хйз
    }
}
