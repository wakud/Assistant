using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    public class EksportForSubs
    {
        public int Rash { get; set; }           //особовий рахунок
        public string Numb { get; set; }        //рахунок в Шидлівського (Укрспецінформ)
        public string Fio { get; set; }         //ПІП абонента
        public string Name_v { get; set; }      //Назва вулиці
        public string Bld { get; set; }         //Будинок
        public string Corp { get; set; }        //Корпус
        public string Flat { get; set; }        //Квартира
        public string Nazva { get; set; }       //тип населення
        public string Tariff { get; set; }      //пільговий або повний тариф
        public decimal Discount { get; set; }   //Відсоток пільги
        public string Pilgovuk { get; set; }
        public decimal Gar_voda { get; set; }
        public decimal Gaz_vn { get; set; }
        public decimal El_opal { get; set; }
        public string Kilk_pilg { get; set; }
        public string T11_cod_na { get; set; }
        public string Orendar { get; set; }
        public decimal Borg { get; set; }
    }
}
