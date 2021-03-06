using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.ViewModels
{
    /// <summary>
    /// Звіт у файл
    /// </summary>
    public class ReportWithFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile FileScript { get; set; }
        public int DbTypeId { get; set; }
        public int TypeReportId { get; set; }
    }
}
