using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BTL_Nhom6.UserControls
{
    // Dòng này giúp ta có thể viết nội dung trực tiếp vào giữa thẻ <MainLayout> trong XAML
    [ContentProperty(nameof(BodyContent))]
    public partial class MainLayout : UserControl
    {
        public MainLayout()
        {
            InitializeComponent();
        }

        // 1. Property để hứng nội dung của trang con (Body)
        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register("BodyContent", typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public object BodyContent
        {
            get { return GetValue(BodyContentProperty); }
            set { SetValue(BodyContentProperty, value); }
        }

        // 2. Property để set menu nào đang Active (VD: "System", "Home"...)
        public static readonly DependencyProperty SidebarCurrentItemProperty =
            DependencyProperty.Register("SidebarCurrentItem", typeof(string), typeof(MainLayout), new PropertyMetadata(string.Empty));

        public string SidebarCurrentItem
        {
            get { return (string)GetValue(SidebarCurrentItemProperty); }
            set { SetValue(SidebarCurrentItemProperty, value); }
        }
    }
}