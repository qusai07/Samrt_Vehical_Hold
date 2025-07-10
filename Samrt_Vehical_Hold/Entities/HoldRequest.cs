namespace Samrt_Vehical_Hold.Entities
{
    public class HoldRequest
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public DateTime RequestDate { get; set; }
    }

}
