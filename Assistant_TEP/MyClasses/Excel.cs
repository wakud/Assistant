using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assistant_TEP.Models;
using ClosedXML.Excel;

namespace Assistant_TEP.MyClasses
{
    public class Excel
    {
        private readonly XLWorkbook wb;
        private IXLWorksheet ws;
        private Dictionary<string, List<Pilga2>> categories;
        private Dictionary<string, CategoryTotals> categoryTotals;

        public Excel(Dictionary<string, 
            List<Pilga2>> categories, Dictionary<string, 
                CategoryTotals> categoryTotals
            )
        {
            wb = new XLWorkbook();
            this.categories = categories;
            this.categoryTotals = categoryTotals;
        }

        private DateTime CreateStanomNa(string period)
        {
            return DateTime.Parse(period);
        }

        private string GetDateToReport(string period)
        {
            string StrMonth = "";
            DateTime DtStanom = CreateStanomNa(period);

            switch (DtStanom.Month)
            {
                case 1:
                    StrMonth = "у січні";
                    break;
                case 2:
                    StrMonth = "У лютому";
                    break;
                case 3:
                    StrMonth = "у березні";
                    break;
                case 4:
                    StrMonth = "у квітні";
                    break;
                case 5:
                    StrMonth = "у травні";
                    break;
                case 6:
                    StrMonth = "у червні";
                    break;
                case 7:
                    StrMonth = "у липні";
                    break;
                case 8:
                    StrMonth = "у серпні";
                    break;
                case 9:
                    StrMonth = "у вересні";
                    break;
                case 10:
                    StrMonth = "у жовтні";
                    break;
                case 11:
                    StrMonth = "у листопаді";
                    break;
                case 12:
                    StrMonth = "у грудні";
                    break;
                default:
                    StrMonth = "";
                    break;
            }
            return StrMonth + " " + DtStanom.Year.ToString() + " р.";
        }

        private void SetDefaultSettings()
        {
            ws.Name = "Звіт";
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;  //ставимо альбомну сторінку
            ws.PageSetup.AdjustTo(80);
            ws.PageSetup.Margins.Left = 0.6;
            ws.PageSetup.Margins.Right = 0.4;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.VerticalDpi = 600;
            ws.PageSetup.HorizontalDpi = 600;
        }

        private void SetZvitHeader(string value)
        {
            ws.Cell("A1").Value = value;
        }

