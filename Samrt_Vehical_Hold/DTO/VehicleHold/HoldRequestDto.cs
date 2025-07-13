namespace Samrt_Vehical_Hold.DTO.VehicleHold
{
    public class HoldRequestDto
    {
        public string PlateNumber { get; set; }
        public DateTime StartDate { get; set; }
        public string Location { get; set; }
        public Guid VehicleId { get; set; } 

    }

}
