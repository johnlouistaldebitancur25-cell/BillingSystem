using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient; // Ensure MySql.Data package is referenced

namespace BillingSystem
{
    public partial class frmChangePassword : Form
    {
        private readonly int _userId;
        // Replace connection string with your project's Database connection string helper
        private readonly string connectionString = "Server=localhost;Database=BillingDB;Uid=root;Password=App_Dev_@Grp6;";

        public frmChangePassword(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void btnSavePassword_Click(object sender, EventArgs e)
        {
            string currentPassword = txtCurrentPassword.Text.Trim();
            string newPassword = txtNewPassword.Text;
            string retypePassword = txtRetypePassword.Text;

            // 1. Check for blank fields
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(retypePassword))
            {
                MessageBox.Show("Please fill in all password fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate Current Password against Database
            if (!VerifyCurrentPassword(_userId, currentPassword))
            {
                MessageBox.Show("The current password you entered is incorrect.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCurrentPassword.Clear();
                txtCurrentPassword.Focus();
                return;
            }

            // 3. Password Complexity Validation
            List<string> complexityErrors = ValidatePasswordComplexity(newPassword);
            if (complexityErrors.Count > 0)
            {
                string errorMessage = "New password does not meet complexity requirements:\n\n" +
                                     string.Join("\n", complexityErrors.Select(err => "• " + err));
                MessageBox.Show(errorMessage, "Password Complexity Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            // 4. Password Match Check
            if (newPassword != retypePassword)
            {
                MessageBox.Show("The new password and retype password do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRetypePassword.Clear();
                txtRetypePassword.Focus();
                return;
            }

            // 5. Parameterized Database Update
            if (UpdateUserPassword(_userId, newPassword))
            {
                MessageBox.Show("Your password has been changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        /// <summary>
        /// Verifies the entered current password matches the record in the database for the logged-in user.
        /// </summary>
        private bool VerifyCurrentPassword(int userId, string enteredPassword)
        {
            string query = "SELECT Password FROM Users WHERE UserID = @UserID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string dbPassword = result.ToString();
                        return dbPassword == enteredPassword;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error during verification: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }

        /// <summary>
        /// Evaluates the password against Section 3.3 rules and returns missing requirements.
        /// </summary>
        private List<string> ValidatePasswordComplexity(string password)
        {
            List<string> errors = new List<string>();

            // Rule 1: Minimum Length 8
            if (password.Length < 8)
            {
                errors.Add("Must be at least 8 characters long.");
            }

            // Rule 2: Uppercase Letter
            if (!password.Any(char.IsUpper))
            {
                errors.Add("Must contain at least one uppercase letter (A-Z).");
            }

            // Rule 3: Lowercase Letter
            if (!password.Any(char.IsLower))
            {
                errors.Add("Must contain at least one lowercase letter (a-z).");
            }

            // Rule 4: Numeric Digit
            if (!password.Any(char.IsDigit))
            {
                errors.Add("Must contain at least one numeric digit (0-9).");
            }

            // Rule 5: Special Character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            {
                errors.Add("Must contain at least one special character (e.g. !@#$%^&*).");
            }

            return errors;
        }

        /// <summary>
        /// Parameterized update query targeting only the logged-in user's UserID row.
        /// </summary>
        private bool UpdateUserPassword(int userId, string newPassword)
        {
            string query = "UPDATE Users SET Password = @Password WHERE UserID = @UserID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@UserID", userId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error while saving new password: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}