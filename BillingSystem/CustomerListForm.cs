using MySql.Data.MySqlClient;
using BillingSystem.Database;
using System;
using System.Data;
using System.Windows.Forms;

namespace BillingSystem
{
    public partial class CustomerListForm : Form
    {
        // Step 1.1 — Field to track the selected CustomerID
        // 0 means no customer is currently selected.
        private int _selectedCustomerId = 0;

        public CustomerListForm()
        {
            InitializeComponent();
            ConfigureDataGridView();
            this.Load += CustomerListForm_Load;
        }

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

        private void CustomerListForm_Load(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        // Step 1.2 — Capture CustomerID whenever selected row changes
        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            // If no row is selected (e.g., grid is cleared or empty), reset selection
            if (dgvCustomers.CurrentRow == null || dgvCustomers.SelectedRows.Count == 0)
            {
                _selectedCustomerId = 0;
                return;
            }

            // Read the CustomerID value from the selected row
            var idCell = dgvCustomers.CurrentRow.Cells["CustomerID"].Value;

            if (idCell != null && int.TryParse(idCell.ToString(), out int id))
            {
                _selectedCustomerId = id;
            }
            else
            {
                _selectedCustomerId = 0;
            }
        }

        // Step 1.4 — Open Edit Form when double-clicking a data row
        private void dgvCustomers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // e.RowIndex is -1 when column header is double-clicked — ignore it
            if (e.RowIndex < 0) return;

            OpenEditForm();
        }

        // Step 3.3 — Open AddCustomerForm in Edit Mode
        private void OpenEditForm()
        {
            if (_selectedCustomerId == 0)
            {
                MessageBox.Show("Please select a customer to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open AddCustomerForm in EDIT mode, passing the selected CustomerID
            AddCustomerForm editForm = new AddCustomerForm(_selectedCustomerId);

            // Refresh the grid automatically once the edit form closes
            editForm.FormClosed += (s, args) => LoadCustomers();

            editForm.ShowDialog(this);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddCustomerForm addCustomerForm = new AddCustomerForm();
            addCustomerForm.ShowDialog();
            LoadCustomers();
        }

        // Step 5.2 — Confirm Before Deleting
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Step 1: Make sure a customer is selected
            if (_selectedCustomerId == 0)
            {
                MessageBox.Show("Please select a customer to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Step 2: Confirm before deleting — this cannot be undone
            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this customer?\n" +
                "All billing records for this customer will also be deleted.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // Step 3: Only delete if the user clicked Yes
            if (confirm == DialogResult.Yes)
            {
                DeleteCustomer(_selectedCustomerId);
            }
            // If the user clicked No, do nothing — the record is preserved
        }

        // Step 5.1 — Execute Parameterized DELETE Statement
        private void DeleteCustomer(int customerId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Parameterized DELETE — removes exactly one row
                    string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Customer deleted successfully.",
                                "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadCustomers();           // Refresh the grid
                        }
                        else
                        {
                            MessageBox.Show("Customer could not be deleted. It may no longer exist.",
                                "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                LoadCustomers();
            }
            else
            {
                SearchCustomers(keyword);
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnSearch_Click(sender, e);
            }
        }

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

        private void SearchCustomers(string keyword)
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
                                   WHERE  FullName      LIKE @keyword
                                      OR  Address       LIKE @keyword
                                      OR  ContactNumber LIKE @keyword
                                   ORDER  BY FullName ASC;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
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

            // Ensures no row is automatically selected when loading/reloading data
            dgvCustomers.ClearSelection();
            _selectedCustomerId = 0;
        }
    }
}