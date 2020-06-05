using Assistant_TEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class DbInitialization
    {
        public static void Initial(MainContext context)
        {
            //Добавимо початкові дані в БД
            if (!context.Coks.Any())
            {
                Organization misto = new Organization 
                { 
                    Name = "Тернопільський міський ЦОК", 
                    Code = "TR40",
                    Nach = "Кордас Ігор Богданович",
                    Buh = "Бойцун Ольга Володимирівна",
                    Address = "46016, м. Тернопіль, вул. Злуки 2В",
                };
                Organization selo = new Organization
                {
                    Name = "Тернопільський районний ЦОК",
                    Code = "TR39",
                    Nach = "Кордас Ігор Богданович",
                    Buh = "Савяк Ірина Ярославівна",
                    Address = "46016, м. Тернопіль, вул. Злуки 2В",
                };

                context.Coks.AddRange(misto, selo);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                Organization cok = context.Coks.FirstOrDefault(c => c.Code == "TR40");
                User adm = new User { FullName = "admin", Login = "admin", Password = "1", IsAdmin = "1", Cok = null };
                User usr = new User { FullName = "TR40", Login = "TR40", Password = "2", IsAdmin = "0", Cok =  cok};

                context.Users.Add(adm);
                context.Users.Add(usr);
                context.SaveChanges();
            }
            if(!context.ReportParamTypes.Any())
            {
                ReportParamType type1 = new ReportParamType { Name = "Стрічка", TypeC = "string", TypeHtml = "text" };
                ReportParamType type2 = new ReportParamType { Name = "Ціле число", TypeC = "int", TypeHtml = "numeric" };
                context.ReportParamTypes.Add(type1);
                context.ReportParamTypes.Add(type2);
                context.SaveChanges();
            }
            if (!context.DbTypes.Any())
            {
                DbType type1 = new DbType { Type = "Utility" };
                DbType type2 = new DbType { Type = "Juridical" };
                context.DbTypes.Add(type1);
                context.DbTypes.Add(type2);
                context.SaveChanges();
            }
            if (!context.TypeReports.Any())
            {
                TypeReport tp1 = new TypeReport { Name = "Місячні" };
                TypeReport tp2 = new TypeReport { Name = "Квартальні" };
                TypeReport tp3 = new TypeReport { Name = "Річні" };
                TypeReport tp4 = new TypeReport { Name = "Інші" };
                context.TypeReports.Add(tp1);
                context.TypeReports.Add(tp2);
                context.TypeReports.Add(tp3);
                context.TypeReports.Add(tp4);
                context.SaveChanges();
            }
            if (!context.Reports.Any())
            {
                DbType tp = context.DbTypes.FirstOrDefault(t => t.Type == "Utility");
                TypeReport tr = context.TypeReports.FirstOrDefault(r => r.Name == "Місячні");
                Report rep = new Report { Name = "Test report", Description = "test desc", DbType = tp, FileScript = "Test.sql", ReportType = tr};
                context.Reports.Add(rep);
                context.SaveChanges();
            }
            if(!context.ReportParams.Any())
            {
                Report rep = context.Reports.First();
                ReportParamType rpt = context.ReportParamTypes.First();
                ReportParam rp = new ReportParam { Name = "period", Description = "Період", ParamType = rpt, Report = rep};
                context.ReportParams.Add(rp);
                context.SaveChanges();
            }
        }
    }
}
