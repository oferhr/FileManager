using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FileManager
{
    /// <summary>
    /// Results display form showing file count information in a formatted grid.
    /// Provides export to CSV and print capabilities.
    /// </summary>
    public partial class ResultGrid : Form
    {
        /// <summary>
        /// Initializes a new instance of the ResultGrid form.
        /// </summary>
        /// <param name="list">List of FileCount objects to display in the grid.</param>
        public ResultGrid(List<FileCount> list)
        {
            InitializeComponent();

            // Set maximum height to fit screen (leaving space for taskbar)
            this.MaximumSize = new Size(this.Size.Width, (Screen.PrimaryScreen.Bounds.Height-50));

            // Configure grid appearance
            this.fileGrid.GridColor = Color.Gray;
            this.fileGrid.BorderStyle = BorderStyle.Fixed3D;
            this.fileGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            this.fileGrid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            this.fileGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // Disable user resizing and multi-selection
            fileGrid.MultiSelect = false;
            fileGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            fileGrid.AllowUserToResizeColumns = false;
            fileGrid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            fileGrid.AllowUserToResizeRows = false;
            fileGrid.RowHeadersWidthSizeMode =
                DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // Set alternating row colors for better readability
            this.fileGrid.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            this.fileGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // Bind data to grid
            fileGrid.DataSource = list;
        }

        /// <summary>
        /// Gets or sets the maximum size of the form.
        /// Sealed to prevent derived classes from changing the sizing behavior.
        /// </summary>
        public sealed override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = value; }
        }


        /// <summary>
        /// Generates a unique filename for CSV export based on current timestamp.
        /// </summary>
        /// <returns>Filename in format "FileCount_YYYYMMDDHHmmss.csv"</returns>
        private string GetExcelName()
        {
            DateTime dt = DateTime.Now;
            return "FileCount" + "_" + dt.Year + dt.Month + dt.Day + dt.Hour + dt.Minute + dt.Second + ".csv";
        }

        /// <summary>
        /// Event handler for the Export button.
        /// Prompts user to save grid data as a CSV file.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private void bExport_Click(object sender, EventArgs e)
        {
            // Configure save file dialog
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Documents (*.csv)|*.csv",
                FileName = GetExcelName()
            };

            // If user confirms, export the grid to CSV
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ToCsV(fileGrid, sfd.FileName);
            }
        }

        /// <summary>
        /// Event handler for the Print button.
        /// Opens print dialog to print the grid contents.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private void bPrint_Click(object sender, EventArgs e)
        {
            // Create print handler and display print dialog
            var clsPrint = new ClsPrint(fileGrid, "");
            clsPrint.PrintForm();
        }

        /// <summary>
        /// Event handler for the Close button.
        /// Closes the results grid form.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Exports a DataGridView to a CSV (Comma-Separated Values) file.
        /// </summary>
        /// <param name="dGV">The DataGridView to export.</param>
        /// <param name="filename">The full path where the CSV file will be saved.</param>
        /// <remarks>
        /// The method writes column headers followed by all data rows.
        /// Uses default encoding which supports Hebrew characters.
        /// Previous implementation using Hebrew encoding (1255) is commented out.
        /// </remarks>
        private void ToCsV(DataGridView dGV, string filename)
        {
            /* Previous implementation using Hebrew encoding - kept for reference
            //string stOutput = "";
            //// Export titles:
            //string sHeaders = "";

            //for (int j = 0; j < dGV.Columns.Count; j++)
            //    sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            //stOutput += sHeaders + "\r\n";
            //// Export data.
            //for (int i = 0; i < dGV.RowCount - 1; i++)
            //{
            //    string stLine = "";
            //    for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
            //        stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
            //    stOutput += stLine + "\r\n";
            //}
            //Encoding utf16 = Encoding.GetEncoding(1255);
            //byte[] output = utf16.GetBytes(stOutput);
            //FileStream fs = new FileStream(filename, FileMode.Create);
            //BinaryWriter bw = new BinaryWriter(fs);
            //bw.Write(output, 0, output.Length); //write the encoded file
            //bw.Flush();
            //bw.Close();
            //fs.Close();
            */

            int cols;

            // Open file for writing with default encoding
            StreamWriter wr = new StreamWriter(filename, false, Encoding.Default);

            // Write column headers
            cols = dGV.Columns.Count;
            for (int i = 0; i < cols; i++)
            {
                wr.Write(dGV.Columns[i].HeaderText + ",");
            }
            wr.WriteLine();

            // Write data rows
            for (int i = 0; i < (dGV.Rows.Count); i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // Write cell value or empty string if null
                    if (dGV.Rows[i].Cells[j].Value != null)
                    {
                        wr.Write(dGV.Rows[i].Cells[j].Value + ",");
                    }
                    else
                    {
                        wr.Write(",");
                    }
                }
                wr.WriteLine();
            }

            // Close and flush the file
            wr.Close();
        }

        /// <summary>
        /// Paints the form background with a gradient effect.
        /// </summary>
        /// <param name="sender">The form being painted.</param>
        /// <param name="e">Paint event arguments containing the graphics context.</param>
        private void ResultGrid_Paint(object sender, PaintEventArgs e)
        {
            // Skip painting if form has no width (minimized or hidden)
            if (this.ClientRectangle.Width == 0)
            {
                return;
            }

            // Create and apply vertical gradient from WhiteSmoke to SteelBlue
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                               Color.WhiteSmoke,
                                                               Color.SteelBlue,
                                                               90F)) // 90 degrees = vertical gradient
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Event handler for form resize.
        /// Invalidates the form to trigger a repaint with the new size.
        /// </summary>
        /// <param name="sender">The form being resized.</param>
        /// <param name="e">Event arguments.</param>
        private void ResultGrid_Resize(object sender, EventArgs e)
        {
            // Force repaint to update gradient background
            this.Invalidate();
        }

       


    }
}
