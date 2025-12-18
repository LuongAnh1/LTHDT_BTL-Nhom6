using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup; // Cần cái này để dùng ContentProperty
using BTL_Nhom6.Enums;

namespace BTL_Nhom6.UserControls
{
    // Dòng này giúp bạn có thể viết nội dung trực tiếp vào giữa thẻ <MainLayout>...</MainLayout>
    [ContentProperty(nameof(InnerContent))]
    public partial class MainLayout : UserControl
    {
        public MainLayout()
        {
            InitializeComponent();
        }

        // 1. Dependency Property cho ActiveItem (Để tô màu menu)
        public static readonly DependencyProperty ActiveItemProperty =
            DependencyProperty.Register("ActiveItem", typeof(SidebarItem), typeof(MainLayout), new PropertyMetadata(SidebarItem.None));

        public SidebarItem ActiveItem
        {
            get { return (SidebarItem)GetValue(ActiveItemProperty); }
            set { SetValue(ActiveItemProperty, value); }
        }

        // 2. Dependency Property cho Nội dung bên trong (InnerContent)
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public object InnerContent
        {
            get { return GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }
    }
}