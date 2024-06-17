using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace POSales
{
    public partial class UserAccount : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        MainForm main;
        public string username;
        string name;
        string role;
        string accstatus;
        public UserAccount(MainForm mn)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            main = mn;
            LoadUser();
        }

        public void LoadUser()
        {
            int i = 0;
            dgvUser.Rows.Clear();
            cm = new SqlCommand("SELECT * FROM tbUser", cn);
            cn.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvUser.Rows.Add(i, dr[0].ToString(), dr[3].ToString(), dr[4].ToString(), dr[2].ToString());
            }
            dr.Close();
            cn.Close();
        }

        

        public void Clear()
        {
            txtName.Clear();
            txtPass.Clear();
            txtRePass.Clear();
            txtUsername.Clear();
            txtEmail.Clear();
            cbRole.Text = "";
            txtUsername.Focus();
        }
        private void btnAccSave_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string plainPassword = txtPass.Text.Trim();
                string role = cbRole.Text.Trim();
                string email = txtEmail.Text.Trim();
                string name = txtName.Text.Trim();
                string isActivated = "active";

                if (!ValidateInput(username, plainPassword, email, name, role))
                {
                    return;
                }

                // Check if the username already exists
                if (UsernameExists(username))
                {
                    MessageBox.Show("Username already exists. Please choose a different username.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
                {
                    connection.Open();

                    // Hash the password using SHA-256
                    string hashedPassword = PasswordHasher.HashPassword(plainPassword);

                    // Ensure the hashed password is no longer than 50 characters
                    if (hashedPassword.Length > 50)
                    {
                        hashedPassword = hashedPassword.Substring(0, 50);
                    }

                    string insertQuery = "INSERT INTO tbUser(username, password, role, email, name, isactivate) VALUES (@username, @password, @role, @email, @name, @isActivated)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", hashedPassword); // Store the truncated hashed password
                        command.Parameters.AddWithValue("@role", role);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@isActivated", isActivated);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("New account has been successfully saved!", "Save Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Clear();
                LoadUser();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning");
            }
        }

        private bool UsernameExists(string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM tbUser WHERE username = @username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }
        }


        // Function to hash the password using SHA-256 and truncate to 50 characters
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // Ensure the hashed password is no longer than 50 characters
                if (hashedPassword.Length > 50)
                {
                    hashedPassword = hashedPassword.Substring(0, 50);
                }

                return hashedPassword;
            }
        }



        private bool ValidateInput(string username, string password, string email, string name, string role)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Role is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }


        private void btnAccCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPassSave_Click(object sender, EventArgs e)
        {
            try
            {
                string currentPassword = txtCurPass.Text;
                string newPassword = txtNPass.Text;
                string confirmNewPassword = txtRePass2.Text;

                // Retrieve the current hashed password from the database
                string currentHashedPassword = RetrieveCurrentHashedPasswordFromDatabase(lblUsername.Text);

                if (currentHashedPassword != PasswordHasher.HashPassword(currentPassword))
                {
                    MessageBox.Show("Current password did not match!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (newPassword != confirmNewPassword)
                {
                    MessageBox.Show("Confirm new password did not match!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Hash the new password before updating it
                string hashedNewPassword = PasswordHasher.HashPassword(newPassword);

                // Update the user's password in the database
                dbcon.ExecuteQuery("UPDATE tbUser SET password = '" + hashedNewPassword + "' WHERE username = '" + lblUsername.Text + "'");
                MessageBox.Show("Password has been successfully changed!", "Changed Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private string RetrieveCurrentHashedPasswordFromDatabase(string username)
        {
            // Replace this with your database query to fetch the user's hashed password based on their username
            string query = "SELECT password FROM tbUser WHERE username = @username";

            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", username);
                connection.Open();
                object result = command.ExecuteScalar();

                if (result != null)
                {
                    return result.ToString();
                }

                // Handle the case where the user is not found or any other error.
                return null;
            }
        }


        private void UserAccount_Load(object sender, EventArgs e)
        {
            lblUsername.Text = main.lblUsername.Text;
        }

        private void btnPassCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void ClearCP()
        {
            txtCurPass.Clear();
            txtNPass.Clear();
            txtRePass2.Clear();
        }

        private void dgvUser_SelectionChanged(object sender, EventArgs e)
        {
            int i = dgvUser.CurrentRow.Index;
            username = dgvUser[1, i].Value.ToString();
            name = dgvUser[2, i].Value.ToString();
            role = dgvUser[4, i].Value.ToString();
            accstatus = dgvUser[3, i].Value.ToString();
            if (lblUsername.Text == username)
            {
                btnRemove.Enabled = false;
                btnResetPass.Enabled = false;
                lblAccNote.Text = "To change your password, go to change password tag.";

            }
            else
            {
                btnRemove.Enabled = true;
                btnResetPass.Enabled = true;
                lblAccNote.Text = "To change the password for " + username + ", click Reset Password.";
            }
            gbUser.Text = "Password For " + username;

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if ((MessageBox.Show("You chose to remove this account from this Point Of Sales System's user list. \n\n Are you sure you want to remove '" + username + "' \\ '" + role + "'", "User Account", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes))
            {
                dbcon.ExecuteQuery("DELETE FROM tbUser WHERE username = '" + username + "'");
                MessageBox.Show("Account has been successfully deleted");
                LoadUser();
            }
        }

        private void btnResetPass_Click(object sender, EventArgs e)
        {
            ResetPassword reset = new ResetPassword(this);
            reset.ShowDialog();
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            UserProperties properties = new UserProperties(this);
            properties.Text = name + "\\" + username + " Properties";
            properties.txtName.Text = name;
            properties.cbRole.Text = role;
            properties.cbActivate.Text = accstatus;
            properties.username = username;
            properties.ShowDialog();
        }

        private void gbUser_Enter(object sender, EventArgs e)
        {

        }

        private void lblAccNote_Click(object sender, EventArgs e)
        {

        }

        private void lblUsername_Click(object sender, EventArgs e)
        {

        }

        private void metroTabPage2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvUser_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 5 && e.Value is string value)
            {
                if (value.Equals("True", StringComparison.OrdinalIgnoreCase) || value.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    e.Value = "Active";
                }
                else if (value.Equals("False", StringComparison.OrdinalIgnoreCase) || value.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    e.Value = "Inactive";
                }
                e.FormattingApplied = true;
            }
        }

        private void cbRole_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