        public void CreateZvit(User user, string period)
        {
            string NameDoc = "ТОВ \"Тернопільелектропостач\"";
            string Edrpou = "42145798";
            
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //шапка документу
            SetZvitHeader("Код ЄДРПОУ: " + Edrpou);
            styler.CenterCellText("B1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            ws.Cell("C1").Value = NameDoc;
            styler.CenterAndMerge("C1", "I1");
            ws.Cell("K1").Value = "Форма 2-пільга";
            styler.CenterAndMerge("K1", "M1");
            ws.Cell("A2").Value = "Розрахунок видатків на відшкодування витрат , повязаних з наданням пільг,";
            styler.CenterAndMerge("A2", "M2");
            ws.Cell("C3").Value = GetDateToReport(period);
            styler.CenterAndMerge("C3", "H3");

            //виставляємо ширину стовпчиків
            ws.Column(1).Width = 5.43;       //A
            ws.Column(2).Width = 32.71;      //B
            ws.Column(3).Width = 6;          //C
            ws.Column(4).Width = 6;          //D
            ws.Column(5).Width = 10.29;       //E
            ws.Column(6).Width = 5.86;       //F
            ws.Column(7).Width = 6;          //G
            ws.Column(8).Width = 6;          //H
            ws.Column(9).Width = 6;          //I
            ws.Column(10).Width = 6;         //J
            ws.Column(11).Width = 6;         //K
            ws.Column(12).Width = 3.29;      //L
            ws.Column(13).Width = 6;         //M
            //виставляємо висоту 6 рядочка
            ws.Rows("6").Height = 142;
            
            //робимо шапку таблиці
            ws.Cell("A5").Value = "№ з/п";

            ws.Cell("B5").Value = "Дані про пільговика";
            styler.CenterAndMerge("B5", "E5");
            ws.Cell("B6").Value = "Прізвище, ім'я, по-батькові / Адреса";
            styler.CenterCellText("B6");
            ws.Cell("C6").Value = "К-ть осіб, що отримали пільги";
            styler.Center90Text("C6");
            ws.Cell("D6").Value = "Категорія пільговика / розмір пільги (%)";
            styler.Center90Text("D6");
            ws.Cell("E6").Value = "Ідентифікаційний номер / Номер особового рахунку";
            styler.Center90Text("E6");
            ws.Cell("F5").Value = "Рік / місяць за який проведено нарахування";
            styler.Center90Text("F5");
            ws.Cell("G5").Value = "К-ть днів за які проведено нарахування";
            styler.Center90Text("G5");
            ws.Cell("H5").Value = "Комунальні послуги";
            styler.CenterAndMerge("H5", "L5");
            ws.Cell("H6").Value = "Електропостачання беззоний облік";
            styler.Center90Text("H6");
            ws.Cell("I6").Value = "Електропостачання перша зона";
            styler.Center90Text("I6");
            ws.Cell("J6").Value = "Електропостачання друга зона";
            styler.Center90Text("J6");
            ws.Cell("K6").Value = "Електропостачання третя зона";
            styler.Center90Text("K6");
            ws.Cell("L6").Value = "Освітлення";
            styler.Center90Text("L6");
            ws.Cell("M5").Value = "Разом нараховано";
            styler.Center90Text("M5");

            styler.CenterAndMergeStreamWithOneOffset(new List<string>() {
                "A5", "F5", "G5", "M5"
            }); // Потоково центруємо клітинки і з'єднуємо їх із наступними під ними( А3 з'єднує з A4 і т.д.)

            ws.Cell("A7").Value = "1";
            ws.Cell("A7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B7").Value = "2";
            ws.Cell("B7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C7").Value = "3";
            ws.Cell("C7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D7").Value = "4";
            ws.Cell("D7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E7").Value = "5";
            ws.Cell("E7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F7").Value = "6";
            ws.Cell("F7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G7").Value = "7";
            ws.Cell("G7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H7").Value = "504";
            ws.Cell("H7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I7").Value = "514";
            ws.Cell("I7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("J7").Value = "524";
            ws.Cell("J7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("K7").Value = "532";
            ws.Cell("K7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("L7").Value = "508";
            ws.Cell("L7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("M7").Value = "8";
            ws.Cell("M7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //встановлюємо початок запису даних
            int currRow = 8;        //рядок 8
            int currCell = 1;       //стовпчик 1
            int i = 1;
            decimal debFact = 0;

            foreach (KeyValuePair<string, List<Pilga2>> cat in categories)
            {
                ws.Cell(currRow, currCell).Value = categoryTotals[cat.Key].Code;
                ws.Cell(currRow, currCell + 1).Value = cat.Key;
                styler.SetStreamBold(currRow, currCell, currCell + 1);
                ws.Cell(currRow, currCell + 3).Value = "к-ть пільговиків:";
                ws.Range(ws.Cell(currRow, currCell + 3), ws.Cell(currRow, currCell + 4)).Merge();
                ws.Cell(currRow, currCell + 5).Value = categoryTotals[cat.Key].Count;
                ws.Range(ws.Cell(currRow, currCell + 5), ws.Cell(currRow, currCell + 6)).Merge();
                ws.Cell(currRow, currCell + 7).Value = categoryTotals[cat.Key].WoZoneCount;
                ws.Cell(currRow, currCell + 8).Value = categoryTotals[cat.Key].FirstZoneCount;
                ws.Cell(currRow, currCell + 9).Value = categoryTotals[cat.Key].SecondZoneCount;
                ws.Cell(currRow, currCell + 10).Value = categoryTotals[cat.Key].ThirdZoneCount;
                ws.Cell(currRow, currCell + 11).Value = categoryTotals[cat.Key].Lights;
                ws.Cell(currRow, currCell + 12).Value = categoryTotals[cat.Key].TotalCharged;
                styler.SetStreamBold(currRow, currCell + 5, currCell + 12);
                currCell = 1;   //переводимо на перший стовбець
                currRow++;     //збільшеємо рядок на 1
                foreach (Pilga2 pilga2 in cat.Value)
                {
                    ws.Cell(currRow, currCell).Value = 
                    ws.Cell(currRow, currCell).Value = i;
                    ws.Range(ws.Cell(currRow, currCell), ws.Cell(currRow + 1, currCell)).Merge();
                    styler.CenterRowCell(currRow, currCell);
                    ws.Cell(currRow, currCell + 1).Value = pilga2.FIO;
                    ws.Cell(currRow, currCell + 2).Value = pilga2.LGKOL;
                    ws.Cell(currRow, currCell + 3).Value = pilga2.LGKAT;
                    ws.Cell(currRow, currCell + 4).Value = pilga2.IDCODE;
                    ws.Cell(currRow, currCell + 5).Value = pilga2.DATA1.Year;
                    ws.Cell(currRow, currCell + 6).Value = DateTime.DaysInMonth(pilga2.DATA1.Year, pilga2.DATA1.Month);
                    ws.Range(ws.Cell(currRow, currCell + 6), ws.Cell(currRow + 1, currCell + 6)).Merge();
                    styler.CenterRowCell(currRow, currCell + 6);
                    ws.Cell(currRow, currCell + 7).Value = pilga2.woz;
                    ws.Cell(currRow, currCell + 8).Value = pilga2.z1;
                    ws.Cell(currRow, currCell + 9).Value = pilga2.z2;
                    ws.Cell(currRow, currCell + 10).Value = pilga2.z3;
                    ws.Cell(currRow, currCell + 11).Value = pilga2.z4;
                    ws.Cell(currRow, currCell + 12).FormulaR1C1 = $"=SUM(R{currRow}C{currCell + 7}:R{currRow}C{currCell + 11})";

                    currRow++;     //збільшеємо рядок на 1
                    ws.Cell(currRow, currCell + 1).Value = pilga2.NasPunkt.Trim() + ", " + pilga2.VulName.Trim() + ", " + 
                                                           pilga2.Bild.Trim() + pilga2.Korp.Trim() + "/" + pilga2.Apartment.Trim();
                    ws.Range(ws.Cell(currRow, currCell + 1), ws.Cell(currRow, currCell + 2)).Merge();
                    ws.Cell(currRow, currCell + 3).Value = pilga2.LGPRC;
                    ws.Cell(currRow, currCell + 4).Value = pilga2.RS;
                    ws.Cell(currRow, currCell + 5).Value = pilga2.DATA1.Month;
                    ws.Cell(currRow, currCell + 7).Value = pilga2.wozKwt;
                    ws.Cell(currRow, currCell + 8).Value = pilga2.z1Kwt;
                    ws.Cell(currRow, currCell + 9).Value = pilga2.z2Kwt;
                    ws.Cell(currRow, currCell + 10).Value = pilga2.z3Kwt;
                    ws.Cell(currRow, currCell + 11).Value = pilga2.z4Kwt;
                    ws.Cell(currRow, currCell + 12).FormulaR1C1 = $"=SUM(R{currRow}C{currCell + 7}:R{currRow}C{currCell + 11})";

                    debFact = debFact + pilga2.woz + pilga2.z1 + pilga2.z2 + pilga2.z3 + pilga2.z4;
                    currRow++;     //збільшеємо рядок на 1
                    i++;
                    currCell = 1;   //переводимо на перший стовбець
                }
            }


            ws.Cell(currRow, currCell + 1).Value = "Всього:";
            ws.Cell(currRow, currCell + 6).Value = i-1;
            ws.Cell(currRow, currCell + 11).Value = debFact;
            ws.Range(ws.Cell(currRow, currCell+11), ws.Cell(currRow, currCell+12)).Merge();
            styler.SetStreamBold(currRow, currCell + 1, currCell + 12);

            styler.SetBorder("A5:M" + (currRow - 1), left: false);
        }

        public byte[] CreateFile()
        {
            using MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            byte[] content = stream.ToArray();
            return content;
        }
    }
}
