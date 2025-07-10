namespace Samrt_Vehical_Hold.Entities
{
    public class Vehicle
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PlateNumber { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string OwnerNationalNumber { get; set; } = null!;
        public Guid OwnerUserId { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
