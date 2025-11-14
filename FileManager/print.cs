using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace FileManager
{
    /// <summary>
    /// Handles printing operations for DataGridView controls.
    /// Provides functionality to print grid data with proper formatting, pagination, and RTL (Right-to-Left) support.
    /// </summary>
    class ClsPrint
    {
        #region Variables

        /// <summary>
        /// Height of each data grid cell in the printed output.
        /// </summary>
        int iCellHeight = 0;

        /// <summary>
        /// Total width of all columns combined.
        /// Used for calculating column proportions in the printed output.
        /// </summary>
        int iTotalWidth = 0;

        /// <summary>
        /// Current row counter tracking which row is being printed.
        /// </summary>
        int iRow = 0;

        /// <summary>
        /// Flag indicating whether the first page is being printed.
        /// Used to initialize column widths and positions.
        /// </summary>
        bool bFirstPage = false;

        /// <summary>
        /// Flag indicating whether a new page is being started.
        /// Triggers header rendering on new pages.
        /// </summary>
        bool bNewPage = false;

        /// <summary>
        /// Height of the column header section.
        /// </summary>
        int iHeaderHeight = 0;

        /// <summary>
        /// String format settings for aligning and formatting grid cell text.
        /// </summary>
        StringFormat strFormat;

        /// <summary>
        /// Array storing the left edge X-coordinate for each column.
        /// Used for positioning columns during printing.
        /// </summary>
        ArrayList arrColumnLefts = new ArrayList();

        /// <summary>
        /// Array storing the width of each column.
        /// Calculated proportionally based on available print space.
        /// </summary>
        ArrayList arrColumnWidths = new ArrayList();

        /// <summary>
        /// The print document object that handles the actual printing.
        /// </summary>
        private PrintDocument _printDocument = new PrintDocument();

        /// <summary>
        /// Reference to the DataGridView being printed.
        /// </summary>
        private DataGridView gw = new DataGridView();

        /// <summary>
        /// Header text displayed at the top of each printed page.
        /// </summary>
        private string _ReportHeader;

        #endregion

        /// <summary>
        /// Initializes a new instance of the ClsPrint class.
        /// </summary>
        /// <param name="gridview">The DataGridView control to print.</param>
        /// <param name="reportHeader">Header text to display at the top of each page.</param>
        public ClsPrint(DataGridView gridview, string reportHeader)
        {
            // Subscribe to print events
            _printDocument.PrintPage += new PrintPageEventHandler(_printDocument_PrintPage);
            _printDocument.BeginPrint += new PrintEventHandler(_printDocument_BeginPrint);

            // Store references to the grid and header
            gw = gridview;
            _ReportHeader = reportHeader;
        }

        /// <summary>
        /// Displays the print dialog and initiates printing if the user confirms.
        /// </summary>
        public void PrintForm()
        {
            // Open the print dialog with enhanced options
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = _printDocument;
            printDialog.UseEXDialog = true; // Use the extended print dialog

            // If user clicks OK, proceed with printing
            if (DialogResult.OK == printDialog.ShowDialog())
            {
                _printDocument.DocumentName = "Test Page Print";
                _printDocument.Print();
            }

            // Alternative: Open the print preview dialog (currently commented out)
            // Uncomment below to show preview instead of direct printing
            //PrintPreviewDialog objPPdialog = new PrintPreviewDialog();
            //objPPdialog.Document = _printDocument;
            //objPPdialog.ShowDialog();
        }

        /// <summary>
        /// Event handler for printing each page.
        /// Handles layout, formatting, and rendering of grid data on the print page.
        /// Supports pagination and RTL (Right-to-Left) text rendering.
        /// </summary>
        /// <param name="sender">The print document object.</param>
        /// <param name="e">Print page event arguments containing graphics context and page bounds.</param>
        private void _printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Initialize page margins
            int iLeftMargin = e.MarginBounds.Left;
            int iTopMargin = e.MarginBounds.Top;

            // Flag to track if more pages need to be printed
            bool bMorePagesToPrint = false;
            int iTmpWidth = 0;

            // On the first page, calculate and cache column widths and positions
            if (bFirstPage)
            {
                foreach (DataGridViewColumn GridCol in gw.Columns)
                {
                    // Calculate proportional column width based on available print area
                    // Formula: (ColumnWidth / TotalWidth) * PrintableWidth
                    iTmpWidth = (int)(Math.Floor((double)((double)GridCol.Width /
                        (double)iTotalWidth * (double)iTotalWidth *
                        ((double)e.MarginBounds.Width / (double)iTotalWidth))));

                    // Measure header height based on text and font
                    iHeaderHeight = (int)(e.Graphics.MeasureString(GridCol.HeaderText,
                        GridCol.InheritedStyle.Font, iTmpWidth).Height) + 11;

                    // Cache column positions and widths for subsequent rows
                    arrColumnLefts.Add(iLeftMargin);
                    arrColumnWidths.Add(iTmpWidth);
                    iLeftMargin += iTmpWidth;
                }
            }
            // Loop through all grid rows and render them
            while (iRow <= gw.Rows.Count - 1)
            {
                DataGridViewRow GridRow = gw.Rows[iRow];

                // Calculate cell height with padding
                iCellHeight = GridRow.Height + 5;
                int iCount = 0;

                // Check if current row fits on the page, or if we need a page break
                if (iTopMargin + iCellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                {
                    // Row doesn't fit - trigger new page
                    bNewPage = true;
                    bFirstPage = false;
                    bMorePagesToPrint = true;
                    break;
                }
                else
                {
                    // Row fits on current page
                    if (bNewPage)
                    {
                        // Draw report header at top of new page
                        e.Graphics.DrawString(_ReportHeader,
                            new Font(gw.Font, FontStyle.Bold),
                            Brushes.Black, e.MarginBounds.Left,
                            e.MarginBounds.Top - e.Graphics.MeasureString(_ReportHeader,
                            new Font(gw.Font, FontStyle.Bold),
                            e.MarginBounds.Width).Height - 13);

                        // Draw date (currently empty string - placeholder for future use)
                        String strDate = "";
                        e.Graphics.DrawString(strDate,
                            new Font(gw.Font, FontStyle.Bold), Brushes.Black,
                            e.MarginBounds.Left +
                            (e.MarginBounds.Width - e.Graphics.MeasureString(strDate,
                            new Font(gw.Font, FontStyle.Bold),
                            e.MarginBounds.Width).Width),
                            e.MarginBounds.Top - e.Graphics.MeasureString(_ReportHeader,
                            new Font(new Font(gw.Font, FontStyle.Bold),
                            FontStyle.Bold), e.MarginBounds.Width).Height - 13);

                        // Draw column headers
                        iTopMargin = e.MarginBounds.Top;
                        DataGridViewColumn[] _GridCol = new DataGridViewColumn[gw.Columns.Count];
                        int colcount = 0;

                        // Convert LTR (Left-to-Right) to RTL (Right-to-Left) for Hebrew support
                        foreach (DataGridViewColumn GridCol in gw.Columns)
                        {
                            _GridCol[colcount++] = GridCol;
                        }

                        // Render headers in reverse order (RTL)
                        for (int i = (_GridCol.Count() - 1); i >= 0; i--)
                        {
                            // Fill header background with light gray
                            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                                new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                (int)arrColumnWidths[iCount], iHeaderHeight));

                            // Draw header border
                            e.Graphics.DrawRectangle(Pens.Black,
                                new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                (int)arrColumnWidths[iCount], iHeaderHeight));

                            // Draw header text
                            e.Graphics.DrawString(_GridCol[i].HeaderText,
                                _GridCol[i].InheritedStyle.Font,
                                new SolidBrush(_GridCol[i].InheritedStyle.ForeColor),
                                new RectangleF((int)arrColumnLefts[iCount], iTopMargin,
                                (int)arrColumnWidths[iCount], iHeaderHeight), strFormat);
                            iCount++;
                        }
                        bNewPage = false;
                        iTopMargin += iHeaderHeight;
                    }

                    // Draw row cells
                    iCount = 0;
                    DataGridViewCell[] _GridCell = new DataGridViewCell[GridRow.Cells.Count];
                    int cellcount = 0;

                    // Convert LTR to RTL for cell rendering
                    foreach (DataGridViewCell Cel in GridRow.Cells)
                    {
                        _GridCell[cellcount++] = Cel;
                    }

                    // Render cells in reverse order (RTL)
                    for (int i = (_GridCell.Count() - 1); i >= 0; i--)
                    {
                        // Draw cell content if not null
                        if (_GridCell[i].Value != null)
                        {
                            e.Graphics.DrawString(_GridCell[i].FormattedValue.ToString(),
                                _GridCell[i].InheritedStyle.Font,
                                new SolidBrush(_GridCell[i].InheritedStyle.ForeColor),
                                new RectangleF((int)arrColumnLefts[iCount],
                                (float)iTopMargin,
                                (int)arrColumnWidths[iCount], (float)iCellHeight),
                                strFormat);
                        }

                        // Draw cell border
                        e.Graphics.DrawRectangle(Pens.Black,
                            new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                            (int)arrColumnWidths[iCount], iCellHeight));
                        iCount++;
                    }
                }

                // Move to next row
                iRow++;
                iTopMargin += iCellHeight;
            }

            // Set pagination flag - true if more pages needed, false if printing is complete
            if (bMorePagesToPrint)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
        }

        /// <summary>
        /// Event handler called when printing begins.
        /// Initializes printing variables and calculates total column widths.
        /// </summary>
        /// <param name="sender">The print document object.</param>
        /// <param name="e">Print event arguments.</param>
        private void _printDocument_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                // Initialize string format for centered, ellipsis-trimmed text
                strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Center;          // Center text horizontally
                strFormat.LineAlignment = StringAlignment.Center;      // Center text vertically
                strFormat.Trimming = StringTrimming.EllipsisCharacter; // Add "..." for overflow

                // Clear cached column layout data
                arrColumnLefts.Clear();
                arrColumnWidths.Clear();

                // Reset printing state variables
                iCellHeight = 0;
                iRow = 0;           // Start from first row
                bFirstPage = true;  // Mark as first page for initial setup
                bNewPage = true;    // Trigger header rendering

                // Calculate total width of all columns for proportional scaling
                iTotalWidth = 0;
                foreach (DataGridViewColumn dgvGridCol in gw.Columns)
                {
                    iTotalWidth += dgvGridCol.Width;
                }
            }
            catch (Exception ex)
            {
                // Display error message if initialization fails
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
