using Contract_Monthly_Claim_System.Data;
using OfficeOpenXml;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Contract_Monthly_Claim_System
{
    public partial class HRWindow : Window
    {
        public HRWindow()
        {
            InitializeComponent();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            var invoices = DatabaseHelper.GetInvoices();

            var display = invoices.Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.Amount,
                i.GeneratedAt,
                i.FilePath,
                ClaimID = i.ClaimID
            }).ToList();

            dgInvoices.ItemsSource = display;
        }

        private void OpenInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string file && File.Exists(file))
            {
                Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var invoices = DatabaseHelper.GetInvoices();

            string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
            Directory.CreateDirectory(reportsDir);

            string filePath = Path.Combine(reportsDir,
                    $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            StringBuilder sb = new();
            sb.AppendLine("InvoiceNumber,ClaimID,Amount,GeneratedAt,FilePath");

            foreach (var i in invoices)
                sb.AppendLine($"{i.InvoiceNumber},{i.ClaimID},{i.Amount},{i.GeneratedAt},\"{i.FilePath}\"");

            File.WriteAllText(filePath, sb.ToString());
            MessageBox.Show($"CSV Exported:\n{filePath}");
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var invoices = DatabaseHelper.GetInvoices();
            string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
            Directory.CreateDirectory(reportsDir);

            string filePath = Path.Combine(reportsDir,
                    $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Invoices");

            // Header
            sheet.Cells[1, 1].Value = "InvoiceNumber";
            sheet.Cells[1, 2].Value = "ClaimID";
            sheet.Cells[1, 3].Value = "Amount";
            sheet.Cells[1, 4].Value = "GeneratedAt";
            sheet.Cells[1, 5].Value = "FilePath";

            int row = 2;

            foreach (var invoice in invoices)
            {
                sheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                sheet.Cells[row, 2].Value = invoice.ClaimID;
                sheet.Cells[row, 3].Value = invoice.Amount;
                sheet.Cells[row, 4].Value = invoice.GeneratedAt;
                sheet.Cells[row, 5].Value = invoice.FilePath;
                row++;
            }

            package.SaveAs(new FileInfo(filePath));
            MessageBox.Show($"Excel Exported:\n{filePath}");
        }
    }
}
