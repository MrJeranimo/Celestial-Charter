using Brutal.GlfwApi;
using KSA;

namespace Celestial_Charter
{
    internal class VehicleData
    {
        public static Vehicle? currentVehicle { get; private set; } = null;
        public static Orbit? vehicleOrbit {  get; private set; } = null;
        public static Astronomical? astronomicalOrbiting { get; private set; } = null;
        public static Situation? vehicleSituation { get; private set; } = null;
        
        // Make a 2D array, First array is for each Astronomical, Second array is for status bit flags
 

        public static Vehicle? fetchCurrentVehicle()
        {
            currentVehicle = Program.ControlledVehicle;
            if(currentVehicle != null)
            {
                vehicleOrbit = currentVehicle.Orbit;
                astronomicalOrbiting = vehicleOrbit.Parent;
                vehicleSituation = currentVehicle.LastKinematicStates.Situation;
            }
            return currentVehicle;
        }
    }
}
