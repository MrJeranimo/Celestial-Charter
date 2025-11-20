using Brutal.GlfwApi;
using Brutal.Logging;
using KSA;
using System.Collections;

namespace Celestial_Charter
{
    internal class VehicleData
    {
        public Vehicle Vehicle { get; private set; }
        public Orbit VehicleOrbit {  get; private set; }
        public Astronomical AstronomicalOrbiting { get; private set; }
        public Situation VehicleSituation { get; private set; }

        /// <summary>
        /// An array List containing a status for all the Astronomicals in the current system as BitArrays of size 9.
        /// </summary>
        /// Bit representation:
        /// [0]: Visited
        /// [1]: Are Flying By
        /// [2]: Have Flown By
        /// [3]: Are Orbiting
        /// [4]: Have Orbited
        /// [5]: Are Landed
        /// [6]: Have Landed
        /// [7]: Are Splashed Down
        /// [8]: Have Splashed Down
        public ArrayList StatusArray { get; private set; }

        /// <summary>
        /// Contains a List of all the non-Vehicle Astronomicals as an AstronomicalData class.
        /// </summary>
        public List<AstronomicalData> AstronomicalDataList { get; private set; } = new List<AstronomicalData>();
        public int curStatusIndex { get; private set; }
        public BitArray? AstroStatus { get; private set; }

        public VehicleData(Vehicle vehicle, int numAstronomicalsNonVehicle, List<Astronomical> astronomicalsNonVehicle)
        {
            this.Vehicle = vehicle;
            VehicleOrbit = vehicle.Orbit;
            AstronomicalOrbiting = VehicleOrbit.Parent;
            VehicleSituation = vehicle.LastKinematicStates.Situation;
            StatusArray = new ArrayList(numAstronomicalsNonVehicle);
            foreach(var astro in astronomicalsNonVehicle)
            {
                StatusArray.Add(new BitArray(9, false));
                AstronomicalDataList.Add(new AstronomicalData(astro));
            }
            curStatusIndex = AstronomicalDataList.FindIndex(x => x.Astronomical.Id == AstronomicalOrbiting.Id);
            AstroStatus = StatusArray[curStatusIndex] as BitArray;
        }

        public void Update()
        {
            VehicleOrbit = Vehicle.Orbit;
            var oldOrbit = AstronomicalOrbiting;
            AstronomicalOrbiting = VehicleOrbit.Parent;
            VehicleSituation = Vehicle.LastKinematicStates.Situation;
            if(oldOrbit != AstronomicalOrbiting)
            {
                curStatusIndex = AstronomicalDataList.FindIndex(x => x.Astronomical == AstronomicalOrbiting);
                AstroStatus = StatusArray[curStatusIndex] as BitArray;
            }
            if (AstroStatus != null)
            {
                UpdateStatus(AstroStatus);
            }
            else
            {
                DefaultCategory.Log.Error($"AstroStatus in {Vehicle.Id} is null");
            }
        }

        private void UpdateStatus(BitArray astroStatus)
        {
            // Visited = true
            astroStatus.Set(0, true);

            // Fix edge-case where astroStatus cannot reset TempSituation variables if another type of Situation is true
            if(isFlyingBy())
            {
                astroStatus.Set(1, true);
                astroStatus.Set(2, true);
            }
            else if(isOrbiting())
            {
                astroStatus.Set(3, true);
                astroStatus.Set(4, true);
            }
            else if(VehicleSituation.HasTerrainContact())
            {
                astroStatus.Set(5, true);
                astroStatus.Set(6, true);
            }
            else if(VehicleSituation.HasOceanContact())
            {
                astroStatus.Set(7, true);
                astroStatus.Set(8, true);
            }
            else
            {
                astroStatus.Set(1, false);
                astroStatus.Set(3, false);
                astroStatus.Set(5, false);
                astroStatus.Set(7, false);
            }
        }

        private bool isFlyingBy()
        {
            // Try and improve
            return (VehicleOrbit.Apoapsis > AstronomicalOrbiting.SphereOfInfluence || (VehicleOrbit.Apoapsis < 0 && VehicleOrbit.Periapsis > 10000));
        }

        private bool isOrbiting()
        {
            // Try and improve
            return (VehicleOrbit.Apoapsis > AstronomicalOrbiting.MeanRadius + 10000 && VehicleOrbit.Periapsis > AstronomicalOrbiting.MeanRadius + 10000);
        }
    }
}
