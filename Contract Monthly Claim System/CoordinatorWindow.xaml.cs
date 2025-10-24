using Contract_Monthly_Claim_System.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Contract_Monthly_Claim_System
{
    public partial class CoordinatorWindow : Window
    {
        public CoordinatorWindow()
        {
            InitializeComponent();
            LoadClaims();
        }

        private void LoadClaims()
        {
            var claims = DatabaseHelper.GetAllClaims();
            dgClaims.ItemsSource = claims;
        }

        private void ViewDocs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is int claimId)
            {
                var docs = DatabaseHelper.GetDocumentsByClaimId(claimId);
                if (docs == null || docs.Count == 0)
                {
                    MessageBox.Show("No documents attached to this claim.", "No Documents", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new System.Text.StringBuilder();
                for (int i = 0; i < docs.Count; i++)
                {
                    dlg.AppendLine($"{i + 1}. {docs[i].FileName}");
                }
                var selected = Microsoft.VisualBasic.Interaction.InputBox($"Documents:\n{dlg}\nEnter number to open, or Cancel.", "Open Document", "1");
                if (int.TryParse(selected, out int idx) && idx >= 1 && idx <= docs.Count)
                {
                    var path = docs[idx - 1].FilePath;
                    if (File.Exists(path))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Unable to open file: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("File not found on disk.", "Missing File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is int claimId)
            {
                DatabaseHelper.UpdateClaimStatus(claimId, "Approved");
                LoadClaims();
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is int claimId)
            {
                DatabaseHelper.UpdateClaimStatus(claimId, "Rejected");
                LoadClaims();
            }
        }
    }
}
