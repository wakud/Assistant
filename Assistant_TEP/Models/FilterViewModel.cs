using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Assistant_TEP.Models
{
    public class FilterViewModel
    {
        public FilterViewModel(List<Report> reports, int? report, string name)
        {
            // устанавливаем начальный элемент, который позволит выбрать все
            reports.Insert(0, new Report { Name = "Всі", Id = 0 });
            Reports = new SelectList(reports, "Id", "Name", report);
            SelectedReport = report;
            SelectedName = name;
        }
        public SelectList Reports { get; private set; } //список звітів
        public int? SelectedReport { get; private set; }    //вибраний звіт
        public string SelectedName { get; private set; }    //введена назва
    }
}
