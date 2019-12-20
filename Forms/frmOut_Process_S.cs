﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using cf01.Reports;
using System.Data.SqlClient;
using cf01.CLS;
using cf01.ModuleClass;
using DevExpress.XtraEditors;
using System.IO;
using System.Threading;
using DevExpress.XtraReports.UI;

namespace cf01.Forms
{
    public partial class frmOut_Process_S : Form
    {
       private clsPublicOfGEO clsConErp = new clsPublicOfGEO();
       private DataTable dtPlate = new DataTable();
       private DataTable dtMo_Data = new DataTable();
        public frmOut_Process_S()
        {
            InitializeComponent();
        }

        private void frmOut_Process_S_Load(object sender, EventArgs e)
        {
            const string strSQL = @"SELECT id,id+' ['+ name +']' AS cdesc FROM it_vendor 
                WHERE within_code ='0000' AND state ='1' AND type='OP'
                ORDER BY id";
            DataTable dtVendor = clsConErp.GetDataTable(strSQL);
            //DataRow dr0 = dtVendor.NewRow(); //插一空行        
            //dtVendor.Rows.InsertAt(dr0, 0);
            for (int i = 0; i < dtVendor.Rows.Count; i++)
            {
                cboVendor_id1.Items.Add(dtVendor.Rows[i]["id"].ToString());
                cboVendor_id2.Items.Add(dtVendor.Rows[i]["id"].ToString());
            }
            //txtVendor_id1.Properties.DataSource = dtVendor;
            //txtVendor_id1.Properties.ValueMember = "id";
            //txtVendor_id1.Properties.DisplayMember = "cdesc";

            //txtVendor_id2.Properties.DataSource = dtVendor;
            //txtVendor_id2.Properties.ValueMember = "id";
            //txtVendor_id2.Properties.DisplayMember = "cdesc";

            //txtVendor.Properties.DataSource = dtVendor;
            //txtVendor.Properties.ValueMember = "id";
            //txtVendor.Properties.DisplayMember = "cdesc";

            //txtIssue_date.EditValue = DateTime.Now;
           
            
            //txtIssue_date.EditValue = DateTime.Now.AddDays(-1);            
            
        }
     
        //private void BTNFIND_Click(object sender, EventArgs e)
        //{
        //    txtVendor_id2.Focus();
        //    if (txtDat1.Text == "" && txtDat2.Text == "" && txtID1.Text == "" && txtID2.Text == "" && txtVendor_id1.Text == "" && txtVendor_id2.Text == "")
        //    {
        //        MessageBox.Show("查詢條件不可爲空!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }
        //    LoadData();
        //}

