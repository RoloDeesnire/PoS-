using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace POSales
{
    public partial class ProductModule : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        string stitle = "Point Of Sales";
        Product product;
        public ProductModule(Product pd)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            product = pd;
            LoadBrand();
            LoadCategory();
        }

        public void LoadCategory()
        {
            cboCategory.Items.Clear();
            cboCategory.DataSource = dbcon.getTable("SELECT * FROM tbCategory");
            cboCategory.DisplayMember = "category";
            cboCategory.ValueMember = "id";
        }

        public void LoadBrand()
        {
            cboBrand.Items.Clear();
            cboBrand.DataSource = dbcon.getTable("SELECT * FROM tbBrand");
            cboBrand.DisplayMember = "brand";
            cboBrand.ValueMember = "id";
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void Clear()
        {
            txtPcode.Clear();
            txtBarcode.Clear();
            txtPdesc.Clear();
            txtPrice.Clear();
            cboBrand.SelectedIndex = 0;
            cboCategory.SelectedIndex = 0;
            UDReOrder.Value = 1;

            txtPcode.Enabled = true;
            txtPcode.Focus();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string pcode = txtPcode.Text.Trim();
                string barcode = txtBarcode.Text.Trim();
                string pdesc = txtPdesc.Text.Trim();
                double price = 0;

                if (string.IsNullOrWhiteSpace(pcode))
                {
                    toolTip1.Show("Product code is required.", txtPcode, 0, -40);
                    return;
                }

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    toolTip1.Show("Barcode is required.", txtBarcode, 0, -40);
                    return;
                }

                if (string.IsNullOrWhiteSpace(pdesc))
                {
                    toolTip1.Show("Product description is required.", txtPdesc, 0, -40);
                    return;
                }

                if (!double.TryParse(txtPrice.Text, out price))
                {
                    toolTip1.Show("Invalid price format.", txtPrice, 0, -40);
                    return;
                }

                if (cboBrand.SelectedIndex == -1)
                {
                    toolTip1.Show("Please select a brand.", cboBrand, 0, -40);
                    return;
                }

                if (cboCategory.SelectedIndex == -1)
                {
                    toolTip1.Show("Please select a category.", cboCategory, 0, -40);
                    return;
                }

                // Check if the product code already exists
                if (ProductCodeExists(pcode))
                {
                    MessageBox.Show("Product with the same Product Code already exists.", "Duplicate Product Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the barcode already exists
                if (BarcodeExists(barcode))
                {
                    MessageBox.Show("Product with the same Barcode already exists.", "Duplicate Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to save this product?", "Save Product", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cm = new SqlCommand("INSERT INTO tbProduct(pcode, barcode, pdesc, bid, cid, price, reorder, expiry) VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder, @expiry); SELECT SCOPE_IDENTITY()", cn);
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cm.Parameters.AddWithValue("@barcode", barcode);
                    cm.Parameters.AddWithValue("@pdesc", pdesc);
                    cm.Parameters.AddWithValue("@bid", cboBrand.SelectedValue);
                    cm.Parameters.AddWithValue("@cid", cboCategory.SelectedValue);
                    cm.Parameters.AddWithValue("@price", price);
                    cm.Parameters.AddWithValue("@reorder", UDReOrder.Value);
                    cm.Parameters.AddWithValue("@expiry", dateTimePicker1.Value);
                    cn.Open();

                    // Use ExecuteScalar to retrieve the auto-generated ProductID
                    int newProductID = Convert.ToInt32(cm.ExecuteScalar());

                    cn.Close();

                    MessageBox.Show("Product has been successfully saved with ProductID " + newProductID, stitle);
                    Clear();
                    product.LoadProduct();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ProductCodeExists(string pcode)
        {
            try
            {
                cm = new SqlCommand("SELECT COUNT(*) FROM tbProduct WHERE pcode = @pcode", cn);
                cm.Parameters.AddWithValue("@pcode", pcode);
                cn.Open();

                int count = Convert.ToInt32(cm.ExecuteScalar());

                return count > 0;
            }
            finally
            {
                cn.Close();
            }
        }

        private bool BarcodeExists(string barcode)
        {
            try
            {
                cm = new SqlCommand("SELECT COUNT(*) FROM tbProduct WHERE barcode = @barcode", cn);
                cm.Parameters.AddWithValue("@barcode", barcode);
                cn.Open();

                int count = Convert.ToInt32(cm.ExecuteScalar());

                return count > 0;
            }
            finally
            {
                cn.Close();
            }
        }




        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure want to update this product?", "Update Product", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cm = new SqlCommand("UPDATE tbProduct SET barcode=@barcode,pdesc=@pdesc,bid=@bid,cid=@cid,price=@price, expiry=@expiry, reorder=@reorder WHERE pcode LIKE @pcode", cn);
                    cm.Parameters.AddWithValue("@pcode", txtPcode.Text);
                    cm.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cm.Parameters.AddWithValue("@pdesc", txtPdesc.Text);
                    cm.Parameters.AddWithValue("@bid", cboBrand.SelectedValue);
                    cm.Parameters.AddWithValue("@cid", cboCategory.SelectedValue);
                    cm.Parameters.AddWithValue("@price", double.Parse(txtPrice.Text));
                    cm.Parameters.AddWithValue("@reorder", UDReOrder.Value);
                    cm.Parameters.AddWithValue("@expiry", dateTimePicker1.Value);
                    cn.Open();
                    cm.ExecuteNonQuery();
                    cn.Close();
                    MessageBox.Show("Product has been successfully updated.", stitle);
                    Clear();
                    this.Dispose();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void ProductModule_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }

        private void UDReOrder_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ProductModule_Load(object sender, EventArgs e)
        {
            dateTimePicker1.MinDate = DateTime.Today;
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
              
                e.Handled = true;
            }
        }
    }
}
