using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// сортування
    /// </summary>
    public enum SortState
    {
        NameAsc,    //по назві по збільшенню
        NameDesc,   //по назві по спаданню
        DescriptionAsc,    //по опису збільшення
        DescriptionDsc,     //по опису зменшення
        DbType,     //по типу бд
    }
}
