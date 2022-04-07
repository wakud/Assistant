using Assistant_TEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    /// <summary>
    /// форма 103 для укрпошти
    /// </summary>
    public class Form103Abonent
    {
        public string price { get; set; }       //ціна послуги
        public bool isJuridical { get; set; }   //чи юридичний абонент
        public string osRah { get; set; }       //особовий абонента
    }
}
