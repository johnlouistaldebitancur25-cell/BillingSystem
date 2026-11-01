using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using BillingSystem.Database;
using BillingSystem.Utils;

namespace BillingSystem
{
    public partial class frmAuditLogs : Form
    {
        public frmAuditLogs()
        {
            InitializeComponent();
        }

        private void frmAuditLogs_Load(object sender, EventArgs e)
        {
            // Default date range: last 30 days
            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today;
            LoadAuditLogs(dtpFrom.Value, dtpTo.Value);

            // Log that the user opened the audit log report
            AuditLogger.Log("VIEW_AUDIT_LOG",
                $"{AppSession.CurrentUsername} viewed the Audit Log Report.");
        }

        private void LoadAuditLogs(DateTime from, DateTime to)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Position LogDate second so it is immediately visible in the grid
                    string sql = @"SELECT LogID, LogDate, Username, Role, Action, Details
                                   FROM   AuditLogs
                                   WHERE  LogDate BETWEEN @From AND @To
                                   ORDER  BY LogDate DESC;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@From", from.Date);
                        cmd.Parameters.AddWithValue("@To",
                            to.Date.AddDays(1).AddSeconds(-1));

                        using (var adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvAuditLogs.DataSource = dt;

                            // Apply formatting and column sizing to DataGridView
                            ConfigureGridColumns();

                            lblTitle.Text =
                                $"Audit Log Report  ({dt.Rows.Count} record(s))";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audit logs:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            dgvAuditLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAuditLogs.RowHeadersVisible = false;
            dgvAuditLogs.AllowUserToAddRows = false;

            if (dgvAuditLogs.Columns.Count > 0)
            {
                dgvAuditLogs.Columns["LogID"].HeaderText = "Log ID";
                dgvAuditLogs.Columns["LogID"].FillWeight = 40;

                if (dgvAuditLogs.Columns.Contains("LogDate"))
                {
                    dgvAuditLogs.Columns["LogDate"].HeaderText = "Date / Time";
                    dgvAuditLogs.Columns["LogDate"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                    dgvAuditLogs.Columns["LogDate"].FillWeight = 90;
                }

                dgvAuditLogs.Columns["Username"].HeaderText = "Username";
                dgvAuditLogs.Columns["Username"].FillWeight = 70;

                dgvAuditLogs.Columns["Role"].HeaderText = "Role";
                dgvAuditLogs.Columns["Role"].FillWeight = 60;

                dgvAuditLogs.Columns["Action"].HeaderText = "Action";
                dgvAuditLogs.Columns["Action"].FillWeight = 90;

                dgvAuditLogs.Columns["Details"].HeaderText = "Details";
                dgvAuditLogs.Columns["Details"].FillWeight = 160;
            }
        }

        private bool ValidateDateRange()
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show("Start date cannot be later than end date.",
                    "Invalid Date Range", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!ValidateDateRange()) return;
            LoadAuditLogs(dtpFrom.Value, dtpTo.Value);
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                ValidateDateRange();
            }
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                ValidateDateRange();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}