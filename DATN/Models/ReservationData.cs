namespace DATN.Models
{
    public class ReservationData
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Child { get; set; }
        public int Adult { get; set; }
        public Dictionary<int, int> Quantities { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public double Totals { get; set; }
        public List<int> SelectedServices { get; set; }
        public int? UserId { get; set; }
    }
}
