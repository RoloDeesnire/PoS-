
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.Timers;
using System.Net.Mail;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace POSales
{
    public partial class Dashboard : Form
    {
        SqlConnection cn = new SqlConnection();
        DBConnect dbcon = new DBConnect();
        private System.Timers.Timer dataRefreshTimer;

        public Dashboard()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());

            dataRefreshTimer = new System.Timers.Timer();
            dataRefreshTimer.Interval = TimeSpan.FromHours(6).TotalMilliseconds;
            dataRefreshTimer.Elapsed += DataRefreshTimerElapsed;
            dataRefreshTimer.AutoReset = true;
            dataRefreshTimer.Start();
        }

        private void DataRefreshTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckProductsNearExpiryAndCriticalStocks();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            string sdate = DateTime.Now.ToShortDateString();
            lblDalySale.Text = dbcon.ExtractData("SELECT ISNULL(SUM(total),0) AS total FROM tbCart WHERE status LIKE 'Sold' AND sdate BETWEEN '" + sdate + "' AND '" + sdate + "'").ToString("#,##0.00");
            lblTotalProduct.Text = dbcon.ExtractData("SELECT COUNT(*) FROM tbProduct").ToString("#,##0");
            lblStockOnHand.Text = dbcon.ExtractData("SELECT ISNULL(SUM(qty), 0) AS qty FROM tbProduct").ToString("#,##0");
            lblCriticalItems.Text = dbcon.ExtractData("SELECT COUNT(*) FROM vwCriticalItems").ToString("#,##0");
            CheckProductsNearExpiryAndCriticalStocks();

            Series series = new Series("Daily Sales")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Date,
                BorderWidth = 2, // Adjust the line width
                Color = Color.Green,
            };

            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            {
                connection.Open();
                string query = "SELECT sdate, ISNULL(SUM(total), 0) AS total FROM tbCart WHERE status LIKE 'Sold' GROUP BY sdate";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime date = reader.GetDateTime(0);
                    decimal total = reader.GetDecimal(1);

                    DataPoint dataPoint = new DataPoint(date.ToOADate(), (double)total);
                    dataPoint.Color = Color.Green;
                    series.Points.Add(dataPoint);
                }
            }

            chart1.Series.Clear();
            chart1.Series.Add(series);

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "MMM dd, yyyy";
            chart1.ChartAreas[0].AxisX.Title = "Date";
            chart1.ChartAreas[0].AxisY.Title = "Amount";

            Title chartTitle = new Title("Daily Sales Chart");
            chartTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            chart1.Titles.Add(chartTitle);

            chart1.Legends.Add(new Legend("SalesLegend"));
            chart1.Series[0].Legend = "SalesLegend";

            chart1.Legends["SalesLegend"].Docking = Docking.Bottom;
            chart1.Legends["SalesLegend"].Alignment = StringAlignment.Center;

            DataTable topSellingItems = GetTopSellingItemsFromDatabase();

            Series pieSeries = new Series("Top Selling Items")
            {
                ChartType = SeriesChartType.Pie,
            };

            int totalQuantity = 0; // Calculate the total quantity of all products

            foreach (DataRow row in topSellingItems.Rows)
            {
                string itemName = row["pdesc"].ToString();
                int quantity = Convert.ToInt32(row["qty"]);

                DataPoint dataPoint = new DataPoint();
                dataPoint.AxisLabel = itemName;
                dataPoint.YValues = new double[] { quantity };
                pieSeries.Points.Add(dataPoint);

                totalQuantity += quantity;
            }

            foreach (DataPoint dataPoint in pieSeries.Points)
            {
                double quantity = dataPoint.YValues[0];
                double percentage = (quantity / totalQuantity) * 100;
                dataPoint.Label = $"{dataPoint.AxisLabel} ({percentage:F2}%)";
            }

            chart2.Series.Clear();
            chart2.Series.Add(pieSeries);


            Title pieChartTitle = new Title("Fast Moving Items");
            pieChartTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            chart2.Titles.Add(pieChartTitle);

            chart2.ChartAreas[0].Area3DStyle.Enable3D = true;
            chart2.ChartAreas[0].Area3DStyle.Inclination = 30;

            chart2.Legends.Add(new Legend("Legend"));
            chart2.Series[0].Legend = "Legend";
            chart2.Series[0].IsVisibleInLegend = true;

            chart2.Legends["Legend"].Docking = Docking.Bottom;
            chart2.Legends["Legend"].IsDockedInsideChartArea = false;
            chart2.Legends["Legend"].Position.Auto = true;

            // Adjust label style to fit text
            chart2.Series[0]["PieLabelStyle"] = "Outside";
            chart2.Series[0]["PieLineColor"] = "Black";
            chart2.Series[0]["PieLabelStyleOutsideRadius"] = "30";

            // Execute the SQL command to retrieve the data
            DataTable chartData = GetChartData();

            // Populate the chart control with the data
            PopulateChart(chart3, chartData);
        }

        private DataTable GetChartData()
        {
            DataTable chartData = new DataTable();

            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            {
                connection.Open();
                string query = "SELECT TOP 5 p.pcode, p.barcode, p.pdesc, p.qty AS 'Stock on Hand', p.price AS 'Price' " +
                               "FROM tbProduct AS p " +
                               "WHERE NOT EXISTS (" +
                               "    SELECT 1 FROM tbCart AS c " +
                               "    WHERE c.pcode = p.pcode AND c.sdate >= DATEADD(WEEK, -1, GETDATE())" +
                               ") " +
                               "AND EXISTS (" +
                               "    SELECT 1 FROM tbStockin AS s " +
                               "    WHERE s.pcode = p.pcode AND s.sdate <= DATEADD(WEEK, -1, GETDATE())" +
                               ")";

                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(chartData);
            }

            return chartData;
        }


        private void PopulateChart(Chart chart, DataTable chartData)
        {
            chart.Series.Clear();

           
            Series series = new Series("Stock Data")
            {
                ChartType = SeriesChartType.Column, 
            };

            
            foreach (DataRow row in chartData.Rows)
            {
                string pdesc = row["pdesc"].ToString();
                int stockOnHand = Convert.ToInt32(row["Stock on Hand"]);

                DataPoint dataPoint = new DataPoint();
                dataPoint.AxisLabel = pdesc;
                dataPoint.YValues = new double[] { stockOnHand };

                series.Points.Add(dataPoint);
            }

          
            chart.Series.Add(series);

        
            chart.ChartAreas[0].AxisX.Title = "Product Name";
            chart.ChartAreas[0].AxisY.Title = "Stock on Hand";

           
            Title chartTitle = new Title("Inactive Items Chart")
            {
                Font = new Font("Arial", 16, FontStyle.Bold)
            };
            chart.Titles.Add(chartTitle);

            chart.Invalidate(); // Refresh the chart
        }

        private DataTable GetTopSellingItemsFromDatabase()
        {
            DataTable topSellingItems = new DataTable();

            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            {
                connection.Open();

               
                string query = @"
            SELECT TOP 5 p.pdesc, SUM(c.qty) AS qty
            FROM tbCart AS c
            INNER JOIN tbProduct AS p ON c.pcode = p.pcode
            WHERE c.status = 'Sold' 
            AND c.sdate >= DATEADD(day, -7, GETDATE())
            GROUP BY p.pdesc
            ORDER BY SUM(c.qty) DESC";

                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(topSellingItems);
            }

            return topSellingItems;
        }

        private DataTable GetCriticalStockProducts()
        {
            string criticalStockQuery = "SELECT pcode, pdesc, qty FROM tbProduct WHERE qty <= reorder";
            return dbcon.getTable(criticalStockQuery);
        }

        private void CheckProductsNearExpiryAndCriticalStocks()
        {
            DateTime thresholdDate = DateTime.Now.AddDays(7);

            Dictionary<string, object> nearExpiryParameters = new Dictionary<string, object>();
            nearExpiryParameters.Add("ThresholdDate", thresholdDate);

            string nearExpiryQuery = "SELECT pcode, pdesc, expiry, qty FROM tbProduct WHERE expiry <= @ThresholdDate ORDER BY expiry";
            DataTable nearExpiryProducts = dbcon.getTable(nearExpiryQuery, nearExpiryParameters);

            string criticalStockQuery = "SELECT pcode, pdesc, qty FROM tbProduct WHERE qty <= reorder ORDER BY qty DESC";
            DataTable criticalStockProducts = dbcon.getTable(criticalStockQuery);

            if (nearExpiryProducts.Rows.Count > 0 || criticalStockProducts.Rows.Count > 0)
            {
                string subject = "Products Near Expiry and Critical Stocks";
                string body = "<html><body>";

                if (nearExpiryProducts.Rows.Count > 0)
                {
                    body += "<h2>Products near expiry or expired:</h2>";
                    body += "<table style='width:auto; border-collapse: collapse; border: 1px solid #000;'>";
                    body += "<tr><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Product Code</th><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Product Name</th><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Expiry Date</th><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Current Quantity</th></tr>";

                    foreach (DataRow row in nearExpiryProducts.Rows)
                    {
                        string productCode = row["pcode"].ToString();
                        string productName = row["pdesc"].ToString();
                        DateTime expiryDate = (DateTime)row["expiry"];
                        int currentQuantity = Convert.ToInt32(row["qty"]);

                        body += "<tr>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + productCode + "</td>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + productName + "</td>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + expiryDate.ToString("MMM dd, yyyy") + "</td>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + currentQuantity + "</td>";
                        body += "</tr>";
                    }

                    body += "</table>";
                }

                if (criticalStockProducts.Rows.Count > 0)
                {
                    body += "<h2>Low Stock Products:</h2>";
                    body += "<table style='width:auto; border-collapse: collapse; border: 1px solid #000;'>";
                    body += "<tr><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Product Code</th><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Product Name</th><th style='border: 1px solid #000; padding: 6px; background-color: #f2f2f2;'>Current Quantity</th></tr>";

                    foreach (DataRow row in criticalStockProducts.Rows)
                    {
                        string productCode = row["pcode"].ToString();
                        string productName = row["pdesc"].ToString();
                        int currentQuantity = Convert.ToInt32(row["qty"]);

                        body += "<tr>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + productCode + "</td>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + productName + "</td>";
                        body += "<td style='border: 1px solid #000; padding: 6px; font-family: Lato, sans-serif; color: #000;'>" + currentQuantity + "</td>";
                        body += "</tr>";
                    }

                    body += "</table>";
                }

                body += "</body></html>";



                bool emailSent = CheckIfEmailAlreadySent(nearExpiryProducts);

                if (!emailSent)
                {
                    string smtpServer = "smtp.gmail.com";
                    int smtpPort = 587;
                    string senderName = " ";//name
                    string senderEmail = " "; //email you want to receive it from
                    string smtpPassword = "rvfc ghhm bxta apne\r\n";

                    SmtpClient smtpClient = new SmtpClient(smtpServer)
                    {
                        Port = smtpPort,
                        Credentials = new NetworkCredential(senderEmail, smtpPassword),
                        EnableSsl = true,
                    };

                    MailMessage mailMessage = new MailMessage()
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true, 
                        From = new MailAddress(senderEmail, senderName),
                    };

                    mailMessage.To.Add(" "); //sender email

                    try
                    {
                        smtpClient.Send(mailMessage);

                        RecordSentEmails(nearExpiryProducts);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Email sending failed: " + ex.Message);
                    }
                }
            }
        }




        private bool CheckIfEmailAlreadySent(DataTable nearExpiryProducts)
        {
            List<string> productCodes = new List<string>();

            foreach (DataRow row in nearExpiryProducts.Rows)
            {
                string productCode = row["pcode"].ToString();
                productCodes.Add(productCode);
            }

            string query = @"
        SELECT ProductCode, MAX(SentDateTime) AS LatestSentDateTime
        FROM SentEmails
        WHERE ProductCode IN ('" + string.Join("', '", productCodes) + @"')
        GROUP BY ProductCode";

            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string productCode = reader["ProductCode"].ToString();

                    DateTime latestSentDateTime = (DateTime)reader["LatestSentDateTime"];
                    DateTime oneDayAgo = DateTime.Now.AddDays(-1);   // change this for testing purposes DateTime.Now.AddDays(-1); to check once a day
                    if (latestSentDateTime < oneDayAgo)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RecordSentEmails(DataTable nearExpiryProducts)
        {
            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            {
                connection.Open();

                foreach (DataRow row in nearExpiryProducts.Rows)
                {
                    string productCode = row["pcode"].ToString();

                    string insertQuery = "INSERT INTO SentEmails (ProductCode, SentDateTime) VALUES (@ProductCode, @SentDateTime)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductCode", productCode);
                        cmd.Parameters.AddWithValue("@SentDateTime", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

       




        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void bunifuGradientPanel1_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalProduct_Click(object sender, EventArgs e)
        {

        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }


    }
}
