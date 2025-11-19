using HarmonyLib;
using KSA;

namespace Celestial_Charter
{
    internal class CelestialData
    {
        public static CelestialSystem? celestialSystem { get; private set; } = null;
        public static int numCelestials { get; private set; } = 0;
        public static List<Astronomical>? astronomicalList { get; private set; } = null;


        public static CelestialSystem? FetchCelestialSystem()
        {
            celestialSystem = KSA.Universe.CurrentSystem;
            if (celestialSystem != null)
            {
                numCelestials = celestialSystem.CelestialCount;
                astronomicalList = celestialSystem.All.GetList();
                GenerateEachAstronomicalData();
            }
            return celestialSystem;
        }

        private static void GenerateEachAstronomicalData()
        {
            if (astronomicalList == null) return;

            foreach(var astro in astronomicalList)
            {
                
            }
        }
    }
}
