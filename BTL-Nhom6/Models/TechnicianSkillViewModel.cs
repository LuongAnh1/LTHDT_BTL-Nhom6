using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BTL_Nhom6.Models
{
    // Kế thừa INotifyPropertyChanged để khi tick chọn thì UI tự cập nhật trạng thái Enable/Disable của Combobox
    public class TechnicianSkillViewModel : INotifyPropertyChanged
    {
        public int SkillID { get; set; }
        public string SkillName { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private string _level;
        public string Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(); }
        }

        // Mảng dùng để bind vào Combobox Level
        public string[] LevelOptions { get; } = new string[] { "Cơ bản", "Trung cấp", "Nâng cao", "Chuyên gia" };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}