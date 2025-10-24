
using System.Windows;

namespace Contract_Monthly_Claim_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LecturerButton_Click(object sender, RoutedEventArgs e)
        {
            LecturerWindow lecturerWindow = new LecturerWindow();
            lecturerWindow.Show();
            this.Hide();
            lecturerWindow.Closed += (s, args) => this.Show();
        }

        private void CoordinatorButton_Click(object sender, RoutedEventArgs e)
        {
            CoordinatorWindow coordinatorWindow = new CoordinatorWindow();
            coordinatorWindow.Show();
            this.Hide();
            coordinatorWindow.Closed += (s, args) => this.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    

private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
    {
        this.DragMove();
    }
}


private void Minimize_Click(object sender, RoutedEventArgs e)
{
    this.WindowState = WindowState.Minimized;
}


private void Close_Click(object sender, RoutedEventArgs e)
{
    this.Close();
}


private void Lecturer_Click(object sender, RoutedEventArgs e)
{
    LecturerWindow lecturerWindow = new LecturerWindow();
    lecturerWindow.Show();
    this.Hide();
    lecturerWindow.Closed += (s, args) => this.Show();
}


private void Button_Click_1(object sender, RoutedEventArgs e)
{
    // Placeholder for "Track Claims" button - implement your logic here
    MessageBox.Show("Track Claims clicked (placeholder)");
}

}
}