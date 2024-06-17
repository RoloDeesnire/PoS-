using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace POSales
{
    public partial class Login : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;

        public string _pass = "";
        public string _accountStatus;
        public Login()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            this.Shown += new EventHandler(Login_Shown); // Attach the Shown event handler

        }
        private void Login_Shown(object sender, EventArgs e)
        {
            // Set focus to the txtName textbox when the form is fully shown
            txtName.Focus();
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string _username = "", _name = "", _role = "";
            bool found = false;

            try
            {
                cn.Open();

                // Hash the password input
                string hashedPassword = PasswordHasher.HashPassword(txtPass.Text);

                cm = new SqlCommand("Select * From tbUser Where username = @username and password = @password", cn);
                cm.Parameters.AddWithValue("@username", txtName.Text);
                cm.Parameters.AddWithValue("@password", hashedPassword);

                dr = cm.ExecuteReader();

                if (dr.Read())
                {
                    found = true;
                    _username = dr["username"].ToString();
                    _name = dr["name"].ToString();
                    _role = dr["role"].ToString();
                    _pass = dr["password"].ToString();
                    _accountStatus = dr["isactivate"].ToString();
                }

                dr.Close();
                cn.Close();

                if (found)
                {
                    if (_accountStatus == "deactivated")
                    {
                        MessageBox.Show("Account is deactivated. Unable to login", "Inactive Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (_role == "Cashier")
                    {
                        MessageBox.Show("Welcome " + _name + "", "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtName.Clear();
                        txtPass.Clear();
                        this.Hide();
                        Cashier cashier = new Cashier();
                        cashier.lblUsername.Text = _username;
                        cashier.lblname.Text = _name + " | " + _role;
                        cashier.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Welcome " + _name + "", "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtName.Clear();
                        txtPass.Clear();
                        this.Hide();
                        MainForm main = new MainForm();
                        main.lblUsername.Text = _username;
                        main.lblName.Text = _name;
                        main._pass = _pass;
                        main.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username and password!", "ACCESS DENIED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void txtPass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnLogin.PerformClick();
            }
        }

        private void txtName_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuGradientPanel1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtPass_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {
            sendCode sc = new sendCode();
            this.Hide();
            sc.Show(); 
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
    }
}
