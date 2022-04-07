using System;
using System.Collections.Generic;
using System.Data;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Довідка про оплати абонента
    /// </summary>
    public class Dovidka
    {
        public int Id { get; set; }
        public DataTable Result { get; set; }       //таблиця з оплатами
        public string Vykonavets { get; set; }      //Хто виписав довідку
        public string Cok { get; set; }             //назва організації
        public string Nach { get; set; }            //ПІП начальника
        public string FullName { get; set; }        //ПІП абонента
        public string AccountNumber { get; set; }   //особовий номер
        public string FullAddress { get; set; }     //адреса абонента
        public DateTime DateFrom { get; set; }      //період оплат дата з
        public DateTime DateTo { get; set; }        //період оплат дата до
        public List<Oplata> Oplats { get; set; }    //список оплат
    }
    /// <summary>
    /// Оплати абонента
    /// </summary>
    public class Oplata
    {
        public string DateOplaty { get; set; }  //дата оплати
        public string Suma { get; set; }        //сума оплати
    }
}
