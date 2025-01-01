namespace AdjustableThrusterMultipliers
{
    public class Settings
    {
        public ThrusterSettings GlobalThrusters { get; set; }
        public ThrusterSettings IonThruster { get; set; }
        public ThrusterSettings HydrogenThruster { get; set; }
        public ThrusterSettings AtmosphericThruster { get; set; }
        
        public string[] BlacklistedThrustSubtypes { get; set; } = new string[] { "ThrustSubtypeNameGoesHere" };

        public Settings()
        {
            GlobalThrusters = new ThrusterSettings();
            IonThruster = new ThrusterSettings();
            HydrogenThruster = new ThrusterSettings();
            AtmosphericThruster = new ThrusterSettings();
        }

        public Settings LoadSettings() => new Settings();

        public void SaveSettings() {  }

        public class ThrusterSettings
        {
            public float MaxThrustMultiplier { get; set; } = 11f;
            public float FuelUsePerMultiplier { get; set; } = 2.5f;
        }
    }
}