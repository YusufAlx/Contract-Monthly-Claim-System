using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Services;
using Quartz;
using System.Linq;
using System.Threading.Tasks;

namespace Contract_Monthly_Claim_System.Jobs
{
    public class AutoInvoiceJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            // Find all approved claims
            var approved = DatabaseHelper
                .GetAllClaims()
                .Where(c => c.Status == "Approved")
                .ToList();

            foreach (var claim in approved)
            {
                var invoices = DatabaseHelper.GetInvoicesByClaimId(claim.Id);

                // Only generate if there is no invoice yet
                if (invoices.Count == 0)
                {
                    InvoiceService.GenerateInvoicePdf(claim);
                }
            }

            return Task.CompletedTask;
        }
    }
}
