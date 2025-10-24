using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Contract_Monthly_Claim_System.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Contract_Monthly_Claim_System
{
    public partial class LecturerWindow : Window
    {
        private readonly List<string> selectedFiles = new List<string>();
        private const long MaxFileBytes = 100L * 1024L * 1024L; // 100 MB
        private static readonly string UploadsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");

        public LecturerWindow()
        {
            InitializeComponent();
            txtHourlyRate.TextChanged += Inputs_TextChanged;
            txtHoursWorked.TextChanged += Inputs_TextChanged;
        }

        private void Inputs_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (double.TryParse(txtHourlyRate.Text, out double rate) && double.TryParse(txtHoursWorked.Text, out double hours))
            {
                double monthly = rate * hours;
                txtMonthlyClaim.Text = monthly.ToString("0.00");
            }
            else
            {
                txtMonthlyClaim.Text = string.Empty;
            }
        }

        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Allowed files (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var f in dlg.FileNames)
                {
                    var fi = new FileInfo(f);
                    if (fi.Length > MaxFileBytes)
                    {
                        MessageBox.Show($"File {fi.Name} exceeds 100 MB and will not be added.", "File too large", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }
                    if (!selectedFiles.Contains(f))
                    {
                        selectedFiles.Add(f);
                        lstFiles.Items.Add(fi.Name);
                    }
                }
            }
        }

        private bool ValidateInputs(out string validationMessage)
        {
            validationMessage = null;
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                validationMessage = "Full name is required.";
                return false;
            }
            if (Regex.IsMatch(txtFullName.Text, @"\d"))
            {
                validationMessage = "Full name cannot contain digits.";
                return false;
            }
            if (!double.TryParse(txtHourlyRate.Text, out double rate) || rate < 0)
            {
                validationMessage = "Enter a valid numeric hourly rate.";
                return false;
            }
            if (!double.TryParse(txtHoursWorked.Text, out double hours) || hours < 0)
            {
                validationMessage = "Enter valid hours worked.";
                return false;
            }
            return true;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out string vmsg))
            {
                MessageBox.Show(vmsg, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var trackingId = GenerateTrackingID();
            var claim = new Claim
            {
                TrackingID = trackingId,
                LecturerName = txtFullName.Text.Trim(),
                HourlyRate = double.Parse(txtHourlyRate.Text),
                HoursWorked = double.Parse(txtHoursWorked.Text),
                MonthlyClaim = double.Parse(txtMonthlyClaim.Text),
                Status = "Pending"
            };

            int claimId = DatabaseHelper.InsertClaim(claim);

            var destFolder = Path.Combine(UploadsRoot, trackingId);
            Directory.CreateDirectory(destFolder);

            foreach (var src in selectedFiles)
            {
                var fi = new FileInfo(src);
                var safeName = MakeSafeFileName(fi.Name);
                var destPath = Path.Combine(destFolder, safeName);
                destPath = EnsureUniqueFilePath(destPath);
                File.Copy(src, destPath);
                DatabaseHelper.InsertDocument(new Document
                {
                    ClaimID = claimId,
                    FileName = safeName,
                    FilePath = destPath
                });
            }

            MessageBox.Show($"Claim submitted successfully.\nYour Tracking ID: {trackingId}", "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

            txtFullName.Text = string.Empty;
            txtHourlyRate.Text = string.Empty;
            txtHoursWorked.Text = string.Empty;
            txtMonthlyClaim.Text = string.Empty;
            lstFiles.Items.Clear();
            selectedFiles.Clear();
        }

        private string GenerateTrackingID()
        {
            var rnd = new Random();
            var suffix = rnd.Next(100000, 999999);
            return $"CLM-{suffix}";
        }

        private string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private string EnsureUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            int i = 1;
            string tryPath;
            do
            {
                tryPath = Path.Combine(dir, $"{name}({i}){ext}");
                i++;
            } while (File.Exists(tryPath));
            return tryPath;
        }
    }
}
