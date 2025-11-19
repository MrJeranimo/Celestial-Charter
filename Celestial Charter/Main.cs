using KSA;
using StarMap.API;
using Brutal.Logging;
using Brutal.ImGuiApi;

namespace Celestial_Charter
{
    [StarMapMod]
    public class Main
    {
        private CelestialSystem? _celestialSystem = null;
        private int _numCelestials = 0;
        private List<Astronomical>? _astronomicals = null;
        private Vehicle? _currentVehicle = null;
        private Orbit? _vehicleOrbit = null;
        private Astronomical? _astronomicalOrbiting = null;
        private Situation? _vehicleSituation = null;
        private readonly string _GUINAME = "Celestial Charter";
        private bool _showWindow = true;

        [StarMapAllModsLoaded]
        public void AfterSystemLoaded()
        {
            DefaultCategory.Log.Info("Celestial Charter has been Loaded", "Celestial Charter");
        }

        [StarMapBeforeGui]
        public void BeforeGUI(double dt)
        {
            if(_celestialSystem == null)
            {
                _celestialSystem = CelestialData.FetchCelestialSystem();
                _numCelestials = CelestialData.numCelestials;
                _astronomicals = CelestialData.astronomicalList;
            }
        }


        [StarMapAfterGui]
        public void AfterGUI(double dt)
        {
            _currentVehicle = VehicleData.fetchCurrentVehicle();
            _vehicleOrbit = VehicleData.vehicleOrbit;
            _astronomicalOrbiting = VehicleData.astronomicalOrbiting;
            _vehicleSituation = VehicleData.vehicleSituation;
            bool isLanded = false;

            if (!_showWindow) return;

            ImGuiWindowFlags flags = ImGuiWindowFlags.None;
            if(ImGui.Begin(_GUINAME, ref _showWindow, flags))
            {
                ImGui.Text($"Number of Celestials: {_numCelestials}");
                if (_celestialSystem != null) ImGui.Text($"Current Celestial System: {_celestialSystem.Id}");
                ImGui.Separator();
                if(_currentVehicle != null) ImGui.Text($"Current Vehicle: {_currentVehicle.Id}");
                if(_vehicleOrbit != null) ImGui.Text($"Vehicle Orbit: {_vehicleOrbit}");
                if(_astronomicalOrbiting != null) ImGui.Text($"Astronomical Orbiting: {_astronomicalOrbiting.Id}");
                if (_vehicleSituation == KSA.Situation.Landed) isLanded = true;
                ImGui.Text($"Is Landed: {isLanded}");
                ImGui.Separator();
                if (_astronomicals != null )
                {
                    foreach(var astro in _astronomicals)
                    {
                        ImGui.Text($"Astronomical Name: {astro.Id}");
                    }
                }
                
                if(ImGui.BeginMenu("Celestial Menu")) 
                {
                    ImGui.MenuItem($"{CelestialData.numCelestials}");
                    ImGui.EndMenu();
                }
                if (ImGui.CollapsingHeader("Test"))
                {
                    ImGui.Text("This is a test");
                }
                if(ImGui.TreeNode("Bullets"))
                {
                    ImGui.BulletText("Bullet Text 1");
                    ImGui.BulletText($"Is Item Hovered: {ImGui.IsAnyItemHovered()}");
                    ImGui.TreePop();
                }
            }
            ImGui.End();
        }

        private static void CreatePopup(string name, string[] text)
        {
            // This function is just to show how a ImGui Popup is created
            ImGui.OpenPopup(name);
            if (ImGui.BeginPopup(name))
            {
                foreach(string s in text)
                {
                    ImGui.Text(s);
                }
            }
            ImGui.EndPopup();
        }
    }
}
