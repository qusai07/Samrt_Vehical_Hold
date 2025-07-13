namespace Samrt_Vehical_Hold.Entities
{
    public class Violation
    {
        public Guid ID { get; set; }
        public Guid HoldRequestId { get; set; }
        public HoldRequest HoldRequest { get; set; }
        public string Description { get; set; }
        public DateTime ViolationDate { get; set; }
        public bool IsResolved { get; set; } = false;
    }
}
