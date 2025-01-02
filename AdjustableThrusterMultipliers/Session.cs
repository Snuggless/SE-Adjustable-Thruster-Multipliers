using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace AdjustableThrusterMultipliers
{
    public class Session : MySessionComponentBase
    {
        public static long ModId = 3397453548;
        public static ushort NetworkId = 38537; 
        public static Guid ThrustMultiplierGuid = new Guid("0a5cc20c-96b9-4c2e-b003-dbfa74cd3a81");
        public static string ModBlacklistStorageName = "AdjustableThrustMultipliers-SubtypeNameBlacklist";
        public static Settings ModSettings = new Settings();
        public static bool SetupComplete = false;
        public static bool ControlsCreated = false;
        public static bool ActionsCreated = false;

        public static Dictionary<long, float> PendingThrustSync = new Dictionary<long, float>();
        public static int SyncCheckTimer = 0;

        public override void UpdateBeforeSimulation()
        {
            if (SetupComplete == false)
            {
                SetupComplete = true;
                Setup();
            }

            SyncCheckTimer++;

            if (SyncCheckTimer >= 10)
            {
                SyncCheckTimer = 0;

                if (PendingThrustSync.Keys.Count > 0)
                {
                    var clientData = new ClientData()
                    {
                        ThrustersToChange = PendingThrustSync
                    };

                    var sendData = MyAPIGateway.Utilities.SerializeToBinary<ClientData>(clientData);
                    _ = MyAPIGateway.Multiplayer.SendMessageToOthers(NetworkId, sendData);

                    PendingThrustSync.Clear();
                }
            }
        }

        public static void Setup()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += CreateControls;
            MyAPIGateway.TerminalControls.CustomActionGetter += CreateActions;
            MyAPIGateway.Utilities.RegisterMessageHandler(ModId, RegisterBlacklistedSubtypeName);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(NetworkId, NetworkHandler);

            ModSettings = ModSettings.LoadSettings();

            MyAPIGateway.Utilities.SetVariable<string[]>(ModBlacklistStorageName, ModSettings.BlacklistedThrustSubtypes);

            if (MyAPIGateway.Multiplayer.IsServer == false)
            {
                return;
            }

            MyAPIGateway.Parallel.Start(delegate {
                InitializeExistingThrusters();
            });
        }

        public static void RegisterBlacklistedSubtypeName(object receivedData)
        {
            var receivedString = receivedData as string;

            if (receivedString == null)
            {
                return;
            }

            var tempBlacklist = new List<string>(ModSettings.BlacklistedThrustSubtypes.ToList());
            tempBlacklist.Add(receivedString);
            ModSettings.BlacklistedThrustSubtypes = tempBlacklist.ToArray();
            MyAPIGateway.Utilities.SetVariable<string[]>(ModBlacklistStorageName, ModSettings.BlacklistedThrustSubtypes);
        }

        public static void NetworkHandler(byte[] receivedData)
        {
            var thrusts = MyAPIGateway.Utilities.SerializeFromBinary<ClientData>(receivedData);

            foreach (var kvp in thrusts.ThrustersToChange)
            {
                if (MyAPIGateway.Entities.TryGetEntityById(kvp.Key, out IMyEntity thrustEntity) == false)
                {
                    continue;
                }

                IMyThrust thrust = thrustEntity as IMyThrust;
                if (thrust == null)
                {
                    continue;
                }

                Enums.ThrusterType thrusterType = thrust.GetThrusterType();
                switch (thrusterType)
                {
                    case Enums.ThrusterType.Unknown:
                        if (kvp.Value > ModSettings.GlobalThrusters.MaxThrustMultiplier)
                        {
                            thrust.ThrustMultiplier = ModSettings.GlobalThrusters.MaxThrustMultiplier;
                        }

                        thrust.ThrustMultiplier = kvp.Value;
                        thrust.PowerConsumptionMultiplier = ModSettings.GlobalThrusters.FuelUsePerMultiplier * thrust.ThrustMultiplier;
                        break;
                    case Enums.ThrusterType.Atmospheric:
                        if (kvp.Value > ModSettings.AtmosphericThruster.MaxThrustMultiplier)
                        {
                            thrust.ThrustMultiplier = ModSettings.AtmosphericThruster.MaxThrustMultiplier;
                        }

                        thrust.ThrustMultiplier = kvp.Value;
                        thrust.PowerConsumptionMultiplier = ModSettings.AtmosphericThruster.FuelUsePerMultiplier * thrust.ThrustMultiplier;
                        break;
                    case Enums.ThrusterType.Hydrogen:
                        if (kvp.Value > ModSettings.HydrogenThruster.MaxThrustMultiplier)
                        {
                            thrust.ThrustMultiplier = ModSettings.HydrogenThruster.MaxThrustMultiplier;
                        }

                        thrust.ThrustMultiplier = kvp.Value;
                        thrust.PowerConsumptionMultiplier = ModSettings.HydrogenThruster.FuelUsePerMultiplier * thrust.ThrustMultiplier;
                        break;
                    case Enums.ThrusterType.Ion:
                        if (kvp.Value > ModSettings.IonThruster.MaxThrustMultiplier)
                        {
                            thrust.ThrustMultiplier = ModSettings.IonThruster.MaxThrustMultiplier;
                        }

                        thrust.ThrustMultiplier = kvp.Value;
                        thrust.PowerConsumptionMultiplier = ModSettings.IonThruster.FuelUsePerMultiplier * thrust.ThrustMultiplier;
                        break;
                    default:
                        continue;
                }
            }
        }

        public static void InitializeExistingThrusters()
        {
            var entityList = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entityList);

            foreach (var entity in entityList)
            {
                var cubeGrid = entity as IMyCubeGrid;
                if (cubeGrid == null)
                {

                    continue;

                }

                var blockList = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blockList);

                foreach (var block in blockList)
                {
                    if (block.FatBlock == null)
                    {
                        continue;
                    }

                    var thrust = block.FatBlock as IMyThrust;
                    if (thrust == null)
                    {
                        continue;
                    }

                    if (thrust.Storage == null)
                    {
                        continue;
                    }

                    if (thrust.Storage.ContainsKey(ThrustMultiplierGuid) == false)
                    {
                        continue;
                    }

                    float thrustMultiply;
                    float powerMultiply;

                    if (float.TryParse(thrust.Storage[ThrustMultiplierGuid], out thrustMultiply) == false)
                    {
                        continue;
                    }

                    switch (thrust.GetThrusterType())
                    {
                        case Enums.ThrusterType.Unknown:
                            if (thrustMultiply > ModSettings.GlobalThrusters.MaxThrustMultiplier)
                            {
                                thrustMultiply = ModSettings.GlobalThrusters.MaxThrustMultiplier;
                            }
                            powerMultiply = ModSettings.GlobalThrusters.FuelUsePerMultiplier;
                            break;
                        case Enums.ThrusterType.Atmospheric:
                            if (thrustMultiply > ModSettings.AtmosphericThruster.MaxThrustMultiplier)
                            {
                                thrustMultiply = ModSettings.AtmosphericThruster.MaxThrustMultiplier;
                            }
                            powerMultiply = ModSettings.AtmosphericThruster.FuelUsePerMultiplier;
                            break;
                        case Enums.ThrusterType.Hydrogen:
                            if (thrustMultiply > ModSettings.HydrogenThruster.MaxThrustMultiplier)
                            {
                                thrustMultiply = ModSettings.HydrogenThruster.MaxThrustMultiplier;
                            }
                            powerMultiply = ModSettings.HydrogenThruster.FuelUsePerMultiplier;
                            break;
                        case Enums.ThrusterType.Ion:
                            if (thrustMultiply > ModSettings.IonThruster.MaxThrustMultiplier)
                            {
                                thrustMultiply = ModSettings.IonThruster.MaxThrustMultiplier;
                            }
                            powerMultiply = ModSettings.IonThruster.FuelUsePerMultiplier;
                            break;
                        default:
                            continue;
                    }

                    thrust.ThrustMultiplier = thrustMultiply;
                    thrust.PowerConsumptionMultiplier = thrustMultiply * powerMultiply;

                    thrust.Storage[ThrustMultiplierGuid] = thrustMultiply.ToString();

                    if (PendingThrustSync.ContainsKey(thrust.EntityId) == true)
                    {
                        PendingThrustSync[thrust.EntityId] = thrustMultiply;
                        continue;
                    }

                    PendingThrustSync.Add(thrust.EntityId, thrustMultiply);
                }
            }
        }

        public static void CreateControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (ControlsCreated == true)
            {
                return;
            }

            IMyThrust thrust = block as IMyThrust;
            if (thrust == null)
            {
                return;
            }

            float maxThrustMultiply;
            switch (thrust.GetThrusterType())
            {
                case Enums.ThrusterType.Unknown:
                    maxThrustMultiply = ModSettings.GlobalThrusters.MaxThrustMultiplier;
                    break;
                case Enums.ThrusterType.Atmospheric:
                    maxThrustMultiply = ModSettings.AtmosphericThruster.MaxThrustMultiplier;
                    break;
                case Enums.ThrusterType.Hydrogen:
                    maxThrustMultiply = ModSettings.HydrogenThruster.MaxThrustMultiplier;
                    break;
                case Enums.ThrusterType.Ion:
                    maxThrustMultiply = ModSettings.IonThruster.MaxThrustMultiplier;
                    break;
                default:
                    return;
            }

            ControlsCreated = true;
            var slider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("AdjustThrustMultiplierSlider");
            slider.Enabled = Block => true;
            slider.Visible = Block => { return ControlVisibility(Block); };
            slider.SupportsMultipleBlocks = true;
            slider.Title = MyStringId.GetOrCompute("Thrust Multiplier");
            slider.Getter = Block => { return GetSlider(Block); };
            slider.Setter = SetSlider;
            slider.SetLimits(1, maxThrustMultiply);
            slider.Writer = SetSliderText;
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(slider);
            controls.Add(slider);
        }

        public static void CreateActions(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (ActionsCreated == true)
            {
                return;
            }

            if (block as IMyThrust == null)
            {
                return;
            }
        }

        public static bool ControlVisibility(IMyTerminalBlock block)
        {
            string[] blacklistArray = { "" };

            if (MyAPIGateway.Utilities.GetVariable<string[]>(ModBlacklistStorageName, out blacklistArray) == false)
            {
                return false;
            }

            var blacklist = new List<string>(blacklistArray.ToList());
            if (blacklist.Contains(block.SlimBlock.BlockDefinition.Id.SubtypeName) == true)
            {
                return false;
            }

            return true;
        }

        public static float GetSlider(IMyTerminalBlock block)
        {
            if (block.Storage == null)
            {
                return 1;
            }

            if (block.Storage.ContainsKey(ThrustMultiplierGuid) == false)
            {
                return 1;
            }

            float thrustMultiply = 0;
            if (float.TryParse(block.Storage[ThrustMultiplierGuid], out thrustMultiply) == false)
            {
                return 1;
            }

            return thrustMultiply;
        }

        public static void SetSlider(IMyTerminalBlock block, float sliderValue)
        {
            var roundedValue = (float)Math.Round(sliderValue, 3);
            if (block.Storage == null)
            {
                block.Storage = new MyModStorageComponent();
            }

            if (block.Storage.ContainsKey(ThrustMultiplierGuid) == false)
            {
                block.Storage.Add(ThrustMultiplierGuid, roundedValue.ToString());
            }
            else
            {
                block.Storage[ThrustMultiplierGuid] = roundedValue.ToString();
            }

            var thrust = block as IMyThrust;
            var powerMultiply = CalculatePowerMultiplier(roundedValue);
            thrust.ThrustMultiplier = roundedValue;
            thrust.PowerConsumptionMultiplier = powerMultiply;

            if (PendingThrustSync.ContainsKey(block.EntityId) == true)
            {
                PendingThrustSync[block.EntityId] = roundedValue;
            }
            else
            {
                PendingThrustSync.Add(block.EntityId, roundedValue);
            }
        }

        public static void SetSliderText(IMyTerminalBlock block, StringBuilder builder)
        {
            builder.Clear();
            if (block.Storage == null)
            {
                builder.Append("x1");
                return;
            }

            if (block.Storage.ContainsKey(ThrustMultiplierGuid) == false)
            {
                builder.Append("x1");
                return;
            }

            builder.Append("x").Append(block.Storage[ThrustMultiplierGuid]);
        }

        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= CreateControls;
            MyAPIGateway.TerminalControls.CustomActionGetter -= CreateActions;
            MyAPIGateway.Utilities.UnregisterMessageHandler(ModId, RegisterBlacklistedSubtypeName);
        }
    }
}
