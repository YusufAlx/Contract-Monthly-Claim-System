using System;

namespace Contract_Monthly_Claim_System.Data
{
    public class Claim
    {
        public int Id { get; set; }
        public string TrackingID { get; set; }
        public string LecturerName { get; set; }
        public double HourlyRate { get; set; }
        public double HoursWorked { get; set; }
        public double MonthlyClaim { get; set; }
        public string Status { get; set; } = "Pending";
    }
}