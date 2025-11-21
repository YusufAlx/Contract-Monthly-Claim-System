using System;
using System.IO;
using Contract_Monthly_Claim_System.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Contract_Monthly_Claim_System.Services
{
    public static class InvoiceService
    {
        private static readonly string InvoiceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoices");

        public static string GenerateInvoicePdf(Claim claim)
        {
            Directory.CreateDirectory(InvoiceFolder);
            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{new Random().Next(1000, 9999)}";
            var fileName = $"{invoiceNumber}.pdf";
            var filePath = Path.Combine(InvoiceFolder, fileName);

            // Ensure QuestPDF license context if needed
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INVOICE").FontSize(20).Bold();
                            col.Item().Text($"Invoice #: {invoiceNumber}");
                            col.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}");
                        });

                        row.ConstantItem(200).AlignRight().Column(col =>
                        {
                            col.Item().Text("PAY TO:").Bold();
                            col.Item().Text("Your Institution Name");
                            col.Item().Text("Address line 1");
                            col.Item().Text("Address line 2");
                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Lecturer: {claim.LecturerName}");
                        col.Item().Text($"Hours: {claim.HoursWorked} @ R{claim.HourlyRate:0.00}");
                        col.Item().Text($"Total: R{claim.MonthlyClaim:0.00}").FontSize(14).Bold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Thank you for your service.");
                    });
                });
            }).GeneratePdf(filePath);

            // record invoice in DB (DatabaseHelper is in Data namespace)
            DatabaseHelper.InsertInvoice(claim.Id, invoiceNumber, claim.MonthlyClaim, filePath);
            return filePath;
        }
    }
}
