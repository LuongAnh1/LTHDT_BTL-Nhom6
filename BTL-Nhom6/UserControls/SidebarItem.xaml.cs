using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace BTL_Nhom6.UserControls
{
    public partial class SidebarItem : UserControl
    {
        public SidebarItem()
        {
            InitializeComponent();
        }

        // 1. Tên hiển thị (Text)
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SidebarItem));

        // 2. Icon (Kiểu PackIconKind của MaterialDesign)
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(SidebarItem));

        // 3. NavTag: Định danh riêng của nút này (ví dụ: "Home", "Devices")
        public string NavTag
        {
            get { return (string)GetValue(NavTagProperty); }
            set { SetValue(NavTagProperty, value); }
        }
        public static readonly DependencyProperty NavTagProperty =
            DependencyProperty.Register("NavTag", typeof(string), typeof(SidebarItem), new PropertyMetadata(string.Empty, OnActiveStateChanged));

        // 4. CurrentActiveTag: Tag của trang đang hiển thị (bind từ cha xuống)
        public string CurrentActiveTag
        {
            get { return (string)GetValue(CurrentActiveTagProperty); }
            set { SetValue(CurrentActiveTagProperty, value); }
        }
        public static readonly DependencyProperty CurrentActiveTagProperty =
            DependencyProperty.Register("CurrentActiveTag", typeof(string), typeof(SidebarItem), new PropertyMetadata(string.Empty, OnActiveStateChanged));

        // 5. IsActive: Thuộc tính nội bộ để XAML biết có nên tô màu hay không
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(SidebarItem), new PropertyMetadata(false));

        // Sự kiện Click để form cha bắt
        public event RoutedEventHandler Click;

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            // Truyền Tag của nút ra ngoài sự kiện
            if (Click != null) Click(this, e);
        }

        // Logic tự động kiểm tra: Nếu NavTag == CurrentActiveTag thì IsActive = true
        private static void OnActiveStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SidebarItem;
            if (control != null)
            {
                control.IsActive = (control.NavTag == control.CurrentActiveTag);
            }
        }
    }
}