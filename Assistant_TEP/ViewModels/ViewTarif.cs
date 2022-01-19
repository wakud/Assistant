using Assistant_TEP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Assistant_TEP.ViewModels
{
    public class ViewTarif
    {
        public SelectList Tarifs { get; set; }
        public IEnumerable<TarifUkrPost> TarifUkrPosts { get; set; }
        public IEnumerable<Abonents> People { get; set; }

    }
}
