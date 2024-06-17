using System;
using System.Windows.Forms;

namespace POSales
{
    public partial class CancelOrder : Form
    {
        DailySale dailySale;
        public CancelOrder(DailySale sale)
        {
            InitializeComponent();
            dailySale = sale;
        }

        private void btnCOrder_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboInventory.Text != string.Empty && udCancelQty.Value > 0 && txtReason.Text != string.Empty)
                {
                    if (int.Parse(txtQty.Text) >= udCancelQty.Value)
                    {
                        Void @void = new Void(this);
                        @void.txtUsername.Focus();
                        @void.ShowDialog();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ReloadSoldList()
        {
            dailySale.LoadSold();
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void cboInventory_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CancelOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }

        private void txtReason_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtDisc_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
