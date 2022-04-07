using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// відсортована модель для відображення
    /// </summary>
    public class SortViewModel
    {
        public SortState NameSort { get; private set; }     //значення сортування по імені
        public SortState DescriptionSort { get; private set; }     //значення сортування по опису
        public SortState DbTypeSort { get; private set; }     //значення сортування по бд
        public SortState Current { get; private set; }     //текуче значення сортування

        public SortViewModel(SortState sortOrder)
        {
            NameSort = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            DescriptionSort = sortOrder == SortState.DescriptionAsc ? SortState.DescriptionDsc : SortState.DescriptionAsc;
            DbTypeSort = sortOrder == SortState.DbType  ? SortState.DbType : SortState.DbType;
            Current = sortOrder;
        }
    }
}
