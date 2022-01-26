namespace SodaOrderService.Models
{
    public class SodaOrder { 
        public long Id { get; set; }
        public string? Soda { get; set; }
        public int PinCode { get; set; }
        public bool IsComplete { get; set; }
    }
}
