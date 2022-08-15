using SolidHeight.Controls.DataAccess;
using SolidHeight.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SolidHeight.Controls
{
    class clsSPC
    {


        Common common;
        DataTable dt;
        string strSQL;

        public DataTable getJobInfo(string strSerialNo)
        {
            try
            {
                dt = new DataTable();
                strSQL = "up_GetJobInfo";
                SqlCommand cmd = new SqlCommand();
                List<SqlParameter> sqlParam = new List<SqlParameter>();
                sqlParam.Add(new SqlParameter("@vSerialNo", strSerialNo));
                dt = ExecuteTable(strSQL, sqlParam, "mp2");
                return dt;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public DataTable getJobInfo(string strSerialNo, string JobName)
        {
            try
            {
                dt = new DataTable();
                strSQL = "EXEC [up_GetJobInfo_with_JOBNAME] '" + strSerialNo + "','" + JobName + "'";
                dt = GetDataTable(strSQL, "mp2");
                return dt;
            }
            catch (Exception)
            {

                throw;
            }

        }
        
        internal DataTable GetSubModelConfig(string strSubModel, string strSPCType)
        {
            try
            {
                dt = new DataTable();
                strSQL = "EXEC up_GetSubModelConfig '" + strSubModel + "','" + strSPCType + "';";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal DataTable GetLotPrime(string strjobName, string strsubModel,string strSPCType, string strLotType)
        {
            try
            {


                dt = new DataTable();
                strSQL = "EXEC up_SPC_GetLotPrime '"+ strjobName+ "','" + strsubModel + "','" + strSPCType + "','" + strLotType + "';";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public DataTable GetMachineType(string strHostName)
        {
            try
            {
                dt = new DataTable();
                strSQL = "SELECT [MachineName] ,[MachineType] ,[SPCVHXPath] ,[SPCVHXMovePath] ,[Commport] ,[Settings] ,[Enables] ,[UpdDate] FROM [SPCDATA].[dbo].[SPC_Machine]";
                strSQL += "WHERE [MachineName] = '" + strHostName + "' and  [Enables] = 1;";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetCustomerSpect(string strMSM,string strSPCType)
        {
            try
            {
                dt = new DataTable();
                strSQL = "SELECT [UpperLimit] ,[LowerLimit] FROM [dbo].[SPC_CustomerSpect] ";
                strSQL += "Where [Measurement] = '" + (strMSM.Trim() == "mm" ? "mm" : "other") + "' and MeasurType = '"+ strSPCType.Trim() + "'";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataTable GetCauses()
        {
            try
            {
                dt = new DataTable();
                strSQL = "SELECT [cause_name]  FROM [SPCDATA].[dbo].[Causes]";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetActions()
        {
            try
            {
                dt = new DataTable();
                strSQL = "SELECT [action_name] FROM [SPCDATA].[dbo].[Actions]";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetShifts()
        {
            try
            {
                dt = new DataTable();
                strSQL = "SELECT [shift_name] FROM [SPCDATA].[dbo].[vewSPC_Shift]";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        internal DataTable GetSubModel(string strMPNPre)
        {
            try
            {

                int lenMPNPre = strMPNPre.Length;
                dt = new DataTable();
                strSQL = "SELECT * FROM vewSPC_models where LEFT(model_name,6) ='" + strMPNPre + "'";
                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal DataTable GetDataBinding(string iName, string[] iparam)
        {
            try
            {
                dt = new DataTable();
                strSQL = "EXEC [dbo].[up_SPC_GetDataBinding] '" + iName + "'";
                foreach (var i in iparam)
                {
                    strSQL += ",'" + i + "'";
                }

                dt = GetDataTable(strSQL);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataTable SaveSPCData(clsSPCHeader h, clsReConfirm_info rc, List<clsDataTableList> dtList)
        {
            DataTable dt = new DataTable();

            strSQL = "up_SaveSpcData";

            SqlCommand cmd = new SqlCommand();

            List<SqlParameter> sqlParam = new List<SqlParameter>();


            foreach (var item in h.GetType().GetProperties())
            { 
                var itemValue = item.GetValue(h, null);
                sqlParam.Add(new SqlParameter("@i"+item.Name, itemValue));
            }
            foreach (var item in rc.GetType().GetProperties())
            {
                var itemValue = item.GetValue(rc, null);
                sqlParam.Add(new SqlParameter("@i" + item.Name, itemValue));
            }
            foreach (var item in dtList)
            {
                sqlParam.Add(
                new SqlParameter(item.strParam, item.dt)
            );

            }
            dt = ExecuteTable(strSQL, sqlParam);

            return dt;
        }





        public DataTable GetDataTable(string strSQL)
        {
            try
            {
                common = new Common("SPCConnectionString");
                dt = new DataTable();
                common.OpenConnect();
                StringBuilder sql = new StringBuilder();
                sql.Append(strSQL);
                SqlCommand sqlCmd = new SqlCommand(sql.ToString());
                dt = common.ExecuteReader(sqlCmd);
                common.closeConnect();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetDataTable(string strSQL, string db)
        {
            try
            {
                common = new Common("MPrint2ConnectionString");
                dt = new DataTable();
                common.OpenConnect();
                StringBuilder sql = new StringBuilder();
                sql.Append(strSQL);
                SqlCommand sqlCmd = new SqlCommand(sql.ToString());
                dt = common.ExecuteReader(sqlCmd);
                common.closeConnect();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable ExecuteTable(string strSQL, List<SqlParameter> sqlParam)
        {
            try
            {
                common = new Common("SPCConnectionString");
                dt = new DataTable(); 
                common.OpenConnect();
                StringBuilder sql = new StringBuilder();
                sql.Append(strSQL);
                SqlCommand sqlCmd = new SqlCommand(sql.ToString());
                sqlCmd.CommandType = CommandType.StoredProcedure;

                foreach (var item in sqlParam)
                {
                    sqlCmd.Parameters.Add(item);
                }
                dt = common.ExecuteReader(sqlCmd);
                common.closeConnect();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable ExecuteTable(string strSQL, List<SqlParameter> sqlParam, string db)
        {
            try
            {
                common = new Common("MPrint2ConnectionString");
                dt = new DataTable();
                common.OpenConnect();
                StringBuilder sql = new StringBuilder();
                sql.Append(strSQL);
                SqlCommand sqlCmd = new SqlCommand(sql.ToString());
                sqlCmd.CommandType = CommandType.StoredProcedure;

                foreach (var item in sqlParam)
                {
                    sqlCmd.Parameters.Add(item);
                }
                dt = common.ExecuteReader(sqlCmd);
                common.closeConnect();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
