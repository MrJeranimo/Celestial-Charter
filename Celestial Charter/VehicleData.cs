using Brutal.GlfwApi;
using Brutal.Logging;
using KSA;
using System.Collections;

namespace Celestial_Charter
{
    internal class VehicleData
    {
        public enum BitStatus
        {
            Visited,
            FlyingBy,
            FlownBy,
            Orbiting,
            Orbited,
            IsLanded,
            HasLanded,
            IsSplashedDown,
            HasSplashedDown,
        }
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
            astroStatus.Set((int)BitStatus.Visited, true);

            if(isFlyingBy())
            {
                astroStatus.Set((int)BitStatus.FlyingBy, true);
                astroStatus.Set((int)BitStatus.FlownBy, true);

                // Make sure the other Temporary situations are false.
                astroStatus.Set((int)BitStatus.Orbiting, false);
                astroStatus.Set((int)BitStatus.IsLanded, false);
                astroStatus.Set((int)BitStatus.IsSplashedDown, false);
            }
            else if(isOrbiting())
            {
                astroStatus.Set((int)BitStatus.Orbiting, true);
                astroStatus.Set((int)BitStatus.Orbited, true);

                // Make sure the other Temporary situations are false.
                astroStatus.Set((int)BitStatus.FlyingBy, false);
                astroStatus.Set((int)BitStatus.IsLanded, false);
                astroStatus.Set((int)BitStatus.IsSplashedDown, false);
            }
            else if(VehicleSituation.HasTerrainContact())
            {
                astroStatus.Set((int)BitStatus.IsLanded, true);
                astroStatus.Set((int)BitStatus.HasLanded, true);

                // Make sure the other Temporary situations are false.
                astroStatus.Set((int)BitStatus.FlyingBy, false);
                astroStatus.Set((int)BitStatus.Orbiting, false);
                astroStatus.Set((int)BitStatus.IsSplashedDown, false);
            }
            else if(VehicleSituation.HasOceanContact())
            {
                astroStatus.Set((int)BitStatus.IsSplashedDown, true);
                astroStatus.Set((int)BitStatus.HasSplashedDown, true);

                // Make sure the other Temporary situations are false.
                astroStatus.Set((int)BitStatus.FlyingBy, false);
                astroStatus.Set((int)BitStatus.Orbiting, false);
                astroStatus.Set((int)BitStatus.IsLanded, false);
            }
            else
            {
                astroStatus.Set((int)BitStatus.FlyingBy, false);
                astroStatus.Set((int)BitStatus.Orbiting, false);
                astroStatus.Set((int)BitStatus.IsLanded, false);
                astroStatus.Set((int)BitStatus.IsSplashedDown, false);
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
