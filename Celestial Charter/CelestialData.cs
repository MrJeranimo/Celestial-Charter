using HarmonyLib;
using KSA;

namespace Celestial_Charter
{
    public class CelestialData
    {
        public static CelestialSystem? CelestialSystem { get; private set; } = null;
        public static int NumCelestials { get; private set; } = 0;
        public static List<Astronomical> AstronomicalList { get; private set; } = new List<Astronomical>();
        public static List<Astronomical> AstronomicalNonVehicleList { get; private set; } = new List<Astronomical>();
        public static List<Vehicle> VehicleList { get; private set; } = new List<Vehicle>();


        public static CelestialSystem? FetchCelestialSystem()
        {
            CelestialSystem = KSA.Universe.CurrentSystem;
            if (CelestialSystem != null)
            {
                NumCelestials = CelestialSystem.CelestialCount;
                AstronomicalList = CelestialSystem.All.GetList();
                VehicleList = CelestialSystem.Vehicles.GetList();
                
                // Sort out the vehicles from every astronomical
                foreach(var astro in AstronomicalList)
                {
                    if(astro.Class != "Vehicle")
                    {
                        AstronomicalNonVehicleList.Add(astro);
                    }
                }
            }
            return CelestialSystem;
        }
    }
}
