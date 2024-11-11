namespace DATN.Models
{
    public class EmailSetting
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPassword { get; set; }
        public string? SenderName { get; set; }
    }
}
