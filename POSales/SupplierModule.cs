using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace POSales
{
    public partial class SupplierModule : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        string stitle = "Point Of Sales";
        Supplier supplier;
        public SupplierModule(Supplier sp)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            supplier = sp;
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void Clear()
        {
            txtAddress.Clear();
            txtConPerson.Clear();
            txtEmail.Clear();

            txtPhone.Clear();
            txtSupplier.Clear();

            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtSupplier.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string supplierName = txtSupplier.Text.Trim();
                string address = txtAddress.Text.Trim();
                string contactPerson = txtConPerson.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text.Trim();
                string active = cbActive.Text.Trim();

                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    toolTip1.Show("Supplier name is required.", txtSupplier, 0, -40);
                    return;
                }

                if (string.IsNullOrWhiteSpace(address))
                {
                    toolTip1.Show("Address is required.", txtAddress, 0, -40);
                    return;
                }

                if (MessageBox.Show("Save this record? Click Yes to confirm.", "CONFIRM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("Insert into tbSupplier (supplier, address, contactperson, phone, email, active) values (@supplier, @address, @contactperson, @phone, @email, @active) ", cn);
                    cm.Parameters.AddWithValue("@supplier", supplierName);
                    cm.Parameters.AddWithValue("@address", address);
                    cm.Parameters.AddWithValue("@contactperson", contactPerson);
                    cm.Parameters.AddWithValue("@phone", phone);
                    cm.Parameters.AddWithValue("@email", email);
                    cm.Parameters.AddWithValue("@Active", active);
                    cm.ExecuteNonQuery();
                    cn.Close();
                    MessageBox.Show("Record has been successfully saved!", "Save Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();

                    supplier.LoadSupplier();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
        }




        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Update this record? click yes to confirm.", "CONFIRM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("Update tbSupplier set supplier=@supplier, address=@address, contactperson=@contactperson, phone=@phone, email=@email, active=@active where id=@id ", cn);
                    cm.Parameters.AddWithValue("@id", lblId.Text);
                    cm.Parameters.AddWithValue("@supplier", txtSupplier.Text);
                    cm.Parameters.AddWithValue("@address", txtAddress.Text);
                    cm.Parameters.AddWithValue("@contactperson", txtConPerson.Text);
                    cm.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cm.Parameters.AddWithValue("@email", txtEmail.Text);
                    cm.Parameters.AddWithValue("@Active", cbActive.Text); 
                    cm.ExecuteNonQuery();
                    cn.Close();
                    MessageBox.Show("Record has been successfully updated!", "Update Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Warning");
            }
        }

        private void SupplierModule_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }

        private void txtFaxNo_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void SupplierModule_Load(object sender, EventArgs e)
        {

        }

        private void cbActive_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
