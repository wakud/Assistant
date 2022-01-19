using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant_TEP.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileScript { get; set; }
        public int DbTypeId { get; set; }
        public DbType DbType { get; set; }

        public int TypeReportId { get; set; }
        public TypeReport ReportType { get; set; }
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
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParamSource { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; }
        public int ParamTypeId { get; set; }
        public ReportParamType ParamType { get; set; }
    }

    public class ReportParamType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeC { get; set; }
        public string TypeHtml { get; set; }
    }

}
