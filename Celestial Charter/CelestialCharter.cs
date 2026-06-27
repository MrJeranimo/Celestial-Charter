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
        public List<Astronomical> Astronomicals { get; private set; } = new List<Astronomical>();
        public Vehicle? CurrentVehicle = null;
        public VehicleData? CurrentVehicleData = null;
        public Orbit? VehicleOrbit = null;
        public IParentBody? AstronomicalOrbiting = null;
        public Situation VehicleSituation = new Situation();
        public List<Astronomical> NonVehicleAstronomicalList { get; private set; } = new List<Astronomical>();
        public List<Astronomical> RootAstronomicals { get; private set; } = new List<Astronomical>();
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
                Astronomicals = CelestialData.AstronomicalList;
                NonVehicleAstronomicalList = CelestialData.AstronomicalNonVehicleList;
                if(CelestialSystem != null)
                {
                    // Create one list of all the astronomicals for the StatusArrays to reference
                    foreach(var astro in NonVehicleAstronomicalList)
                    {
                        VehicleData.AstronomicalDataList.Add(new AstronomicalData(astro));
                    }
                    VehicleList = CelestialSystem.All.UnsafeAsList().OfType<Vehicle>().ToList();

                    // Create one VehicleData with a unique StatusArray for each of the astronomicals for each vehicle
                    foreach (var vehicle in VehicleList)
                    {
                        VehicleDataList.Add(new VehicleData(vehicle));
                    }

                    // Determine Root Astronomicals
                    RootAstronomicals = NonVehicleAstronomicalList.Where(a => a is StellarBody).ToList();
                }
            }

            // Check if the Controlled Vehicle has changed
            if (Program.ControlledVehicle != null && Program.ControlledVehicle.Id != CurrentVehicle?.Id)
            {
                // Check if we already have VehicleData for this vehicle
                bool hasVehicleData = false;
                foreach (var vehicleData in VehicleDataList)
                {
                    if (vehicleData.Vehicle.Id == Program.ControlledVehicle.Id)
                    {
                        hasVehicleData = true;
                        break;
                    }
                }

                // If not, create a new VehicleData for it
                if (!hasVehicleData)
                {
                    VehicleDataList.Add(new VehicleData(Program.ControlledVehicle));
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
                VehicleSituation = CurrentVehicle.Situation;

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
                        // Start the Tab Bar
                        if (ImGui.BeginTabBar("Status Tabs"))
                        {
                            // Start the Tree Tab
                            if (ImGui.BeginTabItem("Tree"))
                            {
                                // Start the Status Section
                                ImGuiTreeNodeFlags treeFlags = ImGuiTreeNodeFlags.DrawLinesToNodes;

                                // Only iterate through root-level astronomicals
                                foreach (var astro in RootAstronomicals)
                                {
                                    RenderAstronomicalNode(astro, treeFlags);
                                }
                                ImGui.EndTabItem();
                            }

                            // Start the Linear Tab
                            if (ImGui.BeginTabItem("Linear"))
                            {
                                // Start the Status Section
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
                                            RenderStatus(i, status);
                                        }
                                        ImGui.TreePop();
                                    }
                                    ImGui.Separator();
                                }
                                ImGui.EndTabItem();
                            }

                            // Start the General Info Tab
                            if (ImGui.BeginTabItem("General Info"))
                            {
                                ImGui.Text($"Current Celestial System: {CelestialSystem!.Id}");
                                ImGui.Separator();
                                ImGui.Text($"Current Vehicle: {CurrentVehicle.Id}");
                                ImGui.Separator();
                                ImGui.Text($"Astronomical Orbiting: {AstronomicalOrbiting.Id}");
                                ImGui.Separator();
                                ImGui.EndTabItem();
                            }
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.End();
                }
            }
        }

        private void RenderAstronomicalNode(Astronomical astro, ImGuiTreeNodeFlags treeFlags)
        {
            int index = NonVehicleAstronomicalList.FindIndex(a => a.Id == astro.Id);

            bool hasChildren = ((IParentBody)astro).HasChildren();
            ImGuiTreeNodeFlags nodeFlags = treeFlags;

            // Don't push if no children
            if (!hasChildren)
            {
                nodeFlags |= ImGuiTreeNodeFlags.NoTreePushOnOpen;
            }

            if (ImGui.TreeNodeEx(astro.Id, nodeFlags))
            {
                ImGui.Separator();
                BitArray? status = CurrentVehicleData?.StatusArray[index] as BitArray;
                if (status != null)
                {
                    RenderStatus(index, status);
                }

                // Recursively render children
                if (hasChildren)
                {
                    List<IOrbiter> childAstros = ((IParentBody)astro).Children.Where(c => c is not Vehicle).ToList();
                    foreach (var child in childAstros)
                    {
                        RenderAstronomicalNode((Astronomical)child, treeFlags);
                    }

                    ImGui.TreePop();
                }
            }
        }

        public static void RenderStatus(int index, BitArray status)
        {
            ImGuiTableFlags TableFlags = ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.ContextMenuInBody;
            // Render the 'Visited' status seperately then create a table for the rest
            ImGui.Text($"Visited: {status[0]}");
            if (ImGui.BeginTable("Status", 2, TableFlags))
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

                if (VehicleData.AstronomicalDataList[index].HasAtmosphere)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"In Atmosphere: {status[7]}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"Encountered Atmosphere: {status[8]}");
                }
                if (VehicleData.AstronomicalDataList[index].HasSurface)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Landed: {status[9]}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"Have Landed: {status[10]}");
                }
                if (VehicleData.AstronomicalDataList[index].HasOceans)
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

        [ModMenuEntry("Celestial Charter")]
        public static void CreateModMenu()
        {
            ImGui.MenuItem("Show Main Window", "]", ref ShowWindow, true);
        }
    }
}