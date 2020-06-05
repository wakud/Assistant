using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant_TEP.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }            //Назва звіту
        public string Description { get; set; }     //Опис звіту
        public string FileScript { get; set; }      //назва файлу скрипта
        // Juridical, Utility
        public int DbTypeId { get; set; }
        public DbType DbType { get; set; }        //префікс назви бази (База для виконання скрипта: район + _ + тип бази)

        public int TypeReportId { get; set; }
        public TypeReport ReportType { get; set; }
        //посилання на список параметрів один до багатьох
        public List<ReportParam> ReportParams { get; set; }     

        public Report()
        {
            ReportParams = new List<ReportParam>();
        }

        public string GetDbAddress(string OrganizationCode)
        {
            return OrganizationCode + "_" + DbType;
        }
    }
    
    public class DbType
    {
        [Key]
        public int Id { get; set; }
        [StringLength (10)]
        public string Type { get; set; }
        public List<Report> Reports { get; set; }
        
        public DbType()
        {
            Reports = new List<Report>();
        }
    }

    public class TypeReport
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Report> Reports { get; set; }

        public TypeReport()
        {
            Reports = new List<Report>();
        }
    }

    public class ReportParam
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }                            //назва параметру в скрипті
        public string Description { get; set; }                     //назва параметру для користувача
        //посилання на репорт
        public int ReportId { get; set; }
        public Report Report { get; set; }
        //посилання на тип параметру один до одного
        public int ParamTypeId { get; set; }
        public ReportParamType ParamType { get; set; }
    }

    public class ReportParamType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeC { get; set; }           //тип параметру в прозі (бекенд)
        public string TypeHtml { get; set; }        //тип параметру для юзера (фронтенд)
    }

}