        private void LoadData()
        {
            string strDat1 = txtDat1.Text;
            string strDat2 = txtDat2.Text;
            //if (strDat1 != "" && strDat2 != "")
            //{
            //    if (strDat1 == strDat2)
            //    {                    
            //        strDat2 = txtDat2.DateTime.AddDays(1).ToString();
            //        strDat2 = strDat2.Substring(0, 10);
            //    }
            //}

            SqlParameter[] paras = new SqlParameter[]
            {       
                    new SqlParameter("@within_code", "0000"),
                    new SqlParameter("@order_date", strDat1),
                    new SqlParameter("@order_date_end", strDat2),
                    new SqlParameter("@id", txtID1.Text),
                    new SqlParameter("@id_end", txtID2.Text),
                    new SqlParameter("@vendor_id", cboVendor_id1.Text),
                    new SqlParameter("@vendor_id_end", cboVendor_id2.Text)
			};
            dtPlate = clsConErp.ExecuteProcedureReturnTable("z_rpt_out_process_out_s", paras);            
            //--
            if (dtPlate.Rows.Count == 0)
            {
                MessageBox.Show("沒有滿足查詢條件的數據!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ////加載報表
                //xrOut_process_out_s mMyRepot = new xrOut_process_out_s() { DataSource = dtPlate };
                //mMyRepot.CreateDocument();
                //mMyRepot.PrintingSystem.ShowMarginsWarning = false;
                //mMyRepot.ShowPreviewDialog();
            }
            dgvDetails.DataSource = dtPlate;
        }

        private void BTNEXIT_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BTNCANCEL_Click(object sender, EventArgs e)
        {
            SetObjValue.ClearObjValue(panel1.Controls, "1");
            dtPlate.Clear();
        }

        private void txtDat1_Leave(object sender, EventArgs e)
        {
            string strDate = txtDat1.Text;
            if (string.IsNullOrEmpty(strDate))
            {
                return;
            }

            if (CheckDate(sender))
            {
                txtDat2.EditValue = txtDat1.EditValue;               
            }
        }


        private static bool CheckDate(object obj)
        {
            string strdate = ((DateEdit)obj).Text;
            bool Flag = clsValidRule.CheckDateFormat(strdate);
            if (!Flag)
            {              
                MessageBox.Show("輸入的日期有誤!", "System Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ((DateEdit)obj).Focus();
                ((DateEdit)obj).SelectAll();
            }
            return Flag;
        }

        private void txtDat2_Leave(object sender, EventArgs e)
        {
            string strDate = txtDat2.Text;
            if (!string.IsNullOrEmpty(strDate))
            {
                CheckDate(sender);                 
            }
        }

        private void txtID1_Leave(object sender, EventArgs e)
        {
            txtID2.Text = txtID1.Text;
        }

        private void cboVendor_id1_Leave(object sender, EventArgs e)
        {
            cboVendor_id2.Text  = cboVendor_id1.Text;
        }

        private void BTNPRINT_Click(object sender, EventArgs e)
        {
            if (dgvDetails.RowCount == 0)
            {
                MessageBox.Show("無需要列印的數據!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                //加載報表
                using (xrOut_process_out_s mMyRepot = new xrOut_process_out_s() { DataSource = dtPlate })
                {
                    mMyRepot.CreateDocument();
                    mMyRepot.PrintingSystem.ShowMarginsWarning = false;
                    mMyRepot.ShowPreviewDialog();
                }
            }
        }

        private void dgvDetails_DoubleClick(object sender, EventArgs e)
        {
            if (dtMo_Data.Rows.Count == 0)
            {
                return;
            }

            SetObjValue.ClearObjValue(panel1.Controls, "1");
            dtPlate.Clear();

            txtID1.Text = dgvDetails.CurrentRow.Cells["id"].Value.ToString();
            txtID2.Text = txtID1.Text;   
        }

        private void dgvDetails_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDetails.CurrentRow == null)
            {
                return;
            }
            string art_file = dgvDetails.CurrentRow.Cells["picture_name"].Value.ToString();
            if (File.Exists(art_file))
            {
                picArt.Image = Image.FromFile(art_file);
            }
            else
                picArt.Image = null;
        }

        private void frmOut_Process_S_FormClosed(object sender, FormClosedEventArgs e)
        {
            clsConErp = null;
            dtPlate = null;
            dtMo_Data = null;
        }

        private void BTNFIND_Click(object sender, EventArgs e)
        {
            cboVendor_id2.Focus();
            if (txtDat1.Text == "" && txtDat2.Text == "" && txtID1.Text == "" && txtID2.Text == "" && cboVendor_id1.Text == "" && cboVendor_id2.Text == "")
            {
                MessageBox.Show("查詢條件不可爲空!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //是示查詢進度
            frmProgress wForm = new frmProgress();
            new Thread((ThreadStart)delegate
            {
                wForm.TopMost = true;
                wForm.ShowDialog();
            }).Start();

            LoadData();

            wForm.Invoke((EventHandler)delegate { wForm.Close(); });
        }

        private void dgvDetails_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //產生行號
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                dgvDetails.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dgvDetails.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dgvDetails.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void BTNEXCEL_Click(object sender, EventArgs e)
        {
            if (dgvDetails.RowCount > 0)
            {
                ExpToExcel objExcel = new ExpToExcel();
                objExcel.ExportExcel(dgvDetails);
            }
            else
                MessageBox.Show("請首先查詢出需要匯出的數據！","提示信息");

        }   

     
    }
}
