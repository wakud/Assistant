using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Assistant_TEP.MyClasses
{
    public class ExcelStyling
    {
        private readonly IXLWorksheet ws;

        public ExcelStyling(IXLWorksheet ws)
        {
            this.ws = ws;
        }

        //проставляємо жирний шрифт від стартової клітинки maxOffset клітинок підряд
        public void SetStreamBold(int row, int startCell, int maxOffset)
        {
            for (int cell = startCell; cell <= startCell + maxOffset; cell++)
            {
                _ = ws.Cell(row, cell).Style.Font.SetBold();
            }
        }

        // Ставимо рамку в ренджі(формат A1:A2)
        // top, bottom, left, right - по замовчуванню активуються, якщо передати іменований параметр можна якийсь з них виключити
        // Передається тип рамки, по замовчуванню тонка: Thin, також переписується як іменований параметр 
        public void SetBorder(
            string range, bool top = true,
            bool right = true, bool bottom = true,
            bool left = true, XLBorderStyleValues type = XLBorderStyleValues.Thin
        )
        {
            IXLRange rngTable = ws.Range(range);
            if (top)
            {
                rngTable.Style.Border.TopBorder = type;
            }

            if (bottom)
            {
                rngTable.Style.Border.BottomBorder = type;
            }

            if (right)
            {
                rngTable.Style.Border.RightBorder = type;
            }

            if (left)
            {
                rngTable.Style.Border.LeftBorder = type;
            }
        }

        // Виставлення жирного шрифта потоком для декількох клітинок, вказується початкова клітинка і масив з відхиленнями
        // Наприклад: стартова 3, выдхилення: 0, 6, 7; Означає що жирний шрифт накладеться на клітинки 3(3 + 0), 9(3 + 6), 10(3 + 7)
        public void SetStreamBold(int row, int startCell, int[] cellOffsets)
        {
            foreach (int cellOffset in cellOffsets)
            {
                _ = ws.Cell(row, startCell + cellOffset).Style.Font.SetBold();
            }
        }

        // Центрування вказаної клітинки
        public void CenterCellText(string cell)
        {
            ws.Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        // надпис 90 градусів
        public void Center90Text(string cell)
        {
            ws.Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(cell).Style.Alignment.TextRotation = 90;
            ws.Cell(cell).Style.Alignment.WrapText = true;
        }

        // Центрування клітинки по номеру і рядку, приймає параметр wrap для переносу тексту, по замовчуванню true
        // Також є параметр для горизонтального центрування, по замовчуванню центрує - true
        public void CenterRowCell(int row, int cell, bool wrap = true, bool centerHor = true)
        {
            if (centerHor)
            {
                ws.Cell(row, cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            ws.Cell(row, cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            if (wrap)
            {
                ws.Cell(row, cell).Style.Alignment.WrapText = true;
            }
        }

        //Потоково центрує клітинки і врапає(згідно параметру), приймає стартову клітинку і максимальне відхилення,
        //Приймає параметр з відхиленнями які не потрібно горизонтально центрувати
        public void CenterRowCellStramRange(int row, int startCell, int maxOffset, int[]? notCenterHorizontalOffsets, bool wrap = true)
        {
            for (int cell = startCell; cell <= startCell + maxOffset; cell++)
            {
                if (notCenterHorizontalOffsets == null || !notCenterHorizontalOffsets.Contains(cell - startCell))
                {
                    CenterRowCell(row, cell, wrap);
                }
                else
                {
                    CenterRowCell(row, cell, wrap, centerHor: false);
                }
            }
        }

        // Об'єднати по ренджу(A1:A3)
        public void MergeRange(string range)
        {
            _ = ws.Range(range).Merge();
        }

        // Центрувати і об'єднати клітинку. Центрує вибрану клітинку, і об'єднує з вказаною у другому параметрі
        public void CenterAndMerge(string cell, string mergeTo)
        {
            CenterCellText(cell);
            MergeRange(string.Format("{0}:{1}", cell, mergeTo));
        }

        // Центрувати і об'єднати потоком з відхиленням на 1 вниз. Передається ліст із клітинками, які потрібно центрувати та об'єднати із нижніми
        public void CenterAndMergeStreamWithOneOffset(List<string> cells)
        {
            foreach (string cell in cells)
            {
                char num = cell[1];
                int numInt = int.Parse(num.ToString()) + 1;
                char nextCharForMerge = cell[0];
                string mergeTo = string.Format("{0}{1}", nextCharForMerge.ToString(), numInt.ToString());
                CenterAndMerge(cell, mergeTo);
            }
        }
    }
}
