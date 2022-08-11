using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.IO.Ports;
using SolidHeight.Controls;
using SolidHeight.Models;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace SolidHeight.View
{


    public partial class frmSPCMain_R : Telerik.WinControls.UI.RadForm
    {
        #region initial


        #region Declares
        // BackgroundWorker bgWorker;
        MethodInvoker mivStatus;
        System.Timers.Timer ProcTimeVHX;
        clsSPC objRun = new clsSPC();
        clsCalculate ObjCal = new clsCalculate();
        clsConvertion ObjConv = new clsConvertion();
        clsMSG setMSG = new clsMSG();
        DataTable dt, dtMachine, dtSerial, dtModels;
        String strSPCPath, strSPCMovePath
            , strCommPort, strSettings
            , strMCName, strMCType
            , strSPCType, strLotType
            , myHeight
            , strShift, strSubModel, strCauses, strAction
            , DataZ;
        private Double dcmyHeight;
       // SerialPort rs; 
        List<clsSerial> SerialList;
        List<clsHieght> HeightList = new List<clsHieght>();
        String[] iparam;
        Int32 Led_num, led_count, MAX_Cav;
        Boolean ReadZ = false;
        private int iRunningFile;  // the name field
        public int intRunningFile    // the Name property
        {
            get
            {
                return iRunningFile;
            }
        }
        private int iTotalFile;  // the name field
        public int intTotalFile    // the Name property
        {
            get
            {
                return iTotalFile;
            }
        }
        private string strStatus;  // the name field
        public string sStatus    // the Name property
        {
            get
            {
                return strStatus;
            }
        }

        private string strStatusEvent;  // the name field
        public string sStatusEvent    // the Name property
        {
            get
            {
                return strStatusEvent;
            }
        }

        private string strSaveDataState;  // the name field
        public string sSaveDataState    // the Name property
        {
            get
            {
                return strSaveDataState;
            }
        }
        ResourceManager rm = new ResourceManager("SolidHeight.Resources.rsControl", Assembly.GetExecutingAssembly());

        #endregion

        public frmSPCMain_R()
        {
            InitializeComponent();
        }

        private void frmSPCMain_R_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = "SPC Measure Version : " + Application.ProductVersion;
                init();
               
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);

                //throw;
            }
        }

        private void frmSPCMain_R_FormClosing(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy)//Check if the worker is already in progress
            {
                //bgWorker.WorkerSupportsCancellation = true; //Call the background worker
                ProcTimer.Stop();
                ProcTimeVHX.Stop();
                bgWorker.CancelAsync();
                strStatusEvent = "ERROR";
                strStatus = "ERROR";
                mivStatus = new MethodInvoker(this.UpdateUI);
                this.BeginInvoke(mivStatus);
            }

            setCommClosing();
        }
        private void init()
        {
            try
            {

                Machineconfiguration();
                DefaultControl();
                BindingCauses();
                BindingActions();
                BindingShifts();
                strSaveDataState = string.Empty;
                gvHeight.ReadOnly = true;
                gvSPC.ReadOnly = true;
                btnStart.Text = "START";

                
                strStatus = "READY";
                mivStatus = new MethodInvoker(this.UpdateUI);
                this.BeginInvoke(mivStatus);

                myHeight = "";
                Led_num = 6;
                led_count = 1;
               // setDelay(false);
            }
            catch (Exception)
            {
                throw;
            }

        }
        private void DefaultControl()
        {
            try
            {
                rdbPrime.Checked = true;
                rdbSH.Checked = true;
                if (this.rdbPrime.Checked)
                {
                    tblReconfirm.Enabled = false;
                    strLotType = "Prime";
                    UpdateMSG("", 0);
                }
                UpdateMSG("", 0);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Events 
        private void txtEN_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    inputEN();
                }
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);

                //throw;
            }

        }
        private void inputEN()
        {
            if (this.txtEN.Text == "")
            {
                UpdateMSG("กรุณากรอกข้อมูล EN", 1);
                this.txtEN.Focus();
            }
            else
            {
                UpdateMSG("", 0);
                cmbShift.Focus();
            }
        }
        private void txtSerialNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    if (this.txtSerialNo.Text == "")
                    {
                        UpdateMSG("กรุณากรอก SerialNo", 1);
                        this.txtSerialNo.Focus();
                    }
                    else
                    {
                        if (getSerialDetail(this.txtSerialNo.Text))
                        {
                            txtStencil.Focus();
                        }
                        else
                        {
                            ClearControl("Serial");
                            txtSerialNo.Focus();
                        };

                    }
                }
            }

            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);

                //throw;
            }
        }
        private void txtStencil_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    if (this.txtStencil.Text == "")
                    {
                        UpdateMSG("กรุณากรอกข้อมูล Stencil", 1);
                        this.txtStencil.Focus();
                    }
                    else
                    {
                        btnStart.Focus();
                        UpdateMSG("", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbMacroScope_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rdbMacroScope.Checked == true) {



                    if (setMachine(this.rdbMacroScope.Text))
                    {
                        if (txtSerialNo.Text != string.Empty)
                        {
                            getSerialDetail(txtSerialNo.Text);

                        }
                        UpdateMSG("", 0);
                    };
                
                }
               
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbVHX_CheckedChanged(object sender, EventArgs e)
        {
            try
            { 
                if (this.rdbVHX.Checked == true) {

                    if (setMachine(this.rdbVHX.Text))
                    {
                        if (txtSerialNo.Text != string.Empty)
                        {
                            getSerialDetail(txtSerialNo.Text);
                        }

                        UpdateMSG("", 0);
                    }               
                
                }
               
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbPrime_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rdbPrime.Checked)
                {
                    tblReconfirm.Enabled = false;
                    strLotType = "Prime";
                    UpdateMSG("", 0);
                }

            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbReConfirm_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rdbReConfirm.Checked)
                {
                    tblReconfirm.Enabled = true;
                    strLotType = "Re-Confirm";
                    UpdateMSG("", 0);
                }


            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbSH_CheckedChanged(object sender, EventArgs e)
        {
            try
            {


                if (this.rdbSH.Checked)
                {
                    strSPCType = "SH";
                    this.txtEN.Focus();

                    if (txtSerialNo.Text != string.Empty)
                    {
                        getSerialDetail(txtSerialNo.Text);
                    }
                    UpdateMSG("", 0);
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void rdbSO_CheckedChanged(object sender, EventArgs e)
        {
            try
            {


                if (this.rdbSO.Checked)
                {
                    strSPCType = "SO";
                    this.txtEN.Focus();
                    if (txtSerialNo.Text != string.Empty)
                    {
                        getSerialDetail(txtSerialNo.Text);
                    }
                    UpdateMSG("", 0);
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void cmbShift_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.txtSerialNo.Focus();
                UpdateMSG("", 0);
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }

        }
        private void cmbSubModel_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                String SubModel = "";
                var t = this.cmbSubModel.DataSource;
                if (t != null)
                {
                    SubModel = (this.cmbSubModel.DataSource == null ? "" : this.cmbSubModel.SelectedValue.ToString());
                }


                //SubModel = cmbSubModel.AccessibilityObject.Value;
                if (SubModel != "")
                {
                    BindingModelConfig(SubModel);
                }


            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnStart.Text == "START")
                {
                    if(rdbReConfirm.Checked ==true)
                    {
                        DialogResult dr = MessageBox.Show("คุณต้องการทำ Lot Re-Confirm หรือไม่.", "WARNNING", MessageBoxButtons.YesNo,MessageBoxIcon.Information);

                        if (dr == DialogResult.No ||dr == DialogResult.Cancel)
                        {
                            return;
                        }

                    }
                    if (!ValidationControls())
                    {
                        return;
                    }
                    btnStart.Text = "Reset".ToUpper();
                    int strTotalPCCA = Convert.ToInt32(txtTotalPCCA.Text);
                    SerialList = new List<clsSerial>();

                    //Begin Get Serial in jobName
                    if (strTotalPCCA <= 1)
                    {
                        SerialList.Add(new clsSerial
                        {
                            SeqNo = strTotalPCCA,
                            strSerialNo = "SerialNo :" + strTotalPCCA.ToString(),
                            strSerialValues = txtSerialNo.Text.Trim()
                        });
                    }
                    else
                    {
                        frmSerialNo frmSN = new frmSerialNo(dtSerial.Rows[0]["JOBNAME"].ToString(), dtModels.Rows[0]["PCCA_PER_CYCLE"].ToString(), txtSerialNo.Text.Trim());
                        frmSN.ShowDialog();

                        if (frmSN.DialogResult == DialogResult.OK)
                        {
                            SerialList = frmSN.resultSerialList;
                        }
                        UpdateMSG("", 0);
                    }
                    setDelay(true);
                    //End Get Serial in jobName




                    strShift = cmbShift.Text;
                    strSubModel = cmbSubModel.Text;
                    strCauses = cmbCauses.Text;
                    strAction = cmbActions.Text;

                    switch (strMCType)
                    {
                        case "microscope":
                            if (!setCommPort())
                            {
                                LockHeaderControl(true);
                                btnStart.Text = "START";
                            };

                            break;
                        case "vhx":
                            SetBGWorker();
                            break;
                    }
                }
                else if (btnStart.Text == "TRY AGAIN" || btnStart.Text == "Stop")
                {
                    if (!bgWorker.IsBusy)//Check if the worker is already in progress
                    {
                        // btnNext.Enabled = false;
                        strStatusEvent = "Waitting";
                        btnStart.Text = "Reset".ToUpper();

                        ProcTimeVHX = new System.Timers.Timer(200);
                        ProcTimeVHX.Elapsed += EventUpdates;

                        ProcTimeVHX.Start();



                    }
                }
                else
                {
                    ClearControl("");
                }
            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);

            }

        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearControl("");
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }
        }
   
       

       
        #endregion

        #region functions
        private bool setMachine(string strMachine)
        {
            try
            {
                strMCType = strMachine.ToLower();
                DataRow[] dataRows = dtMachine.Select();
                txtInput.Enabled = false;
                if (strMachine.ToLower() == "vhx")
                {
                    strSPCPath = dataRows[0]["SPCVHXPath"].ToString();
                    strSPCMovePath = dataRows[0]["SPCVHXMovePath"].ToString();
                    if (strSPCPath == "")
                    {
                        UpdateMSG("SPCVHXPath ไม่ได้ถูกตั้งค่า กรุณาตั้งค่าแล้วลองใหม่อีกครั้ง", 1);
                        return false;
                    }
                    if (strSPCMovePath == "")
                    {
                        UpdateMSG("SPCVHXMovePath ไม่ได้ถูกตั้งค่า กรุณาตั้งค่าแล้วลองใหม่อีกครั้ง", 1);
                        return false;
                    }

                    bgWorker = new BackgroundWorker();
                    bgWorker.WorkerReportsProgress = true;
                    bgWorker.WorkerSupportsCancellation = true;
                    bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
                    bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
                    bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
                    
                }
                else if (strMachine.ToLower() == "microscope")
                {
                    strCommPort = dataRows[0]["Commport"].ToString();
                    strSettings = dataRows[0]["Settings"].ToString();
                    if (strCommPort == "")
                    {
                        UpdateMSG("'SerialPort microScope' ไม่ได้ถูกตั้งค่า กรุณาตั้งค่าแล้วลองใหม่อีกครั้ง", 1);
                        return false;
                    }
                    if (strSettings == "")
                    {
                        UpdateMSG("'Settings microScope' ไม่ได้ถูกตั้งค่า กรุณาตั้งค่าแล้วลองใหม่อีกครั้ง", 1);
                        return false;
                    }


                   // rs = new SerialPort();
                  

                    List<string> portNames = SerialPort.GetPortNames().ToList();
                    if (portNames.Count != 0)
                    {                        
                        strCommPort = dataRows[0]["Commport"].ToString();
                        strCommPort = "COM" + strCommPort;
                        string gr = portNames.Where(w => w.Contains(strCommPort)).FirstOrDefault();
                        if (gr == string.Empty || gr == null)
                        {
                            UpdateMSG("ไม่พบ COMM Port ที่ตั้งค่าไว้ กรุณาตรวจสอบ COMM Port ที่ Device manager", 1);
                            return false;
                        }
                        char[] spearator = { ',', ' ' };
                        string[] aSettings = strSettings.Split(spearator, 4, StringSplitOptions.None);

                       // serialPort1.gr.ToString();
                        using (serialPort1)
                        {
                            serialPort1.PortName = gr.ToString();
                            serialPort1.BaudRate = Convert.ToInt32(aSettings[0]);
                            serialPort1.Parity = Parity.Even;
                            serialPort1.DataBits = 8;
                            serialPort1.StopBits = Convert.ToInt32(aSettings[0]) == 1 ? StopBits.One : StopBits.Two;
                            serialPort1.ReceivedBytesThreshold = 1;
                            serialPort1.Handshake = Handshake.None;
                            serialPort1.RtsEnable = true;
                            serialPort1.DtrEnable = true;
                            //  serialPort1.DataReceived += new SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
                            serialPort1.ReadTimeout = 100;

                        }
                         
                    }
                    else
                    {
                        UpdateMSG("ไม่พบ COMM Port ในคอมพิวเตอร์ [-FAIL-] ", 1); 
                        return false;
                    }

                   
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool getSerialDetail(string strSerialNo)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                dtSerial = new DataTable();
                dtSerial = objRun.getJobInfo(strSerialNo);
                if (dtSerial != null)
                {
                    if (dtSerial.Rows.Count > 0)
                    {
                        DataRow dataRows = dtSerial.Rows[0];
                        txtLotNo.Text = dataRows["JOBNAME"].ToString();
                        txtLine.Text = dataRows["LINE_NUMBER"].ToString();
                        txtModel.Text = dataRows["MPN"].ToString();

                        UpdateMSG("", 0);

                        return BindingSubModels(dataRows["MPN_PREFIX"].ToString());
                    }
                    else
                    {
                        UpdateMSG("ไม่พบ SerialNo นี้ในฐานข้อมูล. กรุณาตรวจสอบแล้วลองใหม่อีกครั้ง.", 1);                        
                    }
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private Boolean Machineconfiguration()
        {

            Boolean blResult = true;
            try
            {
                dtMachine = new DataTable();
                strMCName = Environment.MachineName;
                this.txtHostName.Text = strMCName;


                dtMachine = objRun.GetMachineType(strMCName);
                if (dtMachine != null)
                {
                    if (dtMachine.Rows.Count > 0)
                    {
                        DataRow[] dataRows = dtMachine.Select();
                        if (dataRows[0]["MachineType"].ToString().ToLower() == "vhx")
                        {
                            rdbVHX.Checked = true; 
                        }
                        else if (dataRows[0]["MachineType"].ToString().ToLower() == "microscope")
                        {
                            rdbMacroScope.Checked = true; 
                        }
                    }
                    else
                    {
                        UpdateMSG("เครื่องนี้ยังไม่ได้ลงทะเบียนเพื่อใช้งานระบบนี้ กรุณาตรวจสอบแล้วลองใหม่อีกครั้ง", 1);

                        blResult = false;
                    }
                   
                    blResult = true;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return blResult;
        }
        private bool ValidationControls()
        {
            String strMsgReturn = string.Empty;
            List<Control> ListControls = new List<Control>();
            if (string.IsNullOrEmpty(strSPCType))
            {
                UpdateMSG("กรุณาเลือก [Measure Type]", 1);
                return false;
            }

            //Check string Empty tbl Detail

            ListControls = tblDetail.Controls.Cast<Control>().OrderBy(x => Convert.ToInt32(x.Tag)).Where(x => x.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel").ToList();
            foreach (var control in ListControls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel")
                {
                    var controls = (TableLayoutPanel)control;
                    foreach (var Con in controls.Controls)
                    {
                        String Ctls = Con.GetType().ToString();
                        String[] authorsList = Ctls.Split('.');
                        String strCheckControl = authorsList[authorsList.Length - 1];

                        switch (strCheckControl)
                        {
                            case "TextBox":
                                TextBox txtBox = (TextBox)Con;
                                if (string.IsNullOrEmpty(txtBox.Text))
                                {
                                    strMsgReturn = (txtBox.Enabled ? "กรุณากรอกข้อมูลที่กล่องข้อความ [" + rm.GetString(txtBox.Name) + "]" : "ข้อมูล  [" + rm.GetString(txtBox.Name) + "] ไม่ได้ถูกตั้งค่า. กรุณาตั้งค่า แล้วลองใหม่อีกครั้ง. ");
                                    UpdateMSG(strMsgReturn, 1);
                                    txtBox.Focus();
                                    return false;
                                }
                                break;
                            case "ComboBox":
                                ComboBox txtCombo = (ComboBox)Con;
                                if (string.IsNullOrEmpty(txtCombo.Text))
                                {
                                    UpdateMSG("กรุณาเลือก " + rm.GetString(txtCombo.Name), 1);
                                    return false;
                                }
                                break;
                        }
                    }
                }
            }


            /********* LIMIT CONTROL **********/
            ListControls = new List<Control>();

            ListControls = tblControlLimit.Controls.Cast<Control>().OrderBy(x => Convert.ToInt32(x.Tag)).Where(x => x.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel").ToList();
            foreach (var control in ListControls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel")
                {
                    var controls = (TableLayoutPanel)control;
                    foreach (var Con in controls.Controls)
                    {
                        String Ctls = Con.GetType().ToString();
                        String[] authorsList = Ctls.Split('.');
                        String strCheckControl = authorsList[authorsList.Length - 1];

                        switch (strCheckControl)
                        {
                            case "TextBox":
                                TextBox txtBox = (TextBox)Con;
                                if (string.IsNullOrEmpty(txtBox.Text))
                                {
                                    strMsgReturn = (txtBox.Enabled ? "กรุณากรอกข้อมูลที่กล่องข้อความ [" + rm.GetString(txtBox.Name) + "]" : "ข้อมูล  [" + rm.GetString(strSPCType) + " " + rm.GetString(txtBox.Name) + "] ไม่ได้ถูกตั้งค่า. กรุณาติดต่อ Engineer เนื่องจากช่วง Control Limit ไม่ถูกต้อง แล้วลองใหม่อีกครั้ง. ");
                                    UpdateMSG(strMsgReturn, 1);
                                    txtBox.Focus();
                                    return false;
                                }
                                break;
                            case "ComboBox":
                                ComboBox txtCombo = (ComboBox)Con;
                                if (string.IsNullOrEmpty(txtCombo.Text))
                                {
                                    UpdateMSG("กรุณาเลือก " + rm.GetString(txtCombo.Name), 1);
                                    return false;
                                }
                                break;
                        }
                    }
                }
            }

            /*********CAVITY SEQ ROW PAD**********/
            ListControls = new List<Control>();
            ListControls = tableLayoutPanel28.Controls.Cast<Control>().ToList();
            foreach (var Con in ListControls)
            {
                String Ctls = Con.GetType().ToString();
                String[] authorsList = Ctls.Split('.');
                String strCheckControl = authorsList[authorsList.Length - 1];

                switch (strCheckControl)
                {
                    case "TextBox":
                        TextBox txtBox = (TextBox)Con;
                        if (string.IsNullOrEmpty(txtBox.Text))
                        {
                            strMsgReturn = (txtBox.Enabled ? "กรุณากรอกข้อมูลที่กล่องข้อความ [" + rm.GetString(txtBox.Name) + "]" : "ข้อมูล  PAD SetUp ประกอบด้วย Cavity , Row ,Pad ไม่ได้ถูกตั้งค่า!!! \r\nกรุณาติดต่อ Engineer เพื่อกรอกข้อมูลให้ครบ แล้วลองใหม่อีกครั้ง. ");
                            UpdateMSG(strMsgReturn, 1);
                            txtBox.Focus();
                            return false;
                        }
                        break;
                    case "ComboBox":
                        ComboBox txtCombo = (ComboBox)Con;
                        if (string.IsNullOrEmpty(txtCombo.Text))
                        {
                            UpdateMSG("กรุณาเลือก " + rm.GetString(txtCombo.Name), 1);
                            return false;
                        }
                        break;
                }
            }

            if (txtRow.Text.Length != 0)
            {
                var strrowsss = txtRow.Text.Substring(0, 1);
                var chkChar = int.TryParse(strrowsss, out int n);

                if (!chkChar)
                {
                    UpdateMSG("ROW CAVITY MODEL :'" + this.cmbSubModel.Text + "' Format ไม่ถูกต้อง กรุณาติดต่อ Engineer เพื่อตรวจสอบ Row cavity ของ Model ดังกล่าว", 1);
                    return false;
                }
            }
            else
            {
                UpdateMSG("ROW CAVITY MODEL :'" + this.cmbSubModel.Text + "' ไม่ได้ถูกตั้งค่า กรุณาติดต่อ Engineer เพื่อตรวจสอบ Row cavity ของ Model ดังกล่าว", 1);
                return false;
            }
            /********* RE CONFIRM CONTROL **********/
            //Check string Empty tbl Re-Confirm
            if (strLotType == "Re-Confirm")
            {
                foreach (var control in tblReconfirm.Controls)
                {
                    String Ctls = control.GetType().ToString();
                    String[] authorsList = Ctls.Split('.');
                    String strCheckControl = authorsList[authorsList.Length - 1];

                    switch (strCheckControl)
                    {
                        case "ComboBox":
                            ComboBox txtCombo = (ComboBox)control;
                            if (string.IsNullOrEmpty(txtCombo.Text))
                            {
                                UpdateMSG("กรุณาเลือกข้อมูลในกล่องข้อความ " + txtCombo.Name, 1);
                                return false;
                            }
                            break;
                    }
                }
            }

            if (!IsLotPrime(txtLotNo.Text, cmbSubModel.Text))
            {
                if (strLotType == "Prime")
                {
                    UpdateMSG("JobName : " + txtLotNo.Text + " subModel :" + cmbSubModel.Text + " ได้รับการวัดแล้ว.", 1);
                }
                else
                {
                    UpdateMSG("JobName : " + txtLotNo.Text + " subModel :" + cmbSubModel.Text + " เป็น Job Prime.", 1);

                }
                return false;
            }
            LockHeaderControl(false);
            return true;
        }
        private bool ValidationLimit_Cavity()
        {
            String strMsgReturn = string.Empty;
            List<Control> ListControls = new List<Control>();
            try
            {





                // Control LIMIT \\
                ListControls = new List<Control>();

                ListControls = tblControlLimit.Controls.Cast<Control>().OrderBy(x => Convert.ToInt32(x.Tag)).Where(x => x.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel").ToList();
                foreach (var control in ListControls)
                {
                    if (control.GetType().ToString() == "System.Windows.Forms.TableLayoutPanel")
                    {
                        var controls = (TableLayoutPanel)control;
                        foreach (var Con in controls.Controls)
                        {
                            String Ctls = Con.GetType().ToString();
                            String[] authorsList = Ctls.Split('.');
                            String strCheckControl = authorsList[authorsList.Length - 1];

                            switch (strCheckControl)
                            {
                                case "TextBox":
                                    TextBox txtBox = (TextBox)Con;
                                    if (string.IsNullOrEmpty(txtBox.Text)||txtBox.Text =="0")
                                    {
                                        strMsgReturn = (txtBox.Enabled ? "กรุณากรอกข้อมูลที่กล่องข้อความ [" + rm.GetString(txtBox.Name) + "]" : "ข้อมูล  [" + rm.GetString(strSPCType) + " " + rm.GetString(txtBox.Name) + "] ไม่ได้ถูกตั้งค่า. กรุณาติดต่อ Engineer เนื่องจากช่วง Control Limit ไม่ถูกต้อง แล้วลองใหม่อีกครั้ง. ");
                                        UpdateMSG(strMsgReturn, 1);
                                        txtBox.Focus();
                                        return false;
                                    }
                                    break;
                                case "ComboBox":
                                    ComboBox txtCombo = (ComboBox)Con;
                                    if (string.IsNullOrEmpty(txtCombo.Text))
                                    {
                                        UpdateMSG("กรุณาเลือก " + rm.GetString(txtCombo.Name), 1);
                                        return false;
                                    }
                                    break;
                            }
                        }
                    }
                }

                // CAVITY PAD ROW \\
                ListControls = new List<Control>(); 
                ListControls = tableLayoutPanel28.Controls.Cast<Control>().ToList(); 
                foreach (var Con in ListControls)
                        {
                            String Ctls = Con.GetType().ToString();
                            String[] authorsList = Ctls.Split('.');
                            String strCheckControl = authorsList[authorsList.Length - 1];

                            switch (strCheckControl)
                            {
                                case "TextBox":
                                    TextBox txtBox = (TextBox)Con;
                                    if (string.IsNullOrEmpty(txtBox.Text))
                                    {
                                        strMsgReturn = (txtBox.Enabled ? "กรุณากรอกข้อมูลที่กล่องข้อความ [" + rm.GetString(txtBox.Name) + "]" : "ข้อมูล  PAD SetUp ประกอบด้วย Cavity , Row ,Pad ไม่ได้ถูกตั้งค่า!!! \r\nกรุณาติดต่อ Engineer เพื่อกรอกข้อมูลให้ครบ แล้วลองใหม่อีกครั้ง. ");
                                        UpdateMSG(strMsgReturn, 1);
                                        txtBox.Focus();
                                        return false;
                                    }
                                    break;
                                case "ComboBox":
                                    ComboBox txtCombo = (ComboBox)Con;
                                    if (string.IsNullOrEmpty(txtCombo.Text))
                                    {
                                        UpdateMSG("กรุณาเลือก " + rm.GetString(txtCombo.Name), 1);
                                        return false;
                                    }
                                    break;
                            }
                        }
          
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool IsLotPrime(string strjobName, string strsubModel)
        {
            try
            {
                bool blResults = false;
                dt = new DataTable();
                dt = objRun.GetLotPrime(strjobName, strsubModel, strSPCType, strLotType);
                if (dt.Rows.Count > 0)
                {
                    blResults = dt.Rows[0]["LOT_TYPE"].ToString() == strLotType;

                }
                else
                {
                    blResults = false;
                }
                if (strLotType == "Re-Confirm" && blResults == false)
                {
                    blResults = true;
                }
                else if (strLotType == "Prime")
                {
                    //if (blResults == true)
                    //{
                    //    blResults = false;
                    //}
                    //else
                    //{

                    //}

                    blResults = !blResults;

                }
                return blResults;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void LockHeaderControl(Boolean blSwiching)
        {
            try
            {
                tblMachineType.Enabled = blSwiching;
                tblDetail.Enabled = blSwiching;
                tblReconfirm.Enabled = blSwiching;
                tblMeasureType.Enabled = blSwiching;
                tblLotType.Enabled = blSwiching;

            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
                throw;
            }


        }
        private void ClearControl(string strTbl)
        {
            try
            {
                if (strTbl == "Serial")
                {
                    Cursor.Current = Cursors.WaitCursor;

                    iRunningFile = 0;
                    iTotalFile = 0;
                    txtSerialNo.Text = string.Empty;
                    txtLotNo.Text = string.Empty;
                    txtLine.Text = string.Empty;
                    txtModel.Text = string.Empty;
                    txtPadPerCol.Text = string.Empty;
                    txtMeasurement.Text = string.Empty;

                    txtLower.Text = string.Empty;
                    txtCenterLimit.Text = string.Empty;
                    txtUpper.Text = string.Empty;
                    
                    txtCPKLower.Text = string.Empty;                   
                    txtCPKUpper.Text = string.Empty;

                    txtSDUpper.Text = string.Empty;

                    txtQty.Text = string.Empty;
                    txtTotalPCCA.Text = string.Empty;
                    txtSeq.Text = string.Empty;
                    txtRow.Text = string.Empty;
                    txtCav.Text = string.Empty;
                    txtPad.Text = string.Empty;
                    txtStencil.Text = string.Empty;
                    txtInput.Text = string.Empty;
                    txtHeight.Text = string.Empty;
                    txtDie.Text = string.Empty;
                    cmbSubModel.DataSource = null;
                    cmbSubModel.BindingContext = BindingContext;

                    HeightList = new List<clsHieght>();
                    SerialList = new List<clsSerial>();
                    gvHeight.DataSource = null;
                    gvSPC.DataSource = null;

                    return;

                }

                if (strTbl == "")
                {
                    strStatus = "Waiting";
                    mivStatus = new MethodInvoker(this.UpdateUI);
                    this.BeginInvoke(mivStatus);
                    if (strMCType == "vhx")
                    {
                        if (bgWorker.IsBusy)
                        {
                            ProcTimeVHX.Stop();
                            bgWorker.CancelAsync();
                        }
                        //ProcTimeVHX.Stop();
                    }
                    else
                    {

                        mivStatus = new MethodInvoker(this.setCommClosing);
                        this.BeginInvoke(mivStatus);


                       
                    }
                    if (ProcTimeVHX!= null)
                    {
                        ProcTimeVHX.Enabled = false;
                        ProcTimeVHX.Stop();
                    }   

                    this.Invalidate();
                    //txtEN.Text = string.Empty;
                    //cmbShift.SelectedIndex = 0;
                    LockHeaderControl(true);
                    tblDetail.Enabled = true;
                    tblMachineType.Enabled = true;
                    tblMeasureType.Enabled = true;
                    tableLayoutPanel16.Enabled = true;
                    groupBox1.Enabled = true;
                    rdbSH.Checked = false;
                    rdbSO.Checked = false;
                    txtRemark.Text = string.Empty;
                    strSaveDataState = "";
                    strStatusEvent = "";
                    strStatus = "";
                    ClearControl("Serial");
                    GC.Collect();
                    Machineconfiguration();
                    DefaultControl();
                    BindingCauses();
                    BindingActions();
                    //BindingShifts();
                    strSaveDataState = string.Empty;
                    gvHeight.ReadOnly = true;
                    gvSPC.ReadOnly = true;
                    btnStart.Text = "START"; 
                    myHeight = "";
                    Led_num = 6;
                    led_count = 1;


                    strStatus = "READY";
                    mivStatus = new MethodInvoker(this.UpdateUI);
                    this.BeginInvoke(mivStatus);


                    ObjCal = new clsCalculate();
                    txtSerialNo.Focus();
                }
                UpdateMSG("", 0);
            }
            catch (Exception)
            {

                throw;
            }
        }




        #endregion

        #region BindingDDL       
        private void BindingModelConfig(String SubModel)
        {
            dtModels = new DataTable();
            dtModels = objRun.GetSubModelConfig(SubModel, strSPCType);
            if (dtModels != null)
            {
                if (dtModels.Rows.Count > 0)
                {
                    DataRow dataRow = dtModels.Rows[0];
                    String strPADPERCOL = (dataRow["PAD_PER_COL"].ToString() == "" ? "0" : dataRow["PAD_PER_COL"].ToString());

                    if (strPADPERCOL == "0")
                    {
                        UpdateMSG("PAD/Column = 0 Model : " + SubModel + "ยังไม่ได้ทำการกำหนด PAD/Column  กรุณาติดต่อ Engineer เนื่องจาก PAD/Column ไม่ถูกต้อง", 1);

                        ClearControl("Serial");
                        return;
                    } 
                    else
                    {
                        txtPadPerCol.Text = strPADPERCOL;
                        txtMeasurement.Text = dataRow["MEASUREMENT"].ToString();
                        // upper center lower
                        txtLower.Text = (dataRow["LOWERLIMIT"].ToString() != "0" ? dataRow["LOWERLIMIT"].ToString() : "");
                        txtCenterLimit.Text = (dataRow["CENTERLIMIT"].ToString() != "0" ? dataRow["CENTERLIMIT"].ToString() : "");
                        txtUpper.Text = (dataRow["UPPERLIMIT"].ToString() != "0" ? dataRow["UPPERLIMIT"].ToString() : "");
                        //CPK upper lower
                        txtCPKLower.Text = (dataRow["LOWERLIMIT_CPK"].ToString() != "0" ? dataRow["LOWERLIMIT_CPK"].ToString() : "");
                        txtCPKUpper.Text = (dataRow["UPPERLIMIT_CPK"].ToString() != "0" ? dataRow["UPPERLIMIT_CPK"].ToString() : "");
                        //SD Upper
                        txtSDUpper.Text = (dataRow["UPPERLIMIT_SD"].ToString() != "0" ? dataRow["UPPERLIMIT_SD"].ToString() : "");

                        txtQty.Text = (dataRow["TOTAL_QTY"].ToString() != "0" ? dataRow["TOTAL_QTY"].ToString() : "");
                        txtTotalPCCA.Text = dataRow["PCCA_PER_CYCLE"].ToString();
                        txtSeq.Text = dataRow["CAV_SEQ"].ToString();
                        txtRow.Text = dataRow["ROW"].ToString();
                        txtCav.Text = dataRow["CAV"].ToString();
                        txtPad.Text = dataRow["PAD"].ToString();
                        txtDie.Text = dataRow["DIETHICKNESS"].ToString();

                        if (ValidationLimit_Cavity())
                        {
                            if (txtRow.Text.Length != 0)
                            {
                                var strrowsss = txtRow.Text.Substring(0, 1);
                                var chkChar = int.TryParse(strrowsss, out int n);

                                if (!chkChar)
                                {
                                    UpdateMSG("ROW CAVITY Model : " + SubModel + "format ไม่ถูกต้อง กรุณาติดต่อ Engineer", 1);
                                    return;
                                }
                            }
                            else
                            {
                                UpdateMSG("ROW CAVITY Model : " + SubModel + " ไม่ได้ถูกตั้งค่า กรุณาติดต่อ Engineer", 1);
                                return;
                            }



                            if (dataRow["MAX_CAV_SEQ"].ToString() == "")
                            {
                                UpdateMSG("CAVITY Model : " + SubModel + "ไม่ได้ถูกตั้งค่า Model pad cavity กรุณาติดต่อ Engineer เนื่องจาก Model pad cavity ไม่ถูกต้อง", 1);
                                return;
                            }
                            else
                            {
                                MAX_Cav = Convert.ToInt32(dataRow["MAX_CAV_SEQ"].ToString());
                            }
                        }
                        else
                        {
                            return;
                        }
                        HeightList = new List<clsHieght>();

                    }
                }
            }

        } 
        private void BindingActions()
        {
            try
            {
                cmbActions.DataSource = null;
                dt = new DataTable();
                iparam = new String[4];
                //dt = objRun.GetActions();
                dt = objRun.GetDataBinding("GetActions", iparam);

                DataRow dataRow;
                dataRow = dt.NewRow();
                dataRow["action_name"] = "";
                dt.Rows.InsertAt(dataRow, 0);

                cmbActions.DataSource = dt;
                cmbActions.ValueMember = "action_name";
                cmbActions.DisplayMember = "action_name";
                cmbActions.SelectedIndex = 0;
                cmbActions.BindingContext = this.BindingContext;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void BindingCauses()
        {
            try
            {
                cmbCauses.DataSource = null;

                iparam = new String[4];
                dt = new DataTable();
                // dt = objRun.GetCauses();
                dt = objRun.GetDataBinding("GetCauses", iparam);
                DataRow dataRow;
                dataRow = dt.NewRow();
                dataRow["cause_name"] = "";
                dt.Rows.InsertAt(dataRow, 0);
                cmbCauses.DataSource = dt;
                cmbCauses.ValueMember = "cause_name";
                cmbCauses.DisplayMember = "cause_name";
                cmbCauses.SelectedIndex = 0;
                cmbCauses.BindingContext = this.BindingContext;

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void BindingShifts()
        {
            try
            {
                iparam = new String[4];
                dt = new DataTable();
                //dt = objRun.GetShifts();

                dt = objRun.GetDataBinding("GetShifts", iparam);
                DataRow dataRow;
                dataRow = dt.NewRow();
                dataRow["shift_name"] = "";
                dt.Rows.InsertAt(dataRow, 0);
                cmbShift.DataSource = dt;
                cmbShift.ValueMember = "shift_name";
                cmbShift.DisplayMember = "shift_name";
                cmbShift.SelectedIndex = 0;
                cmbShift.BindingContext = this.BindingContext;

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            ZtoDB();
        }
        private bool BindingSubModels(string strMPNPrefix)
        {
            try
            {
                iparam = new String[4];
                iparam[0] = strMPNPrefix;
                iparam[1] = strMCType;
                dtModels = new DataTable();
                // dtModels = objRun.GetSubModel(strMPNPrefix.Trim());
                dtModels = objRun.GetDataBinding("GetSubModel", iparam);
                cmbSubModel.DataSource = dtModels;
                cmbSubModel.ValueMember = "model_Name";
                cmbSubModel.DisplayMember = "model_Name";
                //cmbSubModel.SelectedIndex = 0;
                cmbSubModel.BindingContext = this.BindingContext;
                return (cmbSubModel.DataSource == null ? false : true);

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void ProcTimer_Tick(object sender, EventArgs e)
        {
            led_count += 1;
            if (led_count > Led_num)
            {
                led_count = 1;
                setDelay(false);
            }
        }

        



        #endregion

        #region CommPort

        private Boolean setCommPort()
        {
            try
            {
                //serialPort1      
                if (serialPort1 != null)
                {
                    if (serialPort1.IsOpen)
                    {
                        this.serialPort1.Close();
                    }
                    else
                    {
                        this.serialPort1.Open();                       
                    }
                }
                if (serialPort1.IsOpen)
                {
                    UpdateMSG("COMM Port : " + strCommPort + " เปิดใช้งานแล้ว [-OK-]", 3);
                    txtInput.Enabled = true;
                    txtInput.Focus();
                }
                else
                {
                    UpdateMSG("COMM Port : " + strCommPort + " ยังไม่ถูกเปิดใช้งาน [-FAIL-] ", 1);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (this.serialPort1.IsOpen)
                {
                    string dataReceived = this.serialPort1.ReadExisting();
                    MessageBox.Show(serialPort1.PortName + " : " + dataReceived, "WARNING Message Return", MessageBoxButtons.OK);
                    if (dataReceived != "")
                    {
                        int indexZ = dataReceived.IndexOf('Z');
                        int Zinit = indexZ + 1;
                        int Zfsh = 15;
                        if (dataReceived.Length > indexZ + 16)
                        {
                            string dataReceived1 = dataReceived.Substring(Zinit, Zfsh).ToString();
                            gentxtInput("Z");
                            gentxtInput(dataReceived1.ToString());
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show(serialPort1.PortName +" : " + ex.Message, "WARNING", MessageBoxButtons.OK);                 
            }
        } 
        private void setCommClosing()
        {
            try
            {
                if (serialPort1 != null)
                {
                    if (serialPort1.IsOpen)
                    {
                        //serialPort1.DataReceived -= Rsrecivedata;
                        serialPort1.DiscardInBuffer();
                        serialPort1.Close();
                    }
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 1);
                return;
            }
        }
        //private void Rsrecivedata(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        //DataZ = rs.ReadLine();



        //        DataZ = serialPort1.ReadExisting();
        //        gentxtInput(DataZ);
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateMSG(ex.Message.ToString(), 2);
        //    }
        //}

        private void gentxtInput(String strInput)
        {
            try
            {
                strStatus = "Waitting";
                mivStatus = new MethodInvoker(this.UpdateUI);
                this.BeginInvoke(mivStatus); 
                txtInput.BeginInvoke((MethodInvoker)(() =>
                {
                    txtInput.Text = txtInput.Text + strInput;
                    txtInput.SelectionStart = txtInput.Text.Length;
                    txtInput.SelectionLength = 0;
                })); 

                var G = strInput.Trim().ToLower();
                if (strInput.Trim().ToLower() == "z" && !ProcTimer.Enabled)
                {
                    ReadZ = true;
                }
                if (ReadZ && myHeight.Length < 16)
                {
                    myHeight += strInput;
                }
                //if (strInput.Trim().ToLower() == "mm")
                //{
                //    strSaveDataState = "";
                //    strStatus = "READY";
                //    mivStatus = new MethodInvoker(this.UpdateUI);
                //    this.BeginInvoke(mivStatus);
                //}
                if (myHeight.Length == 16)
                {
                    myHeight = myHeight.Substring(myHeight.Length - 7);
                    dcmyHeight = Convert.ToDouble(myHeight);
                    if (strSPCType == "SO")
                    {
                        dcmyHeight -= Convert.ToDouble(txtDie.Text);
                    } 

                    txtHeight.BeginInvoke((MethodInvoker)(() =>
                    {
                        txtHeight.Text = dcmyHeight.ToString("0.000000");
                    }));
                     

                    if (dcmyHeight != 0)
                    {
                        switch (ZtoArray(dcmyHeight))
                        {
                            case 0:
                                ReadZ = false;
                                break;
                            case 1:
                                UpdateMSG("", 0); 
                                break;
                            case 2:
                                if (ZtoDB())
                                {
                                    
                                    //Thread.Sleep(100);
                                    strSaveDataState = "Complete";
                                    strStatus = "Complete";
                                    setCommClosing();
                                    mivStatus = new MethodInvoker(this.UpdateUI);
                                    this.Invoke(mivStatus);

                                    //ClearControl("");
                                };
                                break;
                        }
                        myHeight = "";
                        txtInput.BeginInvoke((MethodInvoker)(() =>
                        {
                            txtInput.Text = "";
                        }));
                        ReadZ = false;
                        ProcTimer.Enabled = false;
                        GC.Collect();
                    }
                }
            }
            catch (Exception ex)
            {

                UpdateMSG(ex.Message.ToString(), 2);
            }

        }
        private void setDelay(Boolean sdelay)
        {
            int delayTime;

            if (sdelay)
            {
                delayTime = Led_num;


                for (int i = 0; i < Led_num; i++)
                {



                    if (strMCType == "microscope")
                    {
                        //lblStatus.Text = "WAITING";

                        lblStatus.BackColor = Color.Yellow;
                        lblStatus.ForeColor = Color.Black;


                    }




                }
                this.ProcTimer.Enabled = true;
                strStatus = "WAITING";
                mivStatus = new MethodInvoker(this.UpdateUI);
                this.BeginInvoke(mivStatus);
                Cursor.Current = Cursors.WaitCursor;

                ProcTimer.Enabled = true;
            }
            else
            {
                for (int i = 0; i < Led_num; i++)
                {
                    //lblStatus.Text = "READY";
                    //lblStatus.BackColor = Color.Green;
                    //lblStatus.ForeColor = Color.White;

                    //if (strMCType == "microscope")
                    //{
                    //    lblStatus.Text = "READY";
                    //    lblStatus.BackColor = Color.Green;
                    //    lblStatus.ForeColor = Color.White;
                    //}
                    //else
                    //{
                        strStatus = "READY";
                        mivStatus = new MethodInvoker(this.UpdateUI);
                        this.BeginInvoke(mivStatus);
                    //}
                }
                ProcTimer.Enabled = false;
                Cursor.Current = Cursors.Default;
               


            }

        }
        private Int32 ZtoArray(Double myHeight)
        {
            try
            {
                //if(strMCType == "microscope")
                //{
                setDelay(true);
                // }



                DataRow dr = dtModels.Rows[0];
                Double dclower_limit = Convert.ToDouble(dr["LOWERLIMIT"].ToString())
                    , dcUpper_limit = Convert.ToDouble(dr["UPPERLIMIT"].ToString())
                    , dcSDUpper_limit = Convert.ToDouble(dr["UPPERLIMIT_SD"].ToString())
                    , dcCustLower_limit, dcCustUpper_limit;

                Int32 iZtoArray = 1;

                dt = objRun.GetCustomerSpect(txtMeasurement.Text, strSPCType);
                if (dt.Rows.Count <= 0)
                {
                    UpdateMSG("ไม่พบข้อมูลสเปคของลูกค้า กรุณาติดต่อ Engineer เนื่องจากช่วง Control Limit ไม่ถูกต้อง", 1);
                    iZtoArray = 0;
                    return iZtoArray;
                }

                dcCustLower_limit = Convert.ToDouble(dt.Rows[0]["LowerLimit"].ToString());
                dcCustUpper_limit = Convert.ToDouble(dt.Rows[0]["UpperLimit"].ToString());

                // Check Customer Spect
                if (myHeight <= dcCustLower_limit || myHeight >= dcCustUpper_limit)
                {
                    UpdateMSG("ความสูงของ Solder " + myHeight.ToString() + " Out of Control Limit ของลูกค้า \r\nUpper Customer :"+ dcCustUpper_limit.ToString()+"\r\nLower Customer : "+ dcCustLower_limit.ToString()+" \r\nกรุณาตรวจสอบแล้วลองใหม่อีกครั้ง.", 1);
                    iZtoArray = 0;
                    return iZtoArray;
                }

                //Check User Spect
                if (myHeight <= dclower_limit || myHeight >= dcUpper_limit)
                {
                    UpdateMSG("ความสูงของ Solder : " + myHeight.ToString() + " Out of Control Limit \r\nกรุณาวัดค่าใหม่.", 1) ;
                    iZtoArray = 0;
                    return iZtoArray;
                }

                int iSeqNo = HeightList.Count() + 1;
                HeightList.Add(new clsHieght
                {
                    SeqNo = iSeqNo,
                    strHeight = Math.Round(myHeight,6).ToString("0.000000"),
                    Height = myHeight,
                    Results = null
                });

                double SDbar = ObjCal.CalSD(ref HeightList, myHeight, iSeqNo, txtPadPerCol.Text, txtCPKUpper.Text, txtCPKLower.Text);
                // Check SD Upper
                if (myHeight >= dcUpper_limit)
                {
                    UpdateMSG("ความสูงของ Solder ส่วนเบี่ยงเบนมาตรฐาน(SD)เกินขีดจำกัด กรุณาตรวจสอบแล้วลองใหม่ทั้งหมดอีกครั้ง.", 2);
                    HeightList.RemoveAll(x => x.SeqNo == iSeqNo);


                    iZtoArray = 0;
                    return iZtoArray;
                }
                if (strMCType == "microscope")
                {
                    // ZtoGridview(iSeqNo);
                    Thread.Sleep(500);
                    strStatus = "Complete";
                    mivStatus = new MethodInvoker(this.UpdateUI);
                    this.BeginInvoke(mivStatus);
                }





                if (Convert.ToInt32(txtQty.Text) == iSeqNo)
                {
                    iZtoArray = 2;
                }
                return iZtoArray;
            }
            catch (Exception)
            {

                throw;
            }


        }
        private void ZtoGridview(int Seqno)
        {
            try
            {
                
                gvHeight.DataSource = null;
                gvHeight.DataSource = HeightList.OrderByDescending(o => o.SeqNo).ToList();
                gvHeight.Columns[2].Visible = false;
                gvHeight.Columns["SeqNo"].Width = 60; 
                this.gvHeight.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
             

                clsHieght SPCs = new clsHieght();
                SPCs = HeightList.Where(w => w.SeqNo == Seqno).FirstOrDefault();
                List<clsCal> SPC = new List<clsCal>();

                SPC = SPCs.Results.ToList();
                gvSPC.DataSource = null;

                gvSPC.DataSource = SPC;
                gvHeight.DataSource = HeightList.OrderByDescending(o => o.SeqNo).ToList();
                gvSPC.BindingContext = BindingContext;
                DataGridViewColumn columnPSC = gvSPC.Columns[0];
                columnPSC.Width = 40;

            }
            catch (Exception ex)
            {
                UpdateMSG(ex.Message.ToString(), 2);
            }

        }
        private Boolean ZtoDB()
        {
            Boolean blResult = false;

            try
            {
                DataTable dt = new DataTable(), dtHeight = new DataTable(), dtSPC = new DataTable(), dtSerials = new DataTable();


                clsSPCHeader mdSPCHeader = new clsSPCHeader()
                {
                    MaxID = "",
                    dateTime = DateTime.Now,
                    Type = strSPCType,
                    EN = txtEN.Text,
                    Shift = strShift,
                    Line = dtSerial.Rows[0]["LINE_NUMBER"].ToString(),
                    Lot = dtSerial.Rows[0]["JOBNAME"].ToString(),
                    Stencil = txtStencil.Text,
                    Model = strSubModel,
                    LotType = strLotType,
                    CavSeq = Convert.ToInt32(dtModels.Rows[0]["CAV_SEQ"]),
                    MaxCavSeq = MAX_Cav,
                    Machine_Type = strMCType,
                    Machine_Name = strMCName
                };

                clsReConfirm_info mdReConfirm = new clsReConfirm_info()
                {
                    Cause = strCauses,
                    Action = strAction,
                    Remark = txtRemark.Text
                };


                dtSerials = ObjConv.ListToDataTable(SerialList);
                dtHeight = ObjConv.GVtoDataTable(gvHeight);
                dtSPC = ObjConv.GVtoDataTable(gvSPC);



                int chkSerialRow = dtSerials.Rows.Count;
                for (int i = chkSerialRow; i < 10; i++)
                {
                    dtSerials.Rows.Add(i + 1, "", "");
                }

                List<clsDataTableList> paramDT = new List<clsDataTableList>()
                {
                    new clsDataTableList { strParam = "@dtSerial", dt = dtSerials },
                    new clsDataTableList { strParam = "@dtSPC", dt = dtSPC },
                    new clsDataTableList { strParam = "@dtHeight", dt = dtHeight },
                };

                dt = objRun.SaveSPCData(mdSPCHeader, mdReConfirm, paramDT);


                if (dt.Rows.Count != 0)
                {

                    UpdateMSG("การวัดความสูงของSolder ประสบความสำเร็จ ", 3);
                    setDelay(true);

                    blResult = true;
                }
                else
                {
                    UpdateMSG("การวัดความสูงของSolder ล้มเหลว", 2);
                    setDelay(true);

                    blResult = false;
                }

                return blResult;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region BackgroundWorker
        private void SetBGWorker()
        {
            try
            {
                VHX_DeleteFileTemp();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void VHX_DeleteFileTemp()
        {
            string[] csvList = Directory.GetFiles(strSPCPath, "*.csv");
            foreach (string f in csvList)
            {
                File.Delete(f);
            }
            if (!bgWorker.IsBusy)//Check if the worker is already in progress
            {
                iRunningFile = 0;
                iTotalFile = int.Parse(txtQty.Text);


                strStatusEvent = "Waitting";
                ProcTimeVHX = new System.Timers.Timer(200);
                ProcTimeVHX.Elapsed += EventUpdates;
                ProcTimeVHX.Start();
            }
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker;
            worker = sender as BackgroundWorker;
            try
            {


                if (strStatusEvent != "Running")
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        //string strlogFinishPath = "";
                        //string strLogErrorPath = "";
                        String strlogFinishPath = CreateFolderLog(strSPCMovePath + "\\Finish")
                            , strLogErrorPath = CreateFolderLog(strSPCMovePath + "\\Error");

                        DirectoryInfo diSPCPath = new DirectoryInfo(strSPCPath);
                        FileInfo[] fileSPC = diSPCPath.GetFiles("*.csv");

                        if (fileSPC.Length <= 0)
                        {
                            strStatusEvent = "Waitting";
                            worker.ReportProgress(0);
                            strStatus = "Waitting";
                        }
                        else
                        {
                            Double ipers = 0.00;
                            int iPer = 0;

                            foreach (FileInfo fi in fileSPC)
                            {
                                if (worker.CancellationPending == true)
                                {
                                    e.Cancel = true;

                                    strStatus = "Stop";
                                    strStatusEvent = "Stop";

                                    mivStatus = new MethodInvoker(this.UpdateUI);
                                    this.BeginInvoke(mivStatus);

                                }
                                else
                                {
                                    try
                                    {

                                        Double r = Convert.ToDouble(iRunningFile.ToString("0.000")), t = Convert.ToDouble(iTotalFile.ToString("0.000"));
                                        ipers = ((r / t) * 100);

                                        iPer = Convert.ToInt32(ipers);

                                        if (iRunningFile < iTotalFile)
                                        {
                                            string fileSPCPath = fi.DirectoryName + "\\" + fi.Name;
                                            strStatusEvent = "Running";

                                            worker.ReportProgress(iPer);
                                            strStatus = "Process File";

                                            Thread.Sleep(200);

                                            dcmyHeight = ProcessFile(fileSPCPath);
                                            if (strSPCType == "SO")
                                            {
                                                var dieTickness = Convert.ToDouble(txtDie.Text);

                                                dcmyHeight -= dieTickness;
                                            } 
                                            txtHeight.Invoke((MethodInvoker)(() =>
                                            {
                                                txtHeight.Text = dcmyHeight.ToString("0.000000");
                                            })); 
                                            if (dcmyHeight != 0)
                                            {
                                                switch (ZtoArray(dcmyHeight))
                                                {
                                                    case 0:
                                                        ReadZ = false;
                                                        System.ArgumentException argEx = new System.ArgumentException("Hieght is not spec");
                                                        throw argEx;
                                                        break;
                                                    case 1:
                                                        iRunningFile++;
                                                        File.Move(fi.FullName, strlogFinishPath + "\\" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fi.Name.ToString());
                                                        strStatusEvent = "Complete";
                                                        worker.ReportProgress(iPer);
                                                        strStatus = "Complete";
                                                        Thread.Sleep(300);

                                                        UpdateMSG("", 0);
                                                        break;
                                                    case 2:
                                                        iRunningFile++;
                                                        File.Move(fi.FullName, strlogFinishPath + "\\" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fi.Name.ToString());
                                                        strStatusEvent = "Complete";
                                                        worker.ReportProgress(100);
                                                        strStatus = "Complete";
                                                        Thread.Sleep(300);
                                                        break;
                                                }
                                                myHeight = "";
                                                txtInput.Text = "";
                                                ReadZ = false;
                                            }
                                            GC.Collect();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Thread.Sleep(100);
                                        File.Move(fi.FullName, strLogErrorPath + "\\" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fi.Name.ToString());
                                        if (bgWorker.WorkerSupportsCancellation == true)
                                        {
                                            ProcTimeVHX.Stop();
                                            bgWorker.CancelAsync();
                                            strStatusEvent = "ERROR";
                                            strStatus = "ERROR";
                                            mivStatus = new MethodInvoker(this.UpdateUI);
                                            this.BeginInvoke(mivStatus);
                                            //UpdateMSG(ex.Message.ToString(), 1);
                                        }
                                    }
                                }
                            }

                            if (worker.CancellationPending == false)
                            {
                                strStatusEvent = "Ending";
                                worker.ReportProgress(iPer);
                                strStatus = "Ending";
                                Thread.Sleep(100);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(100);
                if (bgWorker.WorkerSupportsCancellation == true)
                {
                    ProcTimeVHX.Stop();
                    bgWorker.CancelAsync();
                    strStatusEvent = "ERROR";
                    strStatus = "ERROR";
                    mivStatus = new MethodInvoker(this.UpdateUI);
                    this.BeginInvoke(mivStatus);
                    UpdateMSG(ex.Message.ToString(), 2);

                }
            }
        }
        delegate void SetTextCallback(string text); 
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (iTotalFile == 0)
                {
                    return;
                }
                if (iRunningFile != iTotalFile)
                {
                    return;
                }
                if (bgWorker.WorkerSupportsCancellation == true)
                {
                    if (strSaveDataState == "" || strSaveDataState == string.Empty)
                    {
                        strSaveDataState = "Waiting";
                        if (ZtoDB())
                        {
                            strSaveDataState = "Complete";

                            ProcTimeVHX.Stop();
                            bgWorker.CancelAsync();


                            strStatus = "Complete";

                            mivStatus = new MethodInvoker(this.UpdateUI);
                            this.BeginInvoke(mivStatus);
                            ProcTimeVHX.Stop();
                            bgWorker.CancelAsync();
                            Thread.Sleep(100);
                        }

                    }
                    else if (strSaveDataState == "Complete")
                    {
                        ProcTimeVHX.Stop();
                        bgWorker.CancelAsync();

                        strStatusEvent = "Stop";
                        strStatus = "Stop";

                        mivStatus = new MethodInvoker(this.UpdateUI);
                        this.BeginInvoke(mivStatus);
                        Thread.Sleep(100);
                    }

                }
            }
            catch (Exception ex)
            {

                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                UpdateMSG("การวัดความสูงของ Solder ล้มเหลว : " + ex.Message.ToString(), 1);
            }
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mivStatus = new MethodInvoker(this.UpdateUI);
            this.BeginInvoke(mivStatus);
        }
        private string CreateFolderLog(string PathFile)
        {
            string StrPath = "";
            try
            {
                DateTime datenow = DateTime.Now;
                String strYear = datenow.ToString("yyyy")
                    , strMonth = int.Parse(datenow.ToString("MM")).ToString()
                    , strDay = datenow.ToString("dd")
                    , PathYear, PathMonth, PathDay;
                bool folderExistsCom = Directory.Exists(PathFile);
                if (!folderExistsCom)
                {
                    Directory.CreateDirectory(PathFile);
                }

                //YEAR
                PathYear = PathFile + "\\" + strYear;
                bool folderExistsYear = Directory.Exists(PathYear);
                if (!folderExistsYear)
                {
                    Directory.CreateDirectory(PathYear);
                }

                //MONTH
                PathMonth = PathYear + "\\" + strMonth;
                bool folderExistsMonth = Directory.Exists(PathMonth);
                if (!folderExistsMonth)
                {
                    Directory.CreateDirectory(PathMonth);
                }

                //DAY
                PathDay = PathMonth + "\\" + strDay;
                bool folderExistsDay = Directory.Exists(PathDay);
                if (!folderExistsDay)
                {
                    Directory.CreateDirectory(PathDay);
                }

                StrPath = (PathDay + "\\");

                return StrPath;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void EventUpdates(object sender, EventArgs e)
        {
            if (strStatusEvent != "Running" && strStatusEvent != "Stop" && strStatusEvent != "ERROR")
            {
                strStatusEvent = "Waitting";
                strStatus = "Waitting : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");


                Task.Factory.StartNew(() =>
                {
                    if (bgWorker.IsBusy == false)//Check if the worker is already in progress
                    {
                        bgWorker.RunWorkerAsync();//Call the background worker

                    }
                    else if (bgWorker.IsBusy == false)
                    {
                        bgWorker.CancelAsync();
                    }
                });
            }
        }
        private Double ProcessFile(string filePath)
        {
            Double Result = 0.0;
            string StrDateTime = DateTime.Now.ToString();
            string StrDateCode = DateTime.Now.ToString("yyyyMMddhhmmssFFF");

            StreamReader StrWer;
            String strLine;
            StrWer = File.OpenText(System.IO.Path.GetFullPath(filePath));

            int i = 1;
            try
            {
                while (StrWer.EndOfStream == false)
                {
                    strLine = StrWer.ReadLine();

                    if (strLine.Trim() != "")
                    {

                        string ss = strLine.Split(',')[0];


                        if (strLine.Split(',')[0].IndexOf("Height") >= 0)
                        {
                            Result = Math.Round(Double.Parse(strLine.Split(',')[1].ToString()) / 1000, 7);



                            break;
                        }
                    }
                    i++;
                }

                StrWer.Close();
                return Result;
            }
            catch (Exception ex)
            {
                StrWer.Close();
                StrWer.Dispose();
                System.ArgumentException argEx = new System.ArgumentException("Index is out of range", "index", ex);
                throw argEx;
            }

        }

        #endregion

        #region MSG
        public void UpdateMSG(string strMSG, Int32 blStatus)
        {
            strStatus = string.Empty;
            Color colors;
            MessageBoxIcon icon;
            switch (blStatus)
            {
                case 3:
                    strStatus = "SUCCESS";
                    colors = Color.Orange;
                    icon = MessageBoxIcon.Information;
                    break;
                case 2:
                    strStatus = "ERROR";
                    colors = Color.Red;
                    icon = MessageBoxIcon.Error;
                    break;
                case 1:
                    strStatus = "Warnning";
                    colors = Color.Orange;
                    icon = MessageBoxIcon.Error;
                    break;
                
                default:
                    strStatus = "READY";
                    colors = Color.Blue;
                    icon = MessageBoxIcon.Information;
                    break;
            }

            Thread.Sleep(100);
            //this.lblMsg_E.Text = strStatus;
            //this.lblMsg_E.ForeColor = colors;
            //this.lblMsg.Text = strMSG.ToString();
            //this.lblMsg.Visible = true;
            setMSG = new clsMSG();
            setMSG.icons = icon;
            setMSG.Colors = colors;
            setMSG.strStatus = strMSG.ToString();
            if (blStatus != 0)
            {

                MessageBox.Show( strMSG.ToString(), strStatus.ToUpper(), MessageBoxButtons.OK, icon);
                if (blStatus == 2)
                {
                    //tblDetail.Enabled = false;
                    //tblMachineType.Enabled = false;
                    //tblMeasureType.Enabled = false;
                    strStatus = "ERROR";
                    mivStatus = new MethodInvoker(this.UpdateUI);
                    this.BeginInvoke(mivStatus);

                }
                //tblAll.Enabled = false;
            }
            Cursor.Current = Cursors.Default;
        }
        private void UpdateUI()
        {

            Color colr = Color.White;
            this.lblStatus.Text = strStatus;

            if (strStatus.IndexOf("READY") >= 0)
            {
                this.lblStatus.BackColor = Color.Green;
                this.lblStatus.ForeColor = Color.White;
                this.lblMsg_E.Text = strStatus;
                this.lblMsg_E.ForeColor = Color.Green;
                this.lblMsg.Visible = false;
            }
            else if (strStatus.IndexOf("SUCCESS") >= 0)
            {
                colr = Color.Red;
                this.lblStatus.ForeColor = Color.White;
                this.lblStatus.BackColor = colr;
                this.lblMsg_E.Text = strStatus;
                this.lblMsg_E.ForeColor = setMSG.Colors;
                this.lblMsg.Text = setMSG.strStatus.ToString();
                this.lblMsg.Visible = true;


            }
            else if (strStatus.IndexOf("WAITING") >= 0)
            {
                this.lblStatus.BackColor = Color.Yellow;
                this.lblStatus.ForeColor = Color.Black;

            }
            else if (strStatus.IndexOf("Waitting") >= 0)
            {
                this.lblStatus.BackColor = Color.Yellow;
                this.lblStatus.ForeColor = Color.Blue;
            }
            else if (strStatus.IndexOf("ERROR") >= 0)
            {
                tblDetail.Enabled = false;
                tblMachineType.Enabled = false;
                tblMeasureType.Enabled = false;
                if (strMCType == "microscope")
                {
                    tableLayoutPanel16.Enabled = false;
                    groupBox1.Enabled = false;
                }
                colr = Color.Red;
                this.lblStatus.ForeColor = Color.White;
                this.lblStatus.BackColor = colr;
                this.lblMsg_E.Text = strStatus;
                this.lblMsg_E.ForeColor = setMSG.Colors;
                this.lblMsg.Text = setMSG.strStatus.ToString();
                this.lblMsg.Visible = true;
                if (strMCType == "vhx")
                {

                    if (bgWorker.IsBusy == false)
                    {
                        ProcTimeVHX.Stop();
                        bgWorker.CancelAsync();
                    }
                    this.btnStart.Text = "TRY AGAIN";
                }

            }
            else if (strStatus.IndexOf("Stop") >= 0)
            {
                this.lblStatus.BackColor = Color.Yellow;
                this.lblStatus.ForeColor = Color.Blue;
                this.btnStart.Text = "TRY AGAIN";
            }
            else if (strStatus.IndexOf("Record") >= 0)
            {
                colr = Color.Red;
                this.lblStatus.BackColor = colr;
                this.lblStatus.ForeColor = Color.White;
            }
            else if (strStatus.IndexOf("Warnning") >= 0)
            {
                colr = Color.DarkOrange;
                this.lblStatus.ForeColor = colr;
                this.lblMsg_E.Text = strStatus;
                this.lblMsg_E.ForeColor = setMSG.Colors;
                this.lblMsg.Text = setMSG.strStatus.ToString();
                this.lblMsg.Visible = true;


            }
            else if (strStatus.IndexOf("Process") >= 0)
            {
                colr = Color.DarkOrange;
                this.lblStatus.ForeColor = colr;

            }
            else if (strStatus.IndexOf("Complete") >= 0)
            {
                gvHeight.DataSource = null;
                gvHeight.DataSource = HeightList.OrderByDescending(o => o.SeqNo).ToList();
                gvHeight.Columns[2].Visible = false;
                this.gvHeight.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;

                clsHieght SPCs = new clsHieght();
                SPCs = HeightList.Last();
                List<clsCal> SPC = new List<clsCal>();

                SPC = SPCs.Results.ToList();
                gvSPC.DataSource = null;

                gvSPC.DataSource = SPC;
                gvHeight.DataSource = HeightList.OrderByDescending(o => o.SeqNo).ToList();
                gvSPC.BindingContext = BindingContext;

                this.lblMsg_E.Text = strStatus;
                this.lblMsg_E.ForeColor = Color.Green;
                this.lblMsg.Text = setMSG.strStatus.ToString();
                this.lblMsg.Visible = false;

            }
            else
            {
                colr = Color.Green;
                this.lblStatus.ForeColor = colr;
            }



            if (strSaveDataState == "Complete")
            {
               // Thread.Sleep(500);
                ClearControl("");

            }

            this.gvHeight.Update();
            this.lblStatus.Update();
        }

        #endregion



    }
}
