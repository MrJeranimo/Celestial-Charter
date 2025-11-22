using Brutal.Logging;
using KSA;
using KSA.Rendering.Water.Data;
using System.Runtime.InteropServices;

namespace Celestial_Charter
{
    internal class AstronomicalData : IEquatable<AstronomicalData>
    {
        public Astronomical Astronomical { get; private set; }
        public string Id { get; private set; }
        public string Class { get; private set; }
        public bool IsMoon { get; private set; }
        public bool IsStar { get; private set; }
        public bool IsPlanet { get; private set; }
        public bool IsVehicle { get; private set; }
        public bool HasOceans { get; private set; }
        public OceanReference? OceanReference { get; private set; }
        public bool HasSurface { get; private set; }

        public AstronomicalData(Astronomical astronomical)
        {
            this.Astronomical = astronomical;
            Id = Astronomical.Id;
            IsMoon = Astronomical.IsMoon();
            IsStar = Astronomical.IsStar();
            Class = Astronomical.Class;
            if (Class == "Vehicle") { IsVehicle = true; } else { IsVehicle = false; }
            if (!IsVehicle && !IsMoon && !IsStar) { IsPlanet = true; } else { IsPlanet = false; }
            OceanReference = Astronomical.BodyTemplate.OceanReference;
            if (OceanReference != null) HasOceans = true;
            if (Astronomical.BodyTemplate.TerrainReference != null) HasSurface = true;
        }

        public bool Equals(AstronomicalData? other)
        {
            if (other == null) return false;
            return this.Astronomical.Equals(other.Astronomical);
        }

        public override bool Equals(object? obj)
        {
            if(obj == null) return false;
            return Equals(obj as AstronomicalData);
        }

        public override int GetHashCode()
        {
            return Astronomical.GetHashCode();
        }
    }
}
