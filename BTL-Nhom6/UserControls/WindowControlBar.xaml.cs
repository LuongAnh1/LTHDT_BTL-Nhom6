using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace BTL_Nhom6.UserControls
{
    public partial class WindowControlBar : UserControl
    {
        public WindowControlBar()
        {
            InitializeComponent();
        }

        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            bool isNormal = window.WindowState == WindowState.Normal;
            window.WindowState = isNormal ? WindowState.Maximized : WindowState.Normal;

            // Đổi icon tương ứng
            iconMaximize.Kind = isNormal ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
