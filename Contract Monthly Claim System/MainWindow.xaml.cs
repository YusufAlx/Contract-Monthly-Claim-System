
using System.Windows;

namespace Contract_Monthly_Claim_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Lecturer_Click(object sender, RoutedEventArgs e)
        {
            new LecturerWindow().ShowDialog();
        }

        private void Coordinator_Click(object sender, RoutedEventArgs e)
        {
            new CoordinatorWindow().ShowDialog();
        }

        private void Track_Click(object sender, RoutedEventArgs e)
        {
            new TrackClaimsWindow().ShowDialog();
        }

        private void HR_Click(object sender, RoutedEventArgs e)
        {
            new HRWindow().ShowDialog();
        }
    }
}
