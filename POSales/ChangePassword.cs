using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace POSales
{
    public partial class ChangePassword : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        Cashier cashier;
        public ChangePassword(Cashier cash)
        {
            InitializeComponent();
            cashier = cash;
            lblUsername.Text = cashier.lblUsername.Text;
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                string username = lblUsername.Text;
                string oldEnteredPassword = txtPass.Text;

                // Retrieve the stored hashed password from the database
                string storedHashedPassword = dbcon.getPassword(username);

                // Hash the entered password for comparison
                string enteredHashedPassword = PasswordHasher.HashPassword(oldEnteredPassword);

                if (storedHashedPassword != enteredHashedPassword)
                {
                    MessageBox.Show("Wrong password, please try again!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    txtPass.Visible = false;
                    btnNext.Visible = false;

                    txtNewPass.Visible = true;
                    txtComPass.Visible = true;
                    btnSave.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string username = lblUsername.Text;
                string newPass = txtNewPass.Text;
                string confirmNewPass = txtComPass.Text;

                if (newPass != confirmNewPass)
                {
                    MessageBox.Show("Confirm new password does not match!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Hash the new password
                string hashedNewPassword = PasswordHasher.HashPassword(newPass);

                // Update the user's password in the database
                dbcon.ExecuteQuery("UPDATE tbUser SET password = '" + hashedNewPassword + "' WHERE username = '" + username + "'");
                MessageBox.Show("Password has been successfully changed!", "Changed Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangePassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }

        }

        private void txtComPass_Click(object sender, EventArgs e)
        {

        }
    }
}
