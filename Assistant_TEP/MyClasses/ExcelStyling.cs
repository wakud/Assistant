using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Assistant_TEP.MyClasses
{
    /// <summary>
    /// Стилі файлу Excel
    /// </summary>
    public class ExcelStyling
    {
        private readonly IXLWorksheet ws;

        public ExcelStyling(IXLWorksheet ws)
        {
            this.ws = ws;
        }
        //Жирний шрифт рядка комірок
        public void SetStreamBold(int row, int startCell, int maxOffset)
        {
            for (int cell = startCell; cell <= startCell + maxOffset; cell++)
            {
                _ = ws.Cell(row, cell).Style.Font.SetBold();
            }
        }
        //жирний шрифт діапазону комірок
        public void SetStreamBold(int row, int startCell, int[] cellOffsets)
        {
            foreach (int cellOffset in cellOffsets)
            {
                _ = ws.Cell(row, startCell + cellOffset).Style.Font.SetBold();
            }
        }
        //рамка комірки
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
        //центрування тексту в комірці
        public void CenterCellText(string cell)
        {
            ws.Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        //центрування тексту повернутого на 90 градусів
        public void Center90Text(string cell)
        {
            ws.Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(cell).Style.Alignment.TextRotation = 90;
            ws.Cell(cell).Style.Alignment.WrapText = true;
        }
        //центрування тексту в рядку
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
        //центрування тексту у об'єднаних комірках
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
        //об'єднання комірок
        public void MergeRange(string range)
        {
            _ = ws.Range(range).Merge();
        }
        //центрування і об'єднання комірок
        public void CenterAndMerge(string cell, string mergeTo)
        {
            CenterCellText(cell);
            MergeRange(string.Format("{0}:{1}", cell, mergeTo));
        }
        //центрування і об'єднання виділених комірок
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
