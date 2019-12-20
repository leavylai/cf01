﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using cf01.CLS;
using cf01.ModuleClass;
using System.Data.SqlClient;

namespace cf01.Forms
{
    public partial class frmType : Form
    {
        public string mID = "";    //臨時的主鍵值
        public string mGroup = "";
        public string mState = ""; //新增或編號的狀態
        public static string str_language = "0";
        public string msgCustom;
        public int row_delete;
        MsgInfo myMsg = new MsgInfo();
        DataTable dtGroup ;

        public frmType()
        {
            InitializeComponent();
            str_language = DBUtility._language;
            try
            {
                clsControlInfoHelper control = new clsControlInfoHelper(this.Name, this.Controls);
                control.GenerateContorl();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            dgvDetails.RowHeadersVisible = true;  //因類clsControlInfoHelper DataGridView中已屏蔽行標頭,此處重新恢覆回來
            dgvDetails.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect; //因類clsControlInfoHelper的問題，此處重新恢覆整行選中的屬性
        }

        private void BTNEXIT_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BTNNEW_Click(object sender, EventArgs e)
        {
            AddNew();
        }

        private void BTNEDIT_Click(object sender, EventArgs e)
        {
            Edit();
        }

        private void BTNSAVE_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void BTNCANCEL_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void BTNDELETE_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void BTNFIND_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void BTNPRINT_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void BTNEXCEL_Click(object sender, EventArgs e)
        {
            Excel();
        }

        private void frmType_Load(object sender, EventArgs e)
        {
            string sql_group;
            if (str_language == "2")
            {                
                sql_group = @"SELECT gtyp_code as id,gtyp_desc as type_desc FROM dbo.bs_type_group";                
            }
            else
            {
                sql_group = @"SELECT gtyp_code as id,gtyp_cdesc as type_desc FROM dbo.bs_type_group";                
            }           


            //組別 
            dtGroup = clsPublicOfCF01.GetDataTable(sql_group);
            DataRow row2 = dtGroup.NewRow();
            dtGroup.Rows.Add(row2);
            dtGroup.DefaultView.Sort = "id ASC";
            dtGroup = dtGroup.DefaultView.ToTable();
            txtTyp_group.Properties.DataSource = dtGroup;            
            txtTyp_group.Properties.ValueMember = "id";
            txtTyp_group.Properties.DisplayMember = "id";
            

        }

        private void frmType_FormClosed(object sender, FormClosedEventArgs e)
        {
            dtGroup.Dispose();
        }

        // ===============以下爲自定義義方法=======================

        private void SetButtonSatus(bool _flag)
        {
            BTNNEW.Enabled = _flag;
            BTNEDIT.Enabled = _flag;
            BTNPRINT.Enabled = _flag;
            BTNDELETE.Enabled = _flag;
            BTNFIND.Enabled = _flag;
            BTNSAVE.Enabled = !_flag;
            BTNCANCEL.Enabled = !_flag;
        }

        private bool Valid_Doc() //主建是否已存在
        {
            bool flag;
            string doc = txtID.Text.Trim();
            string type_group = txtTyp_group.EditValue.ToString();
            string strSql = String.Format("Select '1' FROM bs_type WHERE typ_group='{0}' AND typ_code='{1}'", type_group, doc);
            DataTable dt = clsPublicOfCF01.GetDataTable(strSql);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show(myMsg.msgIsExists + String.Format("【{0}】", doc), myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                flag = true;
            }
            else
            {
                flag = false;
            }
            dt.Dispose();
            return flag;
        }

        private void Find_doc(string temp_id) //非新增或編號狀態下帶出的資料
        {
            if (!String.IsNullOrEmpty(temp_id))
            {
                string strsql =
                    @"SELECT A.typ_group, A.typ_code as id,isnull(A.typ_cdesc,'') as cdesc,isnull(A.typ_desc,'') as edesc,
                    A.typ_rmk as rmk,A.crusr,A.crtim,A.amusr,A.amtim,B.gtyp_cdesc as type_desc
                    FROM dbo.bs_type A with(nolock),
                         dbo.bs_type_group B with(nolock) 
                    WHERE A.typ_group *=B.gtyp_code AND A.typ_group='";;
                strsql += txtTyp_group.EditValue + "'";
                strsql += " AND A.typ_code='"+ temp_id + "'";
                DataTable dt = clsPublicOfCF01.GetDataTable(strsql);
                dgvDetails.DataSource = dt;
                if (dt.Rows.Count == 0)
                {
                    if (str_language == "2")
                    {
                        msgCustom = "The Code of the data does not exist.";
                    }
                    else
                    {
                        msgCustom = "編號資料不存在！";
                    }
                    MessageBox.Show(msgCustom, myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetObjValue.ClearObjValue(this.Controls, "2");
                    return;
                }
                else
                {
                    mID = txtID.Text;//保存臨時的ID號                    
                }
                Set_Master_Data(dt);
            }
        }

        private void Find() //查詢出所有數據
        {
            mGroup = txtTyp_group.EditValue.ToString();

            dgvDetails.Focus();
            const string strSql =
                    @"SELECT A.typ_group, A.typ_code as id,isnull(A.typ_cdesc,'') as cdesc,isnull(A.typ_desc,'') as edesc,
                    A.typ_rmk as rmk,A.crusr,A.crtim,A.amusr,A.amtim,B.gtyp_cdesc as type_desc
                    FROM dbo.bs_type A with(nolock),
                         dbo.bs_type_group B with(nolock)                         
                    WHERE A.typ_group *=B.gtyp_code";
            DataTable dt = clsPublicOfCF01.GetDataTable(strSql);
            dgvDetails.DataSource = dt;
            mID = "";            
        }

        private bool Save_Before_Valid() //保存前檢查
        {
            if (txtID.Text == "" || txtCdesc.Text == "")
            {
                if (str_language == "2")
                {
                    msgCustom = "Code or Description Data cannot be empty.";
                }
                else
                {
                    msgCustom = "編號或描述資料不可爲空！";
                }
                MessageBox.Show(msgCustom, myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Set_Master_Data(DataTable dtName)
        {
            txtID.Text = dtName.Rows[0]["id"].ToString();
            txtEdesc.Text = dtName.Rows[0]["edesc"].ToString();
            txtCdesc.Text = dtName.Rows[0]["cdesc"].ToString();            

            txtRemark.Text = dtName.Rows[0]["rmk"].ToString();
            txtCrusr.Text = dtName.Rows[0]["crusr"].ToString();
            txtCrtim.Text = dtName.Rows[0]["crtim"].ToString();
            txtAmusr.Text = dtName.Rows[0]["amusr"].ToString();
            txtAmtim.Text = dtName.Rows[0]["amtim"].ToString();

            txtTyp_group.EditValue = dtName.Rows[0]["typ_group"].ToString();
            txtType_desc.Text = dtName.Rows[0]["type_desc"].ToString();
        }

        private void Set_Row_Position(string _doc) //定位到當前行 
        {
            
            Find();            
            for (int i = 0; i < dgvDetails.RowCount; i++)
            {
                if (_doc == dgvDetails.Rows[i].Cells["id"].Value.ToString() &&
                    mGroup == dgvDetails.Rows[i].Cells["typ_group"].Value.ToString())
                {                    
                    dgvDetails.CurrentCell = dgvDetails.Rows[i].Cells["id"]; //設置光標定位到當前選中的行
                    dgvDetails.Rows[i].Selected = true;
                    break;
                }
            }
        }

        private void AddNew()  //新增
        {
            mState = "NEW";
            txtID.Focus();
            SetButtonSatus(false);
            SetObjValue.SetEditBackColor(this.Controls, true);
            SetObjValue.ClearObjValue(this.Controls, "1");
        }

        private void Edit()  //編號
        {
            if (dgvDetails.RowCount == 0)
            {
                return;
            }

            SetButtonSatus(false);
            SetObjValue.SetEditBackColor(this.Controls, true);
            mState = "EDIT";

            txtID.Properties.ReadOnly = true;
            txtID.BackColor = System.Drawing.Color.White;

            txtTyp_group.Properties.ReadOnly = true;
            txtTyp_group.BackColor = System.Drawing.Color.White;           
        }

        private void Cancel() //取消
        {
            mState = "";
            SetButtonSatus(true);
            SetObjValue.SetEditBackColor(this.Controls, false);
            if (!String.IsNullOrEmpty(mID))
            {
                Find_doc(mID);
            }
        }

        private void Save()  //保存新增或修改的資料
        {
            if (!Save_Before_Valid())
            {
                return;
            }

            bool save_flag = false;

            if (mState == "NEW")
            {
                if (Valid_Doc())
                    return;

                const string sql_new = @"INSERT INTO bs_type(typ_group,typ_code,typ_cdesc,typ_desc,typ_rmk,crusr,crtim)" +
                                    " VALUES(@typ_group,@id,@cdesc,@edesc,@remark,@crusr,GETDATE())";
                SqlConnection myCon = new SqlConnection(DBUtility.connectionString);
                myCon.Open();
                SqlTransaction myTrans = myCon.BeginTransaction();
                using (SqlCommand myCommand = new SqlCommand() { Connection = myCon, Transaction = myTrans })
                {
                    try
                    {
                        myCommand.CommandText = sql_new;
                        myCommand.Parameters.Clear();
                        myCommand.Parameters.AddWithValue("@typ_group", txtTyp_group.EditValue.ToString());
                        myCommand.Parameters.AddWithValue("@id", txtID.Text.Trim());
                        myCommand.Parameters.AddWithValue("@cdesc", txtCdesc.Text.Trim());
                        myCommand.Parameters.AddWithValue("@edesc", txtEdesc.Text.Trim());
                        myCommand.Parameters.AddWithValue("@remark", txtRemark.Text.Trim());
                        myCommand.Parameters.AddWithValue("@crusr", DBUtility._user_id);

                        myCommand.ExecuteNonQuery();
                        myTrans.Commit(); //數據提交
                        save_flag = true;
                    }
                    catch (Exception E)
                    {
                        myTrans.Rollback(); //數據回滾
                        save_flag = false;
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        myCon.Close();
                    }
                }
            }

            if (mState == "EDIT")
            {
                const string sql_update = "UPDATE bs_type SET typ_cdesc=@cdesc,typ_desc=@edesc,typ_rmk=@remark,amusr=@amusr,amtim=GETDATE() WHERE typ_group =@typ_group AND typ_code=@id";
                SqlConnection myCon = new SqlConnection(DBUtility.connectionString);
                myCon.Open();
                SqlTransaction myTrans = myCon.BeginTransaction();
                using (SqlCommand myCommand = new SqlCommand() { Connection = myCon, Transaction = myTrans })
                {
                    try
                    {
                        myCommand.CommandText = sql_update;
                        myCommand.Parameters.Clear();
                        myCommand.Parameters.AddWithValue("@typ_group", txtTyp_group.EditValue.ToString());
                        myCommand.Parameters.AddWithValue("@id", txtID.Text.Trim());
                        myCommand.Parameters.AddWithValue("@cdesc", txtCdesc.Text.Trim());
                        myCommand.Parameters.AddWithValue("@edesc", txtEdesc.Text.Trim());
                        myCommand.Parameters.AddWithValue("@remark", txtRemark.Text.Trim());
                        myCommand.Parameters.AddWithValue("@amusr", DBUtility._user_id);

                        myCommand.ExecuteNonQuery();
                        myTrans.Commit(); //數據提交
                        save_flag = true;
                    }
                    catch (Exception E)
                    {
                        myTrans.Rollback(); //數據回滾
                        save_flag = false;
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        myCon.Close();
                    }
                }
            }

            SetButtonSatus(true);
            SetObjValue.SetEditBackColor(this.Controls, false);
            mState = "";

            Set_Row_Position(txtID.Text.Trim());
            if (save_flag)
            {
                MessageBox.Show(myMsg.msgSave_ok, myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(myMsg.msgSave_error, myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void Delete() //刪除當前行
        {
            if (dgvDetails.RowCount == 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show(myMsg.msgIsDelete, myMsg.msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                const string sql_del = "DELETE FROM bs_type WHERE typ_group =@typ_group AND typ_code=@id";
                SqlConnection myCon = new SqlConnection(DBUtility.connectionString);
                myCon.Open();
                SqlTransaction myTrans = myCon.BeginTransaction();
                using (SqlCommand myCommand = new SqlCommand() { Connection = myCon, Transaction = myTrans })
                {
                    try
                    {
                        myCommand.CommandText = sql_del;
                        myCommand.Parameters.Clear();
                        myCommand.Parameters.AddWithValue("@typ_group", txtTyp_group.EditValue.ToString());
                        myCommand.Parameters.AddWithValue("@id", txtID.Text.Trim());
                        myCommand.ExecuteNonQuery();
                        myTrans.Commit(); //數據提交
                        dgvDetails.Rows.Remove(dgvDetails.CurrentRow);//移除當前行
                        MessageBox.Show(myMsg.msgSave_ok, myMsg.msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception E)
                    {
                        myTrans.Rollback(); //數據回滾
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        myCon.Close();
                    }
                }

            }
        }

        private void Print() //通用的列印方法
        {
            if (dgvDetails.RowCount > 0)
            {
                PrintDGV.Print_DataGridView(this.dgvDetails);
            }
        }

        private void Excel() //匯出EXCEL
        {
            if (dgvDetails.RowCount > 0)
            {
                ExpToExcel.DataGridViewToExcel(dgvDetails);
            }
        }


        //===========以下爲控件中的方法================

        private void txtID_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtID.Text))
            {
                if (mState == "")
                {
                    Find_doc(txtID.Text);
                }
            }
        }

        private void dgvDetails_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDetails.RowCount > 0)
            {                
                txtTyp_group.EditValue = dgvDetails.CurrentRow.Cells["typ_group"].Value.ToString();
                txtType_desc.Text = dgvDetails.CurrentRow.Cells["type_desc"].Value.ToString();
                txtID.Text = dgvDetails.CurrentRow.Cells["id"].Value.ToString();
                txtCdesc.Text = dgvDetails.CurrentRow.Cells["cdesc"].Value.ToString();
                txtEdesc.Text = dgvDetails.CurrentRow.Cells["edesc"].Value.ToString();

                txtRemark.Text = dgvDetails.CurrentRow.Cells["rmk"].Value.ToString();
                txtCrusr.Text = dgvDetails.CurrentRow.Cells["crusr"].Value.ToString();
                txtCrtim.Text = dgvDetails.CurrentRow.Cells["crtim"].Value.ToString();
                txtAmusr.Text = dgvDetails.CurrentRow.Cells["amusr"].Value.ToString();
                txtAmtim.Text = dgvDetails.CurrentRow.Cells["amtim"].Value.ToString();
            }
        }     

        private void txtTyp_group_EditValueChanged(object sender, EventArgs e)
        {
            if (txtTyp_group.EditValue.ToString() != "")
            {
                txtType_desc.Text = txtTyp_group.GetColumnValue("type_desc").ToString();
            }
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

    }
}
