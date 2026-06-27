using Brutal.Logging;
using KSA;
using System.Collections;

namespace Celestial_Charter
{
    public class VehicleData
    {
        public enum BitStatus
        {
            Visited,
            FlyingBy,
            FlownBy,
            Orbiting,
            Orbited,
            SubOrbital,
            SubOrbited,
            InAtmosphere,
            EncounteredAtmosphere,
            IsLanded,
            HasLanded,
            IsSplashedDown,
            HasSplashedDown,
        }
        public Vehicle Vehicle { get; private set; }
        public Orbit VehicleOrbit {  get; private set; }
        public IParentBody AstronomicalOrbiting { get; private set; }
        public Situation VehicleSituation { get; private set; }

        /// <summary>
        /// An array List containing a status for all the Astronomicals in the current system as BitArrays of size 13.
        /// </summary>
        /// Bit representation:
        /// [0]: Visited
        /// [1]: Are Flying By
        /// [2]: Have Flown By
        /// [3]: Are Orbiting
        /// [4]: Have Orbited
        /// [5]: SubOrbital
        /// [6]: SubOrbited
        /// [7]: In Atmosphere
        /// [8]: Encountered Atmosphere
        /// [9]: Are Landed
        /// [10]: Have Landed
        /// [11]: Are Splashed Down
        /// [12]: Have Splashed Down
        public ArrayList StatusArray { get; private set; }

        /// <summary>
        /// Contains a List of all the non-Vehicle Astronomicals as an AstronomicalData class.
        /// </summary>
        public static List<AstronomicalData> AstronomicalDataList { get; private set; } = new List<AstronomicalData>();
        public int curStatusIndex { get; private set; }
        public BitArray? AstroStatus { get; private set; }

        public VehicleData(Vehicle vehicle)
        {
            this.Vehicle = vehicle;
            VehicleOrbit = vehicle.Orbit;
            AstronomicalOrbiting = VehicleOrbit.Parent;
            VehicleSituation = vehicle.Situation;
            StatusArray = new ArrayList(AstronomicalDataList.Count);
            foreach(var astro in AstronomicalDataList)
            {
                // Create a Status for each of the astronomicals for this vehicle
                StatusArray.Add(new BitArray(13, false));
            }
            // Get the current astronomical's index in the Data List
            curStatusIndex = AstronomicalDataList.FindIndex(x => x.Astronomical.Id == AstronomicalOrbiting.Id);

            // Get the current astronomical's BitArray
            AstroStatus = StatusArray[curStatusIndex] as BitArray;
        }

        public void Update()
        {
            VehicleOrbit = Vehicle.Orbit;
            var oldOrbit = AstronomicalOrbiting;
            AstronomicalOrbiting = VehicleOrbit.Parent;
            VehicleSituation = Vehicle.Situation;
            if(oldOrbit != AstronomicalOrbiting)
            {
                // Clear Temp Status for old astronomical
                if (AstroStatus != null) setTempStatusFalse(AstroStatus);

                // Find new astronomical's index in th Data List
                curStatusIndex = AstronomicalDataList.FindIndex(x => x.Astronomical == AstronomicalOrbiting);

                // Get the new astronomical's BitArray
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
            astroStatus[(int)BitStatus.Visited] = true;

            astroStatus[(int)BitStatus.FlyingBy] = isFlyingBy();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.FlownBy] = astroStatus[(int)BitStatus.FlownBy] | astroStatus[(int)BitStatus.FlyingBy];

            astroStatus[(int)BitStatus.Orbiting] = isOrbiting();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.Orbited] = astroStatus[(int)BitStatus.Orbited] | astroStatus[(int)BitStatus.Orbiting];

            astroStatus[(int)BitStatus.SubOrbital] = isSubOrbital();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.SubOrbited] = astroStatus[(int)BitStatus.SubOrbited] | astroStatus[(int)BitStatus.SubOrbital];

            astroStatus[(int)BitStatus.InAtmosphere] = inAtmosphere();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.EncounteredAtmosphere] = astroStatus[(int)BitStatus.EncounteredAtmosphere] | astroStatus[(int)BitStatus.InAtmosphere];

            astroStatus[(int)BitStatus.IsLanded] = VehicleSituation.HasTerrainContact();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.HasLanded] = astroStatus[(int)BitStatus.HasLanded] | astroStatus[(int)BitStatus.IsLanded];

            astroStatus[(int)BitStatus.IsSplashedDown] = VehicleSituation.HasOceanContact();
            // Once set to true, will stay as true as the bitwise OR won't change the boolean
            astroStatus[(int)BitStatus.HasSplashedDown] = astroStatus[(int)BitStatus.HasSplashedDown] | astroStatus[(int)BitStatus.IsSplashedDown];

        }

        private void setTempStatusFalse(BitArray astroStatus)
        {
            astroStatus[(int)BitStatus.FlyingBy] = false;
            astroStatus[(int)BitStatus.Orbiting] = false;
            astroStatus[(int)BitStatus.SubOrbital] = false;
            astroStatus[(int)BitStatus.InAtmosphere] = false;
            astroStatus[(int)BitStatus.IsLanded] = false;
            astroStatus[(int)BitStatus.IsSplashedDown] = false;
        }

        private bool isFlyingBy()
        {
            //          Absolute Apoapsis > Astronomical SOI
            return (VehicleOrbit.Apoapsis > AstronomicalOrbiting.SphereOfInfluence || (VehicleOrbit.Apoapsis < 0 && VehicleOrbit.Periapsis > 10000));
        }

        private bool isOrbiting()
        {
            if(AstronomicalDataList[curStatusIndex].HasAtmosphere)
            {
                //                                                             Relative Periapsis > Atmosphere Height
                return (!isFlyingBy() && VehicleOrbit.Periapsis - AstronomicalOrbiting.MeanRadius > AstronomicalDataList[curStatusIndex].AtmosphereHeightM);
            }
            else
            {
                //                           Absolute Periapsis > Astronomical Radius
                return (!isFlyingBy() && VehicleOrbit.Periapsis > AstronomicalOrbiting.MeanRadius);
            }
        }

        private bool isSubOrbital()
        {
            if(AstronomicalDataList[curStatusIndex].HasAtmosphere)
            {
                //                                                          Relative Periapsis < Atmosphere Height                                      &&                                       Relative Apoapsis > Atmosphere Height
                return (isFlying() && VehicleOrbit.Periapsis - AstronomicalOrbiting.MeanRadius < AstronomicalDataList[curStatusIndex].AtmosphereHeightM && VehicleOrbit.Apoapsis - AstronomicalOrbiting.MeanRadius > AstronomicalDataList[curStatusIndex].AtmosphereHeightM);
            }
            else
            {
                //                        Absolute Periapsis < Astronomical Radius
                return (isFlying() && VehicleOrbit.Periapsis < AstronomicalOrbiting.MeanRadius);
            }
        }

        private bool inAtmosphere()
        {
            //                          if Astronomical Has Atmosphere &&              Sea Level Altitude <= Atmosphere Height
            return (AstronomicalDataList[curStatusIndex].HasAtmosphere && Vehicle.GetBarometricAltitude() <= AstronomicalDataList[curStatusIndex].AtmosphereHeightM);
        }

        private bool isFlying()
        {
            return (!VehicleSituation.HasOceanContact() && !VehicleSituation.HasTerrainContact());
        }
    }
}
