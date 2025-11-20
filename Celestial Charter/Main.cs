using KSA;
using StarMap.API;
using Brutal.Logging;
using Brutal.ImGuiApi;
using System.Collections;

namespace Celestial_Charter
{
    [StarMapMod]
    public class Main
    {
        private CelestialSystem? CelestialSystem = null;
        private int NumCelestials = 0;
        public List<Astronomical> Astronomicals { get; private set; } = new List<Astronomical>();
        private Vehicle? CurrentVehicle = null;
        private VehicleData? CurrentVehicleData = null;
        private Orbit? VehicleOrbit = null;
        private Astronomical? AstronomicalOrbiting = null;
        private Situation VehicleSituation = new Situation();
        public List<Astronomical> NonVehicleAstronomicalList { get; private set; } = new List<Astronomical>();
        public List<Vehicle> VehicleList { get; private set; } = new List<Vehicle>();
        private List<VehicleData> VehicleDataList { get; set; } = new List<VehicleData>();
        private readonly string GUINAME = "Celestial Charter";
        private bool ShowWindow = true;

        [StarMapAllModsLoaded]
        public void AfterSystemLoaded()
        {
            DefaultCategory.Log.Info("Celestial Charter has been Loaded", "Celestial Charter");
        }

        [StarMapBeforeGui]
        public void BeforeGUI(double dt)
        {
            if (CelestialSystem == null)
            {
                CelestialSystem = CelestialData.FetchCelestialSystem();
                NumCelestials = CelestialData.NumCelestials;
                Astronomicals = CelestialData.AstronomicalList;
                NonVehicleAstronomicalList = CelestialData.AstronomicalNonVehicleList;
                if(CelestialSystem != null)
                {
                    VehicleList = CelestialSystem.Vehicles.GetList();
                    foreach (var vehicle in VehicleList)
                    {
                        VehicleDataList.Add(new VehicleData(vehicle, NonVehicleAstronomicalList.Count, NonVehicleAstronomicalList));
                    }
                }
            }
        }


        [StarMapAfterGui]
        public void AfterGUI(double dt)
        {
            CurrentVehicle = Program.ControlledVehicle;
            if (CurrentVehicle != null)
            {
                VehicleOrbit = CurrentVehicle.Orbit;
                AstronomicalOrbiting = VehicleOrbit.Parent;
                VehicleSituation = CurrentVehicle.LastKinematicStates.Situation;
                CurrentVehicleData = VehicleDataList.Find(x => x.Vehicle == CurrentVehicle);
                if (CurrentVehicleData != null)
                {
                    CurrentVehicleData.Update();

                    if (!ShowWindow) return;

                    ImGuiWindowFlags flags = ImGuiWindowFlags.None;
                    if (ImGui.Begin(GUINAME, ref ShowWindow, flags))
                    {
                        if (CelestialSystem != null) ImGui.Text($"Current Celestial System: {CelestialSystem.Id}");
                        ImGui.Separator();
                        if (CurrentVehicle != null) ImGui.Text($"Current Vehicle: {CurrentVehicle.Id}");
                        ImGui.Separator();
                        if (AstronomicalOrbiting != null) ImGui.Text($"Astronomical Orbiting: {AstronomicalOrbiting.Id}");
                        ImGui.Separator();
                        if (ImGui.CollapsingHeader("Astronomical Status"))
                        {
                            ImGuiTableFlags tableFlags = ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.ContextMenuInBody;
                            for (int i = 0; i < CurrentVehicleData.StatusArray.Count; i++)
                            {
                                if (ImGui.TreeNode(CurrentVehicleData.AstronomicalDataList[i].Id))
                                {
                                    ImGui.Separator();
                                    BitArray? status = CurrentVehicleData.StatusArray[i] as BitArray;
                                    if (status != null)
                                    {
                                        ImGui.Text($"Visited: {status[0]}");
                                        if (ImGui.BeginTable("Status", 2, tableFlags))
                                        {
                                            // Try new Table patterns
                                            ImGui.TableNextRow();
                                            ImGui.TableSetColumnIndex(0);
                                            ImGui.Text($"Flying By: {status[1]}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"Have Flown By: {status[2]}");
                                            ImGui.TableNextRow();
                                            ImGui.TableSetColumnIndex(0);
                                            ImGui.Text($"Orbiting: {status[3]}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"Have Orbited: {status[4]}");
                                            ImGui.TableNextRow();
                                            ImGui.TableSetColumnIndex(0);
                                            ImGui.Text($"Landed: {status[5]}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"Have Landed: {status[6]}");
                                            ImGui.TableNextRow();
                                            ImGui.TableSetColumnIndex(0);
                                            ImGui.Text($"Splashed Down: {status[7]}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"Have Splashed Down: {status[8]}");
                                            ImGui.EndTable();
                                        }
                                    }
                                    ImGui.TreePop();
                                }
                                ImGui.Separator();
                            }
                        }
                    }
                    ImGui.End();
                }
            }
        }

        // IDK
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
