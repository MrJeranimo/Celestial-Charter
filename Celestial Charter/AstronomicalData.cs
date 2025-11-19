using KSA;
using System.Runtime.InteropServices;

namespace Celestial_Charter
{
    internal class AstronomicalData
    {
        public Astronomical astronomical { get; private set; }
        public string Id { get; private set; }
        public string Class { get; private set; }
        public bool IsMoon { get; private set; }
        public bool IsStar { get; private set; }
        public bool IsPlanet { get; private set; }
        public bool IsVehicle { get; private set; }
        public bool Visited { get; private set; } = false;
        public bool AreOrbiting { get; private set; } = false;
        public bool HasOrbited { get; private set; } = false;
        public bool AreLanded { get; private set; } = false;
        public bool HasLanded { get; private set; } = false;
        public bool AreSplashedDown { get; private set; } = false;
        public bool HasSplashedDown { get; private set; } = false;

        public AstronomicalData(Astronomical Astronomical)
        {
            astronomical = Astronomical;
            Id = astronomical.Id;
            IsMoon = astronomical.IsMoon();
            IsStar = astronomical.IsStar();
            Class = astronomical.Class;
            if(Class == "Vehicle") { IsVehicle = true; } else { IsVehicle = false; }
            if(!IsVehicle && !IsMoon && !IsStar) { IsPlanet = true; } else { IsPlanet = false; }
        }

        public void UpdateStatus(Situation situation)
        {
            switch(situation)
            {
                case Situation.Landed:
                    // Fix how these don't track per Vehicle
                    AreLanded = true;
                    HasLanded = true;
                    AreOrbiting = false;
                    break;
                case Situation.Sailing:
                    AreSplashedDown = true;
                    HasSplashedDown = true;
                    AreOrbiting = false;
                    break;
                default:
                    AreLanded = false;
                    AreSplashedDown = false;
                    break;
            }
        }
    }
}
