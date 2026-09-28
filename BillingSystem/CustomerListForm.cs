using MySql.Data.MySqlClient;
using BillingSystem.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BillingSystem
{
    public partial class CustomerListForm : Form
    {
        public CustomerListForm()
        {
            InitializeComponent();
            ConfigureDataGridView(); // Step 5.4 — Bind DataGridView columns on initialization
        }

        // Step 5.4 — Map DataGridView columns to database column names
        private void ConfigureDataGridView()
        {
            dgvCustomers.AutoGenerateColumns = false;
            dgvCustomers.Columns["CustomerID"].DataPropertyName = "CustomerID";
            dgvCustomers.Columns["FullName"].DataPropertyName = "FullName";
            dgvCustomers.Columns["Address"].DataPropertyName = "Address";
            dgvCustomers.Columns["ContactNumber"].DataPropertyName = "ContactNumber";
            dgvCustomers.Columns["Email"].DataPropertyName = "Email";
            dgvCustomers.Columns["Balance"].DataPropertyName = "Balance";
        }

        // Step 4.4 — Load customers when form opens
        private void CustomerListForm_Load(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddCustomerForm addCustomerForm = new AddCustomerForm();
            addCustomerForm.ShowDialog();
            LoadCustomers();   // Refreshes the grid when AddCustomerForm closes
        }

        // Step 5.2 — Search Button Click Event
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                // Empty search box → show all customers again
                LoadCustomers();
            }
            else
            {
                SearchCustomers(keyword);
            }
        }

        // Step 5.3 — Enter Keypress Event Handler
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Suppress Windows beep sound
                btnSearch_Click(sender, e);
            }
        }

        // Step 4.3 — Write the LoadCustomers Method
        private void LoadCustomers()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT CustomerID,
                                          FullName,
                                          Address,
                                          ContactNumber,
                                          Email,
                                          Balance,
                                          Status
                                   FROM   Customers
                                   ORDER  BY CustomerID ASC;";

                    using (var adapter = new MySqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        ApplyGridFormatting(dt);
                        lblTitle.Text = $"Customer List  ({dt.Rows.Count} record(s))";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Step 5.1 — Write the SearchCustomers Method
        private void SearchCustomers(string keyword)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Parameterized SELECT with WHERE ... LIKE
                    string sql = @"SELECT CustomerID,
                                          FullName,
                                          Address,
                                          ContactNumber,
                                          Email,
                                          Balance,
                                          Status
                                   FROM   Customers
                                   WHERE  FullName      LIKE @keyword
                                      OR  Address       LIKE @keyword
                                      OR  ContactNumber LIKE @keyword
                                   ORDER  BY FullName ASC;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        // %keyword% matches the search text anywhere in the column
                        cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                        using (var adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            ApplyGridFormatting(dt);
                            lblTitle.Text = $"Customer List  ({dt.Rows.Count} result(s))";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers:\n{ex.Message}",
                    "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to keep DataGridView columns properly formatted
        private void ApplyGridFormatting(DataTable dt)
        {
            dgvCustomers.DataSource = dt;

            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomers.RowHeadersVisible = false;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.AllowUserToAddRows = false;

            if (dgvCustomers.Columns.Count > 0)
            {
                dgvCustomers.Columns["CustomerID"].HeaderText = "ID";
                dgvCustomers.Columns["FullName"].HeaderText = "Full Name";
                dgvCustomers.Columns["Address"].HeaderText = "Address";
                dgvCustomers.Columns["ContactNumber"].HeaderText = "Contact No.";
                dgvCustomers.Columns["Email"].HeaderText = "Email";
                dgvCustomers.Columns["Balance"].HeaderText = "Balance (₱)";

                dgvCustomers.Columns["CustomerID"].FillWeight = 40;
                dgvCustomers.Columns["FullName"].FillWeight = 120;
                dgvCustomers.Columns["Address"].FillWeight = 120;
                dgvCustomers.Columns["ContactNumber"].FillWeight = 90;
                dgvCustomers.Columns["Email"].FillWeight = 110;
                dgvCustomers.Columns["Balance"].FillWeight = 80;
            }
        }
    }
}