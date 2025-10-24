using Contract_Monthly_Claim_System.Data;
using System.Text;
using System.Windows;

namespace Contract_Monthly_Claim_System
{
    public partial class TrackClaimsWindow : Window
    {
        public TrackClaimsWindow()
        {
            InitializeComponent();
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            var id = txtTrackingId.Text?.Trim();
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Please enter a Tracking ID.", "Input required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var claim = DatabaseHelper.GetClaimByTrackingID(id);
            if (claim == null)
            {
                txtResult.Text = "No claim found with that Tracking ID.";
                return;
            }

            var docs = DatabaseHelper.GetDocumentsByClaimId(claim.Id);
            var sb = new StringBuilder();
            sb.AppendLine($"Tracking ID: {claim.TrackingID}");
            sb.AppendLine($"Lecturer: {claim.LecturerName}");
            sb.AppendLine($"Hourly rate: R{claim.HourlyRate:0.00}");
            sb.AppendLine($"Hours worked: {claim.HoursWorked:0.00}");
            sb.AppendLine($"Monthly claim: R{claim.MonthlyClaim:0.00}");
            sb.AppendLine($"Status: {claim.Status}");
            if (docs.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Attached documents:");
                foreach (var d in docs)
                    sb.AppendLine($"• {d.FileName}");
            }
            txtResult.Text = sb.ToString();
        }
    }
}
