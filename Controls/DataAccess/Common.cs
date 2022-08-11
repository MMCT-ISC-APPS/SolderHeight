using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SolidHeight.Controls.DataAccess
{
    class Common
    {
        private SqlTransaction trans;
        private SqlConnection conn;
       
        public Common(string strDb)
        {
             
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings[strDb].ToString());
        }


        #region DB Transaction

        public string ExecuteData(SqlCommand sqlCmd)
        {
            sqlCmd.Connection = conn;
            string s = sqlCmd.CommandText;
            sqlCmd.Transaction = trans;
            try
            {
                int rowEffect = sqlCmd.ExecuteNonQuery();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public void OpenConnect()
        {
            try
            {
                bool networkUp = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (networkUp)
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();
                }
                else
                {
                     throw new InvalidOperationException("Network is not connected. Please Checking");
                }
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        public void CommitTrans()
        {
            trans.Commit();
            trans = null;
            conn.Close();
        }

        public void RollBackTrans()
        {
            if (trans != null)
            {
                trans.Rollback();
                trans = null;
                conn.Close();
            }
        }

        public void closeConnect()
        {
            conn.Close();
        }

        public DataTable ExecuteReader(SqlCommand sqlCmd)
        {
            DataTable dt = new DataTable();
            sqlCmd.Connection = conn;
            string s = sqlCmd.CommandText;

            SqlDataReader reader = sqlCmd.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        #endregion

    }
}
