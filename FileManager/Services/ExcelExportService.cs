using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace FileManager.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public void ExportToExcel(DataGridView[] grids, CheckedListBox[] listBoxes, string[] sheetNames)
        {
            var xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = null;

            try
            {
                xlWorkbook = xlApp.Workbooks.Add();
                xlApp.Visible = false;
                string filePath = null;

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.FileName = $"FileManager-Export-{DateTime.Now:yyyy-MM-dd}.xlsx";
                    saveFileDialog.Title = "שמור קובץ אקסל";

                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        xlWorkbook.Close(false);
                        xlApp.Quit();
                        return;
                    }

                    filePath = saveFileDialog.FileName;
                }

                // Export each grid/control to a separate worksheet
                for (int i = 0; i < grids.Length; i++)
                {
                    ExportDataGridViewToExcel(xlWorkbook, grids[i], sheetNames[i]);
                }

                for (int i = 0; i < listBoxes.Length; i++)
                {
                    ExportCheckedListBoxToExcel(xlWorkbook, listBoxes[i], sheetNames[grids.Length + i]);
                }

                // Save the workbook
                xlWorkbook.SaveAs(filePath);
                xlWorkbook.Close(true);
                xlApp.Quit();

                MessageBox.Show($"הקובץ נשמר בהצלחה: {filePath}", "ייצוא לאקסל", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בייצוא לאקסל: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Clean up COM objects
                if (xlWorkbook != null)
                {
                    Marshal.ReleaseComObject(xlWorkbook);
                    xlWorkbook = null;
                }
                if (xlApp != null)
                {
                    Marshal.ReleaseComObject(xlApp);
                    xlApp = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ExportDataGridViewToExcel(Excel.Workbook workbook, DataGridView grid, string sheetName)
        {
            try
            {
                // Create new worksheet
                Excel._Worksheet worksheet = (Excel._Worksheet)workbook.Sheets.Add();
                worksheet.Name = sheetName;

                // Export headers
                for (int col = 0; col < grid.Columns.Count; col++)
                {
                    if (grid.Columns[col].Visible)
                    {
                        worksheet.Cells[1, col + 1] = grid.Columns[col].HeaderText;
                        Excel.Range headerCell = worksheet.Cells[1, col + 1] as Excel.Range;
                        if (headerCell != null)
                        {
                            headerCell.Font.Bold = true;
                        }
                    }
                }

                // Export data
                int excelRow = 2;
                for (int row = 0; row < grid.Rows.Count; row++)
                {
                    if (!grid.Rows[row].IsNewRow)
                    {
                        int excelCol = 1;
                        for (int col = 0; col < grid.Columns.Count; col++)
                        {
                            if (grid.Columns[col].Visible)
                            {
                                var cellValue = grid.Rows[row].Cells[col].Value;
                                if (cellValue != null)
                                {
                                    // Handle checkbox columns
                                    if (grid.Columns[col] is DataGridViewCheckBoxColumn)
                                    {
                                        worksheet.Cells[excelRow, excelCol] = (bool)cellValue ? "✓" : "✗";
                                    }
                                    else
                                    {
                                        worksheet.Cells[excelRow, excelCol] = cellValue.ToString();
                                    }
                                }
                                excelCol++;
                            }
                        }
                        excelRow++;
                    }
                }

                // Auto-fit columns
                Excel.Range usedRange = worksheet.UsedRange;
                usedRange.Columns.AutoFit();

                // Release worksheet
                Marshal.ReleaseComObject(worksheet);
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
        }

        private void ExportCheckedListBoxToExcel(Excel.Workbook workbook, CheckedListBox listBox, string sheetName)
        {
            try
            {
                // Create new worksheet
                Excel._Worksheet worksheet = (Excel._Worksheet)workbook.Sheets.Add();
                worksheet.Name = sheetName;

                // Export headers
                worksheet.Cells[1, 1] = "פריט";
                worksheet.Cells[1, 2] = "נבחר";
                Excel.Range headerCell1 = worksheet.Cells[1, 1] as Excel.Range;
                Excel.Range headerCell2 = worksheet.Cells[1, 2] as Excel.Range;
                if (headerCell1 != null) headerCell1.Font.Bold = true;
                if (headerCell2 != null) headerCell2.Font.Bold = true;

                // Export data
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    worksheet.Cells[i + 2, 1] = listBox.Items[i].ToString();
                    worksheet.Cells[i + 2, 2] = listBox.GetItemChecked(i) ? "✓" : "✗";
                }

                // Auto-fit columns
                Excel.Range usedRange = worksheet.UsedRange;
                usedRange.Columns.AutoFit();

                // Release worksheet
                Marshal.ReleaseComObject(worksheet);
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
        }
    }
}
