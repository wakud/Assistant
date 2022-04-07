using System.Linq;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Наповнення програми початковими даними
    /// </summary>
    public class DbInitialization
    {
        public static void Initial(MainContext context)
        {
            //Добавляємо організації
            if (!context.Coks.Any())
            {
                Organization misto = new Organization 
                { 
                    Name = "Назва організації", 
                    Code = "Код організації",
                    Nach = "ПІП керівника",
                    Buh = "ПІП бухгалтера",
                    Address = "Індекс, поштова адреса",
                };
                Organization selo = new Organization
                {
                    Name = "Назва організації",
                    Code = "1234",
                    Nach = "ПІП керівника",
                    Buh = "ПІП бухгалтера",
                    Address = "Індекс, поштова адреса",
                };

                context.Coks.AddRange(misto, selo);
                context.SaveChanges();
            }
            //Добавляємо користувача адміна
            if (!context.Users.Any())
            {
                Organization cok = context.Coks.FirstOrDefault(c => c.Code == "1234");
                User adm = new User { FullName = "admin", Login = "admin", Password = "Qwerty123", IsAdmin = "1", Cok = null };
                context.Users.Add(adm);
                context.SaveChanges();
            }
            //добавляємо тип параметру
            if(!context.ReportParamTypes.Any())
            {
                ReportParamType type1 = new ReportParamType { Name = "Стрічка", TypeC = "string", TypeHtml = "text" };
                ReportParamType type2 = new ReportParamType { Name = "Ціле число", TypeC = "int", TypeHtml = "numeric" };
                context.ReportParamTypes.Add(type1);
                context.ReportParamTypes.Add(type2);
                context.SaveChanges();
            }
            //добавляємо параметр період
            if(!context.ReportParams.Any())
            {
                Report rep = context.Reports.First();
                ReportParamType rpt = context.ReportParamTypes.First();
                ReportParam rp = new ReportParam { Name = "period", Description = "Період", ParamType = rpt, Report = rep};
                context.ReportParams.Add(rp);
                context.SaveChanges();
            }
            //добавляємо типи баз
            if (!context.DbTypes.Any())
            {
                DbType type1 = new DbType { Type = "Utility" };     //фізичні абоненти
                DbType type2 = new DbType { Type = "Juridical" };   //юридичні абоненти
                context.DbTypes.Add(type1);
                context.DbTypes.Add(type2);
                context.SaveChanges();
            }
            //добавляємо типи звітів
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
            //добавляємо початковий (тестовий) звіт
            if (!context.Reports.Any())
            {
                DbType tp = context.DbTypes.FirstOrDefault(t => t.Type == "Utility");
                TypeReport tr = context.TypeReports.FirstOrDefault(r => r.Name == "Місячні");
                Report rep = new Report { Name = "Test report", Description = "test desc", DbType = tp, FileScript = "Test.sql", ReportType = tr};
                context.Reports.Add(rep);
                context.SaveChanges();
            }
        }
    }
}
