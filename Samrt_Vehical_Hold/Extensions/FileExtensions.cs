using Newtonsoft.Json;
using Samrt_Vehical_Hold.Entities;

namespace Samrt_Vehical_Hold.Extensions
{
    public class FileExtensions
    {
        public class VehicleFileService
        {
            public List<Vehicle> ReadVehiclesFromFile(string filePath)
            {
                var json = System.IO.File.ReadAllText(filePath);

                return JsonConvert.DeserializeObject<List<Vehicle>>(json);
            }


        }
        public class VehicleHoldFileService
        {
            public List<VehicleHold> ReadVehicleHoldsFromFile(string filePath)
            {
                var json = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<VehicleHold>>(json);
            }
        }
    }
}
