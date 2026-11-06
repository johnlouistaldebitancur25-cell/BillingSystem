using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BillingSystem
{
    public partial class frmBillingHistory : Form
    {
        private readonly int _customerId;

        // Updated to point to the host server IP (10.0.0.32) and remote user credentials
        private readonly string connectionString = "Server=10.0.0.32;Database=BillingDB;Uid=billing_user;Password=BillingPass123!;";

        // ALTERNATIVE (if you use a central connection helper class):
        // private readonly string connectionString = DatabaseConnection.ConnectionString;

        public frmBillingHistory(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
        }

        private void frmBillingHistory_Load(object sender, EventArgs e)
        {
            LoadCustomerDetails();
            LoadBillingHistory();
        }

        /// <summary>
        /// Retrieves and displays the customer's full name from the database.
        /// </summary>
        private void LoadCustomerDetails()
        {
            string query = "SELECT FullName FROM Customers WHERE CustomerID = @CustomerID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerID", _customerId);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string customerName = result.ToString();
                        lblHeaderTitle.Text = $"Billing History — {customerName}";
                        this.Text = $"Billing History — {customerName}";
                    }
                    else
                    {
                        lblHeaderTitle.Text = "Billing History — Unknown Customer";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customer name: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Loads billing records filtered by CustomerID into the DataGridView.
        /// </summary>
        private void LoadBillingHistory()
        {
            string query = @"SELECT 
                                BillingMonth AS `Billing Month`, 
                                PreviousReading AS `Previous Reading`, 
                                PresentReading AS `Present Reading`, 
                                (PresentReading - PreviousReading) AS `Consumption`, 
                                RatePerCubic AS `Rate Per Cubic`, 
                                TotalAmount AS `Total Amount`, 
                                Status 
                             FROM Billing 
                             WHERE CustomerID = @CustomerID 
                             ORDER BY BillingID DESC;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerID", _customerId);

                try
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvBillingHistory.DataSource = dt;

                    // Format Currency Columns if data exists
                    if (dgvBillingHistory.Columns["Rate Per Cubic"] != null)
                        dgvBillingHistory.Columns["Rate Per Cubic"].DefaultCellStyle.Format = "C2";
                    if (dgvBillingHistory.Columns["Total Amount"] != null)
                        dgvBillingHistory.Columns["Total Amount"].DefaultCellStyle.Format = "C2";

                    // Zero Billing Records Found
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show(
                            "No billing records were found for this customer.",
                            "No Records Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading billing history: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}