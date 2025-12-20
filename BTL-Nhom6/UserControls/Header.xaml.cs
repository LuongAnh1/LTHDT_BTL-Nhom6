using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BTL_Nhom6.UserControls
{
    /// <summary>
    /// Interaction logic for HeaderTabs.xaml
    /// </summary>
    public partial class Header : Window
    {
        public static readonly DependencyProperty HeaderTitleProperty =
        DependencyProperty.Register(
            nameof(HeaderTitle),
            typeof(string),
            typeof(Header),
            new PropertyMetadata("Quản trị hệ thống")
        );
        public string HeaderTitle
        {
            get => (string)GetValue(HeaderTitleProperty);
            set => SetValue(HeaderTitleProperty, value);
        }
        public Header()
        {
            InitializeComponent();
        }
    }
}
