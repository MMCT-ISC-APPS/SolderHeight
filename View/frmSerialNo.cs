using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SolidHeight.Models;
using SolidHeight.Controls;

namespace SolidHeight.View
{
    public partial class frmSerialNo : Telerik.WinControls.UI.RadForm
    {
        DataGridView dgv;
        String strJobName, strTotalPCCA  , strFstSerialNo;
        int iTatolPCCA = 0;
        DataTable dt;
        clsSPC objRun = new clsSPC();
        List<clsSerial> SerialList = new List<clsSerial>();
         
        public List<clsSerial> resultSerialList
        {
            get { return SerialList; }
            private set { SerialList = value;  }
        }

        public frmSerialNo(string paramJobName, string paramTotalPCCA, string paramSerialNo)
        {
            try
            {

                InitializeComponent();
                strJobName = paramJobName;
                strTotalPCCA = paramTotalPCCA;
                strFstSerialNo = paramSerialNo;

                UpdateMSG("", 0);
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

        private void frmSerialNo_Load(object sender, EventArgs e)
        {
            try
            {
                txtJobName.Text = strJobName;
                txtTotalPCCA.Text = strTotalPCCA;
                getDataGridview();

                UpdateMSG("", 0);
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

        #region DataGridViews Event
        
        private void getDataGridview()
        {
            try
            {
                iTatolPCCA = Convert.ToInt32(strTotalPCCA);


                iTatolPCCA = iTatolPCCA > 10 ? 10 : iTatolPCCA;



                for (int i = 1; i <= iTatolPCCA; i++)
                {
                    SerialList.Add(new clsSerial
                    {
                        SeqNo = i,
                        strSerialNo = "SerialNo :" + i.ToString(),
                        strSerialValues = string.Empty
                    });
                }
                DataGridViewTextBoxColumn txt1 = new DataGridViewTextBoxColumn();

                DataGridViewColumn firstCol = new DataGridViewColumn();
                dgv = dgvSerialList;
                dgv.DataSource = SerialList;

                firstCol = dgv.Columns[0];
                firstCol.ReadOnly = true;
                firstCol.Visible = false;
                firstCol.Frozen = true;
                dgv.Columns[1].ReadOnly = true;
                txt1.Name = "txtSerials";
                txt1.HeaderText = "SerialNo";
                dgv.Columns[0].ReadOnly = true;
                

                dgv.Rows[0].Cells[2].Value = strFstSerialNo;
                dgv.CurrentCell = dgv.Rows[0].Cells[2].Value.ToString() == string.Empty ? dgv.Rows[0].Cells[2] : dgv.Rows[1].Cells[2];
                dgv.CurrentCell.Selected = true;
                dgv.RowHeadersVisible = false;
                dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.Columns[1].Width = 100;
                btnOK.Enabled = false;


                UpdateMSG("", 0);
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 1);
            }
        }
        private void dgvSerialList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewCell cell = (DataGridViewTextBoxCell)dgvSerialList.Rows[e.RowIndex].Cells[e.ColumnIndex];
                dgv = dgvSerialList;
                if (cell.ColumnIndex != 2)
                {
                    selectCell(e.RowIndex);
                }
                UpdateMSG("", 0);
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 1);
            }

        }
        public delegate void MyDelegate(Int32 intRowIndex);
        private void dgvSerialList_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                dgv = dgvSerialList;
                DataGridViewCell cell = (DataGridViewTextBoxCell)dgvSerialList.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.ColumnIndex == 2)
                {
                    for (int i = 0; i < cell.RowIndex; i++)
                    {
                        DataGridViewCell cell2 = (DataGridViewTextBoxCell)dgvSerialList.Rows[i].Cells[e.ColumnIndex];
                        String strSerialResult = cell2.EditedFormattedValue.ToString();
                        if (strSerialResult == string.Empty)
                        {
                            dgv.BeginInvoke(new MyDelegate(selectCell), i);
                            return;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

        private void dgvSerialList_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!GridValidate())
                {
                    btnOK.Enabled = false;
                    // selectCell(e.RowIndex);
                }
                else
                {
                    btnOK.Enabled = true;
                }
                   ;                 
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

        private void dgvSerialList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                DataGridViewCell cell = (DataGridViewTextBoxCell)dgvSerialList.Rows[e.RowIndex].Cells[e.ColumnIndex];

                String cellValue = cell.FormattedValue.ToString();
                if (cellValue==string.Empty)
                {
                    return;
                }
                //Begin// Validate By SerialNo
                


                if (ValidateSerialNo(cellValue, strJobName))
                {
                    clsSerial ChkDub = SerialList
                        .Where(w => (w.strSerialValues == cellValue)&&(w.SeqNo!= e.RowIndex+1)).FirstOrDefault();

                    if (ChkDub == null) { 


                    SerialList.Where(w => w.SeqNo == Convert.ToInt32(dgv.Rows[e.RowIndex].Cells[0].Value)).ToList().ForEach(s => s.strSerialValues = cellValue);
                   
                    UpdateMSG("", 0);
                    }
                    else
                    {
                        UpdateMSG("SerialNo : '" + cellValue + "' ซ้ำกัน กรุณาตรวจสอบและลองอีกครั้ง.", 1);


                        if (btnOK.Enabled)
                        {
                            btnOK.Enabled = false;
                        }
                        cell.Value = string.Empty;
                        selectCell(e.RowIndex);
                        return;
                    }
                }
                else
                {
                    UpdateMSG(" ไม่พบ SerialNo : '" + cellValue + "'ใน job กรุณาตรวจสอบและลองอีกครั้ง.", 1);
                    if (btnOK.Enabled)
                    {
                        btnOK.Enabled = false;
                    }
                    cell.Value = string.Empty;
                    selectCell(e.RowIndex);
                    return;
                }
                //End// Validate By SerialNo



                if (e.RowIndex == Convert.ToInt32(iTatolPCCA) - 1)
                {
                    btnOK.Enabled = true;
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

        private bool ValidateSerialNo(string cellValue, string strJobName)
        {
            try
            {
                dt = new DataTable();
                dt = objRun.getJobInfo(cellValue, strJobName);

                if (dt != null)
                {
                    return (dt.Rows.Count > 0);
                }
            }
            catch (Exception)
            {

                throw;
            }
            throw new NotImplementedException();
        }
        private bool GridValidate()
        {
            try
            {
                foreach (DataGridViewRow rw in this.dgvSerialList.Rows)
                {
                    for (int i = 0; i < rw.Cells.Count; i++)
                    {
                        if (rw.Cells[i].Value == null || rw.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[i].Value.ToString()))
                        { 
                            dgv.BeginInvoke(new MyDelegate(selectCell), rw.Index);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void selectCell(Int32 intRowIndex)
        {
            try
            {
                dgv = dgvSerialList;
                dgv.CurrentCell = dgv.Rows[intRowIndex].Cells[2];
                dgv.BeginEdit(true);
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 1);
            }
        }



        #endregion

        private void frmSerialNo_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (DialogResult == DialogResult.None)
                {
                   e.Cancel =true;
                }else if (DialogResult == DialogResult.Cancel)
                {
                    //DialogResult dr = new DialogResult();
                   var dr =  MessageBox.Show(new Form { TopMost = true }, "คุณแน่ใจว่าต้องการ\"ยกเลิก\"หรือไม่", "Serial Cancle", MessageBoxButtons.OKCancel,MessageBoxIcon.Question);
                    if(dr == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (GridValidate())
                {
                    foreach (DataGridViewRow item in dgvSerialList.Rows)
                    {
                        SerialList.Where(w => w.SeqNo == Convert.ToInt32(item.Cells[0].Value)).ToList().ForEach(s => s.strSerialValues = item.Cells[2].Value.ToString());
                    }
                    resultSerialList = SerialList;
                }
                else
                {
                    btnOK.Enabled = false;
                    this.DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
            }
        }

       

        public void UpdateMSG(string strMSG, Int32 blStatus)
        {
            String strStatus = string.Empty;
            Color colors;
            MessageBoxIcon icon;
            switch (blStatus)
            {
                case 2:
                    strStatus = "Error !!";
                    colors = Color.Red;
                    icon = MessageBoxIcon.Error;
                    break;
                case 1:
                    strStatus = "Warnning !!";
                    colors = Color.Orange;
                    icon = MessageBoxIcon.Warning;
                    break;
                default:
                    strStatus = "READY";
                    colors = Color.Blue;
                    icon = MessageBoxIcon.Information;
                    break;
            }


            //Thread.Sleep(100);

            if (blStatus != 0)
            {
                this.lblMSG.Text = strStatus + " : ";
                this.lblMSG.ForeColor = colors;
                this.lblMSG.Text += strMSG.ToString();
                this.lblMSG.Visible = true;
                MessageBox.Show(new Form { TopMost = true }, strMSG.ToString(), strStatus.ToUpper(), MessageBoxButtons.OK, icon);


            }
            else
            {
                this.lblMSG.ForeColor = colors;
                this.lblMSG.Text = strMSG.ToString();
                this.lblMSG.Visible = true;
            }
            Cursor.Current = Cursors.Default;
        }


    }
}
