using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Звітм
    /// </summary>
    public class Report
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }                        //назва звіту
        public string Description { get; set; }                 //опис звіту
        public string FileScript { get; set; }                  //файл скрипт sql
        public int DbTypeId { get; set; }                       //айді бази
        public DbType DbType { get; set; }                      //тип бази
        public int TypeReportId { get; set; }                   //айді звіту
        public TypeReport ReportType { get; set; }              //тип звіту
        public List<ReportParam> ReportParams { get; set; }     //параметри звіту   
        /// <summary>
        /// список параметрів для звіту
        /// </summary>
        public Report()
        {
            ReportParams = new List<ReportParam>();
        }
        /// <summary>
        /// назва бази
        /// </summary>
        /// <param name="OrganizationCode"></param>
        /// <returns></returns>
        public string GetDbAddress(string OrganizationCode)
        {
            return OrganizationCode + "_" + DbType;
        }
    }
    /// <summary>
    /// тип бази
    /// </summary>
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
    /// <summary>
    /// тип звіту
    /// </summary>
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
    /// <summary>
    /// параметри звіту
    /// </summary>
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
    /// <summary>
    /// тип параметрів
    /// </summary>
    public class ReportParamType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }        
        public string TypeC { get; set; }       
        public string TypeHtml { get; set; }    
    }

}
