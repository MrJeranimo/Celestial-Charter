using Brutal.ImGuiApi;
using HarmonyLib;
using KSA;
using ModMenu;
using StarMap.API;
using System.Collections;

namespace Celestial_Charter
{
    [StarMapMod]
    public class CelestialCharter
    {
        public readonly Harmony MHarmony = new Harmony("Celestial Charter");
        public CelestialSystem? CelestialSystem = null;
        public int NumCelestials = 0;
        public List<Astronomical> Astronomicals { get; private set; } = new List<Astronomical>();
        public Vehicle? CurrentVehicle = null;
        public VehicleData? CurrentVehicleData = null;
        public Orbit? VehicleOrbit = null;
        public Astronomical? AstronomicalOrbiting = null;
        public Situation VehicleSituation = new Situation();
        public List<Astronomical> NonVehicleAstronomicalList { get; private set; } = new List<Astronomical>();
        public List<Vehicle> VehicleList { get; private set; } = new List<Vehicle>();
        private List<VehicleData> VehicleDataList { get; set; } = new List<VehicleData>();
        public readonly string GUINAME = "Celestial Charter";
        public static bool ShowWindow = true;

        [StarMapAllModsLoaded]
        public void OnFullyLoaded()
        {
            MHarmony.PatchAll(typeof(CelestialCharter).Assembly);
        }

        [StarMapUnload]
        public void OnUnload()
        {
            MHarmony.UnpatchAll(nameof(CelestialCharter));
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
                    // Create one list of all the astronomicals for the StatusArrays to reference
                    foreach(var astro in NonVehicleAstronomicalList)
                    {
                        VehicleData.AstronomicalDataList.Add(new AstronomicalData(astro));
                    }
                    VehicleList = CelestialSystem.Vehicles.GetList();

                    // Create one VehicleData with a unique StatusArray for each of the astronomicals for each vehicle
                    foreach (var vehicle in VehicleList)
                    {
                        VehicleDataList.Add(new VehicleData(vehicle));
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

                // Get the Current Vehicle from the Data List
                CurrentVehicleData = VehicleDataList.Find(x => x.Vehicle == CurrentVehicle);
                if (CurrentVehicleData != null)
                {
                    // Update the Status array
                    CurrentVehicleData.Update();

                    // Don't render the window if closed
                    if (!ShowWindow) return;

                    // Starts the ImGui window
                    ImGuiWindowFlags flags = ImGuiWindowFlags.None;
                    if (ImGui.Begin(GUINAME, ref ShowWindow, flags))
                    {
                        // Basic Debug info
                        if (CelestialSystem != null) ImGui.Text($"Current Celestial System: {CelestialSystem.Id}");
                        ImGui.Separator();
                        if (CurrentVehicle != null) ImGui.Text($"Current Vehicle: {CurrentVehicle.Id}");
                        ImGui.Separator();
                        if (AstronomicalOrbiting != null) ImGui.Text($"Astronomical Orbiting: {AstronomicalOrbiting.Id}");
                        ImGui.Separator();

                        // Start the Status Section
                        if (ImGui.CollapsingHeader("Astronomical Status"))
                        {
                            // Set up flags and loop for each StatusArray
                            ImGuiTableFlags tableFlags = ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.ContextMenuInBody;
                            for (int i = 0; i < CurrentVehicleData.StatusArray.Count; i++)
                            {
                                // Create a TreeNode for each astronomical
                                if (ImGui.TreeNode(VehicleData.AstronomicalDataList[i].Id))
                                {
                                    // Get the StatusArray and create the text if it exists
                                    ImGui.Separator();
                                    BitArray? status = CurrentVehicleData.StatusArray[i] as BitArray;
                                    if (status != null)
                                    {
                                        // Render the 'Visited' status seperately then create a table for the rest
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
                                            ImGui.Text($"Suborbital: {status[5]}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"Suborbited: {status[6]}");

                                            if (VehicleData.AstronomicalDataList[i].HasAtmosphere)
                                            {
                                                ImGui.TableNextRow();
                                                ImGui.TableSetColumnIndex(0);
                                                ImGui.Text($"In Atmosphere: {status[7]}");
                                                ImGui.TableSetColumnIndex(1);
                                                ImGui.Text($"Encountered Atmosphere: {status[8]}");
                                            }
                                            if (VehicleData.AstronomicalDataList[i].HasSurface)
                                            {
                                                ImGui.TableNextRow();
                                                ImGui.TableSetColumnIndex(0);
                                                ImGui.Text($"Landed: {status[9]}");
                                                ImGui.TableSetColumnIndex(1);
                                                ImGui.Text($"Have Landed: {status[10]}");
                                            }
                                            if (VehicleData.AstronomicalDataList[i].HasOceans)
                                            {
                                                ImGui.TableNextRow();
                                                ImGui.TableSetColumnIndex(0);
                                                ImGui.Text($"Splashed Down: {status[11]}");
                                                ImGui.TableSetColumnIndex(1);
                                                ImGui.Text($"Have Splashed Down: {status[12]}");
                                            }
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

        [ModMenuEntry("Celestial Charter")]
        public static void CreateModMenu()
        {
            ImGui.MenuItem("Show Main Window", "]", ref ShowWindow, true);
        }
    }
}