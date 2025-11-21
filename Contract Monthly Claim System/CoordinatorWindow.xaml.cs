using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Services;
using System;
using System.Diagnostics;
using System.Linq;
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
            dgClaims.ItemsSource = DatabaseHelper.GetAllClaims();
        }

        private void ViewDocs_Click(object sender, RoutedEventArgs e)
        {
            if (dgClaims.SelectedItem is not Claim claim) return;

            var docs = DatabaseHelper.GetDocumentsByClaimId(claim.Id);

            foreach (var doc in docs)
                Process.Start(new ProcessStartInfo(doc.FilePath) { UseShellExecute = true });
        }

        private void ApproveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgClaims.SelectedItems.Cast<Claim>().ToList();
            if (!selected.Any()) return;

            foreach (var claim in selected)
                DatabaseHelper.ApproveClaimWithAudit(claim.Id, Environment.UserName, "Approved by Coordinator");

            LoadClaims();
        }

        private void RejectSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgClaims.SelectedItems.Cast<Claim>().ToList();
            if (!selected.Any()) return;

            foreach (var claim in selected)
                DatabaseHelper.RejectClaimWithAudit(claim.Id, Environment.UserName, "Rejected by Coordinator");

            LoadClaims();
        }

        private void GenerateInvoices_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgClaims.SelectedItems.Cast<Claim>()
                          .Where(c => c.Status == "Approved").ToList();

            if (!selected.Any()) return;

            foreach (var claim in selected)
            {
                var path = InvoiceService.GenerateInvoicePdf(claim);
                MessageBox.Show($"Invoice generated:\n{path}");
            }
        }
    }
}
