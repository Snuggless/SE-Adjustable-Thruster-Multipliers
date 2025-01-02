using Sandbox.ModAPI;

namespace AdjustableThrusterMultipliers
{
    public static class Helpers
    {
        public static Enums.ThrusterType GetThrusterType(this IMyThrust thruster)
        {
            if (thruster.BlockDefinition.SubtypeId.Contains("Atmo")) return Enums.ThrusterType.Atmospheric;
            else if (thruster.BlockDefinition.SubtypeId.Contains("Hydro")) return Enums.ThrusterType.Hydrogen;
            else if (thruster.BlockDefinition.SubtypeId.Contains("Ion")) return Enums.ThrusterType.Ion;

            return Enums.ThrusterType.Unknown;
        }
    }
}
