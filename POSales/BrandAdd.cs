using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace POSales
{
    public partial class BrandAdd : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        Brand brand;
        public BrandAdd(Brand br)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            brand = br;
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string brandName = txtBrand.Text.Trim();

                if (string.IsNullOrWhiteSpace(brandName))
                {
                    MessageBox.Show("Please enter a brand name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to save this brand?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("INSERT INTO tbBrand(brand) VALUES (@brand)", cn);
                    cm.Parameters.AddWithValue("@brand", brandName);
                    cm.ExecuteNonQuery();
                    cn.Close();
                    MessageBox.Show("Record has been successfully saved.", "POS");
                    Clear();
                    brand.LoadBrand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        public void Clear()
        {
            txtBrand.Clear();
            btnUpdate.Enabled = false;
            btnSave.Enabled = true;
            txtBrand.Focus();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to update this brand?", "Update Record!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                cn.Open();
                cm = new SqlCommand("UPDATE tbBrand SET brand = @brand WHERE id LIKE'" + lblId.Text + "'", cn);
                cm.Parameters.AddWithValue("@brand", txtBrand.Text);
                cm.ExecuteNonQuery();
                cn.Close();
                MessageBox.Show("Brand has been successfully updated.", "POS");
                Clear();
                this.Close();
            }
        }

        private void txtBrand_TextChanged(object sender, EventArgs e)
        {

        }

        private void BrandAdd_Load(object sender, EventArgs e)
        {

        }
    }
}
