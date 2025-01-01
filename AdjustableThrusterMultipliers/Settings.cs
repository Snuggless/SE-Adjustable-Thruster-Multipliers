namespace AdjustableThrusterMultipliers
{
    public class Settings
    {
        public float MaxThrustMultiplier { get; set; } = 100f;
        public float FuelUsePerMultiplier { get; set; } = 2f;
        public string[] BlacklistedThrustSubtypes { get; set; } = new string[] { "ThrustSubtypeNameGoesHere" };

        public Settings LoadSettings() => new Settings();

        public void SaveSettings() {  }
    }
}