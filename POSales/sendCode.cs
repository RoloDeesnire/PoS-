using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace POSales
{
    public partial class sendCode : Form
    {
        private string randomCode;
        public static string to;

        public sendCode()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string from, pass, messageBody;
            string emailToReset = txtEmail.Text;

            Random rand = new Random();
            randomCode = (rand.Next(99999)).ToString();

            DBConnect dbConnect = new DBConnect();
            using (SqlConnection connection = new SqlConnection(dbConnect.myConnection()))
            {
                connection.Open();

                string query = "SELECT email FROM tbUser WHERE email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", emailToReset);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string email = result.ToString();

                        MailMessage message = new MailMessage();
                        to = email;
                        from = "abcforgotpass@gmail.com";
                        pass = "rvfc ghhm bxta apne\r\n";
                        messageBody = "Your Reset Code is " + randomCode;
                        message.To.Add(to);
                        message.From = new MailAddress(from);
                        message.Body = messageBody;
                        message.Subject = "Password Reset Code";

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                        smtp.EnableSsl = true;
                        smtp.Port = 587;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Credentials = new NetworkCredential(from, pass);

                        try
                        {
                            smtp.Send(message);
                            MessageBox.Show("Code has been sent to " + email);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error sending code to " + email + ": " + ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email not found in the database.");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string inputCode = txtVerCode.Text.Trim();

            if (randomCode == inputCode)
            {

                EmailReset er = new EmailReset(txtEmail.Text);
                this.Hide();
                er.Show();
            }
            else
            {
                MessageBox.Show("Wrong Code");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string fromName = "Abisocho";
            string fromEmail = "abcforgotpass@gmail.com";
            string pass, messageBody;
            string emailToReset = txtEmail.Text;

            Random rand = new Random();
            randomCode = (rand.Next(99999)).ToString();

            DBConnect dbConnect = new DBConnect();
            using (SqlConnection connection = new SqlConnection(dbConnect.myConnection()))
            {
                connection.Open();

                string query = "SELECT email FROM tbUser WHERE email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", emailToReset);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string email = result.ToString();

                        MailMessage message = new MailMessage();
                        to = email;
                        pass = "rvfc ghhm bxta apne\r\n";
                        messageBody = "Your Reset Code is " + randomCode;
                        message.To.Add(to);
                        message.From = new MailAddress(fromEmail, fromName);
                        message.Body = messageBody;
                        message.Subject = "Password Reset Code";

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                        smtp.EnableSsl = true;
                        smtp.Port = 587;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Credentials = new NetworkCredential(fromEmail, pass);

                        try
                        {
                            smtp.Send(message);
                            MessageBox.Show("Code has been sent to " + email);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error sending code to " + email + ": " + ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email not found in the database.");
                    }
                }
            }
        }



        private void button4_Click(object sender, EventArgs e)
        {

            string inputCode = txtVerCode.Text.Trim();

            if (randomCode == inputCode)
            {

                EmailReset er = new EmailReset(txtEmail.Text);
                this.Hide();
                er.Show();
            }
            else
            {
                MessageBox.Show("Wrong Code");
            }
        }

        private void sendCode_Load(object sender, EventArgs e)
        {

        }

        private void picClose_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();

            loginForm.Show();
            this.Close();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            Login loginForm = new Login();

            loginForm.Show();
            this.Close();
        }

        private void sendCode_Load_1(object sender, EventArgs e)
        {

        }
    }
}
