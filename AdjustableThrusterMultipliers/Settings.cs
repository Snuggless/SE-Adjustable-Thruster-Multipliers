using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace AdjustableThrust
{	
	public class Settings
	{
		public float MaxThrustMultiplier { get; set; }
		public float FuelUsePerMultiplier { get; set; }
		public string[] BlacklistedThrustSubtypes { get; set; }
		
		public Settings()
		{
			MaxThrustMultiplier = 11;
			FuelUsePerMultiplier = 2.5f;
			BlacklistedThrustSubtypes = new string[]{"ThrustSubtypeNameGoesHere"};
		}
		
		public Settings LoadSettings()
		{
			return new Settings();
		}
		
		public void SaveSettings() { }
	}
}