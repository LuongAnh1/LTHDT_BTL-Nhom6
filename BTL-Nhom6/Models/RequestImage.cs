using System;

namespace BTL_Nhom6.Models
{
    public class RequestImage
    {
        public int ImageID { get; set; }
        public int RequestID { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}