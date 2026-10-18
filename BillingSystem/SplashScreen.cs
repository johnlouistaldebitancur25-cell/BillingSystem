using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSystem.Utils;

namespace BillingSystem
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            SetupSplashUI();
        }

        private void SetupSplashUI()
        {
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppTheme.PrimaryColor;

            Label lblAppName = new Label
            {
                Text = "BILLING SYSTEM",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 60),
                Location = new Point(0, 60)
            };

            Label lblTagline = new Label
            {
                Text = "Electric Billing Management System",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = AppTheme.AccentColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 30),
                Location = new Point(0, 130)
            };

            Label lblLoading = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = AppTheme.AccentColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 30),
                Location = new Point(0, 220)
            };

            this.Controls.Add(lblAppName);
            this.Controls.Add(lblTagline);
            this.Controls.Add(lblLoading);
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            splashTimer.Start();
        }

        private void splashTimer_Tick(object sender, EventArgs e)
        {
            splashTimer.Stop();
            this.Hide();
            var loginForm = new LoginForm();
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
        }
    }
}