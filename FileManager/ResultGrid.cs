using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FileManager
{
    public partial class ResultGrid : Form
    {
        public ResultGrid(List<FileCount> list)
        {
            InitializeComponent();

            this.MaximumSize = new Size(this.Size.Width, (Screen.PrimaryScreen.Bounds.Height-50));
            this.fileGrid.GridColor = Color.Gray;
            this.fileGrid.BorderStyle = BorderStyle.Fixed3D;
            this.fileGrid.CellBorderStyle =
                DataGridViewCellBorderStyle.Single;
            this.fileGrid.RowHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.Single;
            this.fileGrid.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.Single;
            fileGrid.MultiSelect = false;
            fileGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            fileGrid.AllowUserToResizeColumns = false;
            fileGrid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            fileGrid.AllowUserToResizeRows = false;
            fileGrid.RowHeadersWidthSizeMode =
            DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.fileGrid.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            this.fileGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            fileGrid.DataSource = list;
            
        }


        private string GetExcelName()
        {
            DateTime dt = DateTime.Now;
            return "FileCount" + "_" + dt.Year + dt.Month + dt.Day + dt.Hour + dt.Minute + dt.Second + ".csv";
        }

        private void bExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.csv)|*.csv";
            sfd.FileName = GetExcelName();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(fileGrid, sfd.FileName); // Here dataGridview1 is your grid view name 
            }  

            
        }

        private void bPrint_Click(object sender, EventArgs e)
        {
            var clsPrint = new ClsPrint(fileGrid, "");
            clsPrint.PrintForm();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void ToCsV(DataGridView dGV, string filename)
        {
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

            int cols;
            //open file 
            StreamWriter wr = new StreamWriter(filename, false, Encoding.Default);

            //determine the number of columns and write columns to file 
            cols = dGV.Columns.Count;
            for (int i = 0; i < cols; i++)
            {
                wr.Write(dGV.Columns[i].HeaderText + ",");
            }
            wr.WriteLine();

            //write rows to excel file
            for (int i = 0; i < (dGV.Rows.Count); i++)
            {
                for (int j = 0; j < cols; j++)
                {
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

            //close file
            wr.Close();
        }

        private void ResultGrid_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                               Color.WhiteSmoke,
                                                               Color.SteelBlue,
                                                               90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void ResultGrid_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

       


    }
}
