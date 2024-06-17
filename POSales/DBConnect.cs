using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic; 

namespace POSales
{
    class DBConnect
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        private string con;

        public string myConnection()
        {
            con = @"Server=LAPTOP-2JMKFQ3P\SQLEXPRESS;Database=pos;Integrated Security=True;Connect Timeout=30";
            return con;
        }

        public DataTable getTable(string qury, Dictionary<string, object> parameters = null)
        {
            cn.ConnectionString = myConnection();
            cm = new SqlCommand(qury, cn);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cm.Parameters.AddWithValue("@" + param.Key, param.Value);
                }
            }

            SqlDataAdapter adapter = new SqlDataAdapter(cm);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void ExecuteQuery(String sql, object parameters = null)
        {
            try
            {
                cn.ConnectionString = myConnection();
                cn.Open();
                cm = new SqlCommand(sql, cn);

                if (parameters != null)
                {
                    // Add parameters to the SqlCommand
                    foreach (var param in parameters.GetType().GetProperties())
                    {
                        cm.Parameters.AddWithValue("@" + param.Name, param.GetValue(parameters));
                    }
                }

                cm.ExecuteNonQuery();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public String getPassword(string username)
        {
            string password = "";
            cn.ConnectionString = myConnection();
            cn.Open();
            cm = new SqlCommand("SELECT password FROM tbUser WHERE username = @Username", cn);
            cm.Parameters.AddWithValue("@Username", username);
            dr = cm.ExecuteReader();
            dr.Read();
            if (dr.HasRows)
            {
                password = dr["password"].ToString();
            }
            dr.Close();
            cn.Close();
            return password;
        }

        public double ExtractData(string sql)
        {
            cn = new SqlConnection();
            cn.ConnectionString = myConnection();
            cn.Open();
            cm = new SqlCommand(sql, cn);
            double data = double.Parse(cm.ExecuteScalar().ToString());
            cn.Close();
            return data;
        }
    }
}
