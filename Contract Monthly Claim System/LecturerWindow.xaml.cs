using System;
using System.Collections.Generic;
using System.Linq;
using Contract_Monthly_Claim_System.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Contract_Monthly_Claim_System
{
    public partial class LecturerWindow : Window
    {
        private List<string> uploadedFiles = new();

        public LecturerWindow()
        {
            InitializeComponent();
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Documents|*.pdf;*.docx;*.xlsx",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var file in dlg.FileNames)
                {
                    FileInfo fi = new(file);
                    if (fi.Length > 100 * 1024 * 1024)
                    {
                        MessageBox.Show("File exceeds 100MB limit.");
                        continue;
                    }

                    uploadedFiles.Add(file);
                    lstFiles.Items.Add(fi.Name);
                }
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(txtName.Text, @"^[A-Za-z\s\.\-']+$"))
            {
                MessageBox.Show("Full name must contain letters and spaces only.");
                return;
            }

            if (!double.TryParse(txtRate.Text, out double rate) ||
                !double.TryParse(txtHours.Text, out double hours))
            {
                MessageBox.Show("Hourly rate & hours worked must be numeric.");
                return;
            }

            double monthly = rate * hours;
            string tracking = "CLM-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            var claim = new Claim
            {
                TrackingID = tracking,
                LecturerName = txtName.Text,
                HourlyRate = rate,
                HoursWorked = hours,
                MonthlyClaim = monthly,
                Status = "Pending"
            };

            int claimId = DatabaseHelper.InsertClaim(claim);

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                         "uploads", tracking);
            Directory.CreateDirectory(folder);

            foreach (string srcPath in uploadedFiles)
            {
                string dest = Path.Combine(folder, Path.GetFileName(srcPath));
                File.Copy(srcPath, dest, true);

                var doc = new Document
                {
                    ClaimID = claimId,
                    FileName = Path.GetFileName(dest),
                    FilePath = dest
                };

                DatabaseHelper.InsertDocument(doc);
            }

            MessageBox.Show($"Claim submitted! Tracking ID:\n{tracking}");
            Close();
        }
    }
}
