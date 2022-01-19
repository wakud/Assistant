using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant_TEP.Models;
using ClosedXML.Excel;

namespace Assistant_TEP.MyClasses
{
    public class MoneyExcel
    {
        private readonly XLWorkbook wb;
        private IXLWorksheet ws;
        Dictionary<string, MoneySubsydii> zap;

        public MoneyExcel(Dictionary<string, MoneySubsydii> zap, string filePath)
        {
            wb = new XLWorkbook(filePath);
            this.zap = zap;
        }

        public void ToExcel()
        {
            ws = wb.Worksheets.First();
            var rows = ws.RangeUsed().RowsUsed();
            Dictionary<string, string> upszn = new Dictionary<string, string>();
            upszn["6101"] = "27";
            upszn["6102"] = "28";
            upszn["6103"] = "29";
            upszn["6104"] = "30";
            upszn["6105"] = "31";
            upszn["6106"] = "32";
            upszn["6107"] = "33";
            upszn["6108"] = "34";
            upszn["6109"] = "35";
            upszn["6110"] = "36";
            upszn["6111"] = "37";
            upszn["6112"] = "38";
            upszn["6113"] = "41";
            upszn["6114"] = "39";
            upszn["6115"] = "42";
            upszn["6116"] = "43";
            upszn["6117"] = "40";
            upszn["6118"] = "44";
            upszn["6119"] = "42";
            upszn["6120"] = "35";
            
            int currRow = 7;
            int currCell = 5;       
            
            foreach (var row in rows.Skip(5))
            {
                if (!upszn.ContainsKey(row.Cell(1).Value.ToString().Substring(0, 4)))
                {
                    Console.OutputEncoding = Encoding.GetEncoding(1251);
                    Console.WriteLine("УПСЗН не знайдено");
                }
                else
                {
                    string accountNumberOrNew = row.Cell(4).Value != null ? row.Cell(4).Value.ToString() : "";

                    if (accountNumberOrNew.StartsWith("0"))
                    {
                        accountNumberOrNew = accountNumberOrNew.TrimStart('0');
                    }
                    if (accountNumberOrNew.Contains("/"))
                    {
                        accountNumberOrNew = accountNumberOrNew.Substring(accountNumberOrNew.IndexOf('/') + 1);
                    }
                    if (accountNumberOrNew.Contains("\\"))
                    {
                        accountNumberOrNew = accountNumberOrNew.Substring(accountNumberOrNew.IndexOf('\\') + 1);
                    }

                    string accountCode = upszn.GetValueOrDefault(row.Cell(1).Value.ToString().Substring(0, 4), "") + ":" + accountNumberOrNew;
                    MoneySubsydii? subs = null;

                    if (zap.ContainsKey(accountNumberOrNew))
                    {
                        subs = zap[accountNumberOrNew];
                    }
                    else if (zap.ContainsKey(accountCode))
                    {
                        subs = zap[accountCode];
                    }
                    
                    if(subs != null)
                    {
                        if (subs.Spogyto > 0)
                        {
                            row.Cell(currCell).Value = subs.Spogyto;
                        }
                        else
                        {
                            row.Cell(currCell).Value = 0;
                        }
                        if (subs.Borg > 0)
                        {
                            row.Cell(currCell + 1).Value = subs.Borg;
                        }
                        else
                        {
                            row.Cell(currCell + 1).Value = 0;
                        }
                        //Console.WriteLine($"Ключ знайдено {subs.OsRah.ToString().Trim()}");
                        //Console.WriteLine($"Аккаунт: {subs.OsRah},Спожито: {subs.Spogyto}, Борг: {subs.Borg}");
                    }
                    else
                    {
                        row.Cell(currCell).Value = 0;
                        row.Cell(currCell + 1).Value = 0;
                        //Console.WriteLine("Ключа не знайдено");
                    }
                }
                //string rowNumber = $"Номер УПСЗН {row.Cell(1).Value} Номер особового рахунку {row.Cell(4).Value}";
                //Console.WriteLine(rowNumber);

                currRow++;
                currCell = 5;
            }
        }

        public byte[] CreateFile()
        {
            using MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            byte[] content = stream.ToArray();
            return content;
        }

        public List<MoneySubsydii> FromExcel(string cok)
        {
            Encoding encoding = Encoding.GetEncoding("Windows-1251");
            ws = wb.Worksheets.First();
            var rows = ws.RangeUsed().RowsUsed();
            List<MoneySubsydii> subs_p = new List<MoneySubsydii>();

            Dictionary<string, List<string>> upszn = new Dictionary<string, List<string>>
            {
                ["27"] = new List<string> { "6101" },   //бережани
                ["28"] = new List<string> { "6102" },   //борщів
                ["29"] = new List<string> { "6103" },   //бучач
                ["30"] = new List<string> { "6104" },   //гусятин
                ["31"] = new List<string> { "6105" },   //заліщики
                ["32"] = new List<string> { "6106" },   //збараж
                ["33"] = new List<string> { "6107" },   //зборів
                ["34"] = new List<string> { "6108" },   //козова
                ["35"] = new List<string> { "6109", "6120" },   
                ["36"] = new List<string> { "6110" },   //ланівці
                ["37"] = new List<string> { "6111" },   //монастириськ
                ["38"] = new List<string> { "6112" },   //підволочиськ
                ["39"] = new List<string> { "6114" },   //терн. район
                ["40"] = new List<string> { "6117" },   //м. тернопіль
                ["41"] = new List<string> { "6113" },   //теребовля
                ["42"] = new List<string> { "6115", "6119" },
                ["43"] = new List<string> { "6116" },   //шумськ
                ["44"] = new List<string> { "6118" }   //підгайці
            };

            DateTime datePay = DateTime.Parse(ws.Row(3).Cell(6).Value.ToString());  
            int currRow = 7;
            //робим перебір по екселю
            foreach (var row in rows.Skip(5))
            {
                 if(upszn[cok.Substring(2, 2)].Contains(row.Cell(1).Value.ToString().Substring(0, 4)))
                 {
                    string accountNumberOrNew = row.Cell(4).Value != null ? row.Cell(4).Value.ToString() : "";
                    if (accountNumberOrNew.StartsWith("0"))
                    {
                        accountNumberOrNew = accountNumberOrNew.TrimStart('0');
                    }
                    if (accountNumberOrNew.Contains("/"))
                    {
                        accountNumberOrNew = accountNumberOrNew.Substring(accountNumberOrNew.IndexOf('/') + 1);
                    }
                    if (accountNumberOrNew.Contains("\\"))
                    {
                        accountNumberOrNew = accountNumberOrNew.Substring(accountNumberOrNew.IndexOf('\\') + 1);
                    }

                    try
                    {
                        subs_p.Add(new MoneySubsydii    
                        {
                            Raj = int.Parse(cok.Substring(2, 2)),
                            Pip = row.Cell(3).Value.ToString(),
                            DataOplaty = datePay,
                            NumberUPSZN = row.Cell(1).Value.ToString().Substring(0, 4),
                            SumaOplaty = decimal.Parse(row.Cell(7).Value.ToString(), CultureInfo.InvariantCulture),
                            //OsRah = accountNumberOrNew,
                            AccNumber = long.Parse(accountNumberOrNew)
                        });

                    } 
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                 }
                currRow++;
            }

            return subs_p;
        }
    }
}
