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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Assistant_TEP.Controllers
{
    /// <summary>
    /// контролер для роботи з укрпоштою
    /// </summary>
    public class UkrPosta : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public UkrPosta(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }
        /// <summary>
        /// Вивід сторінки укрпошти
        /// </summary>
        /// <param name="tariff"></param>
        /// <returns></returns>
        public ActionResult Index(int? tariff)
        {
            string UserName = User.Identity.Name;   
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName); 
            int id = currentUser.Id;        
            string CokCode = currentUser.Cok.Code;      
            if (id == 0)
            {
                return NotFound();
            }

            IEnumerable<Abonents> abon = _context.abonents
                .Where(a => a.UserId == id);

            List<TarifUkrPost> tarifs = _context.TarifUkrPosts.ToList();
            tarifs.Insert(0, new TarifUkrPost { Name = "Виберіть тариф", Id = 0 });

            SelectList tarifList = tariff != null
                ? new SelectList(tarifs, "Id", "Name", tariff)
                : new SelectList(tarifs, "Id", "Name", 0);

            ViewTarif viewTarif = new ViewTarif
            {
                People = abon,
                Tarifs = tarifList,
                TarifUkrPosts = _context.TarifUkrPosts.ToList()
            };

            return View(viewTarif);
        }
        /// <summary>
        /// Добавлення абонента по особовому
        /// </summary>
        /// <param name="isJuridical"></param>
        /// <param name="OsRah"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(bool isJuridical, string OsRah, string price)
        {
            try
            {
                string UserName = User.Identity.Name;
                User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName);
                string CokCode = currentUser.Cok.Code;
                int id = currentUser.Id;
                //добавляємо абонентів юридичних
                if (isJuridical == true)
                {
                    if (!string.IsNullOrEmpty(OsRah))
                    {
                        bool isNum = int.TryParse(OsRah, out int Num);
                        if (isNum)
                        {
                            DataTable abon = BillingUtils.AddJuridic(appEnvir, OsRah, CokCode);
                            
                            if (abon != null && abon.Rows.Count > 0)
                            {
                                DataRow row = abon.Rows[0];
                                Abonents men = _context.abonents.
                                    FirstOrDefault(m => m.OsRah == row["номер договору"].ToString().Trim() && m.UserId == id);
                                if (men == null)
                                {
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
                                    _ = _context.Add(abonents);
                                    _ = _context.SaveChanges();

                                    return Json(new
                                    {
                                        success = true,
                                        id = abonents.Id,
                                        osRah = abonents.OsRah,
                                        fullName = abonents.FullName,
                                        fullAddress = abonents.FullAddress
                                    });
                                }
                                else
                                {
                                    return Json(new { success = false, error = "Особовий вже внесено" });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, error = "Особовий не знайдено" });
                            }
                        }
                    }
                    return Json(new { success = false, error = "Особовий не введено" });
                }
                else     //добавляємо фізичних абонентів
                {
                    if (!string.IsNullOrEmpty(OsRah))
                    {
                        bool isNum = int.TryParse(OsRah, out int Num);
                        if (isNum)
                        {
                            DataTable abon = BillingUtils.AddAbon(appEnvir, OsRah, CokCode);
                            if (abon != null && abon.Rows.Count > 0)
                            {
                                DataRow row = abon.Rows[0];
                                Abonents men = _context.abonents.
                                    FirstOrDefault(m => m.OsRah == row["особовий"].ToString().Trim() && m.UserId == id);
                                if (men == null)
                                {
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
                                    _ = _context.Add(abonents);
                                    _ = _context.SaveChanges();
                                    return Json(new {
                                        success = true,
                                        id = abonents.Id,
                                        osRah = abonents.OsRah,
                                        fullName = abonents.FullName,
                                        fullAddress = abonents.FullAddress
                                    });
                                }
                                else
                                {
                                    return Json(new { success = false, error = "Особовий вже внесено" });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, error = "Особовий не знайдено" });
                            }
                        }
                    }
                    return Json(new { success = false, error = "Особовий не введено" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, error = ex.ToString() });
            }
        }
        /// <summary>
        /// іидалення одного абонента
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
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
        /// <summary>
        /// видалення цілого списку абонентів
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteAll()
        {
            try
            {
                string UserName = User.Identity.Name;
                User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == UserName);
                int id = currentUser.Id;

                IEnumerable<Abonents> abonents = _context.abonents.
                                    Where(m => m.UserId == id);
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
        /// <summary>
        /// формування і друк конвертів
        /// </summary>
        /// <returns></returns>
        public ActionResult Converty()
        {
            string userName = User.Identity.Name;       
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName); 
            string vykonavets = currentUser.FullName;   
            int id = currentUser.Id;    
            string filePath = "\\files\\Forma103\\";
            string generatedPath = "converty.docx";
            string fileName = "conv.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;
            Forma103 viewModel = new Forma103
            {
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
                OrgAdres = currentUser.Cok.Address.Trim().Substring(7),
                OrganizationName = currentUser.Cok.Name,
                OrgIndex = currentUser.Cok.Address.ToString().Trim().Substring(0, 5),
                Pusto = " "
            };

            DocumentBase convert = DocumentFactory.Create(fullPath, viewModel);
            convert.Generate(fullGenerated);

            string NewFileName = "converty_" + DateTime.Now.ToString() + ".docx";
            
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }
        /// <summary>
        /// формування дбф файлу
        /// </summary>
        /// <returns></returns>
        public ActionResult Dbf()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            int id = currentUser.Id;
            
            Forma103 viewModel = new Forma103
            {
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
            };
            
            string filePath = "\\files\\Forma103\\";
            string fileName = "42145798_" + id.ToString() + "_";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using DBFWriter writer = new DBFWriter
                {
                    CharEncoding = Encoding.GetEncoding("windows-1251"),
                    Signature = DBFSignature.DBase3
                };

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

                foreach (var dibifi in viewModel.People)
                {
                    string rcpidx = dibifi.PostalCode.ToString();
                    string rcaddr = dibifi.FullAddress.ToString();
                    string rcname = dibifi.FullName;

                    writer.AddRecord(psgvno, psbarc, rccn3c, rcpidx, rcaddr, rcname, snmtdc, psappc,
                                        pscatc, psrazc, psnotc, pswgt, pkprice, aftpay, phone);
                }
                writer.Write(fos);
            }
            
            string fileNameNew = fileName + DateTime.Now.ToString() + ".dbf";
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
        }
        /// <summary>
        /// формування і друк супровідної до дбф
        /// </summary>
        /// <returns></returns>
        public ActionResult Supr()
        {
            string userName = User.Identity.Name;       
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName); 
            string vykonavets = currentUser.FullName;   
            int id = currentUser.Id;    
            string filePath = "\\files\\Forma103\\";
            string generatedPath = "suprovidna_DBF.docx";
            string fileName = "suprovidDBF.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

            Forma103 viewModel = new Forma103
            {
                People = _context.abonents.Where(a => a.UserId == id).ToList(),
            };

            int kt = viewModel.People.Count();
            decimal suma = 0;

            foreach (var item in viewModel.People)
            {
                suma += decimal.Parse(item.SumaStr);
            }

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
