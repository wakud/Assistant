using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Оплати з приватбанку
    /// </summary>
    public class Privat
    {
        public long OS_RAH_B { get; set; }      //особовий приватбанку
        public long OS_RAH_N { get; set; }      //новий особовий AccountNumberNew
        public long? AccountNumber { get; set; } //в програмі AccountNumber
        public long? AccountId { get; set; }    //ID особового
        public DateTime PAYDATE { get; set; }   //дата оплати
        public decimal SUMMA { get; set; }      //сума оплати
        public int? PARAMETER { get; set; }     //показники
        public string? FAMILY { get; set; }      //прізвище абонента
        public string? NAME { get; set; }        //ім'я абонента
        public string? NAME_1 { get; set; }      //по батькові абонента
        public string? TOWN { get; set; }        //населений пункт
        public string? STREET { get; set; }      //вулиця
        public string? HOUSE { get; set; }       //будинок
        public string? HOUSE_S { get; set; }     //буква до будинку
        public string? APARTMENT { get; set; }   //квартира
        public string? APARTMENTS { get; set; }  //буква до квартири
        public string? PAYTYPE { get; set; }     //тип оплати
        public long? OPERATOR { get; set; }      //оператор
        public DateTime CREATDATE { get; set; } //дата створення платежу
        public int? CREATHH { get; set; }        //година створення платежу
        public int? CREATMM { get; set; }        //хвилини створення платежу
    }
}
