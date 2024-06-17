using System;
using System.Windows.Forms;

namespace POSales
{
    public partial class EmailReset : Form
    {
        private string email;
        private DBConnect dbConnect;     

        public EmailReset(string email)
        {
            InitializeComponent();
            this.email = email;     
            dbConnect = new DBConnect();    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string newPassword = txtResetPass.Text;
            string confirmPassword = txtResetPassVer.Text;

            if (newPassword == confirmPassword)
            {
                try
                {
                    // Hash the new password
                    string hashedNewPassword = PasswordHasher.HashPassword(newPassword);

                    string updateQuery = "UPDATE [dbo].[tbUser] SET [password] = @NewPassword WHERE email = @Email";
                    dbConnect.ExecuteQuery(updateQuery, new
                    {
                        NewPassword = hashedNewPassword,
                        Email = email
                    });

                    MessageBox.Show("Password reset successfully");
                    Login loginForm = new Login();

                    loginForm.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("The new password entries do not match. Please enter the same password.");
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();

            loginForm.Show();
            this.Close();
        }

        private void EmailReset_Load(object sender, EventArgs e)
        {

        }
    }
}
