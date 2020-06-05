using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.ViewModels
{
    public class ReportWithFile
    {
        public int Id { get; set; }
        public string Name { get; set; }            //Назва звіту
        public string Description { get; set; }     //Опис звіту
        public IFormFile FileScript { get; set; }   //шдях до файлу
        public int DbTypeId { get; set; }        //префікс назви бази (База для виконання скрипта: район + _ + тип бази)
        public int TypeReportId { get; set; }   //
    }
}
