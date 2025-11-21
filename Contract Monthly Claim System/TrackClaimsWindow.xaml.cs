using Contract_Monthly_Claim_System.Data;
using System.Linq;
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

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var claim = DatabaseHelper.GetClaimByTrackingID(txtTracking.Text);

            if (claim == null)
            {
                txtResult.Text = "No claim found with that tracking ID.";
                return;
            }

            var docs = DatabaseHelper.GetDocumentsByClaimId(claim.Id);
            StringBuilder sb = new();

            sb.AppendLine($"Tracking ID: {claim.TrackingID}");
            sb.AppendLine($"Lecturer: {claim.LecturerName}");
            sb.AppendLine($"Status: {claim.Status}");
            sb.AppendLine("Documents:");

            foreach (var d in docs)
                sb.AppendLine(" - " + d.FileName);

            txtResult.Text = sb.ToString();
        }
    }
}
