using Assistant_TEP.Models;
using Assistant_TEP.ViewModels;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SharpDocx;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using DotNetDBF;

namespace Assistant_TEP.Controllers
{
    public class UkrPosta : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public UkrPosta(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        // GET: UkrPost
        public ActionResult Index()
        {
            string UserName = User.Identity.Name;   //витягуємо ім'я користувача
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName); //витягуємо користувача з бази
            int id = currentUser.Id;        //витягуємо айді користувача
            string CokCode = currentUser.Cok.Code;      //витягуємо Цок користувача
            if (id == 0)
            {
                return NotFound();
            }

            //вибираємо всіх абонентів внесених користувачем
            IEnumerable<Abonents> abon = _context.abonents
                .Where(a => a.UserId == id);
            //виводимо на екран вибірку
            return View(abon);
        }

        // POST: UkrPost/Create
        [HttpPost]
        public JsonResult Create(bool isJuridical, string OsRah, string price)
        {
            try
            {
                string UserName = User.Identity.Name;
                User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName);
                string CokCode = currentUser.Cok.Code;
                int id = currentUser.Id;
                
                //перевіряємо чи це юридичний споживач
                if (isJuridical == true)
                {
                    //перевіряємо чи було введено особовий
                    if (!string.IsNullOrEmpty(OsRah))
                    {
                        bool isNum = int.TryParse(OsRah, out int Num);
                        if (isNum)
                        {
                            //вибираємо з бази Ресу абонента
                            DataTable abon = BillingUtils.AddJuridic(appEnvir, OsRah, CokCode);
                            
                            //якщо абонента знайдено
                            if (abon != null && abon.Rows.Count > 0)
                            {
                                DataRow row = abon.Rows[0];
                                Abonents men = _context.abonents.
                                    FirstOrDefault(m => m.OsRah == row["номер договору"].ToString().Trim() && m.UserId == id);
                                //перевіряємо чи такий абонент вже є
                                if (men == null)
                                {
                                    //якщо нема то добавляємо в таблицю
                                    Abonents abonents = new Abonents
                                    {
                                        CodeCok = "TR" + row["Код ЦОК"].ToString().Trim(),
                                        OsRah = row["номер договору"].ToString().Trim(),
                                        FullName = row["Коротка назва"].ToString().Trim(),
                                        FullAddress = row["Повна адреса"].ToString().Trim(),
                                        PostalCode = row["Індекс"] != null && row["Індекс"].ToString().Trim() != ""
                                                    ? int.Parse(row["Індекс"].ToString().Trim())
                                                    : CokCode == "TR40" ? 46000 : 99999,
                                        Oblast = row["область"].ToString().Trim(),
                                        Rajon = row["район"].ToString().Trim(),
                                        TypeOfCityFull = row["тип пункту"].ToString().Trim(),
                                        TypeOfCityAbbr = row["тип н.п."].ToString().Trim(),
                                        City = row["Нас.пункт"].ToString().Trim(),
                                        TypVul = row["тип в"].ToString().Trim(),
                                        TypeStreet = row["тип вул"].ToString().Trim(),
                                        Street = row["вулиця"].ToString().Trim(),
                                        House = row["будинок"].ToString().Trim(),
                                        Housing = row["корпус"].ToString().Trim(),
                                        Apartment = row["квартира"].ToString().Trim(),
                                        Juridical = isJuridical,
                                        SumaStr = price,
                                        UserId = id
                                    };
                                    //добавляємо і зберігаємо
                                    _ = _context.Add(abonents);
                                    _ = _context.SaveChanges();

                                    //вертаємо результат все гуд
                                    return Json(new
                                    {
                                        success = true,
                                        id = abonents.Id,
                                        osRah = abonents.OsRah,
                                        fullName = abonents.FullName,
                                        fullAddress = abonents.FullAddress
                                    });
                                }
                                //абонент вже є
                                else
                                {
                                    return Json(new { success = false, error = "Особовий вже внесено" });
                                }
                            }
                            //абонента в базі ресу не знайдено
                            else
                            {
                                return Json(new { success = false, error = "Особовий не знайдено" });
                            }
                        }
                    }
                    //поле пусте де особовий
                    return Json(new { success = false, error = "Особовий не введено" });
                }
                //якщо побутовий
                else
                {
                    //перевіряємо чи було введено особовий
                    if (!string.IsNullOrEmpty(OsRah))
                    {
                        //особовий може бути тільки з цифр
                        bool isNum = int.TryParse(OsRah, out int Num);
                        //робимо перевірку на цифри
                        if (isNum)
                        {
                            //вибираємо з бази Ресу абонента
                            DataTable abon = BillingUtils.AddAbon(appEnvir, OsRah, CokCode);
                            //якщо абонента знайдено
                            if (abon != null && abon.Rows.Count > 0)
                            {
                                DataRow row = abon.Rows[0];
                                Abonents men = _context.abonents.
                                    FirstOrDefault(m => m.OsRah == row["особовий"].ToString().Trim() && m.UserId == id);
                                //перевіряємо чи такий абонент вже є
                                if (men == null)
                                {
                                    //якщо нема то добавляємо в таблицю
                                    Abonents abonents = new Abonents
                                    {
                                        CodeCok = "TR" + row["Код ЦОК"].ToString().Trim(),
                                        OsRah = row["особовий"].ToString().Trim(),
                                        FullName = row["ПІП"].ToString().Trim(),
                                        LastName = row["Прізвище"].ToString().Trim(),
                                        FirstName = row["Ім\'я"].ToString().Trim(),
                                        SecondName = row["По батькові"].ToString().Trim(),
                                        FullAddress = row["Повна адреса"].ToString().Trim(),
                                        PostalCode = row["Індекс"] != null && row["Індекс"].ToString().Trim() != ""
                                                    ? int.Parse(row["Індекс"].ToString().Trim())
                                                    : CokCode == "TR40" ? 46000 : 99999,
                                        Oblast = row["область"].ToString().Trim(),
                                        Rajon = row["район"].ToString().Trim(),
                                        TypeOfCityFull = row["тип пункту"].ToString().Trim(),
                                        TypeOfCityAbbr = row["тип н.п."].ToString().Trim(),
                                        City = row["Нас.пункт"].ToString().Trim(),
                                        TypVul = row["тип в"].ToString().Trim(),
                                        TypeStreet = row["тип вул"].ToString().Trim(),
                                        Street = row["вулиця"].ToString().Trim(),
                                        House = row["будинок"].ToString().Trim(),
                                        Housing = row["корпус"].ToString().Trim(),
                                        Apartment = row["квартира"].ToString().Trim(),
                                        Juridical = isJuridical,
                                        SumaStr = price,
                                        UserId = id
                                    };
                                    //добавляємо і зберігаємо
                                    _ = _context.Add(abonents);
                                    _ = _context.SaveChanges();
                                    //вертаємо результат все гуд
                                    return Json(new {
                                        success = true,
                                        id = abonents.Id,
                                        osRah = abonents.OsRah,
                                        fullName = abonents.FullName,
                                        fullAddress = abonents.FullAddress
                                    });
                                }
                                //абонент вже є
                                else
                                {
                                    return Json(new { success = false, error = "Особовий вже внесено" });
                                }
                            }
                            //абонента в базі ресу не знайдено
                            else
                            {
                                return Json(new { success = false, error = "Особовий не знайдено" });
                            }
                        }
                    }
                    //поле пусте де особовий
                    return Json(new { success = false, error = "Особовий не введено" });
                }
            }
            //непередбачувальна помилка
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, error = ex.ToString() });
            }
        }

        // POST: UkrPost/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                //вибираємо абонента по айді і видаляємо
                Abonents men = _context.abonents.
                                    FirstOrDefault(m => m.Id == id);
                _ = _context.abonents.Remove(men);
                _ = _context.SaveChanges();
                
                return Json(new
                {
                    success = true
                });
            }
            catch
            {
                return Json(new { success = false, error = "Абонента не видалено" });
            }
        }

        [HttpPost]
        public JsonResult DeleteAll()
        {
            try
            {
                //вибираємо всіх абонентів внесених користувачем
                string UserName = User.Identity.Name;
                User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName);
                int id = currentUser.Id;

                IEnumerable<Abonents> abonents = _context.abonents.
                                    Where(m => m.UserId == id);
                //видаляємо і зберігаємо
                _context.abonents.RemoveRange(abonents);
                _ = _context.SaveChanges();

                return Json(new
                {
                    success = true
                });
            }
            catch
            {
                return Json(new { success = false, error = "Таблицю не очищено" });
            }
        }
        
        public ActionResult Converty()
        {
            string userName = User.Identity.Name;       //витягуємо який користувач залогінився
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName); //витягуємо користувача з бази
            string vykonavets = currentUser.FullName;   //витягуємо ПІП користувача
            int id = currentUser.Id;    //витягуємо айді юзера
            //вказуємо де знаходяться наші вордовські шаблони для конвертів і де буде файл для друку
            string filePath = "\\files\\Forma103\\";
            string generatedPath = "converty.docx";
            string fileName = "conv.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;
            //формуємо вюху для ворда
            Forma103 viewModel = new Forma103
            {
                //вибираємо внесених абонентів користувачем
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
                OrgAdres = currentUser.Cok.Address.Trim().Substring(7),
                OrganizationName = currentUser.Cok.Name,
                OrgIndex = currentUser.Cok.Address.ToString().Trim().Substring(0, 5),
                Pusto = " "
            };
            
            //Формуємо список людей для конвертів
            DocumentBase convert = DocumentFactory.Create(fullPath, viewModel);
            convert.Generate(fullGenerated);

            //формуємо файл для юзера
            string NewFileName = "converty_" + DateTime.Now.ToString() + ".docx";
            
            //видаємо файл юзеру
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }

        public ActionResult Dbf()
        {
            string userName = User.Identity.Name;       //витягуємо який користувач залогінився
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName); //витягуємо користувача з бази
            string vykonavets = currentUser.FullName;   //витягуємо ПІП користувача
            int id = currentUser.Id;    //витягуємо айді юзера
            
            //створюємо модель для дбф-ки
            Forma103 viewModel = new Forma103
            {
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
            };
            
            //вказуємо шлях до файла
            string filePath = "\\files\\Forma103\\";
            string fileName = "42145798_" + id.ToString() + "_";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;

            //якщо є файл то видаляємо його
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            //створюємо новий дбф файл згідно заданої нами структури
            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using DBFWriter writer = new DBFWriter
                {
                    CharEncoding = Encoding.GetEncoding("windows-1251"),
                    Signature = DBFSignature.DBase3
                };

                //структура дбф-файлу
                DBFField field1 = new DBFField("psgvno", NativeDbType.Numeric, 9);
                DBFField field2 = new DBFField("psbarc", NativeDbType.Char, 13);
                DBFField field3 = new DBFField("rccn3c", NativeDbType.Numeric, 9);
                DBFField field4 = new DBFField("rcpidx", NativeDbType.Char, 5);
                DBFField field5 = new DBFField("rcaddr", NativeDbType.Char, 80);
                DBFField field6 = new DBFField("rcname", NativeDbType.Char, 40);
                DBFField field7 = new DBFField("snmtdc", NativeDbType.Numeric, 9);
                DBFField field8 = new DBFField("psappc", NativeDbType.Numeric, 9);
                DBFField field9 = new DBFField("pscatc", NativeDbType.Numeric, 9);
                DBFField field10 = new DBFField("psrazc", NativeDbType.Numeric, 9);
                DBFField field11 = new DBFField("psnotc", NativeDbType.Numeric, 9);
                DBFField field12 = new DBFField("pswgt", NativeDbType.Numeric, 9);
                DBFField field13 = new DBFField("pkprice", NativeDbType.Numeric, 10, 2);
                DBFField field14 = new DBFField("aftpay", NativeDbType.Numeric, 10, 2);
                DBFField field15 = new DBFField("phone", NativeDbType.Char, 15);

                writer.Fields = new[] { field1, field2, field3, field4, field5, field6, field7, field8, field9, field10,
                                    field11, field12, field13, field14, field15 };

                int psgvno = 1;
                string psbarc = "";
                int rccn3c = 804;
                int snmtdc = 1;
                int psappc = 2;
                int pscatc = 2;
                int psrazc = 1;
                int psnotc = 1;
                int pswgt = 40;
                int pkprice = 0;
                int aftpay = 0;
                string phone = "";

                //наповнюємо файл даними
                foreach (var dibifi in viewModel.People)
                {
                    string rcpidx = dibifi.PostalCode.ToString();
                    string rcaddr = dibifi.FullAddress.ToString();
                    string rcname = dibifi.FullName;

                    writer.AddRecord(psgvno, psbarc, rccn3c, rcpidx, rcaddr, rcname, snmtdc, psappc,
                                        pscatc, psrazc, psnotc, pswgt, pkprice, aftpay, phone);
                }
                writer.Write(fos);  //записуємо у файл
            }
            
            //видаємо користувачу файл
            string fileNameNew = fileName + DateTime.Now.ToString() + ".dbf";
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            
            //якщо є файл то видаляємо його
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
        }

        public ActionResult Supr()
        {
            string userName = User.Identity.Name;       //витягуємо який користувач залогінився
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName); //витягуємо користувача з бази
            string vykonavets = currentUser.FullName;   //витягуємо ПІП користувача
            int id = currentUser.Id;    //витягуємо айді юзера
            //вказуємо де знаходяться наші вордовські шаблони для конвертів і де буде файл для друку
            string filePath = "\\files\\Forma103\\";
            string generatedPath = "suprovidna_DBF.docx";
            string fileName = "suprovidDBF.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

            //формуємо вюху для ворда
            Forma103 viewModel = new Forma103
            {
                //вибираємо внесених абонентів користувачем
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
            };

            //то для обрахунку суми за конверти
            int kt = viewModel.People.Count();
            decimal suma = 0;

            foreach (var item in viewModel.People)
            {
                suma = suma + decimal.Parse(item.SumaStr);
            }

            //формуємо модель для ворда
            Forma103 context = new Forma103
            {
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
                OrgAdres = currentUser.Cok.Address.Trim().Substring(7),
                OrganizationName = currentUser.Cok.Name,
                OrgIndex = currentUser.Cok.Address.ToString().Trim().Substring(0, 5),
                Suma = suma,
                PDV = Math.Round(suma / 6, 2),
                SumaStr = MoneyToStr.GrnPhrase(suma),
                Nach = currentUser.Cok.Nach,
                Buh = currentUser.Cok.Buh,
                Postal = currentUser.Cok.Postal
            };

            //видаємо документ юзеру
            DocumentBase suprovid = DocumentFactory.Create(fullPath, context);
            suprovid.Generate(fullGenerated);
            string NewFileName = "suprovidna_" + DateTime.Now.ToString() + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }
    }
}
